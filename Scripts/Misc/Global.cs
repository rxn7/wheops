using Godot;

public class Global : Node {
	public static Global Instance { get; private set; }

	private static PackedScene CONSOLE_SCENE = GD.Load<PackedScene>("res://Scenes/Console.tscn");

	public override void _EnterTree() {
		Instance = this;

		AddChild(CONSOLE_SCENE.Instance());
	}

	public override void _Ready() {
		Random.Init();
		CommandManager.Init();
	}
}
