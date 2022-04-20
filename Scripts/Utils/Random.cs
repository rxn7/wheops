using Godot;

public static class Random {
	public static RandomNumberGenerator Rng { get; private set; }
	public static OpenSimplexNoise SimplexNoise { get; private set; }

	public static void Init() {
		Rng = new RandomNumberGenerator();
		Rng.Randomize();

		SimplexNoise = new OpenSimplexNoise();
		SimplexNoise.Seed = (int)OS.GetSystemTimeSecs();

		Logger.Info("Random initialized");
	}

	public static int RangeI(int min, int max) =>Rng.RandiRange(min, max);
	public static float RangeF(float min, float max) => Rng.RandfRange(min, max);
	public static int Sign() => RangeI(0,1) * 2 - 1;

	public static float Simplex1D(float x, int octaves = 4, float period = 4, float lacunarity = 1.5f, float persistence = 0.75f) {
		SimplexNoise.Octaves = octaves;
		SimplexNoise.Period = period;
		SimplexNoise.Lacunarity = lacunarity;
		SimplexNoise.Persistence = persistence;

		return SimplexNoise.GetNoise1d(x);
	}
} 
