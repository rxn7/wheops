using Godot;

public class Weapon : Spatial {
	[Export] public int DataID { get; private set; }
	public Position3D MuzzlePoint { get; private set; }
	public uint AmmoLeft { get; private set; }
	public WeaponData Data { get; private set; }

	private Vector3 m_RecoilPosition;
	private Vector3 m_RecoilRotation;
	private Vector3 m_StartPosition;

	public override void _Ready() {
		Data = WeaponDB.Weapons[DataID];
		MuzzlePoint = GetNode<Position3D>("MuzzlePoint");
		m_StartPosition = Transform.origin;
		AmmoLeft = Data.AmmoCap;
	}

	public override void _Process(float dt) {
		CalculateRecoil(dt);
		ApplyRecoil(dt);
	}

	private void CalculateRecoil(float dt) {
		m_RecoilPosition = m_RecoilPosition.LinearInterpolate(Vector3.Zero, Data.RecoilReturnSpeed*dt);
		m_RecoilRotation = m_RecoilRotation.LinearInterpolate(Vector3.Zero, Data.RecoilReturnSpeed*dt);
	}

	private void ApplyRecoil(float dt) {
		Transform t = Transform;
		t.origin = t.origin.LinearInterpolate(m_RecoilPosition + m_StartPosition, 20*dt);
		Transform = t;
		RotationDegrees = RotationDegrees.LinearInterpolate(m_RecoilRotation, 20*dt);
	}

	public void Shoot() {
		--AmmoLeft;
		SoundEffect.Spawn(Global.Instance, Data.ShootSound, Random.RangeF(0.95f, 1.05f));

		float mul_y = Random.RangeF(0.8f, 1.2f) * Random.Sign();
		float mul = 1;

		if(Global.Player.IsFullyAiming) {
			m_RecoilPosition.z = Data.AimRecoilKickback;
			m_RecoilPosition.y = Data.AimRecoilKickup;
			m_RecoilRotation.x = Data.AimRecoilRotation.x;
			m_RecoilRotation.y = Data.AimRecoilRotation.z;
			m_RecoilRotation.z = Data.AimRecoilRotation.z;
			Global.Player.CameraRecoil.AddRecoil(Data.AimCameraRecoil.x * mul, Data.AimCameraRecoil.y * mul_y * mul, Data.AimCameraRecoil.z * mul * mul_y);
		} else {
			m_RecoilPosition.z = Data.RecoilKickback;
			m_RecoilPosition.y = Data.RecoilKickup;
			m_RecoilRotation.x = Data.RecoilRotation.x;
			m_RecoilRotation.y = Data.RecoilRotation.z;
			m_RecoilRotation.z = Data.RecoilRotation.z;
			Global.Player.CameraRecoil.AddRecoil(Data.CameraRecoil.x * mul, Data.CameraRecoil.y * mul_y * mul, Data.CameraRecoil.z * mul * mul_y);
		}

		m_RecoilPosition.z *= Random.RangeF(0.8f, 1.2f) * mul;
		m_RecoilPosition.y *= Random.RangeF(0.8f, 1.2f) * mul;
		m_RecoilRotation.x *= mul;
		m_RecoilRotation.y *= mul_y * mul;
		m_RecoilRotation.z *= -mul_y * mul;
	}

	public void Reload() {
		AmmoLeft = Data.AmmoCap;
	}
}
