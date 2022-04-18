using Godot;

public static class WeaponDB {
	public static WeaponData[] Weapons { get; private set; }

	public static void Init() {
		Weapons = new WeaponData[] {
			GD.Load<WeaponData>("res://Shared/Weapons/s&w_model_39.tres"),
			GD.Load<WeaponData>("res://Shared/Weapons/aac_honey_badger.tres"),
		};

		Logger.Info("WeaponDB initialized");
	}
}
