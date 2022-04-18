using Godot;

public class PlayerBase : KinematicBody {
	public enum EState {
		Walking,
		Running,
		FastRunning,
		Crouching,
		Sliding,
	};

	public EState State { get; protected set; } = EState.Walking;

	protected ViewmodelConfiguration m_viewmodel_config;
}
