using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class VideoSettings : Settings
{
    [SerializeField]
    public GlobalSettings.Wrapper.Video settings = new();

    public void Start()
    {
        Json = "Video.json";
    }

    public override void load(string data)
    {
        settings = JsonUtility.FromJson<GlobalSettings.Wrapper.Video>(data);
    }

    public override string save()
    {
        if(GlobalSettings.GameplayBool[SettingKey.Debug]) return JsonUtility.ToJson(settings, true);
        return JsonUtility.ToJson(settings);
    }
    
    public override void reseatUI()
    {
        int val = 0;

        switch(settings.Resolution.Second)
        {
            case 1080:
                val = 0;
                break;
            case 1200:
                val = 1;
                break;
            case 1440:
                val = 2;
                break;
            case 1600:
                val = 3;
                break;
            case 2160:
                val = 4;
                break;
        }

        setDropDown("Resolution", val);

        setToggle("V-Sync", settings.VideoBool[SettingKey.Vsync]);
        setToggle("Fullscreen", settings.VideoBool[SettingKey.Fullscreen]);
        setToggle("AnimateWater", settings.VideoBool[SettingKey.AnimateWater]);
        setToggle("AnimateTiles", settings.VideoBool[SettingKey.AnimateTiles]);
    }

    public override void syncSettings()
    {
        settings.VideoBool = new(GlobalSettings.VideoBool);
        settings.Resolution.copy(GlobalSettings.Resolution);

        Screen.SetResolution(GlobalSettings.Resolution.First, GlobalSettings.Resolution.Second, true);

        hideApply();
    }

    public void checkToggle(bool value)
    {
        string selected = EventSystem.current.currentSelectedGameObject.name;

        if(selected.Equals("V-Sync")) settings.VideoBool[SettingKey.Vsync] = value;
        if(selected.Equals("Fullscreen")) settings.VideoBool[SettingKey.Fullscreen] = value;
        if(selected.Equals("AnimateWater")) settings.VideoBool[SettingKey.AnimateWater] = value;
        if(selected.Equals("AnimateTiles")) settings.VideoBool[SettingKey.AnimateTiles] = value;

        sameSettingsCheck();
    }


    public void resolutionChange(int value)
    {
        switch(value)
        {
            case 0:
                settings.Resolution.set(1920, 1080);
                break;
            case 1:
                settings.Resolution.set(1920, 1200);
                break;
            case 2:
                settings.Resolution.set(2560, 1440);
                break;
            case 3:
                settings.Resolution.set(2560, 1600);
                break;
            case 4:
                settings.Resolution.set(3840, 2160);
                break;
        }

        sameSettingsCheck();
    }

    public override void sameSettingsCheck()
    {
        bool flag = GlobalSettings.VideoBool.OrderBy(kv => kv.Key).SequenceEqual(settings.VideoBool.OrderBy(kv => kv.Key)) 
        && settings.Resolution.equals(GlobalSettings.Resolution);

        if(flag) hideApply();
        else showApply();
    }
}
