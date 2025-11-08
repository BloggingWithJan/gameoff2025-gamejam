using GameJam.Resource;
using Resource;
using UnityEngine;

//TODO: class name is politically incorrect
public class WorkCamp : MonoBehaviour
{
    public void DepositResources(int amount)
    {
        // Debug.Log($"Workcamp received {amount} resources.");
        ResourceManager.Instance.AddResource(ResourceType.Wood, amount);
    }
}
