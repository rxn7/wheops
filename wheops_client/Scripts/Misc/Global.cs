using Godot;

public class Global : Node {
	public static Global Instance { get; private set; }
	public Map CurrentMap { get; private set; }

	public override void _EnterTree() {
		Instance = this;
	}

	public override void _Ready() {
		Random.Init();
		Config.Init();
		WeaponDB.Init();
		CommandManager.Init();
	}

	public void LoadMap(string name) {
		if(CurrentMap != null) {
			CurrentMap.QueueFree();
		}

		Logger.Info($"Loading map '{name}'...");

		PackedScene scene = GD.Load<PackedScene>($"res://Maps/{name}.tscn");
		CurrentMap = (Map)scene.Instance();
		AddChild(CurrentMap);
	}
}
