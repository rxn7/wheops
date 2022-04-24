using Godot;
using System;

public class NetworkWeaponManager : WeaponManagerBase {
	public override void _Ready() {
		base._Ready();
	}

	public override void _Process(float dt) {
		base._Process(dt);
	}

	public override void QueueWeaponChange(int idx) {
		if(WeaponDB.Weapons.Length <= idx || idx < 0) return;
		if(idx == QueuedWeaponID) return;

		QueuedWeaponID = idx;
		DrawTimer = WeaponDB.Weapons[QueuedWeaponID].DrawTime;
		IsReloading = false;
	}

	protected override void ChangeWeapon(int idx) {
		if(WeaponDB.Weapons.Length <= idx || idx < 0) return;
		if(idx == HeldWeapon?.Data.ID) return;

		if(HeldWeapon != null) {
			HeldWeapon.Visible = false;
		}

		HeldWeapon = WeaponDB.Spawn(this, idx);
		HeldWeapon.Visible = true;
	}

	private void StartReload() {
		if(IsReloading) return;

		IsReloading = true;
	}

	private void FinishReload() {
		IsReloading = false;
		HeldWeapon.Reload();
		SoundEffect.Spawn3D(this, GlobalTransform.origin, RELOAD_SOUND, Random.RangeF(0.9f,1.1f));
	}

	protected override void Shoot() {
		base.Shoot();
		HeldWeapon.Shoot();
	}
}
