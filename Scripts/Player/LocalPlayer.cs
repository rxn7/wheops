using Godot;
using System;

public class LocalPlayer : HumanBase {
	public static readonly PackedScene SCENE = GD.Load<PackedScene>("res://Scenes/LocalPlayer.tscn");

	public const float DEFAULT_MOUSE_SENS = 0.1f;
	public const float HEAD_LERP_WEIGHT = 60; 
	public const float RUN_SPEED = 10;
	public const float FASTRUN_SPEED = 12; public const float FASTRUN_RUN_TIME_THRESHOLD = 1.5f;
	public const float FASTRUN_VELOCITY_THRESHOLD = 7f; public const float WALK_SPEED = 6;
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
	public const float CROUCH_RECOIL_MULTIPLIER = 0.8f;
	public const float DEFAULT_FOV = 80;
	public const float SLIDE_SPEED_DECREASE = 1.5f;
	public const float SLIDE_ACCEL = 1f;
	public const float FULLY_AIMING_DISTANCE_THRESHOLD = 0.03f;
	public const float BREATHING_AMPLITUDE = 0.01f;
	public const float BREATHING_FREQUENCY = 2f;
	public const float RUN_VELOCITY_THRESHOLD = 2f;

	public RayCast ShootRaycast { get; private set; }
	public HUD Hud { get; private set; }
	public SmoothCamera CameraHolder { get; private set; }
	public CameraRecoil CameraRecoil { get; private set; }
	public Camera Camera { get; private set; }
	public Vector3 RealVelocity { get; private set; }
	public Vector3 BreathingAnimation { get; private set; }
	public float RunTimer { get; private set; }
	public float CameraTilt { get; private set; } 
	public float MouseSensetivity { get; set; }
	public bool IsFullyAiming { get; private set; }

	public bool HasLanded => IsOnFloor() && !m_WasOnFloorLastFrame;

	private RayCast m_HeadBonker;
	private ViewmodelSway m_WeaponSway;
	private Vector3 m_BreathingAnimationTarget;
	private Position3D m_ViewmodelOffset;

	private bool m_WasOnFloorLastFrame = false;
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
		base._Ready();

		Hud = GetNode<HUD>("HUD");
		m_HeadBonker = Head.GetNode<RayCast>("Bonker");
		CameraHolder = Head.GetNode<SmoothCamera>("CameraHolder");
		CameraRecoil = CameraHolder.GetNode<CameraRecoil>("CameraRecoil");
		CameraHolder.SetAsToplevel(true);
		Camera = CameraRecoil.GetNode<Camera>("Camera");
		m_WeaponSway = Camera.GetNode<ViewmodelSway>("ViewmodelSway");
		m_ViewmodelOffset = m_WeaponSway.GetNode<Position3D>("ViewmodelOffset");
		m_ViewmodelHolder = m_ViewmodelOffset.GetNode<Position3D>("ViewmodelHolder");
		WeaponManager = m_ViewmodelHolder.GetNode<LocalWeaponManager>("WeaponManager");

		MouseSensetivity = Config.GetValue<float>("mouse", "sens", DEFAULT_MOUSE_SENS);
		Input.SetMouseMode(Input.MouseMode.Captured);

