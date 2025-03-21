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

    public const string AUDIO_FADE = "audio/audio_fade";


    //=============================================================================
    // SECTION: Input Keys
    //=============================================================================
    public const string ACCEPT = "Accept";
    public const string CANCEL = "Cancel";
    public const string MENU = "Menu";
    public const string PAUSE = "Pause";
    
    public const string UP = "Up";
    public const string DOWN = "Down";
    public const string LEFT = "Left";
    public const string RIGHT = "Right";

    public const string CLICK = "Click";


    //=============================================================================
    // SECTION: Signals
    //=============================================================================
    public const string ANIM_FINISHED = "animation_finished";
    public const string BATTLE_FINISHED = "onBattleFinish";
    public const string TRANSITION_FINISHED = "onTransitionFinished";
    public const string NAVIGATION_FINISHED = "navigation_finished";
    public const string FINISHED_SIGNAL = "finished";
    public const string PROCESS_FRAME = "process_frame";


    //=============================================================================
    // SECTION: Damage Types
    //=============================================================================
    public const string PHYSICAL = "Physical";
    public const string MAGICAL = "Magical";
    public const string HEALING = "Healing";


    //=============================================================================
    // SECTION: Paths
    //=============================================================================
    public const string FOLDER = "/";
    public const string GAME_FOLDER = "./";
    public const string SAVE_PATH = "";

    public const string LANG_FOLDER = "Lang/";
    public const string SAVE_FOLDER = "Saves/";
    public const string SAVE_FILE = "saveTest";
    public const string SAVE_TYPE = ".saveGame";

    public const string CFG_FILE = "config.cfg";
    public const string LANG_FILE = "InteractTextData.json";

    public const string TXT = ".txt";
    public const string TRES = ".tres";
    public const string TSCN = ".tscn";

    public const string PARAM = "parameters/";
    public const string BLEND = "/blend_position";
    public const string PLAYBACK = "playback";
    public const string BATTLE_SCENE = "res://Scenes/BattleScene.tscn";
    public const string NEWGAME_SCENE = "res://Scenes/MapSysem.tscn"; // EDIT: Change to default scene!!
    public const string MAP_SCENE = "res://Scenes/Maps/";
    public const string MOUSE_TARGET = "res://Resources/UI/Nodes/mouse_target.tscn";


    //=============================================================================
    // SECTION: Targetting
    //=============================================================================
    public const string ALLY = "Ally";
    public const string ENEMY = "Enemy";
    public const string SELF = "Self";
    public const string EITHER = "Either";
    public const string ALL = "All";
    
    public const string SINGLE = "Single";
    public const string GROUP = "Group";

    public const string TARGET = "Target";


    //=============================================================================
    // SECTION: Turn Phases
    //=============================================================================
    public const string WAIT = "Wait";
    public const string COMMAND = "Command";
    public const string ATTACK = "Attack";
    public const string DEFEND = "Defend";
    public const string SKILL = "Skill";


    //=============================================================================
    // SECTION: Menu Phases
    //=============================================================================
    public const string MEMBER = "Member";
    public const string STATUS = "Status";
    public const string STATUS_SCREEN = "StatusScreen";
    public const string SAVE = "Save";
    public const string LOAD = "Load";


    //=============================================================================
    // SECTION: Input Phases
    //=============================================================================
    public const string MOVE = "Move";
    public const string AXIS_MOVE = "AxisMove";
    public const string CLICK_MOVE = "ClickMove";
    public const string INTERACT = "Interact";
    public const string CHASE = "Chase";
    public const string RETURN = "Return";
    public const string MOVE_TO = "MoveTo";
    public const string DO_NOTHING = "DoNothing";

    public const string TEXT = "Text";
    public const string CHOICE = "Choice";

    public const string MOUSE = "Mouse";
    public const string KEY_GAMEPAD = "KeyboardGamepad";
    public const string CONFIG = "Config";
    public const string OPTIONS = "Options";
    public const string REBIND = "Rebind";


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
    public const string CONFIGDATA = "ConfigData";
    public const string BATTLERDATA = "BattlerData";
    public const string PARTYDATA = "PartyData";


    //=============================================================================
    // SECTION: Method Calls
    //=============================================================================
    public const string ON_SAVEGAME = "OnSaveGame";
    public const string ON_LOADGAME = "OnLoadGame";
    public const string ON_SAVECONFIG = "OnSaveConfig";
    public const string ON_LOADCONFIG = "OnLoadConfig";
    public const string TARGETINTERACT = "TargetInteraction";
    public const string UPDATEINTERACT = "UpdateInteraction";


    //=============================================================================
    // SECTION: Variables
    //=============================================================================
    public const string PLAYER = "Player";
    public const string NPC = "NPC";
    public const string TEAM = "Team";
    public const string PARTY = "Party";

    public const string USE = "Use";
    public const string SELECT = "Select";
    public const string CONTINUE = "Continue";

    public const string LIST = "List";
    public const string GRID = "Grid";
    public const string PANEL = "Panel";
    public const string CONTAINER = "Container";
    public const string COLUMNS = "Columns";

    public const string TEXTBOX = "TextBox";
    public const string CHOICEBOX = "ChoiceBox";
    public const string COLLECTBOX = "CollectBox";

    public const string HORIZ = "Horizontal";
    public const string VERT = "Vertical";

    public const string CONTROLLER = "Controller";


    //=============================================================================
    // SECTION: Node Names
    //=============================================================================
    public const string LABEL = "Label";
    public const string BUTTON = "Button";
    public const string MARGIN_CONTAINER = "MarginContainer";
    public const string HBOX_CONTAINER = "HBoxContainer";
    public const string VBOX_CONTAINER = "VBoxContainer";

    public const string START = "Start";
    public const string END = "End";

    public const string CHARBODY2D = "CharacterBody2D";
    public const string SPRITE2D = "Sprite2D";
    public const string COLLIDER2D = "CollisionShape2D";
    public const string COLLIDERPOLY2D = "CollisionPolygon2D";
    public const string NAVAGENT2D = "NavigationAgent2D";
    public const string ANIM_PLAYER = "AnimationPlayer";
    public const string ANIM_TREE = "AnimationTree";
    public const string CURSOR = "Cursor";

    public const string CANVAS_LAYER = "CanvasLayer";
    public const string AREA2D = "Area2D";
    public const string RAYCAST2D = "RayCast2D";
    public const string SHAPECAST2D = "ShapeCast2D";
    public const string CAMERA2D = "Camera2D";
    public const string COLOR_RECT = "ColorRect";

    public const string NAME = "Name";
    public const string HEALTH_BAR = "HealthBar";
    public const string BATTLE_UI = "BattleUI";

    public const string EVENTS = "Events";
    public const string INTERACT_TEXT = "InteractionText";
    public const string RAY_CHECK = "RayCheck";


    //=============================================================================
    // SECTION: Script Nodes
    //=============================================================================
    public const string MAPSYSTEM = "MapSystem";
    public const string PARTYMANAGER = "PartyManager";
    public const string MENU_CONTROLLER = "MenuController";

    public const string BATTLER = "Battler";
    public const string HEALTH = "Health";
    public const string BASESTATS = "BaseStats";
    public const string EXPERIENCE = "Experience";

    public const string HEALTH_DISPLAY = "HealthDisplay";

    public const string ABILITY = "Ability";
    public const string ABILITYDATABASE = "AbilityDatabase";
    public const string STATE = "State";
    public const string STATEDATABASE = "StateDatabase";
    public const string ITEM = "Item";
    public const string ITEMDATABASE = "ItemDatabase";
    public const string CLASSDATABASE = "ClassDatabase";

    public const string NPCMOVE = "NPCMove";


    //=============================================================================
    // SECTION: Node Properties
    //=============================================================================
    public const string PERCENT_VISIBLE = "visible_ratio";


    //=============================================================================
    // SECTION: Map Event Strings
    //=============================================================================
    public const string MAP = "Map";
    public const string EVENT = "Event";
    public const string STEP = "Step";
    public const string DIVIDER = ".";


    //=============================================================================
    // SECTION: Config Options
    //=============================================================================
    public const string AUDIO = "Audio";
    public const string MASTER = "Master";
    public const string BGM = "BGM";
    public const string SOUND = "Sound";
    public const string VOICE = "Voice";

    public const string GRAPHICS = "Graphics";
    public const string BORDERLESS = "Borderless";
    public const string RESOLUTION = "Resolution";

    public const string KEYBINDS = "Keybinds";


    //=============================================================================
    // SECTION: Language Abbreviations
    //=============================================================================
    public const string EN = "EN";
    public const string JP = "JP";
}