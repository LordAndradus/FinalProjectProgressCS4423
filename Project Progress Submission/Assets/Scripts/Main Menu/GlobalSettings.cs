using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GlobalSettings : MonoBehaviour
{
    /* TODO list
     * When hovering over certain options, pop a small window with a description
     * Prompt the user that there are unsaved changes on click back button
    */

    public static string KeyPartOne = "My langga is ";

    public static string DefaultUnitSpriteView = "Sprites/Sample_Unit";
    public static string DefaultUnitSpriteIcon = "Sprites/SpriteDefaultIcon";
    public static string UnknownQuestionMarkIcon = "Sprites/UI/Question Mark";

    public static SerializableDictionary<SettingKey, bool> GameplayBool = new(){
        {SettingKey.InstantMovement, false}, {SettingKey.Tutorial, true}, {SettingKey.MapGrid, false},
        {SettingKey.SkipBattle, false}, 
        
        //Not a normal setting, duh
        {SettingKey.Debug, false}
    };

    public static float UnitMoveSpeed = 8f;

    //Important settings - Video
    public static Pair<int, int> Resolution = new(1920, 1080);
    public static SerializableDictionary<SettingKey, bool> VideoBool = new(){
        {SettingKey.Vsync, true}, {SettingKey.Fullscreen, true}, {SettingKey.AnimateWater, true}, {SettingKey.AnimateTiles, true}
    };

    //Important settings - Audio
    public static SerializableDictionary<SettingKey, object> AudioSettings = new(){

    };

    //Important settings - Controls
    public static float PanSpeed = 10.0f;
    public static SerializableDictionary<SettingKey, KeyCode> ControlMap = new(){
        {SettingKey.Up, KeyCode.W}, {SettingKey.Down, KeyCode.S}, {SettingKey.Right, KeyCode.D}, {SettingKey.Left, KeyCode.A},
        {SettingKey.UpS, KeyCode.UpArrow}, {SettingKey.DownS, KeyCode.DownArrow}, {SettingKey.RightS, KeyCode.RightArrow}, {SettingKey.LeftS, KeyCode.LeftArrow},

        {SettingKey.AltLeftClick, KeyCode.Space}, {SettingKey.FastPan, KeyCode.LeftShift},

        {SettingKey.Objective, KeyCode.O}, {SettingKey.Information, KeyCode.I}, {SettingKey.Map, KeyCode.M},

        {SettingKey.QuickSave, KeyCode.F5}, {SettingKey.QuickLoad, KeyCode.F8}
    };

    //These might be hardcoded, but I haven't decided yet.
    public static SerializableDictionary<SettingKey, MouseButton> MouseMap = new(){
        {SettingKey.LeftClick, MouseButton.Left}, {SettingKey.RightClick, MouseButton.Right}, {SettingKey.MiddleClick, MouseButton.Middle}, 
        {SettingKey.AltPan, MouseButton.Right}, {SettingKey.Pan, MouseButton.Middle}
    };

    public static readonly SerializableDictionary<SettingKey, KeyCode> HardCodedKeys = new(){
        {SettingKey.Escape, KeyCode.Escape}, {SettingKey.SkipCutscene, KeyCode.Space},
    };

    public static SerializableDictionary<CameraKey, float> ZoomSettings= new(){
        {CameraKey.targetZoom, 5f}, {CameraKey.minZoom, 2f}, {CameraKey.maxZoom, 15f}, {CameraKey.zoomSpeed, 5f}
    };

    public static readonly Dictionary<SettingKey, string> descriptions = new(){
        {SettingKey.AnimateTiles, "This will toggle on animating tiles that have animations"}, {SettingKey.AnimateWater, "This will toggle on animating water tiles"},
        {SettingKey.InstantMovement, "This will toggle on whether or not to skip the units pathing to its destination"}, 
        //{SettingKey.MovementSpeed, "This will control how fast the unit will take to reach its destination"},

        {SettingKey.Tutorial, "There is a brief combat tutorial on a new game that teaches you how to move, attack, recruit, and capture objective points"},
        {SettingKey.MapGrid, "Toggles grid lines on the map. This will be useful for counting how many tiles to move"},
        {SettingKey.SkipBattle, "This will skip any battle animations and go straight to calculating damage, healing, and surrenders in an Attack"},
        //Any additional ambiguous settings will be defined here.
    };

    [SerializeField] GameplaySettings gset;
    [SerializeField] VideoSettings vset;
    [SerializeField] AudioSettings aset;
    [SerializeField] ControlSettings cset;

    public void Awake()
    {
        GameObject EngagedEditor = GameObject.Find("UnityEditorText");
        
        #if UNITY_EDITOR
            if(SceneManager.GetActiveScene().name.Equals("Main Menu")) EngagedEditor.SetActive(true);
            Settings.appdataPath = Path.Combine(Application.dataPath, "Debug Jsons");
            GameplayBool[SettingKey.Debug] = true;
        #else
            if(SceneManager.GetActiveScene().name.Equals("Main Menu")) EngagedEditor.SetActive(false);
            Settings.appdataPath = Application.persistentDataPath;
            GameplayBool[SettingKey.Debug] = false;
        #endif

        if(!Directory.Exists(Settings.appdataPath)) Directory.CreateDirectory(Settings.appdataPath);

        loadAll();

        Debug.Log(string.Format(@"Loaded global settings: 
        {0}
        {1}
        {2}
        {3}
        {4}
        {5}", GameplayBool.toString(), VideoBool.toString(), ControlMap.toString(), MouseMap.toString(), "Pan speed: " + PanSpeed, "Resolution = " + Resolution.ToString()));
    }

    public void setup()
    {
        Settings.setup();

        loadAll();

        Settings.hideApply();
    }

    GameObject lastTab;
    public void tabOpened()
    {
        string name = EventSystem.current.currentSelectedGameObject.name;

        if(name.Equals("GameplaySettings")) gset.tabOpened();
        if(name.Equals("VideoSettings")) vset.tabOpened();
        if(name.Equals("AudioSettings")) aset.tabOpened();
        if(name.Equals("ControlSettings")) cset.tabOpened();
    }

    public void saveAll()
    {
        //A delegate would probably be useful here. SaveAll?.invoke(); perhaps
        gset.saveSettings();
        vset.saveSettings();
        aset.saveSettings();
        cset.saveSettings();

        transferSettings();
    }

    public void loadAll()
    {
        if(gset == null) 
        {
            gset = transform.AddComponent<GameplaySettings>();
            gset.Json = "Game.json";
        }

        if(vset == null) 
        {
            vset = transform.AddComponent<VideoSettings>();
            vset.Json = "Video.json";
        }

        if(aset == null) 
        {
            aset = transform.AddComponent<AudioSettings>();
            aset.Json = "Audio.json";
        }

        if(cset == null) 
        {
            cset = transform.AddComponent<ControlSettings>();
            cset.Json = "Control.json";
        }

        gset.forceLoad();
        vset.forceLoad();
        aset.forceLoad();
        cset.forceLoad();

        transferSettings();
    }

    public void checkSync()
    {
        Debug.Log("Need to check if there are unapplied changes!");
    }

    private void transferSettings()
    {
        //Gameplay
        GameplayBool = new(gset.settings.GameplayBool);

        //Video
        Resolution.copy(vset.settings.Resolution);
        VideoBool = new(vset.settings.VideoBool);

        //Audio

        //Controls
        ControlMap = new(cset.settings.ControlMap);
        MouseMap = new(cset.settings.MouseMap);
        PanSpeed = cset.settings.PanSpeed;
    }

    [System.Serializable]
    public abstract class Wrapper
    {
        public static void printAllVariables(System.Object obj)
        {
            Type t = obj.GetType();

            FieldInfo[] fields = t.GetFields(BindingFlags.Public);

            foreach (FieldInfo field in fields) Debug.Log(field.Name + " = " + field.GetValue(null));
        }

        public class Gameplay : Wrapper
        {
            public SerializableDictionary<SettingKey, bool>GameplayBool;

            public Gameplay()
            {
                GameplayBool = new(GlobalSettings.GameplayBool);
            }
        }

        public class Video : Wrapper
        {
            public SerializableDictionary<SettingKey, bool> VideoBool;
            public Pair<int, int> Resolution;
            public Video()
            {
                VideoBool = new(GlobalSettings.VideoBool);
                Resolution = new(GlobalSettings.Resolution);
            }
        }

        public class Audio : Wrapper
        {
            public Audio()
            {

            }
        }

        public class Controls : Wrapper
        {
            public SerializableDictionary<SettingKey, KeyCode> ControlMap;
            public SerializableDictionary<SettingKey, MouseButton> MouseMap;

            public float PanSpeed;

            public Controls()
            {
                ControlMap = new(GlobalSettings.ControlMap);
                MouseMap = new(GlobalSettings.MouseMap);
                PanSpeed = GlobalSettings.PanSpeed;
            }
        }
    }
}


