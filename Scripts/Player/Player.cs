using Godot;

public class Player : HumanBase {
	public static readonly PackedScene SCENE = GD.Load<PackedScene>("res://Scenes/Player.tscn");
	public static readonly AudioStream FOOTSTEP_SOUND = GD.Load<AudioStream>("res://Sounds/footstep.wav");

	public const float DEFAULT_MOUSE_SENS = 0.1f;
	public const float HEAD_LERP_WEIGHT = 60; 
	public const float RUN_SPEED = 10;
	public const float FASTRUN_SPEED = 12;
	public const float FASTRUN_RUN_TIME_THRESHOLD = 1.5f;
	public const float FASTRUN_VELOCITY_THRESHOLD = 7f;
	public const float WALK_SPEED = 6;
	public const float CROUCH_SPEED = 3;
	public const float GROUND_ACCEL = 15;
	public const float AIR_ACCEL = 3;
	public const float GRAVITY = 30;
	public const float JUMP = 10f;
	public const float BOB_FREQUENCY = 14f;
	public const float BOB_RESET_LERP_WEIGHT = 4;
	public const float BOB_LERP_WEIGHT = 10;
	public const float HEADBOB_AMPLITUDE = 0.1f;
	public const float VIEWMODEL_BOB_AMPLITUDE = 0.004f;
	public const float BOB_VELOCITY_THRESHOLD = 2;
	public const float NORMAL_HEIGHT = 1.8f;
	public const float CROUCH_HEIGHT = 1f;
	public const float SLIDE_HEIGHT = 0.4f;
	public const float STANDUP_SPEED = 13f;
	public const float CROUCHDOWN_SPEED = 10f;
	public const float CROUCH_RECOIL_MULTIPLIER = 0.8f;
	public const float DEFAULT_FOV = 80;
	public const float SLIDE_SPEED_DECREASE = 1.5f;
	public const float SLIDE_ACCEL = 1f;
	public const float SLIDEDOWN_SPEED = 15f;
	public const float FULLY_AIMING_DISTANCE_THRESHOLD = 0.03f;
	public const float BREATHING_AMPLITUDE = 0.01f;
	public const float BREATHING_FREQUENCY = 2f;

	public RayCast ShootRaycast { get; private set; }
	public HUD Hud { get; private set; }
	public Position3D Head { get; private set; }
	public SmoothCamera CameraHolder { get; private set; }
	public CameraRecoil CameraRecoil { get; private set; }
	public Camera Camera { get; private set; }
	public WeaponManager WeaponManager { get; private set; }
	public Vector3 RealVelocity { get; private set; }
	public Vector3 BreathingAnimation { get; private set; }
	public bool IsAiming { get; private set; }
	public bool IsFullyAiming { get; private set; }
	public float RunTimer { get; private set; }
	public float CameraTilt { get; private set; } 
	public float MouseSensetivity { get; set; }

	private Position3D m_ViewmodelOffset, m_ViewmodelHolder;
	private CapsuleShape m_Capsule;
	private CollisionShape m_CollisionShape;
	private AudioStreamPlayer m_SlideAudioPlayer;
	private RayCast m_HeadBonker;
	private ViewmodelSway m_WeaponSway;
	private Vector3 m_BreathingAnimationTarget;

	private float m_Gravity = 0;
	private float m_TargetCameraTilt = 0; 
	private float m_BobTimer = 0;
	private float m_BreatheTimer = 0;
	private float m_Speed, m_TargetSpeed;
	private float m_Accel;
	private bool m_WasBobYPositive = false;
	private Vector3 m_Velocity;
	private Vector3 m_Input;
	private Vector3 m_Snap;
	private Vector3 m_Direction;
	private Vector3 m_Bob;

	public override void _Ready() {
		Hud = GetNode<HUD>("HUD");
		m_SlideAudioPlayer = GetNode<AudioStreamPlayer>("SlidePlayer");
		Head = GetNode<Position3D>("Head");
		m_CollisionShape = GetNode<CollisionShape>("CollisionShape");
		m_Capsule = (CapsuleShape)m_CollisionShape.Shape;
		m_HeadBonker = Head.GetNode<RayCast>("Bonker");
		CameraHolder = Head.GetNode<SmoothCamera>("CameraHolder");
		CameraRecoil = CameraHolder.GetNode<CameraRecoil>("CameraRecoil");
		CameraHolder.SetAsToplevel(true);
		Camera = CameraRecoil.GetNode<Camera>("Camera");
		m_WeaponSway = Camera.GetNode<ViewmodelSway>("ViewmodelSway");
		m_ViewmodelOffset = m_WeaponSway.GetNode<Position3D>("ViewmodelOffset");
		m_ViewmodelHolder = m_ViewmodelOffset.GetNode<Position3D>("ViewmodelHolder");
		WeaponManager = m_ViewmodelHolder.GetNode<WeaponManager>("WeaponManager");

		MouseSensetivity = Config.GetValue<float>("mouse", "sens", DEFAULT_MOUSE_SENS);
		Input.SetMouseMode(Input.MouseMode.Captured);

		WeaponManager.QueueWeaponChange(0);
	}

