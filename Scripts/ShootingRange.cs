using Godot;
using System;
using System.Linq;

public class ShootingRange : Node {
	private Target[] m_Targets;

	public override void _Ready() {
		m_Targets = GetNode("Targets").GetChildren().Cast<Target>().ToArray();

		foreach(Target t in m_Targets) {
			t.OnHit += new EventHandler(OnTargetHit);
			t.AutoRestore = false;
		}
	}

	private void OnTargetHit(object sender, EventArgs args) {
		Target target = (Target)sender;
	}
}
