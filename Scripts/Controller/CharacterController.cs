using Godot;
using Godot.Collections;
using System;
// using System.Collections.Generic;

using ZAM.MenuUI;

namespace ZAM.Controller
{
	public partial class CharacterController : CharacterBody2D, IUIFunctions
	{
		// Assigned Variables \\
		[Export] private float baseSpeed = 350f;
		[Export] private float runMultiplier = 2f;

		// Setup Variables \\
		// private RayCast2D interactRay = null;
		private Array<RayCast2D> interactArray = [];
		private Sprite2D charSprite = null;
		private CollisionShape2D charCollider = null;
		private NavigationAgent2D navAgent = null;
		private AnimationTree charAnim = null;
		private AnimationNodeStateMachinePlayback animPlay = null;

		private Camera2D camera2D;
		private CanvasLayer uiLayer = null;
		private Panel textBox = null;
		private PanelContainer choiceBox = null;
		private VBoxContainer choiceList = null;
		private ButtonUI activeControl = null;

		private ButtonUI mouseFocus = null;
		private RayCast2D mouseCursor = null;
		private bool mouseCheck = false;
		private CharacterBody2D mouseTarget = null;

		private int numColumn = 1;
		private int choiceCommand = 0;
		private bool signalsDone = false;
		private string activeInput = ConstTerm.KEY_GAMEPAD;

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
		private float colliderHeight, colliderWidth;
		// private Interactable interactTarget;

		// Delegate Events \\
		[Signal]
		public delegate void onStepAreaEventHandler();
		[Signal]
		public delegate void onCollisionCheckEventHandler(GodotObject collider);
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
		public delegate void onPauseMenuEventHandler();

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
			SetInputPhase(ConstTerm.MOVE);

			moveSpeed = baseSpeed;
			charSize = new Vector2(charSprite.Texture.GetWidth() / charSprite.Hframes, charSprite.Texture.GetHeight() / charSprite.Vframes);
		}

        // protected override void Dispose(bool disposing)
        // {
        //     base.Dispose(disposing);
        // }

        private void IfNull()
		{
			uiLayer ??= GetNode<CanvasLayer>("../../" + ConstTerm.CANVAS_LAYER); // EDIT: Find better path solution

			Node interactLayer = uiLayer.GetNode(ConstTerm.INTERACT_TEXT);
			textBox ??= interactLayer.GetNode<Panel>(ConstTerm.TEXTBOX + ConstTerm.CONTAINER);
			choiceBox ??= interactLayer.GetNode<PanelContainer>(ConstTerm.CHOICEBOX + ConstTerm.CONTAINER);
			choiceList ??= choiceBox.GetNode<MarginContainer>(ConstTerm.MARGIN_CONTAINER).GetNode<VBoxContainer>(ConstTerm.VBOX_CONTAINER);

			// interactRay ??= GetNode<RayCast2D>(ConstTerm.RAYCAST2D);
			int rayCount = GetNode(ConstTerm.RAY_CHECK).GetChildren().Count;
			interactArray = [];
			for (int r = 0; r <	rayCount; r++) {
				interactArray.Add((RayCast2D)GetNode(ConstTerm.RAY_CHECK).GetChild(r)); 
				interactArray[r].AddException(this);
			}

			charSprite ??= GetNode<Sprite2D>(ConstTerm.SPRITE2D);
			charCollider ??= GetNode<CollisionShape2D>(ConstTerm.COLLIDER2D);
			navAgent ??= GetNode<NavigationAgent2D>(ConstTerm.NAVAGENT2D);
			charAnim ??= GetNode<AnimationTree>(ConstTerm.ANIM_TREE);
			animPlay ??= (AnimationNodeStateMachinePlayback)charAnim.Get(ConstTerm.PARAM + ConstTerm.PLAYBACK);

			colliderHeight = charCollider.Shape.GetRect().Size.Y;
			colliderWidth = charCollider.Shape.GetRect().Size.X;
		}

