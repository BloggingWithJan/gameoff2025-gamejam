using System.Collections.Generic;
using UnityEngine;

public class BuildingData : MonoBehaviour
{
    [Header("Basic Info")]
    public string buildingName;
    public string description;
    public Sprite icon;

    [Header("Costs")]
    public List<ResourceCost> costs;
    
    [Header("Refund")]
    public List<ResourceCost> refunds;
}