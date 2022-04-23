using Godot;

public class DamagePopup : Sprite3D {
	public const float MOVE_SPEED = 0.5f;
	public const float OPACITY_SPEED = 2f;

	private Label m_Label;
	private Viewport m_Viewport;
	public int Damage {
		set => m_Label.Text = value.ToString();
	}

	public override void _EnterTree() {
		m_Viewport = GetNode<Viewport>("Viewport");
		m_Label = m_Viewport.GetNode<Label>("Label");
	}

	public override void _Process(float dt) {
		GlobalTranslate(Vector3.Up * MOVE_SPEED * dt);
		Opacity = Mathf.Lerp(Opacity, 0, OPACITY_SPEED*dt);

		if(Opacity < 0.01f) {
			QueueFree();
		}
	}
}
