using Godot;

public class Global : Node {
	public const string VERSION = "pre demo 23.04.2022";

	public static readonly PackedScene MUZZLEFLASH_SCENE = GD.Load<PackedScene>("res://Particles/MuzzleFlashParticles.tscn");
	public static readonly PackedScene BULLET_IMPACT_SCENE = GD.Load<PackedScene>("res://Particles/BulletImpact.tscn");
	public static readonly AudioStream HITMARK_SOUND = GD.Load<AudioStream>("res://Sounds/hitmark.wav");
	public static readonly PackedScene DAMAGE_POPUP = GD.Load<PackedScene>("res://Scenes/DamagePopup.tscn");

	public static Global Instance { get; private set; }
	public static Map CurrentMap { get; private set; }
	public static Player Player { get; set; }

	public override void _EnterTree() {
		Instance = this;
	}

	public override void _Ready() {
		MaterialCache.LoadParticles(MUZZLEFLASH_SCENE);
		MaterialCache.LoadParticles(BULLET_IMPACT_SCENE);

		Random.Init();
		Config.Init();
		WeaponDB.Init();
		CommandManager.Init();
	}

	public static void LoadMap(string name) {
		string path = $"res://Maps/{name}.tscn";
		if(!ResourceLoader.Exists(path)) {
			Logger.Error($"Map '{name}' doesn't exist!");
			return;
		}

		if(CurrentMap != null) {
			CurrentMap.QueueFree();
		}

		Logger.Info($"Loading map '{name}'...");

		PackedScene scene = GD.Load<PackedScene>($"res://Maps/{name}.tscn");
		CurrentMap = (Map)scene.Instance();
		Instance.AddChild(CurrentMap);
	}

	public static void SpawnMuzzleFlash(Spatial parent, Vector3 position) {
		OneShotParticles p = (OneShotParticles)MUZZLEFLASH_SCENE.Instance();
		parent.AddChild(p);

		Transform t = p.GlobalTransform;
		t.origin = position;
		p.GlobalTransform = t;

		p.RotationDegrees = parent.GlobalTransform.basis.GetEuler();
	}

	public static void SpawnBulletImpact(Vector3 position, Vector3 normal) {
		OneShotParticles p = (OneShotParticles)BULLET_IMPACT_SCENE.Instance();
		Instance.AddChild(p);

		Transform t = p.GlobalTransform;
		t.origin = position + normal * 0.001f;
		p.GlobalTransform = t;

		p.ProcessMaterial.Set("direction", normal);
	}

	public static void SpawnDamagePopup(Vector3 position, int damage) {
		DamagePopup popup = (DamagePopup)DAMAGE_POPUP.Instance();
		Instance.AddChild(popup);

		popup.GlobalTranslate(position);
		popup.Damage = damage;
	}
}
