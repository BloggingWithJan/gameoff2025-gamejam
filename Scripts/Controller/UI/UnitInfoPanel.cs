using Data;
using GameJam.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Controller.UI
{
    public class UnitInfoPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public static UnitInfoPanel Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private TMP_Text unitNameText;
        [SerializeField] private TMP_Text attackText;
        [SerializeField] private TMP_Text statusText;

        [Header("Follow Settings")]
        [SerializeField] private Vector3 offset = new Vector3(0, 1f, 0);

        [Header("Fade Settings")]
        [SerializeField] private float fadeAlpha = 0.3f;
        [SerializeField] private float fadeSpeed = 5f;

        private Transform followTarget;
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
            if (!IsShown() || followTarget == null)
                return;

            UpdateAlpha();

            UpdatePosition();
        }

        private void UpdateAlpha()
        {
            //fade if hovered over so the player can see what he clicks on
            float targetAlpha = hovering ? fadeAlpha : 1f;
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
        }

        private void UpdatePosition()
        {
            if (followTarget == null) return;

            Vector3 screenPos = Camera.main.WorldToScreenPoint(followTarget.position + offset);
            rect.position = screenPos;
        }

        public void ShowPanel(Health unit)
        {
            if (unit == null) return;

            unitNameText.text = unit.tag;
            // TODO: Fill in real stats when available
            // attackText.text = ...
            // statusText.text = ...

            followTarget = unit.transform;
            UpdatePosition();
            gameObject.SetActive(true);
        }

        public void HidePanel()
        {
            followTarget = null;
            gameObject.SetActive(false);
        }

        public bool IsShown() => gameObject.activeSelf;

        public void OnPointerEnter(PointerEventData eventData) => hovering = true;
        public void OnPointerExit(PointerEventData eventData) => hovering = false;
    }
}