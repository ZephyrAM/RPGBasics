using Godot;
using System;
using System.Collections.Generic;

// using ZAM.MenuUI;
// using ZAM.Interactions;

namespace ZAM.Control
{
	public partial class CharacterController : CharacterBody2D
	{
		// Assigned Variables \\
		[Export] private float baseSpeed = 350f;
		[Export] private float runMultiplier = 2f;

		// Setup Variables \\
		// private RayCast2D interactRay = null;
		private List<RayCast2D> interactArray = [];
		private Sprite2D charSprite = null;
		private CollisionShape2D charCollider = null;
		private NavigationAgent2D navAgent = null;
		private AnimationTree charAnim = null;
		private AnimationNodeStateMachinePlayback animPlay = null;

		private Camera2D camera2D;
		private CanvasLayer uiLayer = null;
		private PanelContainer textBox = null;
		private PanelContainer choiceBox = null;
		private VBoxContainer choiceList = null;

		private int choiceCommand = 0;
		private Vector2 lookDirection = new Vector2(1, 0);

		private Vector2 moveInput;
		private Vector2 direction;
		private float moveSpeed;

		private string inputPhase = ConstTerm.WAIT;
		private bool interactToggle = false;
		private bool speedText = false;
		private bool runToggle = false;

		// private CanvasLayer menuUI;
		// private Vector2 cameraLowLimit, cameraHighLimit;

		private int frameCounter = 0;
		private bool isControlActive = true;
		private Vector2 charSize;
		// private Interactable interactTarget;

		// Delegate Events \\
		[Signal]
		public delegate void onStepAreaEventHandler();
		[Signal]
		public delegate void onInteractCheckEventHandler(Vector2 direction);
		[Signal]
		public delegate void onSelectChangeEventHandler();
		[Signal]
		public delegate void onTextProgressEventHandler();
		[Signal]
		public delegate void onChoiceSelectEventHandler();
		[Signal]
		public delegate void onMenuOpenEventHandler();
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
			inputPhase = ConstTerm.MOVE;

			moveSpeed = baseSpeed;
			charSize = new Vector2(charSprite.Texture.GetWidth() / charSprite.Hframes, charSprite.Texture.GetHeight() / charSprite.Vframes);
		}

		public override void _PhysicsProcess(double delta)
		{
			if (!isControlActive) { return; }
			PhaseCheck();
			// AxisCheck();
			// InputCheck();
		}

