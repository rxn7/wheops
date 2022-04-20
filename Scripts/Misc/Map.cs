using Godot;

public class Map : Node {
	public override void _Ready() {
		Global.Player = (Player)Player.SCENE.Instance();
		AddChild(Global.Player);
	}
}
