using System;
using Godot;

public class LocalWeaponManager : WeaponManagerBase {
	public float HeatTimer { get; private set; }

	[Export(PropertyHint.Layers3dPhysics)] private uint m_ShootRaycastMask = 0;
	private float m_ReloadTimer, m_ShootTimer;

	private Loadout m_Loadout;
	public Loadout Loadout {
		get => m_Loadout;
		set {
			m_Loadout = value;
			QueueWeaponChange(0);
		}
	}

	public bool ShootTimerFinished => m_ShootTimer >= HeldWeapon.Data.FireRate;
	public bool CanShoot => HeldWeapon != null && DrawTimer == 0 && !IsReloading && HeldWeapon.AmmoLeft > 0;
	public override bool WantsToShoot => HeldWeapon != null && ((HeldWeapon.Data.FireType == EFireType.Automatic && Input.IsActionPressed("fire")) || (HeldWeapon.Data.FireType == EFireType.Manual && Input.IsActionJustPressed("fire"))) && DrawTimer == 0 && !IsReloading;
	public override bool WillShoot => CanShoot && WantsToShoot;

	public override void _Ready() {
		base._Ready();
		Loadout = new Loadout(this, 0, 1);
	}

	public override void _Process(float dt) {
		base._Process(dt);

		if(NetworkManager.IsHost || NetworkManager.IsSinglePlayer) {
			LastShot += dt;
			
			if(HasJustShot && WantsToShoot && HeldWeapon.AmmoLeft > 0) HeatTimer += dt;
			else HeatTimer = 0;

			TakeInput(); 
			HandleReloading(dt);
			HandleShooting(dt);
		}
	}

	public override void QueueWeaponChange(int idx) {
		if(Loadout.Weapons.Length <= idx || idx < 0) return;
		if(idx == QueuedWeaponID) return;

		QueuedWeaponID = idx;
		DrawTimer = m_Loadout.Weapons[QueuedWeaponID].Data.DrawTime;
		IsReloading = false;
		m_ShootTimer = 0;
	}

	protected override void ChangeWeapon(int idx) {
		if(Loadout.Weapons.Length <= idx || idx < 0) return;
		if(idx == HeldWeapon?.Data.ID) return;

		if(HeldWeapon != null) {
			HeldWeapon.Visible = false;
		}

		HeldWeapon = Loadout.Weapons[idx];
		HeldWeapon.Visible = true;
		Global.Player.Hud.UpdateWeaponInfoLabel();
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
		SoundEffect.Spawn(RELOAD_SOUND, Random.RangeF(0.9f,1.1f));
		Global.Player.Hud.UpdateWeaponInfoLabel();
	}

	protected override void Shoot() {
		base.Shoot();
		HeldWeapon.Shoot();
		Global.Player.Hud.UpdateWeaponInfoLabel();
		ShootRaycast();
	}

	private void ShootRaycast() {
		PhysicsDirectSpaceState space = PhysicsServer.SpaceGetDirectState(Global.Player.Camera.GetWorld().Space);
		Vector3 start = Global.Player.Camera.GlobalTransform.origin; 
		Vector3 end = start - Global.Player.Camera.GlobalTransform.basis.z * Global.Player.WeaponManager.HeldWeapon.Data.RaycastDistance;
		Godot.Collections.Dictionary result = space.IntersectRay(start, end, new Godot.Collections.Array(Global.Player), m_ShootRaycastMask, true, true);

		if(result.Count > 0) {
			Spatial collider = (Spatial)result["collider"];
			Vector3 position = (Vector3)result["position"];
			Vector3 normal = (Vector3)result["normal"];
			
			if(collider == null) return;

			if(collider is RigidBody rigidbody) {
				rigidbody.ApplyImpulse(position - collider.GlobalTransform.origin, end * HeldWeapon.Data.HitForce);
			} else if(collider is Target target) {
				target.Hit(position, normal);
			}

			Global.SpawnBulletImpact(position, normal);
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
		if(!Console.Active && m_ShootTimer >= HeldWeapon?.Data.FireRate) {
			if(WantsToShoot) {
				if(CanShoot && ShootTimerFinished) {
					Shoot();
					m_ShootTimer = 0;
				} else if(Input.IsActionJustPressed("fire")) {
					m_ShootTimer = 0;
					LastShot = 0;
					SoundEffect.Spawn(DRYFIRE_SOUND, Random.RangeF(0.8f,1.2f));	
				}
			}
		} 
	}

	private void TakeInput() {
		if(Console.Instance.Visible) return;

		if(Input.IsActionJustPressed("reload") && HeldWeapon.AmmoLeft < HeldWeapon.Data.AmmoCap && DrawTimer == 0) {
			StartReload();
		}

		for(int i=0; i<m_Loadout.Weapons.Length && i<9; ++i) {
			if(Input.IsActionJustPressed("weapon" + i)) {
				QueueWeaponChange(i);
			}
		}
	}
}
