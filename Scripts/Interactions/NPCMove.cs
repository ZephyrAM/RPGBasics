using Godot;
using Godot.Collections;
using System;

namespace ZAM.Interactions
{
    public partial class NPCMove : Node
    {
        [ExportGroup("Player")]
        [Export] private Script playerControlTest = null;

        [ExportGroup("Specs")]
        [Export] private float baseSpeed = 300;
        [Export] private float moveStepDelay = 60;
        [Export] private float moveStepDuration = 60;

        [ExportGroup("Chase")]
        [Export] private float lostSightTimer = 300;

        [ExportGroup("Nodes")]
        [Export] private Interactable npcInteract = null;
        [Export] private CharacterBody2D charBody = null;
        [Export] private Sprite2D charSprite = null;
        [Export] private CollisionShape2D charCollider = null;
        [Export] private Area2D sightArea = null;
        [Export] private NavigationAgent2D navAgent = null;
        [Export] private RayCast2D sightRay = null;
        [Export] private Array<RayCast2D> checkArray = [];
        // [Export] private ShapeCast2D checkShape = null;
        [Export] private AnimationTree animTree = null;

        private AnimationNodeStateMachinePlayback animPlay = null;

        private Node2D playerParty = null;

        private Vector2 charSize;
        private float colliderHeight, colliderWidth;

        private float chaseTimer = 0;
        private float battleMaxTimer = 5;
        private float battleCountTimer = 0;
        private float sightMaxTimer = 10;
        private float sightCountTimer = 0;

        private bool hasCollided = false;
        private bool doesMove = true;
        private float waitTimer = 0;
        private float moveTimer = 0;

        private Vector2 lookDirection = Vector2.Zero;
        private Vector2 moveDirection = Vector2.Zero;
        private Vector2 oldDirection = Vector2.Zero;

        private Vector2 returnPosition = Vector2.Zero;
        private Vector2 blockedDirection = Vector2.Zero;

        // Delegate Events \\
        [Signal]
        public delegate void onEndEventStepEventHandler(Interactable interactor);
        [Signal]
        public delegate void onCatchPlayerEventHandler(PackedScene battle, Interactable toFree);

        //=============================================================================
        // SECTION: OnReady Methods
        //=============================================================================

        public override void _Ready()
        {
            IfNull();
            SubSignals();

            npcInteract.SetInteractPhase(ConstTerm.WAIT);
            
            if (!npcInteract.ShouldChasePlayer) { DisableChaseArea(); }
        }

        // public override void _ExitTree()
        // {
        //     UnSubSignals();
        // }

        public override void _PhysicsProcess(double delta)
        {
            PhaseCheck();
            LineOfSightCheck();
        }

        private void IfNull()
        {
            npcInteract ??= GetParent<Interactable>();

            charBody ??= GetParent<CharacterBody2D>();
            charSprite ??= charBody.GetNode<Sprite2D>(ConstTerm.SPRITE2D);
            charCollider ??= charBody.GetNode<CollisionShape2D>(ConstTerm.COLLIDER2D);
            sightArea ??= charBody.GetNode<Area2D>(ConstTerm.AREA2D);
            animTree ??= charBody.GetNode<AnimationTree>(ConstTerm.ANIM_TREE);
            navAgent ??= charBody.GetNode<NavigationAgent2D>(ConstTerm.NAVAGENT2D);

            int rayCount = npcInteract.GetNode(ConstTerm.RAY_CHECK).GetChildren().Count;
            checkArray = [];
			for (int r = 0; r <	rayCount; r++) {
				checkArray.Add((RayCast2D)npcInteract.GetNode(ConstTerm.RAY_CHECK).GetChild(r));
                checkArray[r].AddException(charBody);
            }
            // checkShape ??= charBody.GetNode<ShapeCast2D>(ConstTerm.SHAPECAST2D);

            animPlay = (AnimationNodeStateMachinePlayback)animTree.Get(ConstTerm.PARAM + ConstTerm.PLAYBACK);

            charSize = new Vector2(charSprite.Texture.GetWidth() / charSprite.Hframes, charSprite.Texture.GetHeight() / charSprite.Vframes);

            colliderHeight = charCollider.Shape.GetRect().Size.Y;
            colliderWidth = charCollider.Shape.GetRect().Size.X;
        }

