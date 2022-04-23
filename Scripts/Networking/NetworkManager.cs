using Godot;
using System;

public class NetworkManager : Node {
	private static NetworkManager instance;

	public override void _EnterTree() {
		instance = this;
	}
}
