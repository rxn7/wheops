using Godot;

public class CameraRecoil : Position3D {
	private Vector3 m_TargetRotation;

	public override void _Process(float dt) {
		if(Global.Player.WeaponManager?.HeldWeapon == null) {
			m_TargetRotation = Vector3.Zero;
			RotationDegrees = m_TargetRotation;
		} else {
			m_TargetRotation = m_TargetRotation.LinearInterpolate(Vector3.Zero, Global.Player.WeaponManager.HeldWeapon.Data.RecoilReturnSpeed*dt);
			RotationDegrees = RotationDegrees.LinearInterpolate(m_TargetRotation, Global.Player.WeaponManager.HeldWeapon.Data.RecoilSnapiness*dt);
		}
	}

	public void AddRecoil(float x, float y, float z) {
		m_TargetRotation += new Vector3(x,y,z);
	}
}
