using Godot;

public class RemotePlayer : HumanBase {
	public static readonly PackedScene SCENE = GD.Load<PackedScene>("res://Scenes/RemotePlayer.tscn");

	public RemotePlayerData NetworkData { get; private set; }
	public Vector3 TargetPosition { get; set; }
	public Vector2 TargetRotation {
		get => new Vector2(Head.RotationDegrees.x, RotationDegrees.y);
		set {
			Head.RotationDegrees = new Vector3(value.x, 0, 0);
			RotationDegrees = new Vector3(0, value.y, 0);
		}
	}

	private Transform m_PrevTransform, m_CurrentTransform;

	public override void _Ready() {
		base._Ready();

		m_ViewmodelHolder = Head.GetNode<Position3D>("ViewmodelHolder");
		WeaponManager = m_ViewmodelHolder.GetNode<RemotePlayerWeaponManager>("WeaponManager");

		m_CurrentTransform = GlobalTransform;
		m_PrevTransform = m_CurrentTransform;

		NetworkManager.OnTick += OnTick;
		if(NetworkManager.IsServer) {
			NetworkManager.OnTick += ServerOnTick;
		}
	}

	public override void _Process(float dt) {
		base._Process(dt);
		SmoothMovement();
	}

	public override void _ExitTree() {
		NetworkManager.OnTick -= OnTick;
		if(NetworkManager.IsServer) {
			NetworkManager.OnTick -= ServerOnTick;
		}
	}

	private void OnTick() {
		m_PrevTransform = m_CurrentTransform;

		Transform t = GlobalTransform;
		t.origin = TargetPosition;
		m_CurrentTransform = t;
	}

	private void ServerOnTick() {
		NetworkManager.Server.Sender.PlayerTransform(-1);
	}

	public void InitNetworkData(RemotePlayerData data) {
		NetworkData = data;
	}

	private void SmoothMovement() {
		float f = Mathf.Clamp(NetworkManager.TickTimer / NetworkManager.MIN_TIME_BETWEEN_TICKS, 0, 1);
		GlobalTransform = m_PrevTransform.InterpolateWith(m_CurrentTransform, f);

		Transform t = GlobalTransform;
		GlobalTransform = t;
	}
}
