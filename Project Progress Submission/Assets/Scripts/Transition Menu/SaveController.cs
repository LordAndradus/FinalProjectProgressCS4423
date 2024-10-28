using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SaveController : MonoBehaviour
{
    [Header("External Systems")]
    [SerializeField] GameObject MainMenu;
    [SerializeField] MainController mc;
    [SerializeField] EventSystem eventSystem;
    [SerializeField] Button BackButton;

    [Header("Parent Objects")]
    [SerializeField] GameObject SaveList;
    [SerializeField] GameObject SaveInformation;
    [SerializeField] GameObject SaveRoster;
    [SerializeField] GameObject SaveDescription;

    [Header("Save List Content")]
    [SerializeField] GameObject FileContent;

    [Header("Save Information")]
    [SerializeField] TextMeshProUGUI ChapterTitle;
    [SerializeField] TextMeshProUGUI BasicInfo;
    [SerializeField] TextMeshProUGUI DifficultyModifiers;
    [SerializeField] GameObject GameMetrics;

    [Header("Save Roster Content")]
    [SerializeField] GameObject RosterContent;

    [Header("Save Information")]
    string RootSaveDirectory;
    [SerializeField] GameObject ActiveSaveInteractable;
    [SerializeField] List<string> SaveFiles;
    [SerializeField] string ActiveSave;
    [SerializeField] SaveFileWrapper ActiveSFW;
    List<Unit> UnitList;
    List<Squad> SquadList;

    public enum SaveOp {save, load}
    public SaveOp operation = SaveOp.save;

    private void Awake()
    {
        BackButton.onClick.AddListener(BackToMenu);

        #if UNITY_EDITOR
            RootSaveDirectory = Path.Combine(Application.dataPath, "Save Files");
        #else
            RootSaveDirectory = Path.Combine(Application.persistentDataPath, "Save Files");
        #endif

        FileContent.transform.GetChild(0).AddComponent<InteractableObject>().LeftClickEvent += () => {
            SaveCurrentInformation();
        };
    }

    private void OnEnable()
    {
        LoadSaveFiles();
        SetActiveFile(0);

        RosterContent.transform.localPosition = Vector3.zero;
        FileContent.transform.localPosition = Vector3.zero;
    }

    private void Update()
    {
        if(Input.GetKeyUp(GlobalSettings.HardCodedKeys[SettingKey.Escape])) BackToMenu();
    }

    private void BackToMenu()
    {
        transform.gameObject.SetActive(false);
        MainMenu.SetActive(true);   
    }

//*****************************************************************************************************************************//
//General Purpose
    private void LoadSaveFiles()
    {   
        if(!Directory.Exists(RootSaveDirectory))
        {
            Directory.CreateDirectory(RootSaveDirectory);
            Debug.LogError("Created root save directory @: " + RootSaveDirectory);
            return;
        }

        SaveFiles.Clear();

        string[] files = Directory.GetFiles(RootSaveDirectory);
        foreach(string file in files) if(Path.GetExtension(file).Equals(".json")) SaveFiles.Add(Path.GetFileName(file));

        foreach(Transform child in FileContent.transform.Cast<Transform>().Skip(1)) Destroy(child.gameObject);
        FileContent.transform.GetChild(0).gameObject.SetActive(operation == SaveOp.save);

        //Set new save files in list
        int index = 0;
        foreach(string file in SaveFiles)
        {
            int InstanceIndex = index++;
            //Create prefab Save Button
            GameObject SaveFile = UtilityClass.CreatePrefabObject("Assets/PreFabs/TransitionMenu/Save Interface/NormalSaveSlot.prefab", FileContent.transform);
            SaveFile.name = file;
            
            SaveFileWrapper LoadInfo = JsonUtility.FromJson<SaveFileWrapper>(Cryptographer.Decrypt(System.IO.File.ReadAllText(Path.Combine(RootSaveDirectory, file)), GlobalSettings.KeyPartOne + Cryptographer.KeyPartTwo));

            //Set text
            TextMeshProUGUI[] text = SaveFile.GetComponentsInChildren<TextMeshProUGUI>();
            text[0].text = file;
            text[1].text = LoadInfo.SaveFileDateTime;
            text[2].text = "Chapter ##: <Placeholder name>";
            text[3].text = "Army: " + LoadInfo.ArmyCount + "/" + LoadInfo.ArmyMax;

            InteractableObject io = SaveFile.GetComponent<InteractableObject>();

            //OnLeftClick -> Check if it's the current active save file, if so then save current information into it
            io.LeftClickEvent += () => {
                if(ActiveSaveInteractable.Equals(SaveFile))
                {
                    //Debug.Log(string.Format("{0} information", operation == SaveOp.save ? "Saving" : "Loading"));
                    if(operation == SaveOp.save) SaveCurrentInformation();
                    else LoadCurrentInformation(file);
                    return;
                }

                ActiveSaveInteractable = SaveFile;
            };

            //OnEnter -> Set Appropriate information
            io.OnEnter += () => {
                //Debug.Log("Setting save file" + InstanceIndex);
                SetActiveFile(InstanceIndex);
            };

            //OnRightClick -> Bring up context menu
            io.RightClickEvent += () => {
                Debug.LogWarning("Need to implement context menu! For now, me delete");
                Debug.LogError("Name of file: " + SaveFiles[InstanceIndex]);
                System.IO.File.Delete(Path.Combine(RootSaveDirectory, SaveFiles[InstanceIndex]));
                LoadSaveFiles();
            };
        }
    }

    private void SetActiveFile(int index)
    {
        if(index < 0 || index > SaveFiles.Count)
        {
            Debug.LogErrorFormat("Passed erroneous index.\nIndex passed = {0}\nSize of Paths = {1}", index, SaveFiles.Count);
            throw new System.Exception("IndexOutOfBounds: Index was out of bounds");
        }

        if(SaveFiles.Count == 0)
        {
            ActiveSFW = CreateSaveFileWrapper();
        }
        else
        {
            ActiveSave = SaveFiles[index];
            string data = Cryptographer.Decrypt(System.IO.File.ReadAllText(Path.Combine(RootSaveDirectory, SaveFiles[index])), GlobalSettings.KeyPartOne + Cryptographer.KeyPartTwo);
            ActiveSFW = JsonUtility.FromJson<SaveFileWrapper>(data);
        }
        
        if(ActiveSFW == null)
        {
            Debug.LogError("Could not load Save file!");
            return;
        }

        RetrieveSaveInfo();
        UpdateUnitRoster();
        UpdateStoryDescriptor();
    }

//*****************************************************************************************************************************//
//Loading Save Information
    private void RetrieveSaveInfo()
    {
        Debug.Log("Need to update save information");
    }

    private void UpdateUnitRoster()
    {
        //Delete existing unit roster
        foreach(Transform child in RosterContent.transform) Destroy(child.gameObject);

        //Update unit roster with new information from save file
        int index = 0;
        foreach(Unit unit in ActiveSFW.units)
        {
            GameObject UnitButton = PopulateRoster(unit, index++);
        }

        //Iterate through squad's as well and add them to the save roster
        foreach(Squad squad in ActiveSFW.squads)
        {
            GameObject SquadObject = new(squad.Name);
            SquadObject.transform.SetParent(RosterContent.transform, false);            
            SquadObject.transform.position = new Vector3(SquadObject.transform.position.x, SquadObject.transform.position.y, 0f);

            TextMeshProUGUI SquadName = SquadObject.AddComponent<TextMeshProUGUI>();
            SquadName.text = squad.Name;
            SquadName.alignment = TextAlignmentOptions.Center;

            SquadObject.GetComponent<RectTransform>().sizeDelta = new Vector2(RosterContent.GetComponent<RectTransform>().sizeDelta.x, 50f);

            index = 0;
            foreach(Unit unit in squad.RetrieveUnits())
            {
                GameObject UnitButton = PopulateRoster(unit, index++);
            }
        }
    }

    GameObject PopulateRoster(Unit unit, int index)
    {
        GameObject UnitButton = UtilityClass.CreatePrefabObject("Assets/PreFabs/TransitionMenu/Army Management/UnitButton.prefab", 
        RosterContent.transform, "Unit " + index);
        UnitButton.transform.localScale = Vector3.one * 1.3f;

        TextMeshProUGUI[] text = UnitButton.GetComponentsInChildren<TextMeshProUGUI>();
        text[0].text = unit.displayQuickInfo();
        text[1].text = unit.UIFriendlyClassName;
        UnitButton.transform.GetChild(2).GetComponent<Image>().sprite = unit.spriteView;
        return UnitButton;
    }

    private void UpdateStoryDescriptor()
    {
        Debug.Log("Need to load save files description");
    }

//*****************************************************************************************************************************//
//Actual saving

    private void SaveCurrentInformation()
    {
        SaveFileWrapper sfw = CreateSaveFileWrapper();

        //Use JsonUtility to save information
        string FileName = string.Format("Save {0:D3} - {1}.json", SaveFiles.Count, "Placeholder Name");
        System.IO.File.WriteAllText(Path.Combine(RootSaveDirectory, FileName), Cryptographer.Encrypt(JsonUtility.ToJson(sfw, true), GlobalSettings.KeyPartOne + Cryptographer.KeyPartTwo));
        SaveFiles.Add(FileName);

        LoadSaveFiles();
    }

    private SaveFileWrapper CreateSaveFileWrapper()
    {
        SaveFileWrapper sfw = new();

        //Save Current time and date
        sfw.SaveFileDateTime = DateTime.Now.ToString("MM/dd/yyyy - HH:mm:ss");

        //Save Current Army Size out of max
        sfw.ArmyCount = mc.UnitList.Count;
        foreach(Squad squad in mc.SquadList)
        {
            sfw.ArmyCount += squad.RetrieveUnits().Count;
        }

        //Save Unit Information from MainController
        sfw.units = mc.UnitList.ToList<Unit>();

        //Save Squad Information from MainController
        sfw.squads = mc.SquadList.ToList<Squad>();

        //Save current chapter information

        //Save player difficulty settings

        //Save metrics

        return sfw;
    }

    private void LoadCurrentInformation(string file)
    {
        //Essentially the opposite of saving
        SaveFileWrapper sfw = JsonUtility.FromJson<SaveFileWrapper>(Cryptographer.Decrypt(System.IO.File.ReadAllText(Path.Combine(RootSaveDirectory, file)), GlobalSettings.KeyPartOne + Cryptographer.KeyPartTwo));
        mc.UnitList.Clear();
        mc.SquadList.Clear();
        mc.UnitList.AddRange(sfw.units);
        mc.SquadList.AddRange(sfw.squads);
        
        foreach(Squad squad in sfw.squads)
        {
            Debug.Log(squad.Name);
            foreach(Unit unit in squad.RetrieveUnits())
            {
                Debug.Log(unit.Name);
            }
        }
    }
}

[Serializable]
public class SaveFileWrapper
{
    public bool NewGamePlus = false;
    public string SaveFileDateTime;
    public int ArmyCount;
    public int ArmyMax;

    public List<Unit> units;
    public List<Squad> squads;

    //Current Chapter Information


    UnitResourceManager.Wrapper resources = new();
}