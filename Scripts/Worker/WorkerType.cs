using System;
using System.Xml.Serialization;
using UnityEngine;

namespace GameJam.Worker
{
    [CreateAssetMenu(fileName = "WorkerType", menuName = "WorkerType/New Worker Type")]
    public class WorkerType : ScriptableObject
    {
        [SerializeField]
        string typeName;

        [SerializeField]
        GameObject toolPrefab;

        [SerializeField]
        string resourceTag;

        [SerializeField]
        float gatherTime = 5f;

        [SerializeField]
        int gatherAmount = 2;

        [SerializeField]
        AudioClip gatherSound;

        public GameObject GetToolPrefab()
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
    }
}
