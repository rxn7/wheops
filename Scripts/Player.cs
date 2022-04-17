using Godot;

public class Player : KinematicBody {
	private enum State {
		Walking,
		Running,
		FastRunning,
		Crouching,
		Sliding,
	};

	private static Player _instance;
	public static Player Instance => _instance;

	public const float HEAD_LERP_WEIGHT = 20; 
	public const float RUN_SPEED = 10;
	public const float FASTRUN_SPEED = 12;
	public const float FASTRUN_RUN_TIME_THRESHOLD = 1.5f;
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
	public const float NORMAL_HEIGHT = 1;
	public const float CROUCH_HEIGHT = 0.2f;
	public const float SLIDE_HEIGHT = 0.05f;
	public const float STANDUP_SPEED = 13f;
	public const float CROUCHDOWN_SPEED = 10f;
	public const float SLIDEDOWN_SPEED = 15f;
	public const float CROUCH_RECOIL_MULTIPLIER = 0.8f;
	public const float DEFAULT_FOV = 80;
	public const float SLIDE_SPEED_DECREASE = 1.5f;
	public const float SLIDE_ACCEL = 1f;
	public static readonly AudioStream FOOTSTEP_SOUND = GD.Load<AudioStream>("res://Sounds/footstep.wav");

	private CollisionShape m_shape;
	private AudioStreamPlayer m_slide_player;
	private RayCast m_head_bonker_raycast;
	public Position3D m_head;
	public Position3D m_camera_holder, m_viewmodel_bobber, m_viewmodel_holder;
	public Camera m_camera;
	private Crosshair m_crosshair;
	public WeaponManager m_weapon_mgr;
	private WeaponSway m_weapon_sway;
	public HUD m_hud;
	private Vector3 m_target_recoil;
	private Vector3 m_recoil;

	private ViewmodelConfiguration m_viewmodel_config;
	private float m_mouse_sensitivity = 0.1f;
	public bool m_aiming = false;
	private float m_gravity = 0;
	private float m_recoil_multiplier = 1;
	private float m_camera_tilt = 0; 
	private float m_bob_timer = 0;
	private float m_run_timer = 0;
	private float m_speed, m_target_speed;
	private float m_accel;
	private bool m_play_footstep = false;
	private State m_state;
	private Vector3 m_velocity, m_real_velocity, m_input, m_snap, m_direction, m_bob;

	public float Speed() => (m_velocity + Vector3.Up * m_gravity).Length();

	public override void _Ready() {
		_instance = this;

		Random.Init();

		m_hud = GetNode<HUD>("HUD");
		m_slide_player = GetNode<AudioStreamPlayer>("SlidePlayer");
		m_head = GetNode<Position3D>("Head");
		m_shape = GetNode<CollisionShape>("CollisionShape");
		m_head_bonker_raycast = m_head.GetNode<RayCast>("Bonker");
		m_camera_holder = m_head.GetNode<Position3D>("CameraHolder");
		m_camera_holder.SetAsToplevel(true);
		m_camera = m_camera_holder.GetNode<Camera>("Camera");
		m_weapon_sway = m_camera.GetNode<WeaponSway>("WeaponSway");
		m_viewmodel_bobber = m_weapon_sway.GetNode<Position3D>("ViewmodelBobber");
		m_viewmodel_holder = m_viewmodel_bobber.GetNode<Position3D>("ViewmodelHolder");
		m_crosshair = GetNode<Crosshair>("HUD/Crosshair");
		m_weapon_mgr = m_viewmodel_holder.GetNode<WeaponManager>("WeaponManager");

		Input.SetMouseMode(Input.MouseMode.Captured);

		m_crosshair.SetGap(24);
		m_crosshair.SetColor(Color.Color8(20,240,20));
		m_weapon_mgr.QueueWeaponChange(0);
	}

	public override void _Process(float dt) {
		TakeInput(dt);
		m_direction = m_direction.LinearInterpolate(m_input.Normalized(), m_accel*dt);

		CalculateSpeed(dt);
		HandleRunTimer(dt);
		CalculateAcceleration();
		SmoothHeadMovement(dt);
		CalculateHeight(dt);
		HandleBobTimer(dt);
		CalculateBob();
		ApplyHeadBob(dt);
		ApplyViewmodelBob(dt);
		ApplyViewmodelConfig(dt);
		ApplyRecoil(dt);
		HandleFOV(dt);
		HandleSlidePlayer();

		m_camera_holder.RotationDegrees = new Vector3(Mathf.Clamp(m_head.RotationDegrees.x + m_recoil.x, -89, 89), RotationDegrees.y + m_recoil.y, Mathf.Lerp(m_camera_holder.RotationDegrees.z, m_camera_tilt, BOB_LERP_WEIGHT*dt));

		m_crosshair.Visible = !m_aiming;
	}

