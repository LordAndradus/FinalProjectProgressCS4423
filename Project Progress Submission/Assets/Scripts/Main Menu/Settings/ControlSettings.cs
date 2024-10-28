using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ControlSettings : Settings, ISettings
{
    public GlobalSettings.Wrapper.Controls settings = new();

    List<Button> GridView = new();
    int waitingIndex = -1;

    [Header("Object Handler")]
    [SerializeField] GameObject ControlMenu;
    [SerializeField] GameObject KeyPressWindow;
    [SerializeField] GameObject eventSystem;

    public void Awake()
    {
        if(ControlMenu == null) return;

        Button[] buttons = ControlMenu.GetComponentsInChildren<Button>();

        foreach (Button b in buttons) 
        {
            if(!b.CompareTag("ActiveButtonHolder")) continue;

            Button[] children = b.GetComponentsInChildren<Button>();

            foreach (Button child in children)
            {   
                if(!child.CompareTag("ButtonSwapper")) continue;

                GridView.Add(child);

                child.onClick.AddListener(() => {
                    if(waitingIndex != -1) return;

                    waitingIndex = GridView.IndexOf(child);

                    eventSystem.SetActive(false);

                    KeyPressWindow.SetActive(true);
                });
            }
        }
    }

    public void Update()
    {
        if(waitingIndex >= 0)
        {
            foreach(KeyCode key in Enum.GetValues(typeof(KeyCode)))
            {
                if(Input.GetKey(KeyCode.Mouse0)) continue;
                if(Input.GetKey(KeyCode.Escape))
                {
                    CloseOperation();
                    break;
                }

                if(Input.GetKey(key))
                {
                    if(settings.ControlMap.ContainsValue(key)) 
                    {
                        SettingKey k = settings.ControlMap.getKey(key);

                        if(settings.ControlMap[SettingKey.Up + waitingIndex] == key)
                        {
                            CloseOperation();
                            break;
                        }

                        KeyPressWindow.transform.Find("Inner panel").gameObject.transform.Find("OpFail").gameObject.GetComponent<TextMeshProUGUI>().text = 
                        "Key already exists: " + GridView[(int) k - (int) SettingKey.Up].transform.parent.name;

                        break;
                    }

                    Button b = GridView[waitingIndex];
                    b.GetComponentInChildren<TextMeshProUGUI>().text = key.ToString();
                    settings.ControlMap[SettingKey.Up + GridView.IndexOf(b)] = key;

                    CloseOperation();
                    sameSettingsCheck();
                }
            }
        }
    }

    public void Start()
    {
        Json = "Controls.json";
    }

    public override void load(string data)
    {
        settings = JsonUtility.FromJson<GlobalSettings.Wrapper.Controls>(data);
    }

    public override string save()
    {
        if(GlobalSettings.GameplayBool[SettingKey.Debug]) return JsonUtility.ToJson(settings, true);
        return JsonUtility.ToJson(settings);
    }
    
    public override void reseatUI()
    {
        setSlider("Camera Speed", settings.PanSpeed, "Camera Speed: " + settings.PanSpeed); 

        foreach(Button b in GridView) b.GetComponentInChildren<TextMeshProUGUI>().text = new string(settings.ControlMap[SettingKey.Up + GridView.IndexOf(b)].ToString());
    }

    public override void syncSettings()
    {
        settings.ControlMap = new(GlobalSettings.ControlMap);
        settings.PanSpeed = GlobalSettings.PanSpeed;
    }

    public override void sameSettingsCheck()
    {
        bool flag = GlobalSettings.ControlMap.OrderBy(kv => kv.Key).SequenceEqual(settings.ControlMap.OrderBy(kv => kv.Key)) 
        && GlobalSettings.PanSpeed == settings.PanSpeed;

        if(flag) hideApply();
        else showApply();
    }

    public void checkSlider(float value)
    {
        GameObject go = EventSystem.current.currentSelectedGameObject;

        if(go.name.Equals("Camera Speed")) 
        {
            settings.PanSpeed = value;
            go.GetComponentInChildren<TextMeshProUGUI>().text = "Camera Speed: " + value;
        }

        sameSettingsCheck();
    }

    public void CloseOperation()
    {
        Debug.Log("Closing popup");
        eventSystem.SetActive(true);
        KeyPressWindow.transform.Find("Inner panel").gameObject.transform.Find("OpFail").GetComponent<TextMeshProUGUI>().text = "";
        waitingIndex = -1;
        KeyPressWindow.SetActive(false);
    }
}
