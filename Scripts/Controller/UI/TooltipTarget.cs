using UnityEngine;
using UnityEngine.EventSystems;

namespace Controller.UI
{
    public class TooltipTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [TextArea] public string tooltipMessage;

        public void OnPointerEnter(PointerEventData eventData)
        {
            TooltipController.Instance.Show(tooltipMessage);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            TooltipController.Instance.Hide();
        }
    }
}