using Data;
using GameJam.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Controller.UI
{
    public class ArmyInfoPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public static ArmyInfoPanel Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private TMP_Text unitNameText;

        [Header("Fade Settings")]
        [SerializeField] private float fadeAlpha = 0.3f;
        [SerializeField] private float fadeSpeed = 5f;

        private RectTransform rect;
        private CanvasGroup canvasGroup;
        private bool hovering;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            rect = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();

            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!IsShown())
                return;

            UpdateAlpha();
        }

        private void UpdateAlpha()
        {
            //fade if hovered over so the player can see what he clicks on
            float targetAlpha = hovering ? fadeAlpha : 1f;
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
        }

        // NEW — called for multi-selection groups
        public void ShowPanel(int unitCount)
        {
            unitNameText.text = $"{unitCount} Units Selected";

            gameObject.SetActive(true);
        }

        public void HidePanel()
        {
            gameObject.SetActive(false);
        }

        public bool IsShown() => gameObject.activeSelf;

        public void OnPointerEnter(PointerEventData eventData) => hovering = true;
        public void OnPointerExit(PointerEventData eventData) => hovering = false;
    }
}