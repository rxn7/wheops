using Godot;

public enum EHumanState {
	Walking,
	Running,
	Crouching,
	Sliding, FastRunning,
};

public struct HeightProperties {
	public HeightProperties(float val, float speed) {
		Value = val;
		ChangeSpeed = speed;
	}

	public float Value { get; private set; }
	public float ChangeSpeed { get; private set; }
};

public class HumanBase : KinematicBody { public static readonly AudioStream FOOTSTEP_SOUND = GD.Load<AudioStream>("res://Sounds/footstep.wav");

	public EHumanState State { get; protected set; } = EHumanState.Walking;
	public Position3D Head { get; private set; }
	public bool IsAiming { get; protected set; }
	public WeaponManagerBase WeaponManager { get; protected set; }

	protected AudioStreamPlayer3D m_SlideAudioPlayer;
	protected ViewmodelProperties m_ViewmodelProperties;
	protected CapsuleShape m_Capsule;
	protected CollisionShape m_CollisionShape;
	protected Position3D m_ViewmodelHolder;

	public static readonly HeightProperties CROUCH_HEIGHT_PROPERTIES = new HeightProperties(1f, 10f);
	public static readonly HeightProperties SLIDE_HEIGHT_PROPERTIES = new HeightProperties(0.4f, 15f);
	public static readonly HeightProperties WALK_HEIGHT_PROPERTIES = new HeightProperties(1.8f, 13f);

	public override void _Ready() {
		Head = GetNode<Position3D>("Head");
		m_SlideAudioPlayer = GetNode<AudioStreamPlayer3D>("SlidePlayer");
		m_CollisionShape = GetNode<CollisionShape>("CollisionShape");
		m_Capsule = (CapsuleShape)m_CollisionShape.Shape;
	}

	public override void _Process(float dt) {
		HandleSlideAudioPlayer();
		ApplyViewmodelConfig(dt);
	}

	protected void CalculateHeight(float dt) {
		HeightProperties properties;

		switch(State) {
			case EHumanState.Crouching: properties =  CROUCH_HEIGHT_PROPERTIES; break;
			case EHumanState.Sliding: properties = SLIDE_HEIGHT_PROPERTIES; break;
			default: properties = WALK_HEIGHT_PROPERTIES; break;
		};
		
		m_Capsule.Height = Mathf.Lerp(m_Capsule.Height, properties.Value, properties.ChangeSpeed*dt);

		Transform t = m_CollisionShape.Transform;
		t.origin.y = (WALK_HEIGHT_PROPERTIES.Value - m_Capsule.Height)/2;
		m_CollisionShape.Transform = t;
	}

	protected void ApplyViewmodelConfig(float dt) {
		if(WeaponManager.IsReloading) {
			m_ViewmodelProperties = ViewmodelProperties.ReloadConfiguration;
		} else if(IsAiming) {
			m_ViewmodelProperties = ViewmodelProperties.AimConfiguration;
			m_ViewmodelProperties.ChangeSpeed = WeaponManager.HeldWeapon.Data.AimSpeed;
		} else if(WeaponManager.WantsToShoot || WeaponManager.HasJustShot) {
			m_ViewmodelProperties = ViewmodelProperties.ShootConfiguration;
		} else if(WeaponManager.HeldWeapon != null && WeaponManager.DrawTimer > WeaponDB.Weapons[WeaponManager.QueuedWeaponID].DrawTime * 0.5f) {
			m_ViewmodelProperties = ViewmodelProperties.DrawConfiguration;
		} else {
			switch(State) {
				case EHumanState.Running:
					m_ViewmodelProperties = ViewmodelProperties.RunConfiguration;
					break;

				case EHumanState.Crouching:
					m_ViewmodelProperties = ViewmodelProperties.CrouchConfiguration;
					break;

				case EHumanState.FastRunning:
					m_ViewmodelProperties = ViewmodelProperties.FastRunConfiguration;
					break;

				case EHumanState.Sliding:
					m_ViewmodelProperties = ViewmodelProperties.SlideConfiguration;
					break;

				default:
					m_ViewmodelProperties = ViewmodelProperties.NormalConfiguration;
					break;
			}
		}

		Transform t = m_ViewmodelHolder.Transform;

		if(m_ViewmodelProperties == ViewmodelProperties.DrawConfiguration) {
			float f = 10 / WeaponManager.HeldWeapon.Data.DrawTime * dt;
			t.origin = t.origin.LinearInterpolate(m_ViewmodelProperties.Position, f);
			m_ViewmodelHolder.RotationDegrees = m_ViewmodelHolder.RotationDegrees.LinearInterpolate(m_ViewmodelProperties.Rotation, f);
		} else {
			if(WeaponManager.DrawTimer > 0) {
				float f = 10 / WeaponManager.HeldWeapon.Data.DrawTime * dt;
				t.origin = t.origin.LinearInterpolate(m_ViewmodelProperties.Position, f);
				m_ViewmodelHolder.RotationDegrees = m_ViewmodelHolder.RotationDegrees.LinearInterpolate(m_ViewmodelProperties.Rotation, f);
			} else {
				t.origin = t.origin.LinearInterpolate(m_ViewmodelProperties.Position, m_ViewmodelProperties.ChangeSpeed*dt);
				m_ViewmodelHolder.RotationDegrees = m_ViewmodelHolder.RotationDegrees.LinearInterpolate(m_ViewmodelProperties.Rotation, m_ViewmodelProperties.ChangeSpeed*dt);
			}
		}

		m_ViewmodelHolder.Transform = t;
	}

	private void HandleSlideAudioPlayer() {
		if(State == EHumanState.Sliding) {
			if(!m_SlideAudioPlayer.Playing) {
				m_SlideAudioPlayer.Play();
			}
		} else {
			m_SlideAudioPlayer.Stop();
		}
	}
}
