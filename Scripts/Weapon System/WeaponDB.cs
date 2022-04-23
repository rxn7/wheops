using Godot;

public static class WeaponDB {
	public static WeaponData[] Weapons { get; private set; }

	public static void Init() {
		Weapons = new WeaponData[] {
			GD.Load<WeaponData>("res://Weapons/Data/s&w_model_39.tres"),
			GD.Load<WeaponData>("res://Weapons/Data/aac_honey_badger.tres"),
			GD.Load<WeaponData>("res://Weapons/Data/desert_eagle.tres"),
		};

		for(int i=0; i<Weapons.Length; ++i) {
			Weapons[i].ID = i;
		}

		Logger.Info("WeaponDB initialized");
	}

	public static Weapon Spawn(Node parent, int idx) {
		if(WeaponDB.Weapons.Length <= idx || idx < 0) return null;

		WeaponData data = Weapons[idx];

		Weapon weapon = (Weapon)data.Scene.Instance();
		weapon.Data = data;

		parent.AddChild(weapon);
		return weapon;
	}

	public static Weapon Spawn(Node parent, WeaponData data) {
		Weapon weapon = (Weapon)data.Scene.Instance();
		weapon.Data = data;

		parent.AddChild(weapon);
		return weapon;
	}
}
