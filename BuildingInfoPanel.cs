using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class BuildingInfoPanel : MonoBehaviour
{
    [Header("UI References")] 
    [SerializeField] private TMP_Text buildingNameText;
    [SerializeField] private TMP_Text descriptionText;

    public void SetData(BuildingData building)
    {
        if (building == null) return;
        
        if (buildingNameText != null)
            buildingNameText.text = building.buildingName;

        if (descriptionText != null)
            descriptionText.text = building.description;
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }
    
    public void ShowPanel()
    {
        gameObject.SetActive(true);
    }
}