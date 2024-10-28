using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameplaySettings : Settings
{
    public GlobalSettings.Wrapper.Gameplay settings = new();

    public void Start()
    {
        Json = "Game.json";
    }

    public override void load(string data)
    {
        settings = JsonUtility.FromJson<GlobalSettings.Wrapper.Gameplay>(data);
    }

    public override string save()
    {
        if(GlobalSettings.GameplayBool[SettingKey.Debug]) return JsonUtility.ToJson(settings, true);
        return JsonUtility.ToJson(settings);
    }

    public override void reseatUI()
    {
        setToggle("Instant Movement Toggle", settings.GameplayBool[SettingKey.InstantMovement]);
        setToggle("Map Grid Toggle", settings.GameplayBool[SettingKey.MapGrid]);
        setToggle("Tutorial Toggle", settings.GameplayBool[SettingKey.Tutorial]);
        setToggle("Skip Battle Toggle", settings.GameplayBool[SettingKey.SkipBattle]);
    }

    public override void syncSettings()
    {
        settings.GameplayBool = new(GlobalSettings.GameplayBool);
        hideApply();
    }

    public void checkToggle(bool value)
    {
        string name = EventSystem.current.currentSelectedGameObject.name;

        if(name.Equals("Instant Movement Toggle")) settings.GameplayBool[SettingKey.InstantMovement] = value;
        if(name.Equals("Map Grid Toggle")) settings.GameplayBool[SettingKey.MapGrid] = value;
        if(name.Equals("Tutorial Toggle")) settings.GameplayBool[SettingKey.Tutorial] = value;
        if(name.Equals("Skip Battle Toggle")) settings.GameplayBool[SettingKey.SkipBattle] = value;

        sameSettingsCheck();
    }

    public override void sameSettingsCheck()
    {
        bool flag = GlobalSettings.GameplayBool.OrderBy(kv => kv.Key).SequenceEqual(settings.GameplayBool.OrderBy(kv => kv.Key));

        if(!flag) showApply();
        else hideApply();
    }
}
