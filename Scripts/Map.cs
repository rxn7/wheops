using Godot;

public class Map : Node {
	public override void _Ready() {
		Global.Player = (LocalPlayer)LocalPlayer.SCENE.Instance();
		AddChild(Global.Player);
	}
}