	public override void _PhysicsProcess(float dt) {
		CalculatePhysicsMovement(dt);
	}

	public override void _UnhandledInput(InputEvent e) {
		if(e is InputEventMouseMotion mouse_motion_e) {
			m_head.RotateX(Mathf.Deg2Rad(-mouse_motion_e.Relative.y * m_mouse_sensitivity)); Vector3 head_r = m_head.RotationDegrees;
			head_r.x = Mathf.Clamp(head_r.x, -89, 89);
			m_head.RotationDegrees = head_r;
RotateY(Mathf.Deg2Rad(-mouse_motion_e.Relative.x * m_mouse_sensitivity));
			Vector3 r = RotationDegrees;
			while(r.y >= 180) r.y -= 180;
			while(r.y <= -180) r.y += 180;
			RotationDegrees = r;
		}
	}

	private void TakeInput(float dt) {
		m_input = new Vector3(0,0,0);
		m_camera_tilt = 0;

		Vector3 forward = GlobalTransform.basis.z;
		Vector3 right = GlobalTransform.basis.x;

		if(Input.IsActionPressed("move_right")) m_input += right;
		if(Input.IsActionPressed("move_left")) m_input -= right;
		if(Input.IsActionPressed("move_forward")) m_input -= forward;
		if(Input.IsActionPressed("move_backward")) m_input += forward;

		m_input.y = 0;
		m_aiming = Input.IsActionPressed("aim") && m_weapon_mgr.m_draw_timer == 0 && !m_weapon_mgr.m_reloading;
		State prev_state = m_state;

		if(!m_aiming && !m_weapon_mgr.m_reloading && Input.IsActionPressed("run") && Input.IsActionPressed("move_forward") && IsOnFloor() && !m_weapon_mgr.WantsToShoot() && !m_weapon_mgr.HasJustShot() && m_weapon_mgr.m_draw_timer == 0) { 
			if(m_run_timer > FASTRUN_RUN_TIME_THRESHOLD) {
				m_state = State.FastRunning;
				m_target_speed = FASTRUN_SPEED;
			} else {
				m_state = State.Running;
				m_target_speed = RUN_SPEED;
			}
		} else {
			m_state = State.Walking;
			m_target_speed = WALK_SPEED;
		}

		if(Input.IsActionPressed("crouch") && IsOnFloor()) {
			if(m_real_velocity.Length() > CROUCH_SPEED*1.3f && (Input.IsActionPressed("run") && m_run_timer > FASTRUN_RUN_TIME_THRESHOLD)) {
				m_state = State.Sliding;
				m_target_speed = CROUCH_SPEED;
			} else {
				m_state = State.Crouching;
				m_target_speed = CROUCH_SPEED;
			}
		}

		if(prev_state == State.Sliding && m_state != State.Sliding) {
			m_run_timer = 0;
		}

		m_target_speed *= m_weapon_mgr.m_held_weapon != null ? m_weapon_mgr.m_held_weapon.m_data.m_move_speed_multiplier : 1;
	}

	private void SmoothHeadMovement(float dt) {
		Transform t = m_camera_holder.GlobalTransform;
		if(Engine.GetFramesPerSecond() > Engine.IterationsPerSecond) {
			Vector3 interval = m_velocity / Engine.GetFramesPerSecond();
			Vector3 position = m_head.GlobalTransform.origin + interval;

			t.origin = t.origin.LinearInterpolate(position, HEAD_LERP_WEIGHT*dt);
		} else {
			t.origin = m_head.GlobalTransform.origin;
		}

		m_camera_holder.GlobalTransform = t;
	}