	public override void _Process(float dt) {
		TakeInput(dt);
		m_Direction = m_Direction.LinearInterpolate(m_Input.Normalized(), m_Accel*dt);

		CalculateSpeed(dt);
		HandleRunTimer(dt);
		CalculateAcceleration();
		CalculateHeight(dt);
		HandleBobTimer(dt);
		CalculateBob();
		ApplyHeadBob(dt);
		ApplyViewmodelBob(dt);
		ApplyViewmodelConfig(dt);

		IsFullyAiming = IsAiming && m_ViewmodelHolder.Transform.origin.DistanceTo(ViewmodelConfiguration.AimConfiguration.Position) <= FULLY_AIMING_DISTANCE_THRESHOLD;

		HandleFOV(dt);
		HandleSlidePlayer();
		HandleBreathingAnimation(dt);

		CameraTilt = Mathf.Lerp(CameraTilt, m_TargetCameraTilt, BOB_LERP_WEIGHT*dt);
		Hud.Crosshair.Visible = !IsAiming && !Console.Instance.Visible;
	}

	public override void _PhysicsProcess(float dt) {
		CalculatePhysicsMovement(dt);
	}

	public override void _Input(InputEvent e) {
		if(Console.Instance.Visible) return;

		if(e is InputEventMouseMotion mouse_motion_e) {
			Head.RotateX(Mathf.Deg2Rad(-mouse_motion_e.Relative.y * MouseSensetivity)); 
			Vector3 head_r = Head.RotationDegrees;
			head_r.x = Mathf.Clamp(head_r.x, -89, 89);
			Head.RotationDegrees = head_r;

			RotateY(Mathf.Deg2Rad(-mouse_motion_e.Relative.x * MouseSensetivity));
			Vector3 r = RotationDegrees;
			while(r.y >= 180) r.y -= 180;
			while(r.y <= -180) r.y += 180;
			RotationDegrees = r;
		}
	}

	private void TakeInput(float dt) {
		m_Input = Vector3.Zero;
		m_TargetCameraTilt = 0;

		EState prev_state = State;
		if(!Console.Instance.Visible) {
			Vector3 forward = Camera.GlobalTransform.basis.z;
			Vector3 right = Camera.GlobalTransform.basis.x;

			if(Input.IsActionPressed("move_right")) m_Input += right;
			if(Input.IsActionPressed("move_left")) m_Input -= right;
			if(Input.IsActionPressed("move_forward")) m_Input -= forward;
			if(Input.IsActionPressed("move_backward")) m_Input += forward;

			m_Input.y = 0;
			IsAiming = Input.IsActionPressed("aim") && WeaponManager.DrawTimer == 0 && !WeaponManager.IsReloading;

			if(!IsAiming && !WeaponManager.IsReloading && Input.IsActionPressed("run") && Input.IsActionPressed("move_forward") && IsOnFloor() && !WeaponManager.WillShoot && !WeaponManager.HasJustShot && WeaponManager.DrawTimer == 0) { 
				if(RunTimer > FASTRUN_RUN_TIME_THRESHOLD){ 
					State = EState.FastRunning;
					m_TargetSpeed = FASTRUN_SPEED;
				} else {
					State = EState.Running;
					m_TargetSpeed = RUN_SPEED;
				}
			} else {
				State = EState.Walking;
				m_TargetSpeed = WALK_SPEED;
			}

			if(Input.IsActionPressed("crouch") && IsOnFloor()) {
				if(RealVelocity.Length() > CROUCH_SPEED*1.5f && Input.IsActionPressed("run") && RunTimer > FASTRUN_RUN_TIME_THRESHOLD) {
					State = EState.Sliding;
					m_TargetSpeed = CROUCH_SPEED;
				} else {
					State = EState.Crouching;
					m_TargetSpeed = CROUCH_SPEED;
				}
			}
		} else {
			State = EState.Walking;
			m_TargetSpeed = WALK_SPEED;
		}

		if(prev_state == EState.Sliding && State != EState.Sliding) {
			RunTimer = 0;
		}

		if(State == EState.FastRunning && RealVelocity.Length() <= FASTRUN_VELOCITY_THRESHOLD) {
			State = EState.Running;
			RunTimer = 0;
		}

		m_TargetSpeed *= WeaponManager.HeldWeapon != null ? WeaponManager.HeldWeapon.Data.MoveSpeedMultiplier : 1;
	}

