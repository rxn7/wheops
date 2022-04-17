using Godot;

public class OneShotParticles : Particles {
	public override void _Ready() {
		Emitting = true;
	}

	public override void _PhysicsProcess(float dt) {
		if(!Emitting) {
			QueueFree();
		}
	}
}
