using Godot;

public class ViewmodelProperties {
	public static readonly ViewmodelProperties NormalConfiguration = new ViewmodelProperties(new Vector3(0.12f, -0.172f, -0.235f), new Vector3(4.291f, 5.891f, 5.256f));
	public static readonly ViewmodelProperties CrouchConfiguration = new ViewmodelProperties(new Vector3(0.05f, -0.172f, -0.15f), new Vector3(-1f, 50f, 20));
	public static readonly ViewmodelProperties AimConfiguration = new ViewmodelProperties(new Vector3(0f, -0.0881f, -0.2f), new Vector3(0f, 0f, 0f));
	public static readonly ViewmodelProperties ShootConfiguration = new ViewmodelProperties(NormalConfiguration.Position, NormalConfiguration.Rotaton, 40.0f);
	public static readonly ViewmodelProperties RunConfiguration = new ViewmodelProperties(new Vector3(-0.004f, -0.172f, -0.134f), new Vector3(4.291f, 71.974f, 5.256f));
	public static readonly ViewmodelProperties DrawConfiguration = new ViewmodelProperties(new Vector3(-0.004f, -0.472f, -0.134f), new Vector3(4.291f, 71.974f, 5.256f), 10.0f);
	public static readonly ViewmodelProperties ReloadConfiguration = new ViewmodelProperties(new Vector3(0.244f, -0.118f, -0.19f), new Vector3(-90.0f, 0, 0));
	public static readonly ViewmodelProperties FastRunConfiguration = new ViewmodelProperties(new Vector3(0.18f, -0.118f, -0.19f), new Vector3(80.0f, 0, 0));
	public static readonly ViewmodelProperties SlideConfiguration = new ViewmodelProperties(new Vector3(0.18f, -0.158f, -0.19f), new Vector3(80.0f, 0, 0));

	public ViewmodelProperties(Vector3 pos, Vector3 rot, float change_speed = 10) {
		Position = pos;
		Rotaton = rot;
		ChangeSpeed = change_speed;
	}

	public Vector3 Position { get; private set; }
	public Vector3 Rotaton { get; private set; }
	public float ChangeSpeed { get; set; }
}
