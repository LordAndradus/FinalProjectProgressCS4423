[System.Serializable]
public enum SettingKey
{
    //Not a normal setting
    Debug,

    //Gameplay Booleans
    InstantMovement, MapGrid, Tutorial, SkipBattle,

    //Video Booleans
    Vsync, Fullscreen, AnimateWater, AnimateTiles,

    //Movement controls
    Up, UpS, Down, DownS, Left, LeftS, Right, RightS,

    //Shortcut controls
    QuickSave, QuickLoad, AltLeftClick, FastPan, Objective, Information, Map, 

    //Hardcoded controls
    Escape, SkipCutscene,

    //Mouse controls
    LeftClick, RightClick, MiddleClick, AltPan, Pan,

    ControlSchema
}

public enum CameraKey
{
    targetZoom, minZoom, maxZoom, zoomSpeed
}

public static class SettingKeyString
{
    public static string toString(SettingKey key)
    {
        return key.ToString();
    }
}

