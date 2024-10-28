public class ConstTerm
{
    //=============================================================================
    // SECTION: Animations
    //=============================================================================
    public const string ANIM_ATTACK = "battle_anim/attack";
    public const string ANIM_DEFEND = "battle_anim/defend";
    public const string ANIM_CAST = "battle_anim/spell_cast";
    public const string ANIM_HEAL = "battle_anim/heal";
    public const string ANIM_RETURN = "battle_anim/return_position";
    public const string TAKE_DAMAGE = "battle_anim/take_damage";
    public const string ANIM_ENEMY_ATTACK = "enemy_anim/attack";

    public const string CURSOR_BOUNCE = "ui_cursor/cursor_bounce";
    public const string CURSOR_BLINK = "ui_cursor/cursor_blink";
    public const string FADE_UP = "fade_up";

    public const string FADE_OUT = "fade_out";
    public const string FADE_IN = "fade_in";

    public const string IDLE = "Idle";
    public const string WALK = "Walk";
    public const string RUN = "Run";


    //=============================================================================
    // SECTION: Input Keys
    //=============================================================================
    public const string ACCEPT = "Accept";
    public const string CANCEL = "Cancel";
    
    public const string UP = "Up";
    public const string DOWN = "Down";
    public const string LEFT = "Left";
    public const string RIGHT = "Right";

    public const string HORIZ = "Horizontal";
    public const string VERT = "Vertical";


    //=============================================================================
    // SECTION: Signals
    //=============================================================================
    public const string ANIM_FINISHED = "animation_finished";
    public const string BATTLE_FINISHED = "onBattleFinish";
    public const string TRANSITION_FINISHED = "onTransitionFinished";
    public const string TWEEN_FINISHED = "finished";


    //=============================================================================
    // SECTION: Damage Types
    //=============================================================================
    public const string PHYSICAL = "Physical";
    public const string MAGICAL = "Magical";
    public const string HEALING = "Healing";


    //=============================================================================
    // SECTION: Paths
    //=============================================================================
    public const string GAME_FOLDER = "./";
    public const string SAVE_PATH = "";
    public const string SAVE_FOLDER = "saves/";
    public const string SAVE_FILE = "saveTeset";
    public const string SAVE_TYPE = ".tres";

    public const string PARAM = "parameters/";
    public const string BLEND = "/blend_position";
    public const string PLAYBACK = "playback";
    public const string BATTLE_SCENE = "res://Scenes/BattleScene.tscn";
    public const string NEWGAME_SCENE = "res://Scenes/MapSysem.tscn"; // EDIT: Change to default scene!!


    //=============================================================================
    // SECTION: Targetting
    //=============================================================================
    public const string ALLY = "Ally";
    public const string ENEMY = "Enemy";
    public const string SELF = "Self";
    
    public const string SINGLE = "Single";
    public const string GROUP = "Group";


    //=============================================================================
    // SECTION: Turn Phases
    //=============================================================================
    public const string WAIT = "Wait";
    public const string COMMAND = "Command";
    public const string ATTACK = "Attack";
    public const string DEFEND = "Defend";
    public const string SKILL_SELECT = "SkillSelect";
    public const string SKILL_USE = "SkillUse";
    public const string ITEM_SELECT = "ItemSelect";
    public const string ITEM_USE = "ItemUse";


    //=============================================================================
    // SECTION: Input Phases
    //=============================================================================
    public const string MOVE = "Move";
    public const string INTERACT = "Interact";
    public const string TEXT = "Text";
    public const string CHOICE = "Choice";
    public const string MENU = "Menu"; // Likely get broken down further


    //=============================================================================
    // SECTION: Colors
    //=============================================================================
    public const string WHITE = "ffffff";
    public const string GREEN = "2cca6d";
    public const string GREY  = "a1a1a1";


    //=============================================================================
    // SECTION: Groups
    //=============================================================================
    public const string SAVEDATA = "SaveData";


    //=============================================================================
    // SECTION: Method Calls
    //=============================================================================
    public const string ON_SAVEGAME = "OnSaveGame";
    public const string TARGETINTERACT = "TargetInteraction";
    public const string UPDATEINTERACT = "UpdateInteraction";


    //=============================================================================
    // SECTION: Variables
    //=============================================================================
    public const string BATTLERPLAYER = "Player";
    public const string BATTLERENEMY = "Enemy";
    public const string NPC = "NPC";

    public const string ITEM = "Item";
    public const string SELECT = "Select";
    public const string CONTINUE = "Continue";

    public const string LIST = "List";
    public const string PANEL = "Panel";
    public const string CONTAINER = "Container";

    public const string TEXTBOX = "TextBox";
    public const string CHOICEBOX = "ChoiceBox";


    //=============================================================================
    // SECTION: Node Names
    //=============================================================================
    public const string LABEL = "Label";
    public const string MARGIN_CONTAINER = "MarginContainer";
    public const string HBOX_CONTAINER = "HBoxContainer";
    public const string VBOX_CONTAINER = "VBoxContainer";

    public const string TEXT_START = "Start";
    public const string TEXT_END = "End";

    public const string CHARBODY2D = "CharacterBody2D";
    public const string SPRITE2D = "Sprite2D";
    public const string COLLIDER2D = "CollisionShape2D";
    public const string ANIM_PLAYER = "AnimationPlayer";
    public const string ANIM_TREE = "AnimationTree";

    public const string CANVAS_LAYER = "CanvasLayer";
    public const string RAYCAST2D = "RayCast2D";
    public const string CAMERA2D = "Camera2D";
    public const string COLOR_RECT = "ColorRect";

    public const string NAMELABEL = "Name";
    public const string HEALTH_BAR = "HealthBar";
    public const string BATTLE_UI = "BattleUI";


    //=============================================================================
    // SECTION: Script Nodes
    //=============================================================================
    public const string MAPSYSTEM = "MapSystem";
    public const string PARTYMANAGER = "PartyManager";
    public const string BATTLER = "Battler";
    public const string HEALTH = "Health";
    public const string BASESTATS = "BaseStats";
    public const string EXPERIENCE = "Experience";
    public const string SKILL_LIST = "SkillList";

    public const string HEALTH_DISPLAY = "HealthDisplay";

    public const string ABILITIES = "Abilities";
    public const string ABILITYDATABASE = "AbilityDatabase";
    public const string STATES = "States";
    public const string STATEDATABASE = "StateDatabase";
    public const string ITEMS = "Items";
    public const string ITEMDATABASE = "ItemDatabase";


    //=============================================================================
    // SECTION: Node Properties
    //=============================================================================
    public const string PERCENT_VISIBLE = "visible_ratio";
}