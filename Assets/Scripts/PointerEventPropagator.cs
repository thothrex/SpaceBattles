using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace SpaceBattles
{
    public class PointerEventPropagator : MonoBehaviour,
                                          IPointerDownHandler,
                                          IPointerUpHandler
    {
        public UnityEvent PointerDownEvent;
        public UnityEvent PointerUpEvent;

        public void OnPointerDown(PointerEventData eventData)
        {
            PointerDownEvent.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            PointerUpEvent.Invoke();
        }
    }
}
