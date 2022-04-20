using Godot;

public enum EFireType { Manual, Automatic }

public class WeaponData : Resource {
	[Export] public PackedScene Scene;
	[Export] public string Name;
	[Export] public float FireRate = 0.3f;
	[Export] public uint AmmoCap = 30;
	[Export] public float ReloadTime = 2.0f;
	[Export] public float DrawTime = 0.3f;
	[Export] public EFireType FireType = EFireType.Automatic;
	[Export] public AudioStream ShootSound;
	[Export] public float MoveSpeedMultiplier = 1.0f;

	[Export] public float RaycastDistance = 100f;
	[Export] public float HitForce = 2f;

	[Export] public float AimFov = 50;
	[Export] public float AimSpeed = 10;
	[Export] public float AimMoveSpeedMultiplier = 0.8f;

	[Export] public float SwaySpeed = 10;
	[Export] public float SwayAmount = 0.06f;

	[Export] public float RecoilSnapiness = 4;
	[Export] public float RecoilReturnSpeed = 4;
	[Export] public float RecoilKickback = 0.1f;
	[Export] public float AimRecoilKickback = 0.08f;
	[Export] public float RecoilKickup = 0.02f;
	[Export] public float AimRecoilKickup = 0.003f;

	[Export] public Vector3 CameraRecoil = new Vector3(5, 2, 0.5f);
	[Export] public Vector3 AimCameraRecoil = new Vector3(4, 1, 0.5f);
	[Export] public Vector3 RecoilRotation = new Vector3(5, 0, 1);
	[Export] public Vector3 AimRecoilRotation = new Vector3(3, 0, 1);
}
