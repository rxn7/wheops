using Godot;

public class Crosshair : Control {
	public const float DEFAULT_GAP = 24;
	public const float DEFAULT_LENGTH = 10;
	public const float DEFAULT_WIDTH = 2;
	public static readonly Color DEFAULT_COLOR = Color.Color8(20,240,20);

	private ColorRect m_Top, m_Bot, m_Left, m_Right;
	private float m_Width, m_Length, m_Gap;

	public override void _Ready() {
		m_Top = GetNode<ColorRect>("Top");
		m_Bot = GetNode<ColorRect>("Bot");
		m_Left = GetNode<ColorRect>("Left");
		m_Right = GetNode<ColorRect>("Right");

		SetGap(Config.GetValue<float>("crosshair", "gap", DEFAULT_GAP));
		SetColor(Config.GetValue<Color>("crosshair", "color", DEFAULT_COLOR));

		m_Length = Config.GetValue<float>("crosshair", "length", DEFAULT_LENGTH);
		m_Width = Config.GetValue<float>("crosshair", "width", DEFAULT_WIDTH);
		UpdateLineRectSizes();
	}

	public override void _Process(float dt) {
		if(Global.Player != null) {
			Visible = !Global.Player.IsAiming && !Console.Instance.Visible;
		}
	}

	public void SetGap(float gap) {
		m_Gap = gap;
		UpdateLineRectSizes();
	}

	public void SetColor(Color color) {
		Modulate = color;
	}

	private void UpdateLineRectSizes() {
		m_Left.RectSize = new Vector2(m_Length, m_Width);
		m_Left.RectPosition = new Vector2(-m_Gap - m_Length, -m_Width*0.5f);

		m_Right.RectSize = new Vector2(m_Length, m_Width);
		m_Right.RectPosition = new Vector2(m_Gap, -m_Width*0.5f);

		m_Top.RectSize = new Vector2(m_Width, m_Length);
		m_Top.RectPosition = new Vector2(-m_Width*0.5f, -m_Gap - m_Length);

		m_Bot.RectSize = new Vector2(m_Width, m_Length);
		m_Bot.RectPosition = new Vector2(-m_Width*0.5f, m_Gap);
	}

	public void SetLength(float val, bool update_rect_sizes=true) {
		m_Length = val;
		if(update_rect_sizes) UpdateLineRectSizes();
	}

	public void SetWidth(float val, bool update_rect_sizes=true) {
		m_Width = val;
		if(update_rect_sizes) UpdateLineRectSizes();
	}
}
