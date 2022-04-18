using Godot;

public class WeaponManager : Node {
	public static readonly PackedScene MUZZLEFLASH_SCENE = GD.Load<PackedScene>("res://Particles/MuzzleFlashParticles.tscn");
	public static readonly AudioStream DRYFIRE_SOUND = GD.Load<AudioStream>("res://Sounds/dryfire.wav");
	public static readonly AudioStream RELOAD_SOUND = GD.Load<AudioStream>("res://Sounds/reload.wav");

	public Weapon m_held_weapon;
	public int m_queued_weapon;
	public bool m_reloading = false;
	public float m_draw_timer = 0.0f;
	public float m_last_shot = Mathf.Inf;
	private float m_reload_timer, m_shoot_timer;

	public bool CanShoot() => m_held_weapon != null && m_draw_timer == 0 && m_shoot_timer >= m_held_weapon.Data.m_fire_rate && !m_reloading && m_held_weapon.m_ammo_left > 0;
	public bool WantsToShoot() => m_held_weapon != null && ((m_held_weapon.Data.m_fire_type == EFireType.Automatic && Input.IsActionPressed("fire")) || (m_held_weapon.Data.m_fire_type == EFireType.Manual && Input.IsActionJustPressed("fire"))) && m_draw_timer == 0 && !m_reloading;
	public bool WillShoot() => CanShoot() && WantsToShoot();
	public bool HasJustShot() => m_draw_timer == 0 && m_last_shot <= 0.3;

	public override void _Ready() {
		MaterialCache.LoadParticles(MUZZLEFLASH_SCENE);
		m_held_weapon = null;
	}

	public override void _Process(float dt) {
		m_last_shot += dt;

		HandlePickup(dt);
		HandleReloading(dt);
		HandleShooting(dt);
		TakeInput();
	}

	public override void _Input(InputEvent e) {
		if(e.IsActionPressed("weapon_up")) {
			QueueWeaponChange(m_queued_weapon+1);
		}  else if(e.IsActionPressed("weapon_down")) {
			QueueWeaponChange(m_queued_weapon-1);
		}
	}

	private void HandleReloading(float dt) {
		if(m_reloading) {
			m_reload_timer += dt;
			if(m_reload_timer >= m_held_weapon.Data.m_reload_time) {
				FinishReload();
			}
		}
	}

	private void HandleShooting(float dt) {
		m_shoot_timer += dt;
		if(!Console.Instance.IsActive() && m_shoot_timer >= m_held_weapon.Data.m_fire_rate) {
			if(WantsToShoot()) {
				if(CanShoot()) {
					Shoot();
					m_shoot_timer = 0;
				} else if(Input.IsActionJustPressed("fire")) {
					m_shoot_timer = 0;
					m_last_shot = 0;
					SoundEffect.Spawn(this, DRYFIRE_SOUND, Random.RangeF(0.8f,1.2f));	
				}
			}
		} 
	}

	private void HandlePickup(float dt) {
		m_draw_timer -= dt;
		if(m_draw_timer <= 0) {
			m_draw_timer = 0;
		} 

		if(m_draw_timer <= WeaponDB.Weapons[m_queued_weapon].m_draw_time*0.5f) {
			ChangeWeapon(m_queued_weapon);
		}
	}

	private void TakeInput() {
		if(Console.Instance.IsActive()) return;

		if(Input.IsActionJustPressed("reload") && m_held_weapon.m_ammo_left < m_held_weapon.Data.m_ammo_cap && m_draw_timer == 0) {
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
		if(idx == m_queued_weapon) return;

		m_queued_weapon = idx;
		m_draw_timer = WeaponDB.Weapons[m_queued_weapon].m_draw_time;
		m_reloading = false;
		m_shoot_timer = 0;
	}

	private void ChangeWeapon(int idx) {
		if(WeaponDB.Weapons.Length <= idx || idx < 0) return;
		if(idx == m_held_weapon?.m_data_id) return;

		m_held_weapon?.QueueFree();

		m_held_weapon = SpawnWeapon(idx);
		Global.Instance.CurrentMap.Player.m_hud.UpdateWeaponInfoLabel();
	}

	private Weapon SpawnWeapon(int idx) {
		if(WeaponDB.Weapons.Length <= idx || idx < 0) return null;

		Weapon w = (Weapon)WeaponDB.Weapons[idx].m_scene.Instance();
		AddChild(w);

		return w;
	}

	private void Shoot() {
		SpawnMuzzleFlash();
		m_held_weapon.Shoot();
		Global.Instance.CurrentMap.Player.m_hud.UpdateWeaponInfoLabel();
		m_last_shot = 0;
	}

	private void SpawnMuzzleFlash() {
		OneShotParticles p = (OneShotParticles)MUZZLEFLASH_SCENE.Instance();
		AddChild(p);

		Transform t = p.GlobalTransform;
		t.origin = m_held_weapon.m_muzzle_point.GlobalTransform.origin;
		p.GlobalTransform = t;

		p.RotationDegrees = m_held_weapon.m_muzzle_point.GlobalTransform.basis.GetEuler();
	}

	private void StartReload() {
		if(m_reloading) return;

		m_reloading = true;
		m_reload_timer = 0;
		Global.Instance.CurrentMap.Player.m_hud.UpdateWeaponInfoLabel();
	}

	private void FinishReload() {
		m_reloading = false;
		m_held_weapon.Reload();
		SoundEffect.Spawn(this, RELOAD_SOUND, Random.RangeF(0.9f,1.1f));
		Global.Instance.CurrentMap.Player.m_hud.UpdateWeaponInfoLabel();
	}
}
