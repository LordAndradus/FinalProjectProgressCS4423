using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSettings : Settings, ISettings
{
    public GlobalSettings.Wrapper.Audio settings = new();

    public void Start()
    {
        Json = "Audio.json";
    }

    public override void load(string data)
    {
        settings = JsonUtility.FromJson<GlobalSettings.Wrapper.Audio>(data);
    }

    public override string save()
    {
        if(GlobalSettings.GameplayBool[SettingKey.Debug]) return JsonUtility.ToJson(settings, true);
        return JsonUtility.ToJson(settings);
    }
    
    public override void reseatUI()
    {
        
    }

    public override void syncSettings()
    {
        
    }

    public override void sameSettingsCheck()
    {
        
    }
}
