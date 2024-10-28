using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LoadController : MonoBehaviour
{
    [Header("External Systems")]
    [SerializeField] GameObject MainMenu;
    [SerializeField] MainController mc;
    [SerializeField] EventSystem eventSystem;
    [SerializeField] Button BackButton;

    [Header("Parent Objects")]
    [SerializeField] GameObject LoadList;
    [SerializeField] GameObject LoadInformation;
    [SerializeField] GameObject LoadRoster;
    [SerializeField] GameObject LoadDescription;

    [Header("Load List Content")]
    [SerializeField] GameObject FileContent;

    [Header("Load Information")]
    [SerializeField] TextMeshProUGUI ChapterTitle;
    [SerializeField] TextMeshProUGUI BasicInfo;
    [SerializeField] TextMeshProUGUI DifficultyModifiers;
    [SerializeField] GameObject GameMetrics;

    [Header("Load Roster Content")]
    [SerializeField] GameObject RosterContent;

    [Header("Load Information")]
    string RootSaveDirectory;
    [SerializeField] GameObject ActiveLoadInteractable;
    [SerializeField] List<string> LoadFiles;
    [SerializeField] string ActiveLoad;
    [SerializeField] SaveFileWrapper ActiveSFW;
    List<Unit> UnitList;
    List<Squad> SquadList;

    private void Awake()
    {
        BackButton.onClick.AddListener(() => {
            transform.gameObject.SetActive(false);
            MainMenu.SetActive(true);
        });

        #if UNITY_EDITOR
            RootSaveDirectory = Path.Combine(Application.dataPath, "Save Files");
        #else
            RootSaveDirectory = Path.Combine(Application.persistentDataPath, "Save Files");
        #endif

        FileContent.transform.GetChild(0).AddComponent<InteractableObject>().LeftClickEvent += () => {
            //LoadCurrentInformation();
        };

        LoadSaveFiles();
        if(LoadFiles.Count != 0) SetActiveFile(0);
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

        LoadFiles.Clear();

        string[] files = Directory.GetFiles(RootSaveDirectory);
        foreach(string file in files) if(Path.GetExtension(file).Equals(".json")) LoadFiles.Add(Path.GetFileName(file));

        foreach(Transform child in FileContent.transform.Cast<Transform>().Skip(1)) Destroy(child.gameObject);
    }

    private void SetActiveFile(int index)
    {

    }
}