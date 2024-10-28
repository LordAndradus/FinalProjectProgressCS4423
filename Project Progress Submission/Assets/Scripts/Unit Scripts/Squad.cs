using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;
using System.Linq;
using System.Text;

[Serializable]
public class Squad
{
    public event Action FieldedUnitsChanged;

    public string Name;

    [Header("Serialized privates")]
    [SerializeField] List<Unit> units = new();
    [SerializeField] Unit[,] fieldedUnits = new Unit[3,3];
    [SerializeField] Equipment[] equipment = new Equipment[3];
    public MoveType MovementType = default(MoveType);

    public int MoveSpeed = 6;

    public int TranslateMovementType(MoveType movement)
    {
        switch(movement)
        {
            case MoveType.Standard:
                MoveSpeed = 6;
                break;
            case MoveType.Slow:
                MoveSpeed = 5;
                break;
            case MoveType.Light:
                MoveSpeed = 6;
                break;
            case MoveType.Cavalry:
                MoveSpeed = 7;
                break;
            case MoveType.LightCavalry:
                MoveSpeed = 7;
                break;
            case MoveType.Flying:
            MoveSpeed = 7;
                break;
        }

        return MoveSpeed;
    }

    public Squad CreateNewSquad(Unit leader)
    {
        FieldUnit(leader, new Pair<int, int>(1, 1));

        return this;
    }

    public List<Unit> RetrieveUnits()
    {
        return units;
    }

    public Unit RetrieveLeader()
    {
        return units[0];
    }
    
    public List<Pair<Unit, Pair<int, int>>> RetrieveUnitPairs()
    {
        List<Pair<Unit, Pair<int, int>>> UnitCoord = new();

        for(int i = 0; i < fieldedUnits.GetLength(0); i++) for(int j = 0; j < fieldedUnits.GetLength(1); j++) if(fieldedUnits[i, j] != null) UnitCoord.Add(new Pair<Unit, Pair<int, int>>(fieldedUnits[i, j], new Pair<int, int>(i, j)));

        return UnitCoord;
    }

    public void DetermineMoveType()
    {
        if(units.Count == 0) return;
        //Categorizes all Unit movement types and puts them in a dictionary with the MoveType as a key, and the sum of each unit's field cost if they're in that MoveType
        Dictionary<MoveType, int> sumByMovement = units.GroupBy(unit => unit.movement).ToDictionary(movementType => movementType.Key, MovementType => MovementType.Sum(unit => unit.GetFieldCost()));
        //Reorder dictionary to pick the highest sum which has the most weight. Set it as this squads MovementType
        MovementType = sumByMovement.OrderByDescending(pair => pair.Value).First().Key;
        TranslateMovementType(MovementType);
    }

    public void AddUnit(Unit unit)
    {
        units.Add(unit);
        DetermineMoveType();
    }

    public void RemoveUnit(Unit unit)
    {
        units.Remove(unit);
        DetermineMoveType();
    }

    public void FieldUnit(Unit unit, Pair<int, int> slot)
    {
        if(slot.First > fieldedUnits.GetLength(0) || slot.Second > fieldedUnits.GetLength(0) 
        || slot.First < 0 || slot.Second < 0)
        {
            Debug.Log("For some reason the user passed something out of the normal array range...");
            throw new Exception("Overbound Unit Field");
        }

        if(!units.Contains(unit)) AddUnit(unit);
        fieldedUnits[slot.First, slot.Second] = unit;
        FieldedUnitsChanged?.Invoke();
    }

    public void FieldUnit(Unit unit, Pair<int, int> slot, int LastListPosition)
    {
        units.Insert(LastListPosition, unit);
        FieldUnit(unit, slot);
    }

    public void UnfieldUnit(Pair<int, int> slot)
    {
        RemoveUnit(fieldedUnits[slot.First, slot.Second]);
        fieldedUnits[slot.First, slot.Second] = null;
        FieldedUnitsChanged?.Invoke();
    }

    public (int, Unit) TemporaryUnfield(Pair<int, int> slot)
    {
        Unit unit = fieldedUnits[slot.First, slot.Second];
        int position = units.FindIndex(unit => units.Contains(unit));

        UnfieldUnit(slot);

        return (position, unit);
    }

    public void MoveFieldedUnit(Pair<int, int> NewSlot, Pair<int, int> OldSlot)
    {
        Unit slottedUnit = fieldedUnits[NewSlot.First, NewSlot.Second];
        fieldedUnits[NewSlot.First, NewSlot.Second] = fieldedUnits[OldSlot.First, OldSlot.Second];
        fieldedUnits[OldSlot.First, OldSlot.Second] = slottedUnit;
        FieldedUnitsChanged.Invoke();
    }

    public Unit RetrieveUnitFromPosition(Pair<int, int> position)
    {
        return fieldedUnits[position.First, position.Second];
    }

    public void AddEquipment(Equipment e)
    {
        bool equipped = false;

        for(int i = 0; i < equipment.Length; i++) if(equipment[i] == null) 
        {
            equipment[i] = e;
            equipped = true;
            break;
        }

        if(!equipped) Debug.Log("Could not equp item! Equipment is already full!");
    }

    public Equipment[] GetEquipment()
    {
        return equipment;
    }

    public (int, int, int, int, int, int) GetResourceCost()
    {
        (int, int, int, int, int, int) resources = units.Aggregate((0, 0, 0, 0, 0, 0), (resources, unit) => (
            resources.Item1 + unit.GoldCost,
            resources.Item2 + unit.IronCost,
            resources.Item3 + unit.MagicGemCost,
            resources.Item4 + unit.HorseCost,
            resources.Item5 + unit.HolyTearCost,
            resources.Item6 + unit.AdamntiumCost
        ));

        return resources;
    }
}