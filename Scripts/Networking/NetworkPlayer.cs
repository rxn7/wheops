using Godot;
using System;

public class NetworkPlayer : HumanBase {
	public static readonly PackedScene SCENE = GD.Load<PackedScene>("res://Scenes/NetworkPlayer.tscn");

	public NetworkPlayerData NetworkData { get; private set; }
	private NetworkPlayerInput m_Input = null;

	public new Vector2 Rotation {
		get {
			return new Vector2(Head.RotationDegrees.x, RotationDegrees.y);
		} set {
			Head.RotationDegrees = new Vector3(value.x, 0, 0);
			RotationDegrees = new Vector3(0, value.y, 0);
		}
	}

	public Vector3 Position => GlobalTransform.origin;

	public override void _Ready() {
		base._Ready();

		if(NetworkManager.IsHost) {
			m_Input = new NetworkPlayerInput();
		}
		
		m_ViewmodelHolder = Head.GetNode<Position3D>("ViewmodelHolder");
		WeaponManager = m_ViewmodelHolder.GetNode<NetworkWeaponManager>("WeaponManager");
	}

	public void InitNetworkData(NetworkPlayerData data) {
		NetworkData = data;
	}
}
