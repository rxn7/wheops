using Godot;

public class HumanBase : KinematicBody {
	public struct HeightProperties {
		public HeightProperties(float val, float speed) {
			Value = val;
			Speed = speed;
		}

		public float Value { get; private set; }
		public float Speed { get; private set; }
	};

	public enum EHumanState {
		Walking,
		Running,
		Crouching,
		Sliding,
		FastRunning,
	};

	public EHumanState State { get; protected set; } = EHumanState.Walking;

	protected ViewmodelProperties m_ViewmodelProperties;
}
