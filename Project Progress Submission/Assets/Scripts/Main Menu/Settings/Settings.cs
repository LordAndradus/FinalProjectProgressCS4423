using System.IO;
using TMPro;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour, ISettings
{
    public static string appdataPath;
    public string Json;

    //General purpose buttons
    static GameObject apply;

    public static void setup()
    {
        if(apply != null) return;
        apply = GameObject.Find("ApplyButton");
        hideApply();
    }

    public static void hideApply()
    {
        apply.SetActive(false);
    }

    public static void showApply()
    {
        apply.SetActive(true);
    }

    public bool saveSettings()
    {
        string savePath = Path.Combine(appdataPath, Json);

        string data = save();

        System.IO.File.WriteAllText(savePath, data);

        if(apply != null) hideApply();

        return System.IO.File.Exists(savePath);
    }

    public bool loadSettings()
    {
        string loadPath = Path.Combine(appdataPath, Json);

        if(!System.IO.File.Exists(loadPath)) return false;

        string data = System.IO.File.ReadAllText(loadPath);

        load(data);

        if(apply != null) hideApply();

        return true;
    }

    public void forceLoad()
    {
        if(!loadSettings()) if(saveSettings()) loadSettings();
    }

    public void tabOpened()
    {
        syncSettings();
        reseatUI();
        hideApply();
    }

    public virtual void syncSettings()
    {
        Debug.Log("Override this to reset settings in class!");
    }

    public virtual void load(string data)
    {
        Debug.Log("Override this settings loader!");
    }

    public virtual string save()
    {
        Debug.Log("Override this settings saver!");
        return null;
    }

    /* public virtual string save()
    {
        ISettings[] childComponents = GetComponentsInChildren<ISettings>();

        string result = "";

        foreach(var comp in childComponents) if(comp != this) result = comp.save();
        
        Debug.Log("Make sure to provide the right wrapper!");
        return result;
    } */

    public virtual void reseatUI() //Yes, reseat, because we're seating the UI values again
    {
        Debug.Log("Make sure to re-seat the UI elements here!");
    }

    //If even one of the settings is false on an equality check, then set apply to be shown
    public virtual void sameSettingsCheck()
    {
        Debug.Log("Make sure you check if there are any changes!");
    }
    
    private protected void setToggle(string GameObjectName, bool reference)
    {
        GameObject.Find(GameObjectName).GetComponent<Toggle>().isOn = reference;
    }

    private protected void setSlider(string GameObjectName, float reference)
    {
        GameObject.Find(GameObjectName).GetComponent<Slider>().value = reference;
    }

    private protected void setSlider(string GameObjectName, float reference, string text)
    {
        setSlider(GameObjectName, reference);
        GameObject.Find(GameObjectName).GetComponentInChildren<TextMeshProUGUI>().text = text;
    }

    private protected void setDropDown(string GameObjectName, int reference)
    {
        GameObject.Find(GameObjectName).GetComponent<TMPro.TMP_Dropdown>().value = reference;
    }

    private protected void setButton(string GameObjectName, string reference)
    {
        GameObject.Find(GameObjectName).GetComponent<Button>().GetComponentInChildren<TMPro.TextMeshProUGUI>().text = reference;
    }
}