	private void CalculateBob() {
		m_bob = Vector3.Zero;

		if(m_state == State.Sliding) return;

		if(m_bob_timer > 0) {
			float multiplier = m_target_speed / 7;
			float amplitude_mul = 1;
			if(m_aiming) amplitude_mul = 0.2f;

			if(m_real_velocity.Length() > BOB_VELOCITY_THRESHOLD) {
				m_bob.y = Mathf.Sin(m_bob_timer * BOB_FREQUENCY * multiplier) * multiplier * amplitude_mul;
				m_camera_tilt = m_bob.y;
				m_bob.x = Mathf.Cos(m_bob_timer * BOB_FREQUENCY * 0.5f * multiplier) * 2 * multiplier * amplitude_mul;
			}

			if(m_bob.y < 0 && m_play_footstep) {
				PlayFootstep();
			} 

			m_play_footstep = m_bob.y >= 0;
		}
	}

	private void ApplyHeadBob(float dt) {
		Transform t = m_camera.Transform;

		if(m_bob_timer > 0)	t.origin = t.origin.LinearInterpolate(m_bob * HEADBOB_AMPLITUDE, BOB_LERP_WEIGHT*dt);
		else			t.origin = t.origin.LinearInterpolate(Vector3.Zero, BOB_RESET_LERP_WEIGHT*dt);

		m_camera.Transform = t;
	}

	private void ApplyViewmodelBob(float dt) {
		Transform t = m_viewmodel_bobber.Transform;

		if(m_bob_timer > 0)	t.origin = t.origin.LinearInterpolate(m_bob * VIEWMODEL_BOB_AMPLITUDE, BOB_LERP_WEIGHT*dt);
		else			t.origin = t.origin.LinearInterpolate(Vector3.Zero, BOB_RESET_LERP_WEIGHT*dt);

		m_viewmodel_bobber.Transform = t;
	}

	private void PlayFootstep() {
		SoundEffect.Spawn3D(this, GlobalTransform.origin + new Vector3(0, -1, 0), FOOTSTEP_SOUND, Random.RangeF(0.7f, 1.3f));
	}

	private void ApplyViewmodelConfig(float dt) {
		if(m_weapon_mgr.m_reloading) {
			m_viewmodel_config = ViewmodelConfiguration.ReloadConfiguration;
		} else if(m_aiming) {
			m_viewmodel_config = ViewmodelConfiguration.AimConfiguration;
		}else if(m_weapon_mgr.WantsToShoot() || m_weapon_mgr.HasJustShot()) {
			m_viewmodel_config = ViewmodelConfiguration.ShootConfiguration;
		} else if(m_weapon_mgr.m_held_weapon != null && m_weapon_mgr.m_draw_timer > m_weapon_mgr.m_weapons[m_weapon_mgr.m_queued_weapon].m_data.m_draw_time * 0.5f) {
			m_viewmodel_config = ViewmodelConfiguration.DrawConfiguration;
		} else if(m_state == State.Running) {
			m_viewmodel_config = ViewmodelConfiguration.RunConfiguration;
		} else if(m_state == State.FastRunning) {
			m_viewmodel_config = ViewmodelConfiguration.FastRunConfiguration;
		} else if(m_state == State.Sliding) {
			m_viewmodel_config = ViewmodelConfiguration.SlideConfiguration;
		} else {
			m_viewmodel_config = ViewmodelConfiguration.NormalConfiguration;
		}

		Transform t = m_viewmodel_holder.Transform;

		if(m_viewmodel_config == ViewmodelConfiguration.DrawConfiguration) {
			float f = 1 - (m_weapon_mgr.m_draw_timer / (m_weapon_mgr.m_weapons[m_weapon_mgr.m_queued_weapon].m_data.m_draw_time));
			t.origin = t.origin.LinearInterpolate(m_viewmodel_config.m_position, f);
			m_viewmodel_holder.RotationDegrees = m_viewmodel_holder.RotationDegrees.LinearInterpolate(m_viewmodel_config.m_rotation, f);
		} else {
			if(m_weapon_mgr.m_draw_timer > 0) {
				float f = 1 - ((m_weapon_mgr.m_draw_timer * 2) / (m_weapon_mgr.m_weapons[m_weapon_mgr.m_queued_weapon].m_data.m_draw_time));
				t.origin = t.origin.LinearInterpolate(m_viewmodel_config.m_position, f);
				m_viewmodel_holder.RotationDegrees = m_viewmodel_holder.RotationDegrees.LinearInterpolate(m_viewmodel_config.m_rotation, f);
			} else {
				t.origin = t.origin.LinearInterpolate(m_viewmodel_config.m_position, m_viewmodel_config.m_change_speed*dt);
				m_viewmodel_holder.RotationDegrees = m_viewmodel_holder.RotationDegrees.LinearInterpolate(m_viewmodel_config.m_rotation, m_viewmodel_config.m_change_speed*dt);
			}
		}

		m_viewmodel_holder.Transform = t;
	}

