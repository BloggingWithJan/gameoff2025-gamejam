using UnityEngine;

public class WorkCamp : MonoBehaviour
{
    public void DepositResources(int amount)
    {
        Debug.Log($"Workcamp received {amount} resources.");
        ResourceManager.Instance.Wood += amount;
    }
}
