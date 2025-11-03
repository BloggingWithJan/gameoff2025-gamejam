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

        public GameObject GetToolPrefab()
        {
            return toolPrefab;
        }

        public string GetResourceTag()
        {
            return resourceTag;
        }
    }
}
