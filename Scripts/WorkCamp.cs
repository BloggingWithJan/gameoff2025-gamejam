using GameJam.Resource;
using UnityEngine;

//TODO: class name is politically incorrect
public class WorkCamp : MonoBehaviour
{
    public void DepositResources(int amount)
    {
        // Debug.Log($"Workcamp received {amount} resources.");
        ResourceManager.Instance.Wood += amount;
    }
}
