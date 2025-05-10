public class ConstTerm
{
    public const string ENCRYPT_KEY = "_*RPGBasics*_";

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
    public const string CFG = ".cfg";

    public const string PARAM = "parameters/";
    public const string BLEND = "/blend_position";
    public const string PLAYBACK = "playback";
    public const string BATTLE_SCENE = "res://Scenes/BattleScene.tscn";
    public const string NEWGAME_SCENE = "res://Scenes/MapSysem.tscn"; // EDIT: Change to default scene!!
    public const string MAP_SCENE = "res://Scenes/Maps/";
    public const string MOUSE_TARGET = "res://Resources/UI/Nodes/mouse_target.tscn";
    //=============================================================================
    // SECTION: Node Properties
    //=============================================================================
    public const string PERCENT_VISIBLE = "visible_ratio";
    public const string COLUMNS = "columns";
    public const string COLLIDER_ID = "collider_id";


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
    public const string COLOR_DECREASE = "ff4044";
    public const string COLOR_INCREASE = "00ae6a";


    //=============================================================================
    // SECTION: Groups
    //=============================================================================
    public const string DATA = "Data";
    public const string SYSTEM = "System";
    public const string INVENTORY = "Inventory";


    //=============================================================================
    // SECTION: Method Calls
    //=============================================================================
    public const string ON_SAVE = "OnSave";
    public const string ON_LOAD = "OnLoad";
    public const string GAME = "Game";
    public const string FILE = "File";

    public const string TARGETINTERACT = "TargetInteraction";
    public const string UPDATEINTERACT = "UpdateInteraction";


    //=============================================================================
    // SECTION: Variables
    //=============================================================================
    public const string PLAYER = "Player";
    public const string NPC = "NPC";
    public const string TEAM = "Team";
    public const string PARTY = "Party";
    public const string EQUIP = "Equip";

    public const string HP = "HP";
    public const string MP = "MP";

    public const string USE = "Use";
    public const string SELECT = "Select";
    public const string CONTINUE = "Continue";

    public const string LIST = "List";
    public const string GRID = "Grid";
    public const string PANEL = "Panel";
    public const string MARGIN = "Margin";
    public const string CONTAINER = "Container";

    public const string TEXTBOX = "TextBox";
    public const string CHOICEBOX = "ChoiceBox";
    public const string COLLECTBOX = "CollectBox";

    public const string HORIZ = "Horizontal";
    public const string VERT = "Vertical";

    public const string CONTROLLER = "Controller";
    public const string EMPTY = "-Empty-";
    public const string COUNT = "Count";
    public const string EQUIPPED = "Equipped";


    //=============================================================================
    // SECTION: Node Names
    //=============================================================================
    public const string CANVAS_ITEM = "CanvasItem";
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
    public const string RESOURCE_DISPLAY = "ResourceDisplay";

    public const string ABILITY = "Ability";
    public const string STATE = "State";
    public const string CLASS = "Class";
    public const string ITEM = "Item";
    public const string WEAPON = "Weapon";
    public const string ARMOR = "Armor";
    public const string ACCESSORY = "Accessory";
    public const string DATABASE = "Database";

    public const string NPCMOVE = "NPCMove";


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
    // SECTION: Save Labels
    //=============================================================================
    public const string SAVED = "Saved";
    public const string LOADING = "Loading";
    public const string INDEX = "Index";

    public const string RESERVE = "Reserve";
    public const string POSITION = "Position";
    public const string DIRECTION = "Direction";
    public const string CURRENCY = "Currency";

    public const string CURRENT = "Current";
    public const string STAT_VALUES = "StatValues";
    public const string LEVEL = "Level";

    public const string DESCRIPTION = "Description";
    public const string TYPE = "Type";
    public const string AREA = "Area";
    public const string NUMERIC = "Numeric";
    public const string COST = "Cost";
    public const string VALUE = "Value";
    public const string DAMAGE = "Damage";
    public const string CALL_ANIM = "CallAnimation";
    
    public const string GEAR_SLOTS = "GearSlots";
    public const string IS = "Is";
    public const string ID = "ID";
    public const string UNIQUE = "Unique";

    public const string ADD = "Add";
    public const string PERCENT = "Percent";
    public const string MODIFIER = "Modifier";
    public const string EXISTS = "Exists";
    public const string USEABLE = "Useable";
    public const string IN = "In";
    public const string OUT_OF = "OutOf";
    public const string BATTLE = "Battle";
    public const string DEAD = "Dead";
    public const string CAN_STACK = "CanStack";

    public const string REPEAT_STEP = "RepeatStep";


    //=============================================================================
    // SECTION: Language Abbreviations
    //=============================================================================
    public const string EN = "EN";
    public const string JP = "JP";
}