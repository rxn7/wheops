using Godot;

public class Startup : Node {
	public override void _Ready() {
		Global.Instance.LoadMap("Sandbox");
	}
}