        private void SubSignals()
        {
            sightArea.BodyEntered += OnBodyEntered;
            navAgent.Connect(NavigationAgent2D.SignalName.NavigationFinished, new Callable(this, MethodName.OnNavigationFinished));
        }

        // private void UnSubSignals()
        // {
        //     sightArea.BodyEntered -= OnBodyEntered;
        // }

        //=============================================================================
        // SECTION: Phase Methods
        //=============================================================================

        private void PhaseCheck()
        {
            // if (!doesMove) { return; }

            switch (npcInteract.GetInteractPhase())
            {
                case ConstTerm.WAIT:
                    WaitCycle();
                    break;
                case ConstTerm.MOVE:
                    MoveCycle();
                    break;
                case ConstTerm.INTERACT:
                    animPlay.Travel(ConstTerm.IDLE);
                    break;
                case ConstTerm.CHASE:
                    ChaseCycle();
                    break;
                case ConstTerm.RETURN:
                    ReturnCycle();
                    break;
                case ConstTerm.MOVE_TO:
                    MoveToCycle();
                    break;
                case ConstTerm.DO_NOTHING:
                    DoNothing();
                    break;
                default:
                    break;
            }
        }

        private void WaitCycle()
        {
            Vector2 moveVelocity = new Vector2(moveDirection.X * baseSpeed, moveDirection.Y * baseSpeed);

            waitTimer++;
            if (waitTimer > moveStepDelay) {
                waitTimer = 0;

                bool[] canMove = [true, true, true, true];
                FindDirection:
                SetDirection(canMove);
                if (UpdateRayCast()) { 
                    if (!canMove[0] && !canMove[1] && !canMove[2] && !canMove[3]) { waitTimer -= moveStepDelay; return; } // If blocked from all directions, restart the Wait cycle and double timer.
                    goto FindDirection; 
                }

                animTree.Set(ConstTerm.PARAM + ConstTerm.WALK + ConstTerm.BLEND, moveDirection);
                animPlay.Travel(ConstTerm.WALK);

                npcInteract.SetInteractPhase(ConstTerm.MOVE);
            } else {
                animPlay.Travel(ConstTerm.IDLE);
            }

            charBody.Velocity = moveVelocity;
            charBody.MoveAndSlide();
        }

        private void MoveCycle()
        {
            Vector2 moveVelocity = new Vector2(moveDirection.X * baseSpeed, moveDirection.Y * baseSpeed);

            moveTimer++;
            if (moveTimer > moveStepDuration) {
                moveTimer = 0;

                moveDirection = Vector2.Zero;
                lookDirection = moveDirection;
                animPlay.Travel(ConstTerm.IDLE);

                npcInteract.SetInteractPhase(ConstTerm.WAIT);
            } else {
                animTree.Set(ConstTerm.PARAM + ConstTerm.IDLE + ConstTerm.BLEND, moveDirection);
                animTree.Set(ConstTerm.PARAM + ConstTerm.WALK + ConstTerm.BLEND, moveDirection);
                animPlay.Travel(ConstTerm.WALK);
            }

            charBody.Velocity = moveVelocity;
            charBody.MoveAndSlide();
        }

        private void ChaseCycle()
        {
            if (!sightArea.OverlapsBody(GetChaseTarget()) || !CheckChaseSight()) { // EDIT: Create raycast to chase target. Out of sight if broken.
                chaseTimer++;
                if (chaseTimer > lostSightTimer) {
                    chaseTimer = 0;
                    npcInteract.SetInteractPhase(ConstTerm.RETURN);
                    return;
                }
            } else { chaseTimer = 0; }

            navAgent.TargetPosition = playerParty.GlobalPosition;

            battleCountTimer++;
            if (battleCountTimer >= battleMaxTimer) {
                battleCountTimer = 0;
                CheckBattleCollision();
            }

            NavToTarget(baseSpeed * 1.5f);

        }

        private void ReturnCycle()
        {
            navAgent.TargetPosition = returnPosition;

            if (navAgent.IsNavigationFinished()) {
                npcInteract.SetInteractPhase(ConstTerm.WAIT);
                return;
             }

            NavToTarget(baseSpeed * 0.75f);
        }

        private void MoveToCycle()
        {
            // GD.Print("Moving to goal!");
            if (navAgent.IsNavigationFinished())
            {
                if (DoesMove()) { npcInteract.SetInteractPhase(ConstTerm.WAIT); }
                else { npcInteract.SetInteractPhase(ConstTerm.DO_NOTHING); }
                // npcInteract.EventComplete();
                return;
            }

            NavToTarget(baseSpeed);
        }

