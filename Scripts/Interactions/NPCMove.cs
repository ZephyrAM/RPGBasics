using Godot;
using System;
using System.Collections.Generic;

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
        [Export] private PackedScene battleGroup = null;

        [ExportGroup("Nodes")]
        [Export] private Interactable npcInteract = null;
        [Export] private CharacterBody2D charBody = null;
        [Export] private Sprite2D charSprite = null;
        [Export] private CollisionShape2D charCollider = null;
        [Export] private Area2D sightArea = null;
        [Export] private NavigationAgent2D navAgent = null;
        [Export] private RayCast2D checkRay = null;
        // [Export] private ShapeCast2D checkShape = null;
        [Export] private AnimationTree animTree = null;

        private AnimationNodeStateMachinePlayback animPlay = null;

        private Node2D playerParty = null;
        // private List<RayCast2D> checkArray = [];

        private bool shouldChasePlayer = false;
        private float chaseTimer = 0;

        private bool doesMove = true;
        private float waitTimer = 0;
        private float moveTimer = 0;
        private Vector2 moveDirection = Vector2.Zero;
        private Vector2 oldDirection = Vector2.Zero;
        private Vector2 returnPosition = Vector2.Zero;
        private Vector2 blockedDirection = Vector2.Zero;

        Vector2 charSize;

        // Delegate Events \\
        [Signal]
        public delegate void onEndEventStepEventHandler(Interactable interactor);
        [Signal]
        public delegate void onCatchPlayerEventHandler(PackedScene battle);

        //=============================================================================
        // SECTION: OnReady Methods
        //=============================================================================

        public override void _Ready()
        {
            IfNull();
            SubSignals();

            npcInteract.SetInteractPhase(ConstTerm.WAIT);
            
            shouldChasePlayer = npcInteract.GetShouldChase();
            if (!shouldChasePlayer) { DisableChaseArea(); }
        }

        // public override void _ExitTree()
        // {
        //     UnSubSignals();
        // }

        public override void _PhysicsProcess(double delta)
        {
            PhaseCheck();
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
            checkRay ??= charBody.GetNode<RayCast2D>(ConstTerm.RAYCAST2D);
            // checkShape ??= charBody.GetNode<ShapeCast2D>(ConstTerm.SHAPECAST2D);

            animPlay ??= (AnimationNodeStateMachinePlayback)animTree.Get(ConstTerm.PARAM + ConstTerm.PLAYBACK);

            charSize = new Vector2(charSprite.Texture.GetWidth() / charSprite.Hframes, charSprite.Texture.GetHeight() / charSprite.Vframes);
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
            if (!sightArea.OverlapsBody(GetChaseTarget())) { // EDIT: Create raycast to chase target. Out of sight if broken.
                chaseTimer++;
                if (chaseTimer > lostSightTimer) {
                    chaseTimer = 0;
                    npcInteract.SetInteractPhase(ConstTerm.RETURN);
                    return;
                }
            } else { chaseTimer = 0; }

            navAgent.TargetPosition = playerParty.GlobalPosition;

            if (charBody.GetLastSlideCollision() != null) { npcInteract.SetInteractPhase(ConstTerm.RETURN); }// GD.Print(charBody.GetLastSlideCollision().GetCollider()); }
            // if () { // OnCatchCode // EDIT: Switch to collider, not navigation
            //     GD.Print("Caught player!");
            //     EmitSignal(SignalName.onCatchPlayer, battleGroup);
            //     QueueFree();         
            // }

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

        private void DoNothing()
        {
            moveDirection = Vector2.Zero;
            animPlay.Travel(ConstTerm.IDLE);
        }

        private void HearingCheck()
        {
            if (!shouldChasePlayer) { return; }
            
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
            Vector2 multi = moveDirection;

            if (moveDirection.Y != 0)
            {
                multi *= charSize.Y / 1.6f;
            }
            else if (moveDirection.X != 0)
            {
                multi *= charSize.X / 2f;
            }

            checkRay.TargetPosition = multi;

            checkRay.ForceRaycastUpdate();
            if (checkRay.IsColliding())
            { return true; }

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
            animTree.Set(ConstTerm.PARAM + ConstTerm.IDLE + ConstTerm.BLEND, moveDirection);

            sightArea.Rotation = moveDirection.Angle();
        }

        private void StartChase(CharacterBody2D target)
        {
            SetPlayer(target);
            if (shouldChasePlayer)
            {
                returnPosition = charBody.GlobalPosition;
                npcInteract.SetInteractPhase(ConstTerm.CHASE);
            }
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

        public void SetChasePlayer(bool value)
        {
            shouldChasePlayer = value;
        }
        
        public Node2D GetChaseTarget()
        {
            return playerParty;
        }

        public void SetPlayer(CharacterBody2D player)
        {
            playerParty = player;
        }

        public NavigationAgent2D GetNavAgent()
        {
            return navAgent;
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
        }

        public void EnableChaseArea()
        {
            sightArea.ProcessMode = ProcessModeEnum.Inherit;
            sightArea.Visible = true;
        }

        //=============================================================================
        // SECTION: Signal Methods
        //=============================================================================

        private void OnBodyEntered(Node2D body)
        {
            if (npcInteract.GetInteractPhase() == ConstTerm.CHASE) { return; }

            if (body.GetScript().AsGodotObject() == playerControlTest){
                StartChase((CharacterBody2D)body);
            }
        }
        
        private void OnNavigationFinished()
        {
            // GD.Print("Nav finished");
            EmitSignal(SignalName.onEndEventStep, npcInteract); // -> MapEventScript
        }
    }
}