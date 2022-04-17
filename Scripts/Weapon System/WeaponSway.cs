using Godot;

public class WeaponSway : Position3D {
	public const float MAX_SWAY_X = 0.05f;
	public const float MAX_SWAY_Y = 0.02f;

	private Vector3 m_target;
	private float m_prev_y_rot;
	private float m_prev_x_rot;

	public override void _Process(float dt) {
		if(Player.Instance.m_weapon_mgr.m_held_weapon == null) return;

		float x = Player.Instance.m_head.RotationDegrees.x;
		float dx = m_prev_x_rot - x;

		float y = Player.Instance.RotationDegrees.y;
		float dy = (y - m_prev_y_rot);

		if(dy < -180) dy += 360;
		if(dy > 180) dy -= 360;

		float amount = Player.Instance.m_weapon_mgr.m_held_weapon.m_data.m_sway_amount;

		if(Player.Instance.m_aiming) {
			amount *= 0.2f;
		}

		m_target.x = Mathf.Clamp(dy * amount, -MAX_SWAY_X, MAX_SWAY_X);
		m_target.y = Mathf.Clamp(dx * amount, -MAX_SWAY_Y, MAX_SWAY_Y);

		ApplySway(dt);

		m_prev_x_rot = x;
		m_prev_y_rot = y;
	}

	private void ApplySway(float dt) {
		Transform t = Transform;
		t.origin = t.origin.LinearInterpolate(m_target, Player.Instance.m_weapon_mgr.m_held_weapon.m_data.m_sway_speed*dt);
		Transform = t;
	}
}
