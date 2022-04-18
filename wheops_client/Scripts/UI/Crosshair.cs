using Godot;

public class Crosshair : Control {
	public const float DEFAULT_GAP = 24;
	public static readonly Color DEFAULT_COLOR = Color.Color8(20,240,20);

	private ColorRect m_top, m_bot, m_left, m_right;

	public override void _Ready() {
		m_top = GetNode<ColorRect>("Top");
		m_bot = GetNode<ColorRect>("Bot");
		m_left = GetNode<ColorRect>("Left");
		m_right = GetNode<ColorRect>("Right");

		SetGap(Config.GetValue<float>("crosshair", "gap", DEFAULT_GAP));
		SetColor(Config.GetValue<Color>("crosshair", "color", DEFAULT_COLOR));
	}

	private void Center() {
		Rect2 viewport_rect = GetViewportRect();
		RectGlobalPosition = new Vector2(viewport_rect.Size.x*0.5f - RectSize.x*0.5f, viewport_rect.Size.y*0.5f - RectSize.y*0.5f);
	}

	public void SetGap(float gap) {
		RectPivotOffset = new Vector2(gap*0.5f, gap*0.5f);
		RectSize = new Vector2(gap, gap);
		Center();
	}

	public void SetColor(Color color) {
		Modulate = color;
	}
}
