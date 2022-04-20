using Godot;

public class WeaponManager : Spatial {
	public static readonly AudioStream DRYFIRE_SOUND = GD.Load<AudioStream>("res://Sounds/dryfire.wav");
	public static readonly AudioStream RELOAD_SOUND = GD.Load<AudioStream>("res://Sounds/reload.wav");

	public Weapon HeldWeapon { get; private set; }
	public int QueuedWeaponID { get; private set; }
	public bool IsReloading { get; private set; }
	public float DrawTimer { get; private set; }
	public float LastShot { get; private set; } = Mathf.Inf;
	public float HeatTimer { get; private set; }

	public bool ShootTimerFinished => m_ShootTimer >= HeldWeapon.Data.FireRate;
	public bool CanShoot => HeldWeapon != null && DrawTimer == 0 && !IsReloading && HeldWeapon.AmmoLeft > 0;
	public bool WantsToShoot => HeldWeapon != null && ((HeldWeapon.Data.FireType == EFireType.Automatic && Input.IsActionPressed("fire")) || (HeldWeapon.Data.FireType == EFireType.Manual && Input.IsActionJustPressed("fire"))) && DrawTimer == 0 && !IsReloading;
	public bool WillShoot=> CanShoot && WantsToShoot;
	public bool HasJustShot => DrawTimer == 0 && LastShot <= HeldWeapon?.Data.FireRate * 1.2f;

	private float m_ReloadTimer, m_ShootTimer;

	public override void _Ready() {
		HeldWeapon = null;
	}

	public override void _Process(float dt) {
		LastShot += dt;

		if(HasJustShot && WantsToShoot && HeldWeapon.AmmoLeft > 0) HeatTimer += dt;
		else HeatTimer = 0;

		HandlePickup(dt);
		HandleReloading(dt);
		HandleShooting(dt);
		TakeInput();
	}

	public override void _Input(InputEvent e) {
		if(e.IsActionPressed("weapon_up")) {
			QueueWeaponChange(QueuedWeaponID+1);
		}  else if(e.IsActionPressed("weapon_down")) {
			QueueWeaponChange(QueuedWeaponID-1);
		}
	}

	private void HandleReloading(float dt) {
		if(IsReloading) {
			m_ReloadTimer += dt;
			if(m_ReloadTimer >= HeldWeapon.Data.ReloadTime) {
				FinishReload();
			}
		}
	}

	private void HandleShooting(float dt) {
		m_ShootTimer += dt;
		if(!Console.Instance.Visible && m_ShootTimer >= HeldWeapon.Data.FireRate) {
			if(WantsToShoot) {
				if(CanShoot && ShootTimerFinished) {
					Shoot();
					m_ShootTimer = 0;
				} else if(Input.IsActionJustPressed("fire")) {
					m_ShootTimer = 0;
					LastShot = 0;
					SoundEffect.Spawn(this, DRYFIRE_SOUND, Random.RangeF(0.8f,1.2f));	
				}
			}
		} 
	}

	private void HandlePickup(float dt) {
		DrawTimer -= dt;
		if(DrawTimer <= 0) {
			DrawTimer = 0;
		} 

		if(DrawTimer <= WeaponDB.Weapons[QueuedWeaponID].DrawTime*0.5f) {
			ChangeWeapon(QueuedWeaponID);
		}
	}

	private void TakeInput() {
		if(Console.Instance.Visible) return;

		if(Input.IsActionJustPressed("reload") && HeldWeapon.AmmoLeft < HeldWeapon.Data.AmmoCap && DrawTimer == 0) {
			StartReload();
		}

		for(int i=0; i<9; ++i) {
			if(Input.IsActionJustPressed($"weapon{i}")) {
				QueueWeaponChange(i);
			}
		}
	}

	public void QueueWeaponChange(int idx) {
		if(WeaponDB.Weapons.Length <= idx || idx < 0) return;
		if(idx == QueuedWeaponID) return;

		QueuedWeaponID = idx;
		DrawTimer = WeaponDB.Weapons[QueuedWeaponID].DrawTime;
		IsReloading = false;
		m_ShootTimer = 0;
	}

	private void ChangeWeapon(int idx) {
		if(WeaponDB.Weapons.Length <= idx || idx < 0) return;
		if(idx == HeldWeapon?.DataID) return;

		HeldWeapon?.QueueFree();

		HeldWeapon = SpawnWeapon(idx);
		Global.Player.Hud.UpdateWeaponInfoLabel();
	}

	private Weapon SpawnWeapon(int idx) {
		if(WeaponDB.Weapons.Length <= idx || idx < 0) return null;

		Weapon w = (Weapon)WeaponDB.Weapons[idx].Scene.Instance();
		AddChild(w);

		return w;
	}

	private void Shoot() {
		LastShot = 0;
		HeldWeapon.Shoot();
		SpawnMuzzleFlash();
		Global.Player.Hud.UpdateWeaponInfoLabel();
		ShootRaycast();
	}

	private void ShootRaycast() {
		PhysicsDirectSpaceState space = PhysicsServer.SpaceGetDirectState(Global.Player.Camera.GetWorld().Space);
		Vector3 start = Global.Player.Camera.GlobalTransform.origin; 
		Vector3 end = start - Global.Player.Camera.GlobalTransform.basis.z * Global.Player.WeaponManager.HeldWeapon.Data.RaycastDistance;
		Godot.Collections.Dictionary result = space.IntersectRay(start, end, new Godot.Collections.Array(Global.Player));

		if(result.Count > 0) {
			Spatial collider = (Spatial)result["collider"];
			Vector3 position = (Vector3)result["position"];
			Vector3 normal = (Vector3)result["normal"];
			
			if(collider == null) return;

			if(collider is RigidBody rigidbody) {
				rigidbody.ApplyImpulse(position - collider.GlobalTransform.origin, end * HeldWeapon.Data.HitForce);
				Global.SpawnMuzzleFlash(rigidbody, position);
			}

			Global.SpawnBulletImpact(position, normal);
		}
	}

	private void SpawnMuzzleFlash() {
		Global.SpawnMuzzleFlash((Spatial)this, HeldWeapon.MuzzlePoint.GlobalTransform.origin);
	}

	private void StartReload() {
		if(IsReloading) return;

		IsReloading = true;
		m_ReloadTimer = 0;
		Global.Player.Hud.UpdateWeaponInfoLabel();
	}

	private void FinishReload() {
		IsReloading = false;
		HeldWeapon.Reload();
		SoundEffect.Spawn(this, RELOAD_SOUND, Random.RangeF(0.9f,1.1f));
		Global.Player.Hud.UpdateWeaponInfoLabel();
	}
}
