using Godot;

public class Map : Node {
	[Export] private PackedScene m_map_colliders;

	public Player Player { get; private set; }

	public override void _Ready() {
		if(m_map_colliders == null) {
			Logger.Error($"Map '{Name}' has got no m_map_colliders assigned!");
			return;
		}

		AddChild(m_map_colliders.Instance());

		Player = (Player)Player.SCENE.Instance();
		AddChild(Player);
	}
}
