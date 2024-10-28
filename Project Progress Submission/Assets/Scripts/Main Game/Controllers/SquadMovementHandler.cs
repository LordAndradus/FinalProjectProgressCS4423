using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquadMovementHandler : InteractableObject
{
    [SerializeField] Squad squad;
    Controller controller;

    public Coroutine FinishedMoving;
    public event Action DeleteSquadCallback;

    List<Vector3> pathList;
    List<Vector3> ValidatedMoves = new();
    List<Pair<int, int>> ValidatedCoordinates = new(); //HighlightedTileGridCoordinate
    List<Vector3> AttackVectors = new();
    List<Pair<int, int>> AttackCoordinates = new(); //HighlightedTileGridCoordinate
    List<GameObject> HighlightedTiles = new();
    int currentPathIdx = 0;
    Vector3 targetPosition;
    Vector3 mouseTarget;

    public Side EntrySide = Side.none;

    public bool moved = false;
    public bool moving = false;
    bool hovered = false;

    void Awake()
    {
        targetPosition = transform.position;

        LeftClickEvent += () => {
            if(!moved && controller.GetRelationship() == Controller.Relationship.controllable) 
            {
                Controller.selectedSquad = this;
                CombatGridSystem.instance.LoadValidTiles(this);
            }
        };

        RightClickEvent += () => {
            if(Controller.selectedSquad != null && controller.GetTeamNumber() != Controller.selectedSquad.GetController().GetTeamNumber())
            {
                Debug.Log(squad.Name + " being attacked");
                Controller.attackedSquad = this;
            }
        };

        OnEnter += () => {
            if(!moved && !moving) CombatGridSystem.instance.LoadValidTiles(this);

            EntrySide = GetSideTriangle(UtilityClass.GetScreenMouseToWorld());

            hovered = true;
        };

        OnExit += () => {
            if(Controller.selectedSquad != this) ClearHighlighting();
            EntrySide = Side.none;
            hovered = false;
        };
    }

    //The BoxCollider is split into 4 equilateral triangles. They formed by 2 diagonal lines
    //Diagonal line 1 => y = x | Diagonal line 2 => y = -x; Set them both to be inequalities and solve to see which triangle they lie inside
    private Side GetSideTriangle(Vector3 MouseTarget)
    {
        //Convert world position to local space
        float x = Mathf.Abs(MouseTarget.x) - Mathf.Abs(transform.position.x);
        float y = Mathf.Abs(MouseTarget.y) - Mathf.Abs(transform.position.y);

        if(y < x && y < -x) return Side.top;
        if(y > x && y > -x) return Side.bottom;
        if(y < x && y > -x) return Side.left;
        if(y > x && y < -x) return Side.right;

        return Side.none;
    }

    void Update()
    {
        Move();

        if(hovered && Input.GetKeyUp(GlobalSettings.ControlMap[SettingKey.Information])) Debug.Log("User wants some info yo");
    }

    public void SetPathList(Vector3 target)
    {
        if(pathList == null)
        {
            currentPathIdx = 0;

            ClearHighlighting();
            
            pathList = CombatGridSystem.instance.PathGrid.FindPath(transform.position, target);

            moving = true;
        }
    }

    public void Move()
    {
        if(pathList != null)
        {
            targetPosition = pathList[currentPathIdx];

            if(Vector3.Distance(transform.position, targetPosition) > 0.1f)
            {
                Vector3 MoveDirection = (targetPosition - transform.position).normalized;

                transform.position += MoveDirection * GlobalSettings.UnitMoveSpeed * Time.deltaTime;
            }
            else
            {
                currentPathIdx++;

                if(currentPathIdx >= pathList.Count)
                {
                    transform.position = pathList[pathList.Count - 1];
                    Deactivate();
                }
            }
        }
    }

    private IEnumerator DeleteSquad(Func<bool> condition)
    {
        while(!condition())
        {
            yield return null;
        }

        DeleteSquadCallback?.Invoke();

        FinishedMoving = null;
    }

    private IEnumerator FinishMovement(Func<bool> condition)
    {
        while(!condition())
        {
            yield return null;
        }

        Controller.selectedSquad = null;

        FinishedMoving = null;
    }

    public void StartDeletionCallback(SquadMovementHandler attacker)
    {
        if(FinishedMoving != null) StopCoroutine(FinishedMoving);
        FinishedMoving = StartCoroutine(DeleteSquad(() => {
            return Controller.attackedSquad != null && attacker.moved == true && attacker.moving == false;
        }));
    }

    public void StartMovementCallback()
    {
        FinishedMoving = StartCoroutine(FinishMovement(() => {
            return moved == true && moving == false;
        }));
    }

    public void Deactivate()
    {
        pathList = null;
        moving = false;
        moved = true;
        GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f, 1f);
    }

    public void Reactivate()
    {
        pathList = null;
        moving = false;
        moved = false;
        GetComponent<SpriteRenderer>().color = Color.white;
    }

    public void ClearHighlighting()
    {
        AttackVectors.Clear();
        ValidatedMoves.Clear();
        HighlightedTiles.ForEach(go => Destroy(go));
        ValidatedCoordinates.Clear();
    }

    public void AssignSquad(Squad squad)
    {
        this.squad = squad;
    }

    public void AssignController(Controller c)
    {
        controller = c;
    }

    public Controller RetrieveController()
    {
        return controller;
    }

    public void AddHighlightedTile(GameObject tile)
    {
        HighlightedTiles.Add(tile);
    }

    public void AddValidatedCoordinate(Pair<int, int> coordinate)
    {
        ValidatedCoordinates.Add(coordinate);
    }

    public void AddAttackCoordinate(Pair<int, int> coordinate)
    {
        AttackCoordinates.Add(coordinate);
    }

    public List<Vector3> GetValidatedPathList()
    {
        return ValidatedMoves;
    }

    public List<Vector3> GetAttackVectors()
    {
        return AttackVectors;
    }

    public List<Pair<int, int>> GetValidCoordinates()
    {
        return ValidatedCoordinates;
    }

    public List<Pair<int, int>> GetAttackCoordinates()
    {
        return AttackCoordinates;
    }

    public Controller GetController()
    {
        return controller;
    }

    public Squad GetSquad()
    {
        return squad;
    }

    public enum Side //For the sides of the rectangle <- This is used to choose where the unit is while attacking
    {
        none, left, right, top, bottom
    }
}