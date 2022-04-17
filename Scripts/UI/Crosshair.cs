using Godot;

public class Crosshair : Control {
	public ColorRect top, bot, left, right;

	public override void _Ready() {
		top = GetNode<ColorRect>("Top");
		bot = GetNode<ColorRect>("Bot");
		left = GetNode<ColorRect>("Left");
		right = GetNode<ColorRect>("Right");
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
