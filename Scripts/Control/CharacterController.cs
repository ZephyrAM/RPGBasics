using Godot;
using System;

namespace ZAM.Control
{
	public partial class CharacterController : CharacterBody2D
	{
		// Assigned Variables \\
		[Export] private float baseSpeed = 350f;
		[Export] private float runMultiplier = 2f;

		// Setup Variables \\
		private AnimationTree charAnim = null;
		private AnimationNodeStateMachinePlayback animPlay = null;
		private Vector2 lookDirection;

		private Vector2 moveInput;
		private Vector2 direction;
		private float moveSpeed;
		private bool runToggle = false;
		private bool menuToggle = false;
		private bool battleToggle = false;

		private Camera2D camera2D;
		// private CanvasLayer menuUI;
		// private Vector2 cameraLowLimit, cameraHighLimit;

		private int frameCounter = 0;

		// Delegate Events \\
		[Signal]
		public delegate void onStepAreaEventHandler();

		//=============================================================================
		// SECTION: Base Methods
		//=============================================================================

		public override void _Ready()
		{
			// Set camera boundaries to limit player movement
			camera2D = GetNode<Camera2D>("../../" + ConstTerm.CAMERA2D);
			// menuUI = (CanvasLayer)GetParent().GetChild(0);

			// cameraLowLimit = new Vector2(camera2D.LimitLeft, camera2D.LimitTop);
			// cameraHighLimit = new Vector2(camera2D.LimitRight, camera2D.LimitBottom);


			// Create follow camera for leader
            RemoteTransform2D tempCamera = new RemoteTransform2D
            { RemotePath = camera2D.GetPath() };
            AddChild(tempCamera);

			
			IfNull();

			moveSpeed = baseSpeed;
		}

		public override void _PhysicsProcess(double delta)
		{
			AxisCheck();
		}

		//=============================================================================
		// SECTION: OnReady Methods
		//=============================================================================

		private void IfNull()
		{
			charAnim ??= GetNode<AnimationTree>(ConstTerm.ANIM_TREE);
			animPlay ??= (AnimationNodeStateMachinePlayback)charAnim.Get(ConstTerm.PARAM + ConstTerm.PLAYBACK);
		}

		//=============================================================================
		// SECTION: Control Methods
		//=============================================================================

		private void AxisCheck()
		{
			if (menuToggle) { return; }

			moveInput = Input.GetVector(ConstTerm.LEFT, ConstTerm.RIGHT, ConstTerm.UP, ConstTerm.DOWN);
			direction = moveInput.Normalized();
			RunCheck();

			Vector2 targetVelocity = new Vector2(moveInput.X * moveSpeed, moveInput.Y * moveSpeed);


			if (direction == Vector2.Zero)
			{
				animPlay.Travel(ConstTerm.IDLE);
			}
			else
			{
				charAnim.Set(ConstTerm.PARAM + ConstTerm.IDLE + ConstTerm.BLEND, direction);
				charAnim.Set(ConstTerm.PARAM + ConstTerm.WALK + ConstTerm.BLEND, direction);
				animPlay.Travel(ConstTerm.WALK);
				EnemyCheck();
			}

			Velocity = targetVelocity;
			MoveAndSlide();
		}

		private void RunCheck()
		{
			runToggle = Input.IsActionPressed(ConstTerm.RUN);

			if (runToggle) { moveSpeed = baseSpeed * runMultiplier; }
			else { moveSpeed = baseSpeed; }
		}

		//=============================================================================
		// SECTION: Misc Methods
		//=============================================================================

		private void SetLookDirection()
		{
			lookDirection = direction;
		}

		private void EnemyCheck()
		{
			frameCounter++;
			if (frameCounter >= 30)
			{
				EmitSignal(SignalName.onStepArea);
				frameCounter = 0;
			}
		}
	}
}