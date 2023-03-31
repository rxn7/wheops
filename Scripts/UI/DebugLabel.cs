using System.Text;
using System;
using Godot;
using System.Threading.Tasks;

public class DebugLabel : Label {
	public static DebugLabel Instance { get; private set; }
	private StringBuilder m_StrBuilder;

	public override void _EnterTree() {
		Instance = this;
	}

	public override void _Ready() {
		m_StrBuilder = new StringBuilder(500);
		Hide();
	}

	private async void UpdateLabelAsyncLoop() {
		while (Visible) {
			await Task.Delay(200);
			m_StrBuilder.Clear();

			m_StrBuilder.AppendLine("== PLATFORM  == ");
			m_StrBuilder.AppendLine($"platform: {OS.GetName()}");
			m_StrBuilder.AppendLine($"api: {OS.GetCurrentVideoDriver()}");

			if (NetworkManager.IsOnline) {
				m_StrBuilder.AppendLine("\n\n== NETWORK ==");
				m_StrBuilder.AppendLine($"ping: {NetworkManager.Ping}");
				m_StrBuilder.AppendLine($"state: {Enum.GetName(typeof(ENetworkState), NetworkManager.State)}");
			}

			m_StrBuilder.AppendLine("\n== STATS ==");
			m_StrBuilder.AppendLine($"frame: {Performance.GetMonitor(Performance.Monitor.TimeProcess)}");
			m_StrBuilder.AppendLine($"fps: {Engine.GetFramesPerSecond()}");
			m_StrBuilder.AppendLine($"phy fps: {Engine.IterationsPerSecond}");
			m_StrBuilder.AppendLine($"nodes: {Performance.GetMonitor(Performance.Monitor.ObjectNodeCount)}");
			m_StrBuilder.AppendLine($"threads: {OS.GetProcessorCount()}");

			if (OS.IsDebugBuild()) {
				m_StrBuilder.AppendLine($"static alloc: {OS.GetStaticMemoryUsage() / 0xf4240} mb");
				m_StrBuilder.AppendLine($"dynamic alloc: {OS.GetDynamicMemoryUsage() / 0xf4240} mb");
				m_StrBuilder.AppendLine($"total alloc: {(OS.GetDynamicMemoryUsage() + OS.GetStaticMemoryUsage()) / 0xf4240} mb");
			}

			m_StrBuilder.AppendLine("\n== PLAYER  == ");
			m_StrBuilder.AppendLine($"state: {Enum.GetName(typeof(EHumanState), Global.Player.State)}");
			m_StrBuilder.AppendLine($"vel: {Global.Player.RealVelocity.Length().ToString("0.00")}");
			m_StrBuilder.AppendLine($"aiming: {Global.Player.IsAiming}");
			m_StrBuilder.AppendLine($"fully aiming: {Global.Player.IsFullyAiming}");
			m_StrBuilder.AppendLine($"heat timer: {((LocalPlayerWeaponManager)Global.Player.WeaponManager).HeatTimer}");
			m_StrBuilder.AppendLine($"run timer: {Global.Player.RunTimer}");

			Text = m_StrBuilder.ToString();
		}
	}

	public static new void Hide() {
		if (Instance != null) {
			Instance.Visible = false;
		}
	}

	public static new void Show() {
		if (Instance != null) {
			Instance.Visible = true;
			Instance.UpdateLabelAsyncLoop();
		}
	}
}
