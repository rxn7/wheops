using Godot;

public class Loadout {
	public const int PRIMARY_IDX = 0;
	public const int SECONDARY_IDX = 1;

	public Weapon[] Weapons { get; private set; }

	public Loadout(Node parent, params WeaponData[] weapon_datas) {
		Weapons = new Weapon[weapon_datas.Length];
		for(byte i=0; i<weapon_datas.Length; ++i) {
			Weapons[i]  = WeaponDB.Spawn(parent, weapon_datas[i]);
			Weapons[i].Visible = false;
			Weapons[i].LoadoutIdx = i;
		}
	}

	public Loadout(Node parent, params int[] weapon_ids) {
		Weapons = new Weapon[weapon_ids.Length];
		for(byte i=0; i<weapon_ids.Length; ++i) {
			Weapons[i] = WeaponDB.Spawn(parent, weapon_ids[i]);
			Weapons[i].Visible = false;
			Weapons[i].LoadoutIdx = i;
		}
	}
}