		private void SubLists(Container targetList)
		{
			for (int c = 0; c < targetList.GetChildCount(); c++) {
				Node tempLabel = targetList.GetChild(c);
				targetList.GetChild(c).GetNode<ButtonUI>(ConstTerm.BUTTON).MouseEntered += () => OnMouseEntered(targetList, tempLabel);
				targetList.GetChild(c).GetNode<ButtonUI>(ConstTerm.BUTTON).Pressed += OnMouseClick; }
		}

		// private void UnSubLists(Container targetList)
		// {
		// 	for (int c = 0; c < targetList.GetChildCount(); c++) {
		// 		Node tempLabel = targetList.GetChild(c);
		// 		targetList.GetChild(c).GetNode<ButtonUI>(ConstTerm.BUTTON).MouseEntered -= () => OnMouseEntered(targetList, tempLabel);
		// 		targetList.GetChild(c).GetNode<ButtonUI>(ConstTerm.BUTTON).Pressed -= OnMouseClick; }
		// }

		// private void SubInteracts(Container targetList)
		// {
		// 	for (int e = 0; e < targetList.GetChildCount(); e++)
		// 	{
		// 		CharacterBody2D tempBody = (CharacterBody2D)targetList.GetChild(e);
		// 		tempBody._InputEvent(GetViewport(), ConstTerm.ACCEPT + ConstTerm.CLICK, 0) += OnMouseClick;
		// 	}
		// }

		// public override void _Draw()
		// {
		//     DrawRayCast();
		// }

		public override void _Input(InputEvent @event)
        {
			if (@event.IsActionPressed(ConstTerm.PAUSE) && IsControlActive()) { // EDIT: Pause/System menu. Skip cutscene menu? Make conditional global?
				EmitSignal(SignalName.onPauseMenu);
			}

			if (@event is InputEventMouse && activeInput == ConstTerm.KEY_GAMEPAD) { activeInput = ConstTerm.MOUSE; 
                Input.MouseMode = Input.MouseModeEnum.Visible; if (mouseFocus != null) { mouseFocus.MouseFilter = Control.MouseFilterEnum.Stop; } }
            else if (@event is not InputEventMouse && activeInput == ConstTerm.MOUSE) { activeInput = ConstTerm.KEY_GAMEPAD; 
                Input.MouseMode = Input.MouseModeEnum.Hidden; if (mouseFocus != null) { mouseFocus.MouseFilter = Control.MouseFilterEnum.Ignore; } }
        	// if (@event is InputEventMouseButton eventMouseButton) { GD.Print(eventMouseButton.ButtonIndex); }

			if (@event is InputEventMouseButton eventMouseButton) { if (eventMouseButton.IsActionPressed(ConstTerm.ACCEPT + ConstTerm.CLICK)) 
			{ MouseRayCheck(); }}
        }

		public override void _PhysicsProcess(double delta)
		{
			if (!isControlActive) { return; }
			PhaseCheck();
		}

		private bool MouseRayCheck()
		{
			if (mouseCursor == null) {
                mouseCursor = new() {
					TargetPosition = new Vector2(0, 1),
					GlobalPosition = GetGlobalMousePosition(),
                    HitFromInside = true };
                mouseCursor.SetCollisionMaskValue(1, false);
				mouseCursor.SetCollisionMaskValue(3, true);
				
				int currentScene = GetTree().Root.GetChildCount() - 1;
				GetTree().Root.GetChild(currentScene).GetChild(0).AddChild(mouseCursor); 
			}

			mouseCursor.GlobalPosition = GetGlobalMousePosition();
			mouseCursor.ForceRaycastUpdate();

			return mouseCursor.IsColliding();
			// GD.Print(mouseCursor.IsColliding());
		}


		//=============================================================================
		// SECTION: Map Control Methods
		//=============================================================================