        public void DoNothing()
        {
            moveDirection = Vector2.Zero;
            lookDirection = moveDirection;
            animPlay.Travel(ConstTerm.IDLE);
        }

        private void HearingCheck()
        {
            if (!npcInteract.ShouldChasePlayer) { return; }
        }

        private void LineOfSightCheck()
        {
            if (GetChaseTarget() == null) { return;}

            sightCountTimer++;
            if (sightCountTimer >= sightMaxTimer) {
                if (CheckChaseSight() && sightArea.OverlapsBody(playerParty)) { StartChase(); }
            }
        }

        private void CheckBattleCollision()
        {
            if (hasCollided) { return; }
            if (charBody.GetLastSlideCollision() != null && charBody.GetLastSlideCollision().GetCollider() == playerParty) { // EDIT: Needs improvement
                // GD.Print("Caught player!");
                if (npcInteract.GetBattleGroup() != null) {
                    TriggerBattler();
                }
                else { npcInteract.SetInteractPhase(ConstTerm.RETURN); }
            }
        }

        public void TriggerBattler()
        {
            hasCollided = true;
            npcInteract.SetInteractPhase(ConstTerm.DO_NOTHING);
            EmitSignal(SignalName.onCatchPlayer, npcInteract.GetBattleGroup(), GetParent() as Interactable); // Mapsystem -> OnCatchPlayer
        }

        //=============================================================================
        // SECTION: Move Checks
        //=============================================================================

        private void SetDirection(bool[] moveTest)
        {
            Random random = new();
            float xy = random.Next(0, 2);
            random = new();
            float direction = random.Next(0, 2);

            if (xy == 0) {
                if (direction == 0) { MoveDirection(new Vector2(-1, 0)); moveTest[0] = false; }
                else { MoveDirection(new Vector2(1, 0)); moveTest[1] = false; }
            } else {
                if (direction == 0) { MoveDirection(new Vector2(0, -1)); moveTest[2] = false; }
                else { MoveDirection(new Vector2(0, 1)); moveTest[3] = false; }
            }
        }

        private bool UpdateRayCast()
        {
            bool rayCheck = CreateRayCheck();
            return rayCheck;
        }

        private bool CreateRayCheck()
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
                checkArray[1].Position = new Vector2(colliderWidth / 2, colliderHeight / 4);
                checkArray[2].Position = new Vector2(-(colliderWidth / 2), colliderHeight / 4);
            }
            else if (lookDirection.X != 0)
            {
                multi *= charSize.X / 2f;
                checkArray[1].Position = new Vector2(0, -(colliderHeight / 4));
                checkArray[2].Position = new Vector2(0, (colliderHeight / 4) * 3);
            }

            int collideIndex = -1;
            for (int r = 0; r < checkArray.Count; r++)
            {
                checkArray[r].TargetPosition = multi;
                checkArray[r].ForceRaycastUpdate();
                if (checkArray[r].IsColliding()) { collideIndex = r; }
            }

            if (collideIndex > 0) { return true; }

