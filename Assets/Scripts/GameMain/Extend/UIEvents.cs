using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameMain
{
    public class UIEvents : MonoBehaviour, IPointerDownHandler, IPointerUpHandler,
        IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IBeginDragHandler, IEndDragHandler
    {
        public float longPressDuration = 1.0f;
        public Action<PointerEventData> OnPointerDownEvent;
        private bool m_IsPressing;


        private Action<float> OnPointerLongPressEvent;
        private bool m_IsListenerLongPress = false;
        private float m_CurLongPressTimer;

        public void AddListenerLongPressEvent(Action<float> action, float longPressDuration)
        {
            m_IsListenerLongPress = true;
            this.longPressDuration = longPressDuration;
            OnPointerLongPressEvent = action;
        }

        public void RemoveListenerLongPressEvent()
        {
            m_IsListenerLongPress = false;
            OnPointerLongPressEvent = null;
            m_CurLongPressTimer = 0;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {

        }

        public void OnEndDrag(PointerEventData eventData)
        {

        }

        public void OnPointerClick(PointerEventData eventData)
        {

        }

        public void OnPointerDown(PointerEventData eventData)
        {
            m_IsPressing = true;
            Debug.Log("BtnEvents OnPointerDown");
            OnPointerDownEvent?.Invoke(eventData);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {

        }

        public void OnPointerExit(PointerEventData eventData)
        {
            m_IsPressing = false;
            m_CurLongPressTimer = 0;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            m_IsPressing = false;
            m_CurLongPressTimer = 0;
        }

        private void Update()
        {
            if (m_IsListenerLongPress)
            {
                m_CurLongPressTimer += Time.deltaTime;
                if (m_IsPressing)
                {
                    OnPointerLongPressEvent?.Invoke(m_CurLongPressTimer);
                }
            }
        }
    }
}