	private void CalculateBob() {
		m_Bob = Vector3.Zero;

		if(State == EState.Sliding) return;

		if(m_BobTimer > 0) {
			float multiplier = m_TargetSpeed / 7;
			float amplitude_mul = 1;
			if(IsFullyAiming) amplitude_mul = 0.2f;

			if(RealVelocity.Length() > BOB_VELOCITY_THRESHOLD) {
				m_Bob.y = Mathf.Sin(m_BobTimer * BOB_FREQUENCY * multiplier) * multiplier * amplitude_mul;
				m_TargetCameraTilt = m_Bob.y;
				m_Bob.x = Mathf.Cos(m_BobTimer * BOB_FREQUENCY * 0.5f * multiplier) * 2 * multiplier * amplitude_mul;
			}

			if(m_Bob.y < 0 && m_WasBobYPositive) {
				PlayFootstep();
			} 

			m_WasBobYPositive = m_Bob.y >= 0;
		}
	}

	private void ApplyHeadBob(float dt) {
		Transform t = Camera.Transform;

		if(m_BobTimer > 0)	t.origin = t.origin.LinearInterpolate(m_Bob * HEADBOB_AMPLITUDE, BOB_LERP_WEIGHT*dt);
		else			t.origin = t.origin.LinearInterpolate(Vector3.Zero, BOB_RESET_LERP_WEIGHT*dt);

		Camera.Transform = t;
	}

	private void ApplyViewmodelBob(float dt) {
		Transform t = m_ViewmodelOffset.Transform;

		if(m_BobTimer > 0)	t.origin = t.origin.LinearInterpolate(m_Bob * VIEWMODEL_BOB_AMPLITUDE, BOB_LERP_WEIGHT*dt);

		m_ViewmodelOffset.Transform = t;
	}

	private void PlayFootstep() {
		SoundEffect.Spawn(this, FOOTSTEP_SOUND, Random.RangeF(0.7f, 1.3f), RealVelocity.Length() - 20);
	}

	private void ApplyViewmodelConfig(float dt) {
		if(WeaponManager.IsReloading) {
			m_ViewmodelConfig = ViewmodelConfiguration.ReloadConfiguration;
		} else if(IsAiming) {
			m_ViewmodelConfig = ViewmodelConfiguration.AimConfiguration;
			m_ViewmodelConfig.ChangeSpeed = WeaponManager.HeldWeapon.Data.AimSpeed;
		}else if(WeaponManager.WantsToShoot || WeaponManager.HasJustShot) {
			m_ViewmodelConfig = ViewmodelConfiguration.ShootConfiguration;
		} else if(WeaponManager.HeldWeapon != null && WeaponManager.DrawTimer > WeaponDB.Weapons[WeaponManager.QueuedWeaponID].DrawTime * 0.5f) {
			m_ViewmodelConfig = ViewmodelConfiguration.DrawConfiguration;
		} else if(State == EState.Running) {
			m_ViewmodelConfig = ViewmodelConfiguration.RunConfiguration;
		} else if(State == EState.FastRunning) {
			m_ViewmodelConfig = ViewmodelConfiguration.FastRunConfiguration;
		} else if(State == EState.Sliding) {
			m_ViewmodelConfig = ViewmodelConfiguration.SlideConfiguration;
		} else {
			m_ViewmodelConfig = ViewmodelConfiguration.NormalConfiguration;
		}

		Transform t = m_ViewmodelHolder.Transform;

		if(m_ViewmodelConfig == ViewmodelConfiguration.DrawConfiguration) {
			float f = 10 / WeaponManager.HeldWeapon.Data.DrawTime * dt;
			t.origin = t.origin.LinearInterpolate(m_ViewmodelConfig.Position, f);
			m_ViewmodelHolder.RotationDegrees = m_ViewmodelHolder.RotationDegrees.LinearInterpolate(m_ViewmodelConfig.Rotaton, f);
		} else {
			if(WeaponManager.DrawTimer > 0) {
				float f = 10 / WeaponManager.HeldWeapon.Data.DrawTime * dt;
				t.origin = t.origin.LinearInterpolate(m_ViewmodelConfig.Position, f);
				m_ViewmodelHolder.RotationDegrees = m_ViewmodelHolder.RotationDegrees.LinearInterpolate(m_ViewmodelConfig.Rotaton, f);
			} else {
				t.origin = t.origin.LinearInterpolate(m_ViewmodelConfig.Position, m_ViewmodelConfig.ChangeSpeed*dt);
				m_ViewmodelHolder.RotationDegrees = m_ViewmodelHolder.RotationDegrees.LinearInterpolate(m_ViewmodelConfig.Rotaton, m_ViewmodelConfig.ChangeSpeed*dt);
			}
		}

		m_ViewmodelHolder.Transform = t;
	}

