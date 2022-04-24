using Godot;
using System;

/*
public class NetworkPlayerInput {
	public Vector3 Movement { get; set; }
	public bool Jump { get; set; }
	public bool Crouch { get; set; }
	public bool Run { get; set; }
	public bool Shoot { get; set; }
}
*/

public class NetworkPlayer : HumanBase {
	public static readonly PackedScene SCENE = GD.Load<PackedScene>("res://Scenes/NetworkPlayer.tscn");

	public NetworkPlayerData NetworkData { get; private set; }
	private Vector3 m_NetworkPosition;
	// public NetworkPlayerInput Input = null;

	public new Vector2 Rotation {
		get => new Vector2(Head.RotationDegrees.x, RotationDegrees.y);
		set {
			Head.RotationDegrees = new Vector3(value.x, 0, 0);
			RotationDegrees = new Vector3(0, value.y, 0);
		}
	}

	public override void _Ready() {
		base._Ready();

		m_ViewmodelHolder = Head.GetNode<Position3D>("ViewmodelHolder");
		WeaponManager = m_ViewmodelHolder.GetNode<NetworkWeaponManager>("WeaponManager");
	}

	public override void _Process(float dt) {
		base._Process(dt);
	}

	public void InitNetworkData(NetworkPlayerData data) {
		NetworkData = data;
	}
}
