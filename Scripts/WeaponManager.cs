using Godot;
using System.Linq;
using System.Collections.Generic;

public class WeaponManager : Node {
	public static readonly PackedScene MUZZLEFLASH_SCENE = GD.Load<PackedScene>("res://Scenes/MuzzleFlashParticles.tscn");
	public static readonly AudioStream DRYFIRE_SOUND = GD.Load<AudioStream>("res://Sounds/dryfire.wav");
	public static readonly AudioStream RELOAD_SOUND = GD.Load<AudioStream>("res://Sounds/reload.wav");

	public List<Weapon> m_weapons;
	public Weapon m_held_weapon;
	public int m_queued_weapon;
	public bool m_reloading = false;
	public float m_draw_timer = 0.0f;
	public float m_last_shot = 999f;
	private float m_reload_timer, m_shoot_timer;

	public bool CanShoot() => m_held_weapon != null && m_draw_timer == 0 && m_shoot_timer >= m_held_weapon.m_data.m_fire_rate && !m_reloading && m_held_weapon.m_ammo_left > 0;
	public bool WantsToShoot() => m_held_weapon != null && ((m_held_weapon.m_data.m_fire_type == FireType.Automatic && Input.IsActionPressed("fire")) || (m_held_weapon.m_data.m_fire_type == FireType.Manual && Input.IsActionJustPressed("fire"))) && m_draw_timer == 0 && !m_reloading;
	public bool WillShoot() => CanShoot() && WantsToShoot();
	public bool HasJustShot() => m_draw_timer == 0 && m_last_shot <= 0.3;

	public override void _Ready() {
		m_weapons = GetChildren().Cast<Weapon>().ToList();

		foreach(Weapon w in m_weapons) {
			w.Visible = false;
		}

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
			if(m_reload_timer >= m_held_weapon.m_data.m_reload_time) {
				FinishReload();
			}
		}
	}

	private void HandleShooting(float dt) {
		m_shoot_timer += dt;
		if(m_shoot_timer >= m_held_weapon.m_data.m_fire_rate) {
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

		if(m_draw_timer <= m_weapons[m_queued_weapon].m_data.m_draw_time*0.5f) {
			ChangeWeapon(m_queued_weapon);
		}
	}

	private void TakeInput() {
		if(Input.IsActionJustPressed("reload") && m_held_weapon.m_ammo_left < m_held_weapon.m_data.m_ammo_cap && m_draw_timer == 0) {
			StartReload();
		}

		for(int i=0; i<9; ++i) {
			if(Input.IsActionJustPressed($"weapon{i}")) {
				QueueWeaponChange(i);
			}
		}
	}

	public void QueueWeaponChange(int idx) {
		if(m_weapons.Count <= idx || idx < 0) return;
		if(idx == m_queued_weapon) return;

		m_queued_weapon = idx;
		m_draw_timer = m_weapons[m_queued_weapon].m_data.m_draw_time;
		m_reloading = false;
		m_shoot_timer = 0;
	}

	private void ChangeWeapon(int idx) {
		if(m_weapons.Count <= idx) return;
		if(m_weapons[idx] == m_held_weapon) return;

		foreach(Weapon w in m_weapons) {
			w.Visible = false;
		}

		m_held_weapon = m_weapons[idx];
		m_held_weapon.Visible = true;
		Player.Instance.m_hud.UpdateAmmoLabel();
	}

	private void Shoot() {
		SpawnMuzzleFlash();
		m_last_shot = 0;
		m_held_weapon.Shoot();
		Player.Instance.m_hud.UpdateAmmoLabel();
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
		Player.Instance.m_hud.UpdateAmmoLabel();
	}

	private void FinishReload() {
		m_reloading = false;
		m_held_weapon.Reload();
		SoundEffect.Spawn(this, RELOAD_SOUND, Random.RangeF(0.9f,1.1f));
		Player.Instance.m_hud.UpdateAmmoLabel();
	}
}
