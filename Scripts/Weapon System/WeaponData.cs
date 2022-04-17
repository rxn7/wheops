using Godot;

public enum FireType { Manual, Automatic }

public class WeaponData : Resource {
	[Export] public string m_name;
	[Export] public float m_fire_rate = 0.3f;
	[Export] public float m_move_speed_multiplier = 1.0f;
	[Export] public float m_reload_time = 2.0f;
	[Export] public float m_draw_time = 0.3f;
	[Export] public float m_aim_fov = 50;
	[Export] public float m_aim_speed = 10;
	[Export] public float m_aim_move_speed_multiplier = 0.8f;
	[Export] public float m_sway_speed = 10;
	[Export] public float m_sway_amount = 0.01f;
	[Export] public float m_camera_recoil_relax_speed = 7;
	[Export] public float m_camera_recoil_x = 4f;
	[Export] public float m_camera_recoil_y = 1f;
	[Export] public float m_recoil_relax_speed = 4;
	[Export] public float m_recoil_kickback = 0.05f;
	[Export] public float m_recoil_kickup = 0.02f;
	[Export] public float m_recoil_rotate = 5;
	[Export] public uint m_ammo_cap = 30;
	[Export] public FireType m_fire_type = FireType.Automatic;
	[Export] public AudioStream m_shoot_sound;
}
