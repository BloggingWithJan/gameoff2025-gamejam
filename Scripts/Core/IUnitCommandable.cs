using UnityEngine;

public interface IUnitCommandable
{
    void MoveTo(Vector3 destination);
    void InteractWith(GameObject target);
}
