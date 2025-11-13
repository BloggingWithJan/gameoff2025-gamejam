using GameJam.Combat;
using UnityEngine;

namespace GameJam.Production
{
    [CreateAssetMenu(fileName = "GathererType", menuName = "GathererType/New Gatherer Type")]
    public class GathererType : ScriptableObject
    {
        [SerializeField]
        string typeName;

        [SerializeField]
        Weapon toolPrefab;

        [SerializeField]
        string resourceTag;

        [SerializeField]
        float gatherTime = 5f;

        [SerializeField]
        int gatherAmount = 2;

        [SerializeField]
        AudioClip gatherSound;

        [SerializeField]
        Mesh headMesh;

        [SerializeField]
        Mesh bodyMesh;

        public Weapon GetToolPrefab()
        {
            return toolPrefab;
        }

        public string GetResourceTag()
        {
            return resourceTag;
        }

        public float GetGatherTime()
        {
            return gatherTime;
        }

        public int GetGatherAmount()
        {
            return gatherAmount;
        }

        public AudioClip GetGatherSound()
        {
            return gatherSound;
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