		NetworkManager.OnTick += OnTick;
	}

	public override void _Process(float dt) {
		TakeInput(dt);
		m_Direction = m_Direction.LinearInterpolate(m_Input.Normalized(), m_Accel*dt);
		HandleRunTimer(dt);
		CalculateAcceleration();
		CalculateSpeed(dt);
		CalculateHeight(dt);

		HandleBobTimer(dt);
		CalculateBob();
		ApplyHeadBob(dt);
		ApplyViewmodelBob(dt);

		IsFullyAiming = IsAiming && m_ViewmodelHolder.Transform.origin.DistanceTo(ViewmodelProperties.AimConfiguration.Position) <= FULLY_AIMING_DISTANCE_THRESHOLD;

		HandleFOV(dt);
		HandleBreathingAnimation(dt);

		CameraTilt = Mathf.Lerp(CameraTilt, m_TargetCameraTilt, BOB_LERP_WEIGHT*dt);
		Hud.Crosshair.Visible = !IsAiming && !Console.Active;

		if(HasLanded) OnLand();
		m_WasOnFloorLastFrame = IsOnFloor();

		base._Process(dt);
	}

	public override void _PhysicsProcess(float dt) {
		CalculatePhysicsMovement(dt);
	}

	private void OnTick(object sender, EventArgs args) {
		if(NetworkManager.IsHost) {
			if(NetworkManager.Network is Server server) {
				server.Sender.PlayerTransform(-1);
			}  else if(NetworkManager.Network is Client client) {
				client.Sender.PlayerTransform();
			}
		}
	}

	public override void _Input(InputEvent e) {
		if(Console.Active) return;

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

		EHumanState prev_state = State;
		if(!Console.Active) {
			Vector3 forward = GlobalTransform.basis.z;
			Vector3 right = GlobalTransform.basis.x;
			if(Input.IsActionPressed("move_right")) m_Input += right;
			if(Input.IsActionPressed("move_left")) m_Input -= right;
			if(Input.IsActionPressed("move_forward")) m_Input -= forward;
			if(Input.IsActionPressed("move_backward")) m_Input += forward;
			m_Input.y = 0;

			IsAiming = Input.IsActionPressed("aim") && WeaponManager.DrawTimer == 0 && !WeaponManager.IsReloading;

			if(!IsAiming && !WeaponManager.IsReloading && Input.IsActionPressed("run") && Input.IsActionPressed("move_forward") && IsOnFloor() && !WeaponManager.WillShoot && !WeaponManager.HasJustShot && WeaponManager.DrawTimer == 0) { 
				if(RunTimer > FASTRUN_RUN_TIME_THRESHOLD){ 
					State = EHumanState.FastRunning;
					m_TargetSpeed = FASTRUN_SPEED;
				} else if(RealVelocity.Length() >= RUN_VELOCITY_THRESHOLD) {
					State = EHumanState.Running;
					m_TargetSpeed = RUN_SPEED;
				} else {
					State = EHumanState.Walking;
					m_TargetSpeed = WALK_SPEED;
				}
			} else {
				State = EHumanState.Walking;
				m_TargetSpeed = WALK_SPEED;
			}

			if(Input.IsActionPressed("crouch") && IsOnFloor()) {
				if(RealVelocity.Length() > CROUCH_SPEED*1.5f && Input.IsActionPressed("run") && RunTimer > FASTRUN_RUN_TIME_THRESHOLD) {
					State = EHumanState.Sliding;
					m_TargetSpeed = CROUCH_SPEED;
				} else {
					State = EHumanState.Crouching;
					m_TargetSpeed = CROUCH_SPEED;
				}
			}
		} else {
			State = EHumanState.Walking;
			m_TargetSpeed = WALK_SPEED;
		}

		if(prev_state == EHumanState.Sliding && State != EHumanState.Sliding) {
			RunTimer = 0;
		}

		if(State == EHumanState.FastRunning && RealVelocity.Length() <= FASTRUN_VELOCITY_THRESHOLD) {
			State = EHumanState.Running;
			RunTimer = 0;
		}

		m_TargetSpeed *= WeaponManager.HeldWeapon != null ? WeaponManager.HeldWeapon.Data.MoveSpeedMultiplier : 1;
	}

	private void CalculateBob() {
		m_Bob = Vector3.Zero;

		if(State == EHumanState.Sliding) return;

		if(m_BobTimer > 0) {
			if(m_Input.Length() > 0 && RealVelocity.Length() > BOB_VELOCITY_THRESHOLD) {
				float multiplier = Mathf.Sqrt(m_TargetSpeed) / 2;
				float amplitude_mul = 1;
				if(IsFullyAiming) amplitude_mul = 0.2f;

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
		else			t.origin = t.origin.LinearInterpolate(Vector3.Zero, BOB_RESET_LERP_WEIGHT*dt);

		m_ViewmodelOffset.Transform = t;
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
			if(State == EHumanState.Sliding) {
				m_Accel = SLIDE_ACCEL;
			} else {
				m_Accel = GROUND_ACCEL;
			}
		} else {
			m_Accel = AIR_ACCEL;
		}
	}

	private void HandleRunTimer(float dt) {
		if(State == EHumanState.Running || State == EHumanState.FastRunning || State == EHumanState.Sliding) {
			RunTimer += dt;
		} else {
			RunTimer = 0;
		}
	}

	private void CalculateSpeed(float dt) {
		if(State == EHumanState.Sliding) {
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
		if(IsAiming && State != EHumanState.Sliding) m_Velocity *= WeaponManager.HeldWeapon.Data.AimMoveSpeedMultiplier;

		RealVelocity = MoveAndSlideWithSnap(m_Velocity + Vector3.Up * m_Gravity, m_Snap, Vector3.Up);
	}

	private void HandleBreathingAnimation(float dt) {
		if((State == EHumanState.Walking || State == EHumanState.Crouching) && RealVelocity.Length() <= 1 && !WeaponManager.WillShoot) {
			m_BreatheTimer += dt;
			m_BreathingAnimationTarget.y = Mathf.Sin(m_BreatheTimer * BREATHING_FREQUENCY) * BREATHING_AMPLITUDE * (IsAiming ? 0.1f : 1) ;
		} else {
			m_BreatheTimer = 0;
			m_BreathingAnimationTarget = Vector3.Zero;
		}

		Transform t = m_ViewmodelOffset.Transform;
		t.origin = t.origin.LinearInterpolate(BreathingAnimation, 10*dt);
		m_ViewmodelOffset.Transform = t;

		BreathingAnimation = BreathingAnimation.LinearInterpolate(m_BreathingAnimationTarget, 10*dt);
	}

	private void OnLand() {
		PlayFootstep();
	}

	private void PlayFootstep() {
		SoundEffect.Spawn(FOOTSTEP_SOUND, Random.RangeF(0.7f, 1.3f), RealVelocity.Length() - 20);
	}
}
