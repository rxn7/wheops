using Godot;

public class Startup : Node {
	public override void _Ready() {
		Global.LoadMap("Sandbox");
	}
}