	private void ApplyRecoil(float dt) {
		if(m_weapon_mgr.m_held_weapon == null) return;

		m_target_recoil = m_target_recoil.LinearInterpolate(Vector3.Zero, m_weapon_mgr.m_held_weapon.m_data.m_camera_recoil_relax_speed*dt);
		m_recoil = m_recoil.LinearInterpolate(m_target_recoil, 10*dt);
	}

	public void AddRecoil(float x, float y) {
		float mul = m_recoil_multiplier;
		if(m_aiming) mul *= 0.7f;

		m_target_recoil.x += x * mul;
		m_target_recoil.y += y * mul;
	}

	private void CalculateHeight(float dt) {
		Vector3 scale = m_shape.Scale;
		if(m_state == State.Crouching) {
			scale.y = Mathf.Lerp(scale.y, CROUCH_HEIGHT, CROUCHDOWN_SPEED*dt);
		} else if(m_state == State.Sliding) {
			scale.y = Mathf.Lerp(scale.y, SLIDE_HEIGHT, CROUCHDOWN_SPEED*dt);
		} else { 
			scale.y = Mathf.Lerp(scale.y, NORMAL_HEIGHT, STANDUP_SPEED*dt);
		}

		m_shape.Scale = scale;
	}

	private void HandleFOV(float dt) {
		if(m_aiming) {
			m_camera.Fov = Mathf.Lerp(m_camera.Fov, m_weapon_mgr.m_held_weapon.m_data.m_aim_fov, m_weapon_mgr.m_held_weapon.m_data.m_aim_speed*dt);
		} else {
			m_camera.Fov = Mathf.Lerp(m_camera.Fov, DEFAULT_FOV, 10*dt);
		}
	}

	private void CalculateAcceleration() {
		if(IsOnFloor()) {
			if(m_state == State.Sliding) {
				m_accel = SLIDE_ACCEL;
			} else  {
				m_accel = GROUND_ACCEL;
			}
		} else {
			m_accel = AIR_ACCEL;
		}
	}

	private void HandleRunTimer(float dt) {
		if(m_state == State.Running || m_state == State.FastRunning || m_state == State.Sliding) {
			m_run_timer += dt;
		} else {
			m_run_timer = 0;
		}
	}

	private void CalculateSpeed(float dt) {
		if(m_state == State.Sliding) {
			m_speed = Mathf.Lerp(m_speed, m_target_speed, SLIDE_SPEED_DECREASE*dt);
		} else {
			if(m_real_velocity.Length() > 0) {
				m_speed = Mathf.Lerp(m_speed, m_target_speed, 10*dt);
			} else {
				m_speed = m_target_speed;
			}
		}
	}

	private void HandleBobTimer(float dt) {
		if(IsOnFloor() && m_input.Length() > 0) {
			m_bob_timer += dt;
		} else {
			m_bob_timer = 0;
		}
	}

	private void CalculatePhysicsMovement(float dt) {
		if(IsOnFloor()) {
			if(Input.IsActionJustPressed("jump")) {
				m_gravity = JUMP;
				m_snap = Vector3.Zero;
			} else {
				m_snap = -GetFloorNormal();
				m_gravity = 0;
			}
		} else {
			m_snap = Vector3.Down;
			m_gravity -= GRAVITY * dt;
			if(m_head_bonker_raycast.IsColliding() && m_gravity > 0) {
				m_gravity = -m_gravity*0.5f;
			}
		}

		m_velocity = m_direction * m_speed;
		if(m_aiming) m_velocity *= m_weapon_mgr.m_held_weapon.m_data.m_aim_move_speed_multiplier;
		m_real_velocity = MoveAndSlideWithSnap(m_velocity + Vector3.Up * m_gravity, m_snap, Vector3.Up);
	}

	private void HandleSlidePlayer() {
		if(m_state == State.Sliding) {
			if(!m_slide_player.Playing) {
				m_slide_player.Play();
			}
		} else {
			m_slide_player.Stop();
		}
	}
}
