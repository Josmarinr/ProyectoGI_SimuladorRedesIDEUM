using UnityEngine;
using UnityEngine.EventSystems;

namespace SimRedes.Simulation
{
    public class PointerClickHandler : MonoBehaviour, IPointerClickHandler
    {
        private System.Action onClick;

        public void Initialize(System.Action callback)
        {
            onClick = callback;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            onClick?.Invoke();
        }
    }
}