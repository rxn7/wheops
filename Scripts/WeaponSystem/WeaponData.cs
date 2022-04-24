using Godot;

public enum EFireType { Manual, Automatic }
public enum EWeaponType { Primary, Secondary }

public class WeaponData : Resource {
	public int ID { get; set; }

	[Export] public PackedScene Scene { get; private set; }
	[Export] public string Name { get; private set; }
	[Export] public float FireRate { get; private set; } = 0.3f;
	[Export] public uint AmmoCap { get; private set; } = 30;
	[Export] public float ReloadTime { get; private set; } = 2.0f;
	[Export] public float DrawTime { get; private set; } = 0.3f;
	[Export] public EFireType FireType { get; private set; } = EFireType.Automatic;
	[Export] public AudioStream ShootSound { get; private set; }
	[Export] public float MoveSpeedMultiplier { get; private set; } = 1.0f;

	[Export] public float RaycastDistance { get; private set; } = 100f;
	[Export] public float HitForce { get; private set; } = 2f;

	[Export] public float AimFov { get; private set; } = 50;
	[Export] public float AimSpeed { get; private set; } = 10;
	[Export] public float AimMoveSpeedMultiplier { get; private set; } = 0.8f;

	[Export] public float SwaySpeed { get; private set; } = 10;
	[Export] public float SwayAmount { get; private set; } = 0.06f;

	[Export] public float RecoilSnapiness { get; private set; } = 4;
	[Export] public float RecoilReturnSpeed { get; private set; } = 4;
	[Export] public float RecoilKickback { get; private set; } = 0.1f;
	[Export] public float AimRecoilKickback { get; private set; } = 0.08f;
	[Export] public float RecoilKickup { get; private set; } = 0.02f;
	[Export] public float AimRecoilKickup { get; private set; } = 0.003f;

	[Export] public Vector3 CameraRecoil { get; private set; } = new Vector3(5, 2, 0.5f);
	[Export] public Vector3 AimCameraRecoil { get; private set; } = new Vector3(4, 1, 0.5f);
	[Export] public Vector3 RecoilRotation { get; private set; } = new Vector3(5, 0, 1);
	[Export] public Vector3 AimRecoilRotation { get; private set; } = new Vector3(3, 0, 1);
}