        private void IfNull()
		{
			uiLayer ??= GetNode<CanvasLayer>("../../" + ConstTerm.CANVAS_LAYER);

			Node interactLayer = uiLayer.GetNode(ConstTerm.INTERACT_TEXT);
			textBox ??= interactLayer.GetNode<PanelContainer>(ConstTerm.TEXTBOX + ConstTerm.CONTAINER);
			choiceBox ??= interactLayer.GetNode<PanelContainer>(ConstTerm.CHOICEBOX + ConstTerm.CONTAINER);
			choiceList ??= choiceBox.GetNode<MarginContainer>(ConstTerm.MARGIN_CONTAINER).GetNode<VBoxContainer>(ConstTerm.VBOX_CONTAINER);

			// interactRay ??= GetNode<RayCast2D>(ConstTerm.RAYCAST2D);
			int rayCount = GetNode(ConstTerm.RAY_CHECK).GetChildren().Count;
			interactArray = [];
			for (int r = 0; r <	rayCount; r++)
			{
				interactArray.Add((RayCast2D)GetNode(ConstTerm.RAY_CHECK).GetChild(r));
			}

			charSprite ??= GetNode<Sprite2D>(ConstTerm.SPRITE2D);
			charCollider ??= GetNode<CollisionShape2D>(ConstTerm.COLLIDER2D);
			navAgent ??= GetNode<NavigationAgent2D>(ConstTerm.NAVAGENT2D);
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

		// private void MenuPhase(InputEvent @event)
		// {
		// 	switch (inputPhase)
		// 	{
		// 		case ConstTerm.COMMAND:
		// 			break;
		// 		case ConstTerm.MEMBER:
		// 			break;
		// 		case ConstTerm.ITEM:
		// 			break;
		// 		case ConstTerm.SKILL:
		// 			break;
		// 		case ConstTerm.SAVE:
		// 			break;
		// 		default:
		// 			break;
		// 	}
		// }


		//=============================================================================
		// SECTION: Map Control Methods
		//=============================================================================

		public void PhaseCheck()
		{
			if (!interactToggle) { if (Input.IsActionJustPressed(ConstTerm.ACCEPT)) { UpdateRayCast(); } }

			switch (inputPhase)
			{
				// case ConstTerm.MOVE:
				// 	MoveCheck();
				// 	break;
				case ConstTerm.MOVE:
					AxisCheck();
					break;
				// case ConstTerm.CLICK_MOVE:
				// 	ClickToMoveCheck();
				// 	break;
				case ConstTerm.CHOICE:
					ChoiceCheck();
					break;
				case ConstTerm.TEXT:
					TextCheck();
					break;
				case ConstTerm.MENU:
					MenuCheck();
					break;
				case ConstTerm.DO_NOTHING:
					break;
				default:
					break;
			}
		}

		// private void MoveCheck()
		// {
		// 	if (menuToggle) { return; }

		// 	Vector2 moveToPos;
		// 	if (Input.IsActionJustPressed("MoveTo"))
		// 	{
		// 		moveToPos = GetGlobalMousePosition();
		// 		navAgent.TargetPosition = moveToPos;

		// 		inputPhase = ConstTerm.CLICK_MOVE;
		// 		ClickToMoveCheck();
		// 	} else
		// 	{ inputPhase = ConstTerm.AXIS_MOVE;}
		// }

		private void AxisCheck()
		{
			if (Input.IsActionJustPressed(ConstTerm.MENU)) { inputPhase = ConstTerm.MENU; return; }

			moveInput = Input.GetVector(ConstTerm.LEFT, ConstTerm.RIGHT, ConstTerm.UP, ConstTerm.DOWN);
			direction = moveInput.Normalized();
			RunCheck();

			Vector2 targetVelocity = new Vector2(moveInput.X * moveSpeed, moveInput.Y * moveSpeed);

			if (direction == Vector2.Zero)
			{
				animPlay.Travel(ConstTerm.IDLE);
				// inputPhase = ConstTerm.MOVE;
			}
			else
			{
				SetLookDirection(direction);

				charAnim.Set(ConstTerm.PARAM + ConstTerm.IDLE + ConstTerm.BLEND, direction);
				charAnim.Set(ConstTerm.PARAM + ConstTerm.WALK + ConstTerm.BLEND, direction);
				animPlay.Travel(ConstTerm.WALK);
				EnemyCheck();
			}

			Velocity = targetVelocity;
			MoveAndSlide();
		}

		// public void ClickToMoveCheck()
		// {
		// 	if (navAgent.IsNavigationFinished()) { 
		// 		animPlay.Travel(ConstTerm.IDLE); 
		// 		inputPhase = ConstTerm.MOVE;

		// 		return; 
		// 	}

		// 	Vector2 currentPos = GlobalTransform.Origin;
		// 	Vector2 nextPos = navAgent.GetNextPathPosition();
		// 	direction = (nextPos - GlobalPosition).Normalized();

		// 	Vector2 clickVelocity = currentPos.DirectionTo(nextPos) * moveSpeed;

		// 	charAnim.Set(ConstTerm.PARAM + ConstTerm.IDLE + ConstTerm.BLEND, direction);
		// 	charAnim.Set(ConstTerm.PARAM + ConstTerm.WALK + ConstTerm.BLEND, direction);
		// 	animPlay.Travel(ConstTerm.WALK);

		// 	Velocity = clickVelocity;
		// 	MoveAndSlide();
		// }

		public void ChoiceCheck()
		{
			if (Input.IsActionJustPressed(ConstTerm.ACCEPT))
			{
				EmitSignal(SignalName.onChoiceSelect);
			}
			else if (Input.IsActionJustPressed(ConstTerm.UP))
			{
				CommandSelect(-1, choiceList);
			}
			else if (Input.IsActionJustPressed(ConstTerm.DOWN))
			{
				CommandSelect(1, choiceList);
			}
		}

		public void TextCheck()
		{
			if (Input.IsActionJustPressed(ConstTerm.ACCEPT))
			{
				EmitSignal(SignalName.onTextProgress);
			}
		}

		public void MenuCheck()
		{
			EmitSignal(SignalName.onMenuOpen);
		}

		private void RunCheck()
		{
			runToggle = Input.IsActionPressed(ConstTerm.RUN);

			if (runToggle) { moveSpeed = baseSpeed * runMultiplier; }
			else { moveSpeed = baseSpeed; }
		}

		public bool TextSpeedCheck()
		{
			return Input.IsActionPressed(ConstTerm.ACCEPT);
		}

		//=============================================================================
		// SECTION: Collision Detection
		//=============================================================================

		// private void UpdateRayCast()
		// {
		// 	Vector2 multi = lookDirection;

		// 	if (lookDirection.Y != 0)
		// 	{
		// 		multi *= charSize.Y / 1.2f;
		// 	}
		// 	else if (lookDirection.X != 0)
		// 	{
		// 		multi *= charSize.X / 1.5f;
		// 	}

		// 	interactRay.TargetPosition = multi;
		// 	EmitSignal(SignalName.onInteractCheck, interactRay, lookDirection);
		// }

		private void UpdateRayCast()
		{
			Vector2 multi = lookDirection;

			if (lookDirection.Y != 0)
			{
				multi *= charSize.Y / 1.2f;
				interactArray[1].Position = new Vector2(22.5f, 15);
				interactArray[2].Position = new Vector2(-22.5f, 15);
			}
			else if (lookDirection.X != 0)
			{
				multi *= charSize.X / 1.5f;
				interactArray[1].Position = new Vector2(0, -15);
				interactArray[2].Position = new Vector2(0, 45);
			}

			for (int r = 0; r < interactArray.Count; r++)
			{
				interactArray[r].TargetPosition = multi;
			}
			// interactRay.TargetPosition = multi;
			EmitSignal(SignalName.onInteractCheck, lookDirection);
		}


		//=============================================================================
		// SECTION: Selection Handling
		//=============================================================================

		private void CommandSelect(int change, Container targetList)
		{
			// if (direction == ConstTerm.VERT) { change *= numColumn; }
			choiceCommand = ChangeTarget(change, choiceCommand, GetCommandCount(targetList));
			EmitSignal(SignalName.onSelectChange);
		}

		private int ChangeTarget(int change, int target, int listSize)
		{
			// if (direction == ConstTerm.HORIZ) { change += change;}
			if (target + change > listSize - 1) { return 0; }
			else if (target + change < 0) { return listSize - 1; }
			else { return target += change; }
		}


		//=============================================================================
		// SECTION: Misc Methods
		//=============================================================================

		private void SetLookDirection(Vector2 currentDir)
		{
			lookDirection = currentDir;
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

		public void ChangeActive(bool change)
		{
			if (!change) { animPlay.Travel(ConstTerm.IDLE); }
			isControlActive = change;
		}

		private int GetCommandCount(Container targetList)
		{
			return targetList.GetChildCount();
		}

		// private void DrawRayCast()
		// {
		// 	DrawLine(ToLocal(new Vector2(Position.X, Position.Y)), ToLocal(new Vector2(Position.X + (direction.X * (charSize.X / 2)), 
		// 		Position.Y + (direction.Y * (charSize.Y / 1.5f)))), Colors.Blue, 1f);
		// }


		//=============================================================================
		// SECTION: External Access
		//=============================================================================

		public int GetChoice()
		{
			return choiceCommand;
		}

		public string GetInputPhase()
		{
			return inputPhase;
		}

		public List<RayCast2D> GetInteractArray()
		{
			return interactArray;
		}

		public void SetIdleAnim()
		{
			animPlay.Travel(ConstTerm.IDLE);
		}

		public void SetInputPhase(string phase)
		{
			inputPhase = phase;
		}

		public void SetInteractToggle(bool active)
		{
			interactToggle = active;
		}
	}
}