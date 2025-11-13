using System.Collections.Generic;
using GameJam.Controller;
using GameJam.Military;
using GameJam.Movement;
using UnityEngine;

public class MilitaryBuilding : MonoBehaviour
{
    [SerializeField]
    private int maxSoldierSlots = 10;

    [SerializeField]
    private SoldierType soldierType;

    private List<Soldier> assignedSoldiers = new List<Soldier>();

    public bool RequestSoldierSlot(Soldier soldier)
    {
        // Check if already assigned
        if (assignedSoldiers.Contains(soldier))
        {
            Debug.LogWarning($"Soldier {soldier.name} already assigned to {gameObject.name}");
            return true;
        }
        //w

        // Check if slots available
        if (assignedSoldiers.Count < maxSoldierSlots)
        {
            assignedSoldiers.Add(soldier);
            return true;
        }

        return false;
    }

    public void ReleaseSoldierSlot(Soldier soldier)
    {
        if (assignedSoldiers.Contains(soldier))
        {
            assignedSoldiers.Remove(soldier);
        }
        else
        {
            Debug.LogWarning(
                $"Tried to release soldier {soldier.name} that wasn't assigned to {gameObject.name}"
            );
        }
    }

    public SoldierType GetSoldierType()
    {
        return soldierType;
    }
}
