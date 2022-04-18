using Godot;

public static class MaterialCache {
	public static void LoadParticles(PackedScene s) {
		Particles p = (Particles)s.Instance();

		p.Emitting = true;
		p.OneShot = true;

		Global.Instance.AddChild(p);
	}
}
