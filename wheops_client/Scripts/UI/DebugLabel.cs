using System;
using Godot;
using System.Threading.Tasks;

public class DebugLabel : Label {
	public static DebugLabel Instance { get; private set; }

	public override void _EnterTree() {
		Instance = this;
	}

	public override void _Ready() {
		Hide();
	}

	private async void UpdateLabelAsyncLoop() {
		while(Visible) {
			await Task.Delay(200);
			Text = "== PLATFORM  == ";
			Text += $"\nplatform: {OS.GetName()}";
			Text += $"\napi: {OS.GetCurrentVideoDriver()}";

			Text += "\n\n== STATS ==";
			Text += $"\nframe: {Performance.GetMonitor(Performance.Monitor.TimeProcess)}";
			Text += $"\nfps: {Engine.GetFramesPerSecond()}";
			Text += $"\nphy fps: {Engine.IterationsPerSecond}";
			Text += $"\nnodes: {Performance.GetMonitor(Performance.Monitor.ObjectNodeCount)}";
			Text += $"\nthreads: {OS.GetProcessorCount()}";

			if(OS.IsDebugBuild()) {
				Text += $"\nstatic alloc: {OS.GetStaticMemoryUsage() / 0xf4240} mb";
				Text += $"\ndynamic alloc: {OS.GetDynamicMemoryUsage() / 0xf4240} mb";
				Text += $"\ntotal alloc: {(OS.GetDynamicMemoryUsage() + OS.GetStaticMemoryUsage()) / 0xf4240} mb";
			}

			Text += "\n\n== PLAYER  == ";
			Text += $"\nstate: {Enum.GetName(typeof(PlayerBase.EState), Global.Instance.CurrentMap.Player.State)}";
			Text += $"\nreal vel: {Global.Instance.CurrentMap.Player.RealVelocity.Length()}";
		}
	}

	public new void Hide() {
		Visible = false;
	}

	public new void Show() {
		Visible = true;
		UpdateLabelAsyncLoop();
	}
}
