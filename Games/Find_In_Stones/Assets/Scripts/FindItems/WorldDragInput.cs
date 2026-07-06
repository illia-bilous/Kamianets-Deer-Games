using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace FindInStones
{
    public class WorldDragInput : MonoBehaviour
    {
        static WorldDragInput _instance;

        Draggable2D _activeDrag;
        Vector3 _dragOffset;

        public static WorldDragInput Instance => _instance;

        void Awake()
        {
            _instance = this;
        }

        void Update()
        {
            var screenPos = GetScreenPosition();
            if (!screenPos.HasValue)
                return;

            var worldPos = Draggable2D.ScreenToWorld(screenPos.Value);
            var pressedThisFrame = IsPressedThisFrame();
            var isPressed = IsPressed();
            var releasedThisFrame = IsReleasedThisFrame();

            if (pressedThisFrame)
            {
                if (IsBlockingUi(screenPos.Value))
                    return;

                var topDraggable = FindTopDraggable(worldPos);
                topDraggable?.BeginDrag(worldPos);
            }
            else if (_activeDrag != null && _activeDrag.IsDragging && isPressed)
            {
                _activeDrag.DragTo(worldPos, _dragOffset);
            }
            else if (_activeDrag != null && _activeDrag.IsDragging && releasedThisFrame)
            {
                _activeDrag.EndDrag(screenPos.Value);
                _activeDrag = null;
            }
        }

        public static void SetActiveDrag(Draggable2D draggable, Vector3 worldPoint)
        {
            if (_instance == null)
                return;

            _instance._activeDrag = draggable;
            _instance._dragOffset = draggable.transform.position - worldPoint;
        }

        static Vector2? GetScreenPosition()
        {
            if (Pointer.current != null)
                return Pointer.current.position.ReadValue();

            if (Mouse.current != null)
                return Mouse.current.position.ReadValue();

            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
                return Touchscreen.current.primaryTouch.position.ReadValue();

            return null;
        }

        static bool IsPressedThisFrame()
        {
            if (Pointer.current != null)
                return Pointer.current.press.wasPressedThisFrame;

            if (Mouse.current != null)
                return Mouse.current.leftButton.wasPressedThisFrame;

            if (Touchscreen.current != null)
                return Touchscreen.current.primaryTouch.press.wasPressedThisFrame;

            return false;
        }

        static bool IsPressed()
        {
            if (Pointer.current != null)
                return Pointer.current.press.isPressed;

            if (Mouse.current != null)
                return Mouse.current.leftButton.isPressed;

            if (Touchscreen.current != null)
                return Touchscreen.current.primaryTouch.press.isPressed;

            return false;
        }

        static bool IsReleasedThisFrame()
        {
            if (Pointer.current != null)
                return Pointer.current.press.wasReleasedThisFrame;

            if (Mouse.current != null)
                return Mouse.current.leftButton.wasReleasedThisFrame;

            if (Touchscreen.current != null)
                return Touchscreen.current.primaryTouch.press.wasReleasedThisFrame;

            return false;
        }

        static bool IsBlockingUi(Vector2 screenPos)
        {
            if (EventSystem.current == null)
                return false;

            var pointerData = new PointerEventData(EventSystem.current)
            {
                position = screenPos
            };

            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            foreach (var result in results)
            {
                if (result.gameObject.layer != LayerMask.NameToLayer("UI"))
                    continue;

                var image = result.gameObject.GetComponent<UnityEngine.UI.Image>();
                if (image != null && image.raycastTarget)
                    return true;
            }

            return false;
        }

        static Draggable2D FindTopDraggable(Vector3 worldPos)
        {
            var hits = Physics2D.OverlapPointAll(worldPos);
            Draggable2D topDraggable = null;
            var topLayer = int.MinValue;
            var topOrder = int.MinValue;

            foreach (var hit in hits)
            {
                var draggable = hit.GetComponent<Draggable2D>();
                if (draggable == null || !draggable.CanBeDragged)
                    continue;

                var layer = draggable.GetSortingLayerId();
                var order = draggable.GetSortingOrder();

                if (layer > topLayer || (layer == topLayer && order >= topOrder))
                {
                    topLayer = layer;
                    topOrder = order;
                    topDraggable = draggable;
                }
            }

            return topDraggable;
        }
    }
}
