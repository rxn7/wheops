using Godot;

public class SoundEffect : AudioStreamPlayer3D {
	public static void Spawn3D(Node parent, Vector3 position, AudioStream stream, float pitch = 1, float volume = 1) {
		AudioStreamPlayer3D player = new AudioStreamPlayer3D(); 
		parent.AddChild(player);

		Transform t = player.GlobalTransform;
		t.origin = position;
		player.GlobalTransform = t;

		player.PitchScale = pitch;
		player.Stream = stream;
		player.UnitDb = volume;
		player.Connect("finished", player, "queue_free");
		player.Play();
	}

	public static void Spawn(Node parent, AudioStream stream, float pitch = 1, float volume = 1) {
		AudioStreamPlayer player = new AudioStreamPlayer(); 
		parent.AddChild(player);

		player.PitchScale = pitch;
		player.Stream = stream;
		player.Connect("finished", player, "queue_free");
		player.VolumeDb = volume;
		player.Play();
	}
}
