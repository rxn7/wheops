using Godot;
using System;

public class Target : Area {
	public const float CIRCLE_RADIUS = 1f;

	public const float RESTORE_TIME = 0.5f;
	public const float ROTATION_LERP_WEIGHT = 15f;

	public bool IsHit { get; private set; }
	public bool AutoRestore { get; set; } = true;
	public event EventHandler OnHit;

	private Vector3 m_StartRotation;
	private float m_RestoreTimer = 0;
	private float m_Rotation;
	private float m_TargetRotation;

	public override void _Ready() {
		m_StartRotation = RotationDegrees;
	}

	public override void _Process(float dt) {
		if(AutoRestore) {
			if(IsHit) {
				m_RestoreTimer += dt;
				if(m_RestoreTimer >= RESTORE_TIME) {
					m_RestoreTimer = 0;
					IsHit = false;
					m_TargetRotation = 0;
				}
			} else {
				m_RestoreTimer = 0;
			}
		}

		m_Rotation = Mathf.Lerp(m_Rotation, m_TargetRotation, ROTATION_LERP_WEIGHT*dt);
		RotationDegrees = m_StartRotation + Vector3.Up * m_Rotation;
	}

	public int CalculateHitScore(Vector3 hit_position) {
		float dist = GlobalTransform.origin.DistanceTo(hit_position);
		float ratio = 1 - Mathf.Clamp(dist / CIRCLE_RADIUS, 0, 1);

		return Mathf.CeilToInt(ratio * 100) / 10 * 10 + 10;
	}

	public void Hit(Vector3 position, Vector3 normal) {
		if(IsHit) return;

		SoundEffect.Spawn(Global.HITMARK_SOUND, Random.RangeF(0.9f,1.1f));
		IsHit = true;
		m_RestoreTimer = 0;

		if(GlobalTransform.origin.x - position.x > 0) m_TargetRotation = -180;
		else m_TargetRotation = 180;

		if(OnHit != null) {
			OnHit(this, EventArgs.Empty);
		}

		Global.SpawnDamagePopup(position + normal, CalculateHitScore(position));
	}
}
