using System.Collections.Generic;
using Controller.UI;
using Core;
using GameJam.Core;
using UnityEngine;
using UnityEngine.UI;

/**
 * Indirectly made by MrNajZs
 */
public class JanButton : MonoBehaviour
{
    public Button button;
    public PlayerBaseBuilding playerBaseBuilding;
    public PurchaseTooltipController purchaseTooltipController;

    private ResourceCost cost = new (ResourceType.Food, 15);
    public void Awake()
    {
        if (button == null)
        {
            Debug.LogError("JanButton.Awake: button is null");
        }

        purchaseTooltipController.AddTooltipEvents(button, "Worker", "Spawns units at the Town Hall for gathering, building, or combat.", new List<ResourceCost>{cost});

        button.onClick.AddListener(() =>
        {
            playerBaseBuilding.SpawnUnit(cost);
        });

    }
}