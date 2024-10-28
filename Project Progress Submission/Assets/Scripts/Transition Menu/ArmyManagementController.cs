using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ArmyManagementController : MonoBehaviour
{
    ObservableStack<ArmyManagementState> StateMachine = new();

    [SerializeField] GameObject ParentMenu;

    [Header("External Systems")]
    [SerializeField] private EventSystem eventSystem;
    private GameObject lastObjectPicked;
    [SerializeField] MainController mc;
    
    [Header("Menu Views")]
    [SerializeField] GameObject DefaultView;
    [SerializeField] GameObject ExamineSquadView;
    [SerializeField] GameObject SelectItemView;

    private List<GameObject> views;

    [Header("Default View")]
    [SerializeField] GameObject UnitList;
    [SerializeField] GameObject SquadList;

    [Header("Popup View")]
    private Unit selectedUnit = null;
    private Squad selectedSquad = null;
    [SerializeField] GameObject ExamineUnitWindow;
    [SerializeField] GameObject PickUnitWindow;

    [Header("Squad Management View")]
    [SerializeField] GameObject UnitSprites;
    [SerializeField] GameObject SquadUnitList;
    [SerializeField] Pair<int, int> SquadCoordinateTracker;
    [SerializeField] Pair<int, int> UnitLastPosition;
    [SerializeField] Squad SquadToManage;
    [SerializeField] Unit UnitToPlace;
    [SerializeField] int UnitToPlaceListPosition;
    [SerializeField] SpriteRenderer[] sr;

    [Header("Context Menus")]
    [SerializeField] GameObject ContextParent;
    [SerializeField] GameObject DefaultUnitContext; 
    [SerializeField] GameObject DefaultSquadContext; 
    [SerializeField] GameObject SquadManagerContext; 


    [Header("Debug only")]
    [SerializeField] Unit _selectedUnit;
    [SerializeField] Squad _selectedSquad;
    [SerializeField] TextMeshProUGUI SelectedUnitText;
    [SerializeField] TextMeshProUGUI SelectedSquadText;

    void Awake()
    {
        deleteList<LayoutElement>(UnitList);
        deleteList<LayoutElement>(SquadUnitList);
        deleteList<HorizontalLayoutGroup>(SquadList);

        views = new(){
            DefaultView, ExamineSquadView, SelectItemView
        };

        sr = UnitSprites.GetComponentsInChildren<SpriteRenderer>();

        DisableAllViews();
        DefaultView.SetActive(true);

        StateMachine.CollectionChanged += StateMachineChanged;
    }

    void Update()
    {
        if(StateMachine.Count == 0) StateMachine.Push(ArmyManagementState.Default);
        
        ArmyManagementState vs = StateMachine.Peek();

        if(_selectedUnit != selectedUnit) _selectedUnit = selectedUnit;
        if(_selectedSquad != selectedSquad) _selectedSquad = selectedSquad;

        bool ExitInterface = false;

        //Will need to break down to specific segments -> One for escape button, one for Right click, one for left, etc
        switch(vs)
        {
            case ArmyManagementState.Default:
                if(eventSystem != null && eventSystem.currentSelectedGameObject != null)
                {
                    if(!eventSystem.currentSelectedGameObject.Equals(SelectedUnitText.gameObject)) lastObjectPicked = SelectedUnitText.gameObject;
                    else if(!eventSystem.currentSelectedGameObject.Equals(SelectedSquadText.gameObject)) lastObjectPicked = SelectedSquadText.gameObject;
                    else eventSystem.SetSelectedGameObject(lastObjectPicked);
                }

                if(Input.GetKeyUp(KeyCode.Escape)) 
                {
                    ExitInterface = true;
                }
                break;
                
            case ArmyManagementState.SquadManager:
                if(Input.GetKeyUp(KeyCode.Escape)) StateMachine.Pop();

                if(SquadToManage == null) throw new Exception("SquadToManage is somehow null");
                break;
            
            case ArmyManagementState.PlaceUnitFromList:
                if(Input.GetKeyUp(KeyCode.Escape) || Input.GetMouseButtonUp(1)) StateMachine.Pop();

                if(SquadCoordinateTracker != null)
                {
                    UpdateSquadGrid();

                    //Is space is occupied, then mark it as occupied. Trip break flag if SquadCoordinateTracker is occupying an existing space
                    if(OccupiedSpot(SquadCoordinateTracker)) break;

                    //If space is initially empty, display what the sprite would look like if it was placed there
                    SetSpritePosition(UnitToPlace.spriteView);
                }
                break;

            case ArmyManagementState.PlaceUnit:
                if(Input.GetMouseButtonUp(1) || Input.GetKeyUp(KeyCode.Escape)) StateMachine.Pop();

                //Update UnitToPlace based on where the mouse hovers on Unit List
                GameObject HoveredObject = UtilityClass.GetHoverObject("UnitGrid");

                UpdateSquadGrid();

                if(HoveredObject == null) break;

                List<GameObject> SquadButtons = new();

                foreach(Transform gObj in SquadUnitList.transform.GetChild(0).transform) if(gObj.gameObject.CompareTag("UnitGrid")) SquadButtons.Add(gObj.gameObject);

                int index = 0;
                bool foundUnit = false;
                foreach(GameObject gObj in SquadButtons)
                {
                    if(HoveredObject.Equals(gObj))
                    {
                        foundUnit = true;
                        break;
                    } 

                    index++;
                }

                if(!foundUnit) break;

                SetSpritePosition(mc.UnitList[index].spriteView);

                if(Input.GetMouseButtonUp(0))
                {
                    SquadToManage.FieldUnit(mc.UnitList[index], SquadCoordinateTracker);
                    mc.UnitList.Remove(mc.UnitList[index]);
                    StateMachine.Pop();
                }

                break;

            case ArmyManagementState.MoveUnit:
                //Pop state machine, but refield unit at the exact same position
                if(Input.GetMouseButtonUp(1) || Input.GetKeyUp(KeyCode.Escape))
                {
                    SquadToManage.FieldUnit(UnitToPlace, UnitLastPosition, UnitToPlaceListPosition);
                    StateMachine.Pop();
                }

                if(SquadCoordinateTracker != null)
                {
                    UpdateSquadGrid();

                    //Is space is occupied, then mark it as occupied. Trip break flag if SquadCoordinateTracker is occupying an existing space
                    if(OccupiedSpot(SquadCoordinateTracker)) break;

                    //If space is initially empty, display what the sprite would look like if it was placed there
                    SetSpritePosition(UnitToPlace.spriteView);
                }
                break;
        }

        //Left click handler
        if(Input.GetMouseButtonUp(0))
        {
            switch(vs)
            {
                case ArmyManagementState.Default:
                    DisableAllPopups();
                    break;
            }
        }

        //Right click handler

        //Middle click handler

        if(ExitInterface)
        {
            while(StateMachine.Count > 0) StateMachine.Pop();
            mc.ParentMenu.SetActive(true);
            ParentMenu.SetActive(false);
        }
    }

    void OnEnable() 
    {
        mc.UnitList.CollectionChanged += UnitList_CollectionChanged;
        mc.SquadList.CollectionChanged += SquadList_CollectionChanged;

        UpdateUnitList(UnitList);
        UpdateSquadList();

        StateMachine.Push(ArmyManagementState.Default);
    }

    private void OnDisable() 
    {
        mc.UnitList.CollectionChanged -= UnitList_CollectionChanged;
        mc.SquadList.CollectionChanged -= SquadList_CollectionChanged;
    }

    void DisableAllViews()
    {
        foreach(var view in views) view.SetActive(false);
    }

    void DisableAllPopups()
    {
        PickUnitWindow.SetActive(false);
        DefaultUnitContext.SetActive(false);
        DefaultSquadContext.SetActive(false);
        SquadManagerContext.SetActive(false);
    }

    void StateMachineChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if(StateMachine.Count <= 0) return;
        
        Debug.Log("State = " + StateMachine.Peek().ToString());

        DisableAllViews();
        DisableAllPopups();

        if(StateMachine.Peek() == ArmyManagementState.Default)
        {
            selectedSquad = null;
            selectedUnit = null;
            SelectedSquadText.text = "null";
            SelectedUnitText.text = "null";
        }

        //Extraneous functions
        switch(StateMachine.Peek())
        {
            case ArmyManagementState.PlaceUnit:
                PickUnitWindow.SetActive(true);
                break;
            case ArmyManagementState.SquadManager:
            case ArmyManagementState.PlaceUnitFromList:
            case ArmyManagementState.RemoveUnit:
                SquadCoordinateTracker = null;
                break;
            case ArmyManagementState.MoveUnit:
                UnitToPlace = SquadToManage.RetrieveUnitFromPosition(SquadCoordinateTracker);

                //Unfield unit temporarily
                (int, Unit) pair = SquadToManage.TemporaryUnfield(SquadCoordinateTracker);

                UnitToPlace = pair.Item2;
                UnitToPlaceListPosition = pair.Item1;

                UnitLastPosition = new Pair<int, int>(SquadCoordinateTracker);
                break;
        }
        
        //Switching windows
        switch(StateMachine.Peek())
        {
            case ArmyManagementState.Default:
                DefaultView.SetActive(true);
                break;

            case ArmyManagementState.SquadManager:
            case ArmyManagementState.PlaceUnit:
            case ArmyManagementState.PlaceUnitFromList:
            case ArmyManagementState.RemoveUnit:
            case ArmyManagementState.MoveUnit:
                ExamineSquadView.SetActive(true);
                UpdateUnitList(SquadUnitList);
                //Load squad info and display it on the screen

                UpdateSquadGrid();
                break;
        }
    }

    void UnitList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if(StateMachine.Count == 0) return;
        //if(e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
        if(StateMachine.Peek() == ArmyManagementState.Default) UpdateUnitList(UnitList);
        if(StateMachine.Peek() == ArmyManagementState.SquadManager) UpdateUnitList(SquadUnitList);
    }

    void SquadList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if(StateMachine.Count == 0) StateMachine.Push(ArmyManagementState.Default);
        if(StateMachine.Peek() == ArmyManagementState.Default) UpdateSquadList();
    }

    void UpdateUnitList(GameObject UnitList)
    {
        deleteList<LayoutElement>(UnitList);

        int elementTracker = 0;
        foreach(Unit unit in mc.UnitList)
        {
            //Replace GameObject with Button and dew it
            GameObject elementInList = UtilityClass.CreatePrefabObject("Assets/PreFabs/TransitionMenu/Army Management/UnitButton.prefab", UnitList.transform.GetChild(0).transform, "Name", new Vector3(-100, 30, 0));
            elementInList.name = "Element_" + elementTracker++;

            InteractableObject iObj = elementInList.GetComponent<InteractableObject>();

            iObj.LeftClickEvent += () => {
                if(StateMachine.Peek() == ArmyManagementState.Default) 
                {
                    if(selectedUnit != unit)
                    {
                        SelectUnitButton(unit, elementInList.name);
                        return;
                    }

                    CreateSquad(unit);
                    return;
                }

                if(StateMachine.Peek() == ArmyManagementState.SquadManager)
                {
                    StateMachine.Push(ArmyManagementState.PlaceUnitFromList);
                    UnitToPlace = unit;
                }
            };

            iObj.RightClickEvent += () => {
                selectedUnit = unit;

                SetContextMenu(DefaultUnitContext);
            };

            iObj.MiddleClickEvent += () => {
                if(StateMachine.Peek() == ArmyManagementState.Default) CreateSquad(unit);
            };

            TextMeshProUGUI[] tmpT = elementInList.GetComponentsInChildren<TextMeshProUGUI>();
            tmpT[0].text = unit.displayQuickInfo();
            tmpT[1].text = "<" + unit.UIFriendlyClassName + ">";

            List<Image> Images = elementInList.GetComponentsInChildren<Image>().ToList<Image>();
            List<Image> TraitImages = new();
            Image UnitViewer;

            foreach (Image i in Images.ToList()) 
            {
                if(i.CompareTag("TraitDisplay")) TraitImages.Add(i);
                else UnitViewer = i;
            }

            //Load trait images & battle sprite here
            elementInList.GetComponentsInChildren<Image>()[1].sprite = unit.spriteView;
        }
    }

    void SelectUnitButton(Unit unit, string element)
    {
        selectedSquad = null;
        selectedUnit = unit;
        SelectedUnitText.text = element + ":" + unit.GetType().Name;
        SelectedSquadText.text = "null";
    }

    void UpdateSquadList()
    {
        deleteList<HorizontalLayoutGroup>(SquadList);

        List<GameObject> groupViews = new();

        for(int i = 0; i < mc.SquadList.Count; i++)
        {
            Squad squad = mc.SquadList[i];

            if(i % 3 == 0)
            {
                GameObject groupView = new("Squad_Element_" + (i / 3));
                groupView.transform.SetParent(SquadList.transform.GetChild(0).transform, false);

                HorizontalLayoutGroup hlg = groupView.AddComponent<HorizontalLayoutGroup>();
                hlg.padding.left = 10;
                hlg.padding.right = 10;
                hlg.spacing = 10;
                hlg.childAlignment = TextAnchor.MiddleLeft;
                hlg.childForceExpandWidth = false;
                hlg.childForceExpandHeight = true;
                hlg.childControlHeight = false;
                hlg.childControlWidth = false;
                hlg.reverseArrangement = false;

                RectTransform rt = groupView.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(1120, 380);

                groupViews.Add(groupView);
            }

            //GameObject SquadButton = Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/PreFabs/TransitionMenu/Army Management/SquadButton.prefab"), groupViews[i/3].transform);
            GameObject SquadButton = UtilityClass.CreatePrefabObject("Assets/PreFabs/TransitionMenu/Army Management/SquadButton.prefab", groupViews[i/3].transform, squad.Name);
            SquadDisplayer sd = SquadButton.GetComponent<SquadDisplayer>();
            sd.AssignSquad(squad);

            InteractableObject iObj = SquadButton.GetComponent<InteractableObject>();

            iObj.LeftClickEvent += () => {
                if(StateMachine.Peek() == ArmyManagementState.Default) 
                {
                    SelectedSquadButton(squad, i);

                    if(SquadToManage != squad)
                    {
                        SquadToManage = squad;
                        return;
                    }
                    StateMachine.Push(ArmyManagementState.SquadManager);
                }
            };

            iObj.RightClickEvent += () => {
                selectedSquad = squad;
                SetContextMenu(DefaultSquadContext);
            };

            iObj.MiddleClickEvent += () => {
                Debug.Log("Reorder squad action");
            };

            //Add images here
            /*for(int j = 0; i < squad.GetEquipment().Length; i++)
            {
                Equipment e = squad.GetEquipment()[j];
                if(e != null && e.Icon != null) SquadButton.transform.GetChild(j + 1).GetComponent<Image>().sprite = e.Icon;
            }  */

            //Add Sample View here
        }
    }

    void SelectedSquadButton(Squad squad, int element)
    {
        selectedSquad = squad;
        selectedUnit = null;
        SelectedUnitText.text = "null";
        SelectedSquadText.text = "Element_" + element +": " + squad.GetType().Name;
    }

    void deleteList<T>(GameObject list) where T : Component
    {
        List<GameObject> children = GetAllChildObjectsWithComponent<T>(list.transform.GetChild(0).gameObject);

        if(children.Count > 0)
        {
            foreach(GameObject child in children.ToList())
            {
                children.Remove(child);
                if(child != null) Destroy(child);
            }
        }
    }

    List<GameObject> GetAllChildObjectsWithComponent<T>(GameObject parent) where T : Component
    {
        List<GameObject> Children = new();

        foreach(Transform child in parent.transform)
        {
            T component = child.GetComponent<T>();
            if(component != null) Children.Add(child.gameObject);
            Children.AddRange(GetAllChildObjectsWithComponent<T>(child.gameObject));
        }

        return Children;
    }

    List<T> GetAllChildrenWithComponent<T>(GameObject parent) where T : Component
    {
        List<T> Children = new();

        foreach(Transform child in parent.transform)
        {
            T component = child.GetComponent<T>();
            if(component != null) Children.Add(component);
            Children.AddRange(GetAllChildrenWithComponent<T>(child.gameObject));
        }

        return Children;
    }

    public void CreateSquad(Unit unit)
    {
        selectedUnit = unit;

        CreateSquad();
    }

    public void CreateSquad()
    {
        if(selectedUnit == null) 
        {
            Debug.Log("Display window that says you need to have a unit selected!");
            return;
        }

        mc.UnitList.Remove(selectedUnit);

        SquadToManage = new();
        SquadToManage.FieldedUnitsChanged += UpdateSquadGrid;

        SquadToManage = SquadToManage.CreateNewSquad(selectedUnit);
        SquadToManage.Name = selectedUnit.Name + "'s Squad";
        mc.SquadList.Add(SquadToManage);

        StateMachine.Push(ArmyManagementState.SquadManager);

        UpdateSquadGrid();
    }

    public void DismissUnit()
    {
        if(selectedUnit == null)
        {
            Debug.Log("Display window that says you need to have a unit selected!");
            return;
        }

        mc.UnitList.Remove(selectedUnit);
        selectedUnit = null;
        SelectedUnitText.text = "null";
    }

    public void DisbandSquad(Squad squad = null)
    {
        if(squad == null) squad = selectedSquad;

        if(StateMachine.Peek() == ArmyManagementState.Default)
        {
            foreach(var unit in squad.RetrieveUnits().ToList()) mc.UnitList.Add(unit);
            mc.SquadList.Remove(squad);
        }
    }

    public void DisbandSquad()
    {
        DisbandSquad(selectedSquad);
    }
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//Squad Manager View

    public void GetCoordinate(Pair<int, int> coordinate)
    {
        if(coordinate == null) 
        {
            SquadCoordinateTracker = null;
            return;
        }

        SquadCoordinateTracker = new(coordinate);
    }

    public void PlaceUnit(Pair<int, int> coordinate, bool TempField = false)
    {
        if(OccupiedSpot(SquadCoordinateTracker)) return;

        if(TempField)
        {
            SquadToManage.FieldUnit(UnitToPlace, coordinate, UnitToPlaceListPosition);
        }
        else
        {
            SquadToManage.FieldUnit(UnitToPlace, coordinate);

            mc.UnitList.Remove(UnitToPlace);
        }

        

        StateMachine.Pop();
    }

    public bool OccupiedSpot(Pair<int, int> coordinate, Squad squad = null)
    {
        if(squad == null) squad = SquadToManage;
        
        bool breakFlag = false;
        List<Pair<Unit, Pair<int, int>>> list = squad.RetrieveUnitPairs();
        foreach(var pair in list.ToList()) if(breakFlag = pair.Second.equals(coordinate)) break;
        return breakFlag;
    }

    public void BackButton()
    {
        StateMachine.Pop();
        if(StateMachine.Count == 0) StateMachine.Push(ArmyManagementState.Default);
    }

    void UpdateSquadGrid()
    {
        foreach(var s in sr.ToList())
        {
            s.sprite = null;
            s.enabled = false;
        }

        foreach(var unit in SquadToManage.RetrieveUnitPairs().ToList())
        {
            int idx = (unit.Second.First * 3) + unit.Second.Second;

            if(unit.First.spriteView != default || unit.First.spriteView != null) sr[idx].sprite = unit.First.spriteView;
            else sr[idx].sprite = Resources.Load<Sprite>(GlobalSettings.DefaultUnitSpriteView);

            sr[idx].enabled = true;
        }   
    }

    public void SetSpritePosition(Sprite sprite = null, Pair<int, int> pair = null)
    {
        if(pair == null) pair = SquadCoordinateTracker;

        if(sprite == null) sprite = Resources.Load<Sprite>(GlobalSettings.DefaultUnitSpriteView);

        int PlaceUnitIdx = (SquadCoordinateTracker.First * 3) + SquadCoordinateTracker.Second;

        //Update sprite on SpriteRenderer at SquadCoordinateTracker
        sr[PlaceUnitIdx].sprite = sprite;
        sr[PlaceUnitIdx].enabled = true;
    }

    void SetContextMenu(GameObject context)
    {
        DisableAllPopups();
        context.SetActive(true);
        SetContextPositionToMouse();
    }

    void SetContextPositionToMouse()
    {
        ContextParent.transform.position = UtilityClass.GetScreenMouseToWorld();
    }

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
///State Machine functions for external scripts

    public ArmyManagementState GetCurrentState()
    {
        return StateMachine.Peek();
    }

    public void RecordNewState(ArmyManagementState vs)
    {
        StateMachine.Push(vs);
    }

    public ArmyManagementState PopState()
    {
        return StateMachine.Pop();
    }
}

public enum ArmyManagementState
{
    //Default View
    Default,
    SquadManager,
    UnitView,
    UseItem,

    //Squad Management View
    PlaceUnit,
    RemoveUnit,
    MoveUnit,
    PlaceUnitFromList,
    ChangeEquipment,
    InvestigateSquadUnit,
    PromoteUnit,
}