using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : Controller
{
    static ObservableCollection<Squad> SquadList = new();
    [Header("Player Controller Specific")]
    [SerializeField] public List<Squad> ReserveSquadList = new(); //Only for being viewed in the Inspector
    [SerializeField] public CameraController cc;
    [SerializeField] public GridSystem<PathNode> PathFindingGrid;
    [SerializeField] Button EndTurn;
    [SerializeField] GameObject confirm;
    [SerializeField] GameObject SquadPicker;

    List<Timer> timers;
    SquadMovementHandler CurrentSelection;
    Stack<State> StateMachine = new();
    Dictionary<State, System.Action> ActionMap;

    private void Start()
    {
        relation = Relationship.controllable;
        SquadList.CollectionChanged += OnSquadListChanged;
        PathFindingGrid = PathFinder.instance.GetGridSystem();

        TeamNumbers.Add(this, 0);

        CloseSpawner();
        confirm.SetActive(false);

        InitializeStateMap();

        Button[] ConfirmButtons = confirm.GetComponentsInChildren<Button>();

        ConfirmButtons[0].onClick.AddListener(FlagEndTurn);
        ConfirmButtons[1].onClick.AddListener(() => {
            confirm.SetActive(false);
        });

        EndTurn.onClick.AddListener(() => {
            TextMeshProUGUI SquadsLeft = confirm.GetComponentInChildren<TextMeshProUGUI>();
            int SquadsToMove = SquadMoverList.Count(squad => !squad.moved);

            AIController.InitialPlayerViewport = Camera.main.transform.position;
            AIController.InitialOrthographicSize = Camera.main.orthographicSize;

            SquadsLeft.text = string.Format("You have {0} {1} left to move.", SquadsToMove, SquadsToMove == 1 ? "squad" : "squads");

            if(SquadsToMove == 0) FlagEndTurn();
            else confirm.SetActive(true);
        });

        foreach(SpawnTile spawner in spawners)
        {
            spawner.SetXY(out int x, out int y);

            spawner.RightClickEvent += () => {
                if(StateMachine.Peek() != State.Normal) return;

                StateMachine.Push(State.Spawning);

                //Set the squad list to active
                SquadPicker.SetActive(true);

                GameObject SquadButtons = SquadPicker.GetComponentInChildren<GridLayoutGroup>().transform.gameObject;

                Button[] buttons = SquadPicker.GetComponentsInChildren<Button>();
                buttons[buttons.Length - 1].onClick.AddListener(() => {
                    CloseSpawner();
                });

                foreach(Transform child in SquadButtons.transform) 
                    Destroy(child.gameObject);
                
                foreach(Squad squad in SquadList)
                {
                    GameObject SquadButton = UtilityClass.CreatePrefabObject("Assets/PreFabs/Main Game/PurchaseSquad.prefab", SquadButtons.transform, squad.Name);
                    
                    SquadDisplayer sd = SquadButton.GetComponent<SquadDisplayer>();
                    sd.AssignSquad(squad);

                    InteractableObject sbIO = SquadButton.GetComponent<InteractableObject>();

                    sbIO.LeftClickEvent += () => {
                        GameObject WorldSquad = CombatGridSystem.instance.CreateWorldSquad(squad, this, x, y);
                        SquadList.Remove(squad);
                        CloseSpawner();
                    };

                    sbIO.RightClickEvent += () => {
                        Debug.Log("Opening a context menu");
                    };

                    sbIO.MiddleClickEvent += () => {
                        Debug.Log("Middle mouse button, might debate on what to do with this");
                    };
                }
            };
        }
        
        //Randomly generate squads for now
        for(int i = Random.Range(4, 10); i >= 0; i--)
        {
            SquadList.Add(UnitGenerator.GenerateSquad());
        }

        SetupTimers();
    }
    private void InitializeStateMap()
    {
        ActionMap = new(){
            {State.Normal, NormalUpdates},
            {State.Spawning, SpawnSquad},
            {State.SquadSelected, MoveSquad},
            {State.SquadAttacking, SquadAttacking},
        };
    }

    private void SetupTimers()
    {
        timers = new()
        {
            new Timer(1f, true, () => {
                Debug.Log("Current Player State: " + StateMachine.Peek().ToString());
                timers[0].reset();
            }),

            new Timer(1f, true, () => {
                if(StateMachine.Peek() == State.SquadSelected)
                {
                    if(selectedSquad != null) Debug.Log("Current selected squad = " + selectedSquad.name);
                    else Debug.LogError("Selected squad not found!");
                }
                timers[1].reset();
            })
        };
    }

    //Handles all the logic for a typical Controller
    public override void ControllerLogic()
    {
        if(StateMachine.Count == 0) StateMachine.Push(State.Normal);

        if(timers != null && timers.Count != 0) timers.ForEach(timer => timer.update());

        ActionMap[StateMachine.Peek()]?.Invoke();
    }

//*****************************************************************************************************************************
//State Machine Handling
    public enum State
    {
        Normal,
        Spawning,
        //Squad Specific States
        SquadSelected,
        SquadAttacking,

    }

//*****************************************************************************************************************************
//State Machine methods
    private void NormalUpdates()
    {
        cc.Move();

        if(selectedSquad != null)
        {
            CurrentSelection = selectedSquad;
            StateMachine.Push(State.SquadSelected);
        }

        if(Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonUp(1))
        {
            Vector3 MouseTarget = UtilityClass.GetScreenMouseToWorld();
            PathNode p = PathFinder.instance.GetGridSystem().GetGridObject(MouseTarget);
            p.SetIsWalkable(!p.GetIsWalkable());

            PathFinder.instance.GetGridSystem().SetDebugText();
        }
    }

    private void SpawnSquad()
    {   
        if(Input.GetKeyUp(GlobalSettings.HardCodedKeys[SettingKey.Escape]))
        {
            CloseSpawner();
        }
    }

    public void CloseSpawner()
    {
        SquadPicker.SetActive(false);
        if(StateMachine.Count != 0) StateMachine.Pop();
    }

    private void MoveSquad()
    {
        cc.Move();

        if(selectedSquad == null) 
        {
            StateMachine.Pop();
            return;
        }

        if(Input.GetMouseButtonUp((int) GlobalSettings.MouseMap[SettingKey.RightClick]))
        {
            if(SquadMoverList.Contains(selectedSquad))
            {
                if(attackedSquad != null) CombatGridSystem.instance.SetSquadAttackPath(selectedSquad, attackedSquad);
                else CombatGridSystem.instance.SetSquadNormalPath(selectedSquad, UtilityClass.GetScreenMouseToWorld());
            }
        }

        if(Input.GetMouseButtonUp((int) GlobalSettings.MouseMap[SettingKey.LeftClick]))
        {
            foreach(SquadMovementHandler smh in SquadMoverList) smh.ClearHighlighting();
            if(selectedSquad == CurrentSelection && !selectedSquad.moving) selectedSquad = null;
            else CombatGridSystem.instance.LoadValidTiles(selectedSquad);
            StateMachine.Pop();
        }

        if(Input.GetKeyUp(KeyCode.Tab))
        {
            
        }
    }

    private void SquadAttacking()
    {
        
    }

//*****************************************************************************************************************************
//General methods
    private void FlagEndTurn()
    {
        EndTurn.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Turn: " + (TurnNumber + 1);
        StateMachine.Clear();
        confirm.SetActive(false);
        Finished = true;
    }

    private void OnSquadListChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        ReserveSquadList.Clear();
        ReserveSquadList = SquadList.ToList();
    }

    public static void LoadSquadList(MainController mc)
    {
        //Try to load from Transition menu first
        PlayerController.SquadList = mc.SquadList;

        //Load from a quicksave file
        if(PlayerController.SquadList == null)
        {

        }
    }
}

