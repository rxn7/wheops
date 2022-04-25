using Godot;

public abstract class WeaponManagerBase : Spatial {
	public static readonly AudioStream DRYFIRE_SOUND = GD.Load<AudioStream>("res://Sounds/dryfire.wav");
	public static readonly AudioStream RELOAD_SOUND = GD.Load<AudioStream>("res://Sounds/reload.wav");

	public Weapon HeldWeapon { get; protected set; }
	public int QueuedWeaponID { get; protected set; }
	public bool IsReloading { get; protected set; }
	public float DrawTimer { get; protected set; }
	public float LastShot { get; protected set; } = Mathf.Inf;

	public bool HasJustShot => DrawTimer == 0 && LastShot <= HeldWeapon?.Data.FireRate * 1.2f;
	public virtual bool WantsToShoot => false;
	public virtual bool WillShoot => false;

	public override void _Ready() {
		HeldWeapon = null;
	}

	public override void _Process(float dt) {
		HandlePickup(dt);
	}

	protected void HandlePickup(float dt) {
		DrawTimer -= dt;
		if(DrawTimer <= 0) {
			DrawTimer = 0;
		} 

		if(DrawTimer <= WeaponDB.Weapons[QueuedWeaponID].DrawTime*0.5f) {
			ChangeWeapon(QueuedWeaponID);
		}
	}

	public abstract void QueueWeaponChange(int idx);
	protected abstract void ChangeWeapon(int idx);

	protected virtual void Shoot() {
		LastShot = 0;
		SoundEffect.Spawn(HeldWeapon.Data.ShootSound, Random.RangeF(0.95f, 1.05f));
		SpawnMuzzleFlash();
	}

	protected void SpawnMuzzleFlash() {
		Global.SpawnMuzzleFlash((Spatial)this, HeldWeapon.MuzzlePoint.GlobalTransform.origin);
	}
}
