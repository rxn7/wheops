using Godot;

public static class Random {
	private static RandomNumberGenerator rng = new RandomNumberGenerator();

	public static void Init() {
		rng.Randomize();
	}

	public static int RangeI(int min, int max) => rng.RandiRange(min, max);
	public static float RangeF(float min, float max) => rng.RandfRange(min, max);
} 
