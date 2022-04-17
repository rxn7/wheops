using Godot;

public class ViewmodelConfiguration {
	public static readonly ViewmodelConfiguration NormalConfiguration = new ViewmodelConfiguration(new Vector3(0.12f, -0.172f, -0.235f), new Vector3(4.291f, 5.891f, 5.256f));
	public static readonly ViewmodelConfiguration AimConfiguration = new ViewmodelConfiguration(new Vector3(0f, -0.0881f, -0.2f), new Vector3(0f, 0f, 0f));
	public static readonly ViewmodelConfiguration ShootConfiguration = new ViewmodelConfiguration(NormalConfiguration.m_position, NormalConfiguration.m_rotation, 40.0f);
	public static readonly ViewmodelConfiguration RunConfiguration = new ViewmodelConfiguration(new Vector3(-0.004f, -0.172f, -0.134f), new Vector3(4.291f, 71.974f, 5.256f));
	public static readonly ViewmodelConfiguration DrawConfiguration = new ViewmodelConfiguration(new Vector3(-0.004f, -0.472f, -0.134f), new Vector3(4.291f, 71.974f, 5.256f), 10.0f);
	public static readonly ViewmodelConfiguration ReloadConfiguration = new ViewmodelConfiguration(new Vector3(0.244f, -0.118f, -0.19f), new Vector3(-90.0f, 0, 0));
	public static readonly ViewmodelConfiguration FastRunConfiguration = new ViewmodelConfiguration(new Vector3(0.18f, -0.118f, -0.19f), new Vector3(80.0f, 0, 0));
	public static readonly ViewmodelConfiguration SlideConfiguration = new ViewmodelConfiguration(new Vector3(0.18f, -0.158f, -0.19f), new Vector3(80.0f, 0, 0));

	public ViewmodelConfiguration(Vector3 pos, Vector3 rot, float change_speed = 10) {
		m_position = pos;
		m_rotation = rot;
		m_change_speed = change_speed;
	}

	public Vector3 m_position;
	public Vector3 m_rotation;
	public float m_change_speed;
}
