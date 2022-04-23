using Godot;

[Tool]
public class Text3DViewport : Viewport {
	private Label m_Label;

	public override void _Ready() {
		m_Label = GetNode<Label>("Label");
	}

	public override void _Process(float dt) {
		Size = m_Label.RectSize;
	}
}
