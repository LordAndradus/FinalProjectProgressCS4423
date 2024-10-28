using Unity.VisualScripting;
using UnityEngine;

public class SquadGridHandler : MonoBehaviour
{
    [SerializeField] Pair<int, int> coordinate;
    [SerializeField] ArmyManagementController amc;

    [Header("Highlighting Mesh")]
    [SerializeField] Mesh highlighter;
    [SerializeField] MeshFilter mf;
    [SerializeField] MeshRenderer mr;
    [SerializeField] Vector3[] vertices = new Vector3[4];
    [SerializeField] int[] triangles = new int[6] {
        0, 1, 2,
        2, 3, 1
    };

    [SerializeField] Vector2[] uv = new Vector2[]{
        new Vector2(0, 0),
        new Vector2(0, 1),
        new Vector2(1, 0),
        new Vector2(1, 1),
    };

    Timer HoldClick = new(0.5f, false);

    void Awake()
    {
        HoldClick.TimerFunction += HeldLeftClick;
        
        mf = this.GetComponent<MeshFilter>();
        mr = this.GetComponent<MeshRenderer>();
    }

    private void OnDisable() 
    {
        if(amc.GetCurrentState() == ArmyManagementState.PlaceUnit) return;

        //Clean up meshes just in case.
        Destroy(highlighter);
        highlighter = null;
    }

    private void HeldLeftClick()
    {
        amc.GetCoordinate(coordinate);
        amc.RecordNewState(ArmyManagementState.MoveUnit);
        HoldClick.stopReset();
    }

    private void OnMouseEnter()
    {
        ArmyManagementState vs = amc.GetCurrentState();


        //Handles cases to provide coordinates
        switch(vs)
        {
            case ArmyManagementState.PlaceUnitFromList:
            case ArmyManagementState.MoveUnit:
                amc.GetCoordinate(coordinate);
                break;
        }

        //Handles cases to highlight tile
        switch(vs)
        {
            //Assign highlighting mesh
            case ArmyManagementState.SquadManager:
            case ArmyManagementState.PlaceUnitFromList:
            case ArmyManagementState.MoveUnit:
            case ArmyManagementState.RemoveUnit:
                if(highlighter == null) CreateMesh();
                break;
        }
    }

    private void OnMouseExit()
    {

        ArmyManagementState vs = amc.GetCurrentState();

        switch(vs)
        {
            case ArmyManagementState.PlaceUnitFromList:
                amc.GetCoordinate(null);
                break;
        }

        switch(vs)
        {
            //Assign highlighting mesh
            case ArmyManagementState.SquadManager:
            case ArmyManagementState.PlaceUnitFromList:
            case ArmyManagementState.MoveUnit:
            case ArmyManagementState.RemoveUnit:
                Destroy(highlighter);
                highlighter = null;
                break;
        }

        HoldClick.stopReset();
    }

    private void OnMouseOver()
    {

        ArmyManagementState vs = amc.GetCurrentState();

        HoldClick.update();

        if(Input.GetMouseButton(0) && vs == ArmyManagementState.SquadManager && amc.OccupiedSpot(coordinate)) HoldClick.start();
        else if(!Input.GetMouseButton(0) && HoldClick.isTimerRunning()) HoldClick.stopReset();

        if(Input.GetMouseButton(2))
        {
            switch(vs)
            {
                case ArmyManagementState.SquadManager:
                    amc.RecordNewState(ArmyManagementState.UseItem);
                    break;
            }
        }
        
        if(Input.GetMouseButtonUp(0))
        {
            switch(vs)
            {
                case ArmyManagementState.SquadManager:
                    if(amc.OccupiedSpot(coordinate)) break;

                    amc.RecordNewState(ArmyManagementState.PlaceUnit);
                    amc.GetCoordinate(coordinate);
                    break;

                case ArmyManagementState.PlaceUnitFromList:
                    amc.PlaceUnit(coordinate);
                    break;
            }
        }

        if(Input.GetMouseButtonDown(0) && vs == ArmyManagementState.MoveUnit) amc.PlaceUnit(coordinate, true);

        if(Input.GetMouseButtonUp(1))
        {
            switch(vs)
            {
                case ArmyManagementState.SquadManager:
                    Debug.Log("Pop Context menu");
                    break;

                case ArmyManagementState.PlaceUnit:
                    Debug.Log("Back out of placing unit");
                    break;
            }
        }

        if(highlighter == null && vs == ArmyManagementState.SquadManager) CreateMesh();
    }

    private void CreateMesh()
    {
        highlighter = new();
        highlighter.name = "Highlighted Tile";
        highlighter.vertices = vertices;
        highlighter.triangles = triangles;
        highlighter.uv = uv;

        Color highlighting = new Color(0.4f, 0.0f, 0.0f, 0.4f);

        highlighter.colors = new Color[4]{
            highlighting, highlighting, highlighting, highlighting
        };

        mf.mesh = highlighter;
    }
}
