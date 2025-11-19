using UnityEngine;

public interface IUnitCommandable
{
    void MoveTo(Vector3 destination, Vector3 formationPosition);
    void InteractWith(GameObject target);
}