            return false;
        }

        // private bool UpdateShapeCast()
        // {
        //     Vector2 multi = moveDirection;

        //     if (moveDirection.Y != 0)
        //     {
        //         multi *= charSize.Y / 1.6f;
        //     }
        //     else if (moveDirection.X != 0)
        //     {
        //         multi *= charSize.X / 2f;
        //     }

        //     checkShape.TargetPosition = multi;

        //     checkShape.ForceShapecastUpdate();
        //     if (checkShape.IsColliding())
        //     { 
        //         blockedDirection = moveDirection;
        //         // GD.Print(blockedDirection);
        //         return true; 
        //     }

        //     return false;
        // }

        // private Vector2 ShiftDirection()
        // {
        //     Vector2 shiftAmount = checkShape.GetCollisionNormal(0) ;
        //     // GD.Print(shiftAmount);
        //     return shiftAmount;
        // }

        private void MoveDirection(Vector2 direction)
        {
            moveDirection = direction;
            lookDirection = moveDirection;
            animTree.Set(ConstTerm.PARAM + ConstTerm.IDLE + ConstTerm.BLEND, moveDirection);

            sightArea.Rotation = moveDirection.Angle();
            // sightRay.Rotation = moveDirection.Angle();
        }

        private void StartChase()
        {           
            if (npcInteract.ShouldChasePlayer)
            {
                returnPosition = charBody.GlobalPosition;
                npcInteract.SetInteractPhase(ConstTerm.CHASE);
            }
        }

        private bool CheckChaseSight() // EDIT: Being called without sightArea overlap
        {
            if (GetChaseTarget() == null) { return false; }
            bool canSee = false;

            PhysicsDirectSpaceState2D spaceState = charBody.GetWorld2D().DirectSpaceState;
            PhysicsRayQueryParameters2D query = PhysicsRayQueryParameters2D.Create(charBody.GlobalPosition, GetChaseTarget().GlobalPosition, sightRay.CollisionMask, [charBody.GetRid()]);
            Dictionary result = spaceState.IntersectRay(query);

            if (result.Count > 0) {
                ulong checkResult = (ulong)result[ConstTerm.COLLIDER_ID];
                if (checkResult == GetChaseTarget().GetInstanceId()) { canSee = true; }
            }

            return canSee;
        }

        private void NavToTarget(float speed) // 
        {
            Vector2 currentPos = charBody.GlobalPosition;
            Vector2 nextPos = navAgent.GetNextPathPosition();
            // if (UpdateShapeCast()) { nextPos = new Vector2(MathF.Round(nextPos.X), MathF.Round(nextPos.Y)) * new Vector2(MathF.Round(blockedDirection.Y), MathF.Round(blockedDirection.X)); }
            MoveDirection((nextPos - charBody.Position).Normalized());

            Vector2 moveVelocity = currentPos.DirectionTo(nextPos) * speed;
            
            animTree.Set(ConstTerm.PARAM + ConstTerm.IDLE + ConstTerm.BLEND, moveDirection);
            animTree.Set(ConstTerm.PARAM + ConstTerm.WALK + ConstTerm.BLEND, moveDirection);
            animPlay.Travel(ConstTerm.WALK);

            charBody.Velocity = moveVelocity;
            charBody.MoveAndSlide();
        }

        public void MoveToTarget(Node2D target)
        {
            npcInteract.SetInteractPhase(ConstTerm.MOVE_TO);
            
            navAgent.TargetPosition = target.GlobalPosition;
            NavToTarget(baseSpeed);
        }

        // private bool ReachTest()
        // {
        //     if (navAgent.TargetPosition)
        // }

        //=============================================================================
        // SECTION: Access Methods
        //=============================================================================

        public bool DoesMove()
        {
            return doesMove;
        }

        public void SetDoesMove(bool value)
        {
            doesMove = value;
        }
        
        public Node2D GetChaseTarget()
        {
            return playerParty;
        }

        public void SetPlayer(Node2D player)
        {
            playerParty = player;
        }

        public NavigationAgent2D GetNavAgent()
        {
            return navAgent;
        }

        public Vector2 GetLookDirection()
        {
            return lookDirection;
        }

        public void SetLookDirection(Vector2 currentDir)
        {
            lookDirection = currentDir;
            animPlay.Set(ConstTerm.PARAM + ConstTerm.IDLE + ConstTerm.BLEND, lookDirection);
        }

        public void FaceDirection(Vector2 direction)
        {
            oldDirection = moveDirection;
            MoveDirection(direction * -1);
        }

        public void RevertDirection()
        {
            MoveDirection(oldDirection);
        }

        public CollisionShape2D GetCollider()
        {
            return charCollider;
        }

        public void DisableChaseArea()
        {
            sightArea.ProcessMode = ProcessModeEnum.Disabled;
            sightArea.Visible = false;
            sightRay.Visible = false;
        }

        public void EnableChaseArea()
        {
            sightArea.ProcessMode = ProcessModeEnum.Inherit;
            sightArea.Visible = true;
            sightRay.Visible = true;
        }

        //=============================================================================
        // SECTION: Signal Methods
        //=============================================================================

        private void OnBodyEntered(Node2D body)
        {
            if (npcInteract.GetInteractPhase() == ConstTerm.CHASE) { return; }

            if (body.GetScript().AsGodotObject() == playerControlTest){
                if (playerParty == null) { SetPlayer(body); }
                if (CheckChaseSight()) { StartChase(); }
            }
        }
        
        private void OnNavigationFinished()
        {
            // GD.Print("Nav finished");
            EmitSignal(SignalName.onEndEventStep, npcInteract); // -> MapEventScript
        }
    }
}