	private void CalculateHeight(float dt) {
		if(State == EState.Crouching) {
			m_Capsule.Height = Mathf.Lerp(m_Capsule.Height, CROUCH_HEIGHT, CROUCHDOWN_SPEED*dt);
		} else if(State == EState.Sliding) {
			m_Capsule.Height = Mathf.Lerp(m_Capsule.Height, SLIDE_HEIGHT, CROUCHDOWN_SPEED*dt);
		} else { 
			m_Capsule.Height = Mathf.Lerp(m_Capsule.Height, NORMAL_HEIGHT, STANDUP_SPEED*dt);
		}

		Transform t = m_CollisionShape.Transform;
		t.origin.y = (NORMAL_HEIGHT - m_Capsule.Height)/2;
		m_CollisionShape.Transform = t;
	}

	private void HandleFOV(float dt) {
		if(IsAiming) {
			Camera.Fov = Mathf.Lerp(Camera.Fov, WeaponManager.HeldWeapon.Data.AimFov, WeaponManager.HeldWeapon.Data.AimSpeed*dt);
		} else {
			Camera.Fov = Mathf.Lerp(Camera.Fov, DEFAULT_FOV, 10*dt);
		}
	}

	private void CalculateAcceleration() {
		if(IsOnFloor()) {
			if(State == EState.Sliding) {
				m_Accel = SLIDE_ACCEL;
			} else  {
				m_Accel = GROUND_ACCEL;
			}
		} else {
			m_Accel = AIR_ACCEL;
		}
	}

	private void HandleRunTimer(float dt) {
		if(State == EState.Running || State == EState.FastRunning || State == EState.Sliding) {
			RunTimer += dt;
		} else {
			RunTimer = 0;
		}
	}

	private void CalculateSpeed(float dt) {
		if(State == EState.Sliding) {
			m_Speed = Mathf.Lerp(m_Speed, m_TargetSpeed, SLIDE_SPEED_DECREASE*dt);
		} else {
			if(RealVelocity.Length() > 0) {
				m_Speed = Mathf.Lerp(m_Speed, m_TargetSpeed, 10*dt);
			} else {
				m_Speed = m_TargetSpeed;
			}
		}
	}

	private void HandleBobTimer(float dt) {
		if(IsOnFloor() && RealVelocity.Length() > 0) {
			m_BobTimer += dt;
		} else {
			m_BobTimer = 0;
		}
	}

	private void CalculatePhysicsMovement(float dt) {
		if(IsOnFloor()) {
			if(!Console.Instance.Visible && Input.IsActionJustPressed("jump")) {
				m_Gravity = JUMP;
				m_Snap = Vector3.Zero;
			} else {
				m_Snap = -GetFloorNormal();
				m_Gravity = 0;
			}
		} else {
			m_Snap = Vector3.Down;
			m_Gravity -= GRAVITY * dt;
			if(m_HeadBonker.IsColliding() && m_Gravity > 0) {
				m_Gravity = -m_Gravity*0.5f;
			}
		}

		m_Velocity = m_Direction * m_Speed;
		if(IsAiming && State != EState.Sliding) m_Velocity *= WeaponManager.HeldWeapon.Data.AimMoveSpeedMultiplier;

		RealVelocity = MoveAndSlideWithSnap(m_Velocity + Vector3.Up * m_Gravity, m_Snap, Vector3.Up);
	}

	private void HandleSlidePlayer() {
		if(State == EState.Sliding) {
			if(!m_SlideAudioPlayer.Playing) {
				m_SlideAudioPlayer.Play();
			}
		} else {
			m_SlideAudioPlayer.Stop();
		}
	}

	private void HandleBreathingAnimation(float dt) {
		if(State == EState.Walking && RealVelocity.Length() <= 1 && !IsAiming) {
			m_BreatheTimer += dt;
			m_BreathingAnimationTarget.y = Mathf.Sin(m_BreatheTimer * BREATHING_FREQUENCY) * BREATHING_AMPLITUDE;
		} else {
			m_BreatheTimer = 0;
			m_BreathingAnimationTarget = Vector3.Zero;
		}

		Transform t = m_ViewmodelOffset.Transform;
		t.origin = t.origin.LinearInterpolate(BreathingAnimation, 10*dt);
		m_ViewmodelOffset.Transform = t;

		BreathingAnimation = BreathingAnimation.LinearInterpolate(m_BreathingAnimationTarget, 10*dt);
	}
}
