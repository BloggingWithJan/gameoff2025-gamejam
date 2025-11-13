using TMPro;
using UnityEngine;

namespace Data
{
    [System.Serializable]
    public class ResourceUIPanel
    {
        public ResourceType resource;
        public GameObject panel;
        public TMP_Text amountText;
    }
}