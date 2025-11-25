using GameJam.Combat;
using UnityEngine;

namespace GameJam.Military
{
    [CreateAssetMenu(fileName = "SoldierType", menuName = "SoldierType/New Soldier Type")]
    public class SoldierType : ScriptableObject
    {
        [SerializeField]
        string typeName;

        [SerializeField]
        Weapon weaponPrefab;

        [SerializeField]
        Mesh headMesh;

        [SerializeField]
        Mesh bodyMesh;

        public string GetTypeName()
        {
            return typeName;
        }

        public Weapon GetWeaponPrefab()
        {
            return weaponPrefab;
        }

        public Mesh GetHeadMesh()
        {
            return headMesh;
        }

        public Mesh GetBodyMesh()
        {
            return bodyMesh;
        }
    }
}