		public void PhaseCheck()
		{
			if (!interactToggle) { if (Input.IsActionJustPressed(ConstTerm.ACCEPT)) { UpdateRayCast(); } }

			SaveCheck(); // EDIT: Temporary for debugging

			switch (inputPhase)
			{
				case ConstTerm.MOVE:
					MoveCheck();
					break;
				case ConstTerm.AXIS_MOVE:
					AxisCheck();
					break;
				case ConstTerm.CLICK_MOVE:
					ClickToMoveCheck();
					if (Input.IsActionJustPressed(ConstTerm.ACCEPT + ConstTerm.CLICK)) { MoveCheck(); }
					break;
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

		private void SaveCheck()
		{
			// if (Input.IsActionJustPressed(ConstTerm.SAVE))
			// {
			// 	GD.Print("Saving game!");
			// 	SaveLoader.Instance.SaveGame();
			// }
			// else if (Input.IsActionJustPressed(ConstTerm.LOAD))
			// {
			// 	GD.Print("Loading Game!");
			// 	SaveLoader.Instance.LoadGame();
			// }
		}

		private void MoveCheck()
		{
			if (Input.IsActionJustPressed(ConstTerm.MENU)) { SetInputPhase(ConstTerm.MENU); return; }

			Vector2 moveToPos;
			if (Input.IsActionPressed(ConstTerm.ACCEPT + ConstTerm.CLICK))
			{
				moveToPos = GetGlobalMousePosition();
				mouseCheck = MouseRayCheck();
				if (mouseCheck) { mouseTarget = mouseCursor.GetCollider() as CharacterBody2D; }
				// Godot.Collections.Dictionary result = GetWorld2D().DirectSpaceState.IntersectRay(PhysicsRayQueryParameters2D.Create(GlobalPosition, moveToPos));

				navAgent.TargetPosition = moveToPos;

				SetInputPhase(ConstTerm.CLICK_MOVE);
				ClickToMoveCheck();
			} else
			{ SetInputPhase(ConstTerm.AXIS_MOVE); }
		}

		private void AxisCheck()
		{
			if (Input.IsActionJustPressed(ConstTerm.MENU)) { SetInputPhase(ConstTerm.MENU); return; }

			moveInput = Input.GetVector(ConstTerm.LEFT, ConstTerm.RIGHT, ConstTerm.UP, ConstTerm.DOWN);
			direction = moveInput.Normalized();
			RunCheck();

			Vector2 targetVelocity = new Vector2(moveInput.X * moveSpeed, moveInput.Y * moveSpeed);

			if (direction == Vector2.Zero)
			{
				animPlay.Travel(ConstTerm.IDLE);
				SetInputPhase(ConstTerm.MOVE);
			}
			else
			{
				SetLookDirection(direction);

				// charAnim.Set(ConstTerm.PARAM + ConstTerm.IDLE + ConstTerm.BLEND, direction);
				charAnim.Set(ConstTerm.PARAM + ConstTerm.WALK + ConstTerm.BLEND, direction);
				animPlay.Travel(ConstTerm.WALK);
				EnemyCheck();
			}

			Velocity = targetVelocity;
			bool collide = MoveAndSlide();
			if (collide) { 
				if (GetLastSlideCollision().GetCollider() is CharacterBody2D) {
					EmitSignal(SignalName.onCollisionCheck, GetLastSlideCollision().GetCollider());
				}
			}
		}

		public void ClickToMoveCheck()
		{
			moveInput = Input.GetVector(ConstTerm.LEFT, ConstTerm.RIGHT, ConstTerm.UP, ConstTerm.DOWN).Normalized();
			if (moveInput != Vector2.Zero) { CancelMouseMove(ConstTerm.AXIS_MOVE); return; }

			if (Input.IsActionJustPressed(ConstTerm.MENU)) { CancelMouseMove(ConstTerm.MENU); return; }

			if (navAgent.IsNavigationFinished()) { NavFinished(); return; }

			Vector2 currentPos = GlobalPosition;
			Vector2 nextPos = navAgent.GetNextPathPosition();
			direction = (nextPos - GlobalPosition).Normalized();
			RunCheck();

			Vector2 clickVelocity = currentPos.DirectionTo(nextPos) * moveSpeed;

			charAnim.Set(ConstTerm.PARAM + ConstTerm.IDLE + ConstTerm.BLEND, direction);
			charAnim.Set(ConstTerm.PARAM + ConstTerm.WALK + ConstTerm.BLEND, direction);
			animPlay.Travel(ConstTerm.WALK);

			Velocity = clickVelocity;
			SetLookDirection(direction);
			EnemyCheck();

			ClickCollisionCheck();
			bool collide = MoveAndSlide();
			if (collide) {
				if (GetLastSlideCollision().GetCollider() is CharacterBody2D) {
					EmitSignal(SignalName.onCollisionCheck, GetLastSlideCollision().GetCollider());
				}
				if (GetLastSlideCollision().GetCollider() == mouseTarget) { UpdateRayCast(); }
			}
		}

		public void ChoiceCheck()
		{
			if (Input.IsActionJustPressed(ConstTerm.ACCEPT)) {
				ChoiceAccept();
			}
			else if (Input.IsActionJustPressed(ConstTerm.UP)) {
				CommandSelect(-1, choiceList, ConstTerm.VERT);
			}
			else if (Input.IsActionJustPressed(ConstTerm.DOWN)) {
				CommandSelect(1, choiceList, ConstTerm.VERT);
			}
		}

		private void ChoiceAccept()
		{
			mouseFocus = null;
			EmitSignal(SignalName.onChoiceSelect);
			activeControl = IUIFunctions.FocusOff(choiceList, choiceCommand);
			choiceCommand = 0;
		}

		public void TextCheck()
		{
			if (Input.IsActionJustPressed(ConstTerm.ACCEPT) || Input.IsActionJustPressed(ConstTerm.ACCEPT + ConstTerm.CLICK)) {
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
			return Input.IsActionPressed(ConstTerm.ACCEPT) || Input.IsActionPressed(ConstTerm.ACCEPT + ConstTerm.CLICK);
		}

		private void CancelMouseMove(string newPhase)
		{
			navAgent.TargetPosition = GlobalPosition;
			NavFinished();
			SetInputPhase(newPhase);
		}

		private void NavFinished()
		{
			animPlay.Travel(ConstTerm.IDLE);
			SetInputPhase(ConstTerm.MOVE);

			if (mouseCheck) { UpdateRayCast(); }
			mouseCheck = false;
			mouseTarget = null;
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
			CreateRayCheck();
			EmitSignal(SignalName.onInteractCheck, lookDirection); // -> MapSystem - OnInteractCheck
		}

		private void CreateRayCheck()
		{
			// GD.Print("Raycast update");
			Vector2 multi = lookDirection;

			if (lookDirection.X != 0 && lookDirection.Y != 0)
			{
				if (Math.Abs(lookDirection.X) >= Math.Abs(lookDirection.Y)) { lookDirection.Y = 0; }
				else if (Math.Abs(lookDirection.X) < Math.Abs(lookDirection.Y)) { lookDirection.X = 0; }
			}
			else if (lookDirection.Y != 0)
			{
				multi *= charSize.Y / 1.8f;
				interactArray[1].Position = new Vector2(colliderWidth / 2, colliderHeight / 4);
				interactArray[2].Position = new Vector2(-(colliderWidth / 2), colliderHeight / 4);
			}
			else if (lookDirection.X != 0)
			{
				multi *= charSize.X / 2f;
				interactArray[1].Position = new Vector2(0, -(colliderHeight / 4));
				interactArray[2].Position = new Vector2(0, (colliderHeight / 4) * 3);
			}

			for (int r = 0; r < interactArray.Count; r++)
			{
				interactArray[r].TargetPosition = multi;
			}
			// interactRay.TargetPosition = multi;
		}

		private void ClickCollisionCheck()
		{
			if (mouseCheck) {
				navAgent.TargetPosition = mouseTarget.GlobalPosition;
			}
			// CreateRayCheck();
			
			// for (int r = 0; r < interactArray.Count; r++)
			// {
			// 	interactArray[r].ForceRaycastUpdate();
			// 	if (interactArray[r].IsColliding())
			// 	{
			// 		Node2D nodeTarget = (Node2D)interactArray[r].GetCollider();
			// 		CollisionShape2D collisionTarget = nodeTarget.GetNode<CollisionShape2D>(ConstTerm.COLLIDER2D);

			// 		Vector2 distance = new(GlobalPosition.X - navAgent.TargetPosition.X, GlobalPosition.Y - navAgent.TargetPosition.Y);
			// 		Vector2 lookDir = new(Math.Abs(lookDirection.X), Math.Abs(lookDirection.Y));
			// 		Vector2 targetSize = collisionTarget.Shape.GetRect().Size;
			// 		// GD.Print(distance + " " + targetSize + " " + lookDir);

			// 		if (lookDir.X > lookDir.Y)
			// 		{
			// 			if (Math.Abs(distance.X) < targetSize.X + (charCollider.Shape.GetRect().Size.X / 2))
			// 			{
			// 				navAgent.TargetPosition = GlobalPosition;
			// 				animPlay.Travel(ConstTerm.IDLE);
			// 				EmitSignal(SignalName.onInteractCheck, lookDirection);
			// 			}
			// 		}
			// 		else
			// 		{
			// 			if (Math.Abs(distance.Y) < targetSize.Y + (charCollider.Shape.GetRect().Size.Y / 1.8))
			// 			{
			// 				navAgent.TargetPosition = GlobalPosition;
			// 				animPlay.Travel(ConstTerm.IDLE);
			// 				EmitSignal(SignalName.onInteractCheck, lookDirection);
			// 			}
			// 		}
			// 	}
			// }
		}


		//=============================================================================
		// SECTION: Selection Handling
		//=============================================================================

		private void CommandSelect(int change, Container targetList, string scrollDirection)
		{			
			change = IUIFunctions.CheckColumn(change, scrollDirection, numColumn);
            IUIFunctions.ChangeTarget(change, ref choiceCommand, IUIFunctions.GetCommandCount(targetList));

            activeControl = IUIFunctions.FocusOn(targetList, choiceCommand);
			EmitSignal(SignalName.onSelectChange);
		}

		// private int ChangeTarget(int change, int target, int listSize)
		// {
		// 	// if (direction == ConstTerm.HORIZ) { change += change;}
		// 	if (target + change > listSize - 1) { return 0; }
		// 	else if (target + change < 0) { return listSize - 1; }
		// 	else { return target += change; }
		// }


		//=============================================================================
		// SECTION: Misc Methods
		//=============================================================================

		public void SetLookDirection(Vector2 currentDir)
		{
			lookDirection = currentDir;
			charAnim.Set(ConstTerm.PARAM + ConstTerm.IDLE + ConstTerm.BLEND, lookDirection);
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

		public bool IsControlActive()
		{
			return isControlActive;
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
		// SECTION: Signal Methods
		//=============================================================================

		private void OnMouseEntered(Container currList, Node currLabel)
		{
			if (currList != choiceList) { return; }

			activeControl = IUIFunctions.FocusOff(currList, choiceCommand);
			choiceCommand = currLabel.GetIndex();

			activeControl = IUIFunctions.FocusOn(currList, choiceCommand);
			mouseFocus = currLabel.GetNode<ButtonUI>(ConstTerm.BUTTON);
			EmitSignal(SignalName.onSelectChange);
		}

		private void OnMouseClick()
		{
			switch (inputPhase)
			{
				case ConstTerm.CHOICE:
					ChoiceAccept();
					break;
				default:
					break;
			}
		}


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

		public Array<RayCast2D> GetInteractArray()
		{
			return interactArray;
		}

		public string GetCharName()
		{
			return Name;
		}

		public Vector2 GetFaceDirection()
		{
			return lookDirection;
		}

		public void SetIdleAnim()
		{
			animPlay.Travel(ConstTerm.IDLE);
		}

		public void SetInputPhase(string phase)
		{
			inputPhase = phase;
			if (phase == ConstTerm.CHOICE) {
				SubLists(choiceList);
				activeControl = IUIFunctions.FocusOn(choiceList, choiceCommand); 
			}
		}

		public void SetInteractToggle(bool active)
		{
			interactToggle = active;
		}
	}
}