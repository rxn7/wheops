using Godot;

public class SmoothCamera : Position3D {
	private Transform m_PrevTransform;
	private Transform m_CurrentTransform;
	private Spatial m_Target;
	private bool m_UpdateTransforms = true;

	public override void _Ready() {
		m_Target = GetParent<Spatial>();
		SetAsToplevel(true);
		
		GlobalTransform = m_Target.GlobalTransform;
		m_PrevTransform = GlobalTransform; 
		m_CurrentTransform = GlobalTransform;
	}

	public override void _Process(float dt) {
		if(m_UpdateTransforms) {
			m_UpdateTransforms = false;
			m_PrevTransform = m_CurrentTransform;
			m_CurrentTransform = m_Target.GlobalTransform;
		}

		float f = Mathf.Clamp(Engine.GetPhysicsInterpolationFraction(), 0, 1);
		GlobalTransform = m_PrevTransform.InterpolateWith(m_CurrentTransform, f);

		Transform t = GlobalTransform;
		GlobalTransform = t;

		Vector3 rotation = Global.Player.Head.RotationDegrees + Global.Player.RotationDegrees + Global.Player.CameraRecoil.RotationDegrees;
		rotation.z += Global.Player.CameraTilt;

		rotation.x = Mathf.Clamp(rotation.x, -89, 89);
		RotationDegrees = rotation;
	}

	public override void _PhysicsProcess(float dt) {
		m_UpdateTransforms = true;
	}
}
