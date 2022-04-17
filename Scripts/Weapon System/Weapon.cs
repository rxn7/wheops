using Godot;

public class Weapon : Spatial {
	[Export] public WeaponData m_data;

	public Position3D m_muzzle_point;
	public uint m_ammo_left;
	private Vector3 m_recoil;
	private Vector3 m_recoil_rot;
	private Vector3 m_start_position;

	public override void _Ready() {
		m_muzzle_point = GetNode<Position3D>("MuzzlePoint");
		m_start_position = Transform.origin;
		m_ammo_left = m_data.m_ammo_cap;
	}

	public override void _Process(float dt) {
		CalculateRecoil(dt);
		ApplyRecoil(dt);
	}

	private void CalculateRecoil(float dt) {
		m_recoil = m_recoil.LinearInterpolate(Vector3.Zero, m_data.m_recoil_relax_speed*dt);
		m_recoil_rot = m_recoil_rot.LinearInterpolate(Vector3.Zero, m_data.m_recoil_relax_speed*dt);
	}

	private void ApplyRecoil(float dt) {
		Transform t = Transform;
		t.origin = t.origin.LinearInterpolate(m_recoil + m_start_position, 20*dt);
		Transform = t;
		RotationDegrees = RotationDegrees.LinearInterpolate(m_recoil_rot, 20*dt);
	}

	public void Shoot() {
		--m_ammo_left;
		SoundEffect.Spawn(this, m_data.m_shoot_sound, Random.RangeF(0.95f, 1.05f));

		float mul = 1;
		if(Player.Instance.m_aiming) mul = 0.2f;

		m_recoil.z = m_data.m_recoil_kickback * Random.RangeF(0.8f, 1.2f) * mul;
		m_recoil.y = m_data.m_recoil_kickup * Random.RangeF(0.8f, 1.2f) * mul;
		m_recoil_rot.x = m_data.m_recoil_rotate * Random.RangeF(0.95f, 1.1f) * mul;
		m_recoil_rot.z = m_data.m_recoil_rotate * Random.RangeF(0.5f, 1.5f) * (Random.RangeI(0, 1)*2-1) * mul;

		Player.Instance.AddRecoil(m_data.m_camera_recoil_x * Random.RangeF(0.8f,1.2f), m_data.m_camera_recoil_y * (Random.RangeI(0,1)*2-1) * Random.RangeF(0.8f,1.2f));
	}

	public void Reload() {
		m_ammo_left = m_data.m_ammo_cap;
	}
}
