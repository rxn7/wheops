using Godot;
using System.Threading.Tasks;

public class DebugLabel : Label {
	public override void _Ready() {
		UpdateLabelAsyncLoop();
	}

	private async void UpdateLabelAsyncLoop() {
		while(true) {
			await Task.Delay(200);
			Text = $"frame: {1f / Engine.GetFramesPerSecond()}";
			Text += $"\nfps: {Engine.GetFramesPerSecond()}";
			Text += $"\nphy fps: {Engine.IterationsPerSecond}";
		}
	}
}
