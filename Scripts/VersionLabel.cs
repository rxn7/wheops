using Godot;

public class VersionLabel : Label {
	public override void _Ready() {
		this.Text = $"Version: {Global.VERSION}";
	}
}
