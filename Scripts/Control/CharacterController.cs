using Godot;
using System;

using ZAM.MenuUI;

namespace ZAM.Control
{
	public partial class CharacterController : CharacterBody2D
	{
		// Assigned Variables \\
		[Export] private float baseSpeed = 350f;
		[Export] private float runMultiplier = 2f;

		// Setup Variables \\
		private RayCast2D interactRay = null;
		private Sprite2D charSprite = null;
		private CollisionShape2D charCollider = null;
		private AnimationTree charAnim = null;
		private AnimationNodeStateMachinePlayback animPlay = null;

		private Camera2D camera2D;
		private CanvasLayer uiLayer = null;
		private TextBox textBox = null;
		// private Vector2 lookDirection;

		private Vector2 moveInput;
		private Vector2 direction;
		private float moveSpeed;
		private bool runToggle = false;
		private bool menuToggle = false;
		private bool battleToggle = false;

		// private CanvasLayer menuUI;
		// private Vector2 cameraLowLimit, cameraHighLimit;

		private int frameCounter = 0;
		private bool isActive = true;
		private Vector2 charSize;

		// Delegate Events \\
		[Signal]
		public delegate void onStepAreaEventHandler();
		[Signal]
		public delegate void onInteractTargetEventHandler(GodotObject target);
		[Signal]
		public delegate void onSaveGameEventHandler();

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
			charSize = new Vector2(charSprite.Texture.GetWidth() / charSprite.Hframes, charSprite.Texture.GetHeight() / charSprite.Vframes);
		}

		public override void _PhysicsProcess(double delta)
		{
			if (!isActive) { return; }
			AxisCheck();
			InputCheck();
		}

		private void IfNull()
		{
			uiLayer ??= GetNode<CanvasLayer>("../../" + ConstTerm.CANVAS_LAYER);
			textBox ??= uiLayer.GetNode<TextBox>(ConstTerm.TEXTBOX_CONTAINER);

			interactRay ??= GetNode<RayCast2D>(ConstTerm.RAYCAST2D);
			charSprite ??= GetNode<Sprite2D>(ConstTerm.SPRITE2D);
			charCollider ??= GetNode<CollisionShape2D>(ConstTerm.COLLIDER2D);
			charAnim ??= GetNode<AnimationTree>(ConstTerm.ANIM_TREE);
			animPlay ??= (AnimationNodeStateMachinePlayback)charAnim.Get(ConstTerm.PARAM + ConstTerm.PLAYBACK);
		}

		// public override void _Draw()
		// {
		//     DrawRayCast();
		// }

		// public override void _Input(InputEvent @event)
		// {
		// 	if (@event != null) { GD.Print(@event); }
		//     if (@event.IsActionPressed("Save"))
		// 	{
		// 		GD.Print("Saving game!");
		// 		EmitSignal(SignalName.onSaveGame);
		// 	}
		// }


		//=============================================================================
		// SECTION: Control Methods
		//=============================================================================

		public void InputCheck()
		{
			if (textBox.Visible) { textBox.FasterText(Input.IsActionPressed(ConstTerm.ACCEPT)); }

			if (Input.IsActionJustPressed(ConstTerm.ACCEPT))
			{
				if (textBox.IsTextComplete()) { textBox.HideTextBox(); }
				else { InteractCheck(); }
			}
			else if (Input.IsActionJustPressed("Save"))
			{
				GD.Print("Saving game!");
				EmitSignal(SignalName.onSaveGame);
			}
		}

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
				UpdateRayCast();
				// QueueRedraw();
				charAnim.Set(ConstTerm.PARAM + ConstTerm.IDLE + ConstTerm.BLEND, direction);
				charAnim.Set(ConstTerm.PARAM + ConstTerm.WALK + ConstTerm.BLEND, direction);
				animPlay.Travel(ConstTerm.WALK);
				EnemyCheck();
			}

			Velocity = targetVelocity;
			MoveAndSlide();
			// CollisionCheck(GetSlideCollisionCount());
		}

		private void UpdateRayCast()
		{
			if (direction.Y != 0)
			{
				interactRay.TargetPosition = moveInput * (charSize.Y / 1.5f);
			}
			else if (direction.X != 0)
			{
				interactRay.TargetPosition = moveInput * (charSize.X / 2);
			}
		}

		// private void CollisionCheck(int count)
		// {
		// 	for (int i = 0; i < count; i++)
		// 	{
		// 		KinematicCollision2D collision = GetSlideCollision(i);
		// 		GD.Print("Collided with " + ((Node)collision.GetCollider()).Name);
		// 	}
		// }

		private void RunCheck()
		{
			runToggle = Input.IsActionPressed(ConstTerm.RUN);

			if (runToggle) { moveSpeed = baseSpeed * runMultiplier; }
			else { moveSpeed = baseSpeed; }
		}

		private void InteractCheck()
		{
			if (interactRay.IsColliding() && !textBox.Visible)
			{
				GodotObject target = interactRay.GetCollider();
				if (target.HasMethod(ConstTerm.TARGETINTERACT)) {
					target.Call(ConstTerm.TARGETINTERACT);
				}
				// EmitSignal(SignalName.onInteractTarget, interactRay.GetCollider());
			}
		}


		//=============================================================================
		// SECTION: Misc Methods
		//=============================================================================

		// private void SetLookDirection()
		// {
		// 	lookDirection = direction;
		// }

		private void EnemyCheck()
		{
			frameCounter++;
			if (frameCounter >= 30)
			{
				EmitSignal(SignalName.onStepArea);
				frameCounter = 0;
			}
		}

		public void ChangeActive(bool change)
		{
			if (!change) { animPlay.Travel(ConstTerm.IDLE); }
			isActive = change;
		}

		// private void DrawRayCast()
		// {
		// 	DrawLine(ToLocal(new Vector2(Position.X, Position.Y)), ToLocal(new Vector2(Position.X + (direction.X * (charSize.X / 2)), Position.Y + (direction.Y * (charSize.Y / 1.5f)))), Colors.Blue, 1f);
		// }
	}
}