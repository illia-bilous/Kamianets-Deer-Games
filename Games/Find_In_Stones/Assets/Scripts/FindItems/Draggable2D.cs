using UnityEngine;
using UnityEngine.InputSystem;

namespace FindInStones
{
    public abstract class Draggable2D : MonoBehaviour
    {
        [SerializeField] protected SpriteRenderer spriteRenderer;

        protected Vector3 startLocalPosition;
        protected Transform startParent;
        protected int startSortingOrder;
        protected bool isDragging;

        public bool IsDragging => isDragging;

        protected virtual void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            RememberStartTransform();
        }

        public void RememberStartTransform()
        {
            startParent = transform.parent;
            startLocalPosition = transform.localPosition;

            if (spriteRenderer != null)
                startSortingOrder = spriteRenderer.sortingOrder;
        }

        public int GetSortingOrder()
        {
            if (spriteRenderer == null)
                return 0;

            return spriteRenderer.sortingOrder;
        }

        public int GetSortingLayerId()
        {
            if (spriteRenderer == null)
                return 0;

            return spriteRenderer.sortingLayerID;
        }

        public virtual bool CanBeDragged => true;

        public void BeginDrag(Vector3 worldPoint)
        {
            if (!CanBeDragged)
                return;

            isDragging = true;
            WorldDragInput.SetActiveDrag(this, worldPoint);
            BringToFront();

            if (spriteRenderer != null)
            {
                var color = spriteRenderer.color;
                color.a = 0.85f;
                spriteRenderer.color = color;
            }

            OnDragStarted();
        }

        public void BringToFront()
        {
            if (spriteRenderer == null)
                return;

            startSortingOrder = SortingOrderManager.BringToFront(spriteRenderer);
        }

        public void DragTo(Vector3 worldPoint, Vector3 offset)
        {
            if (!isDragging)
                return;

            var position = worldPoint + offset;
            var area = PlayAreaBounds.FindInScene();
            if (area != null)
                position = area.ClampPosition(position);

            transform.position = position;
        }

        public void EndDrag(Vector2 screenPoint)
        {
            if (!isDragging)
                return;

            isDragging = false;
            RestoreVisual();
            OnDragEnded(screenPoint);
        }

        public void CancelDrag()
        {
            if (!isDragging)
                return;

            isDragging = false;
            ReturnHome();
        }

        protected virtual void OnDragStarted()
        {
        }

        protected abstract void OnDragEnded(Vector2 screenPoint);

        protected void RestoreVisual()
        {
            if (spriteRenderer == null)
                return;

            startSortingOrder = spriteRenderer.sortingOrder;

            var color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
        }

        public virtual void ReturnHome()
        {
            transform.SetParent(startParent, false);
            transform.localPosition = startLocalPosition;
            RestoreVisual();
        }

        public static Vector3 ScreenToWorld(Vector2 screenPoint)
        {
            var camera = Camera.main;
            if (camera == null)
                return Vector3.zero;

            var point = new Vector3(screenPoint.x, screenPoint.y, -camera.transform.position.z);
            return camera.ScreenToWorldPoint(point);
        }
    }
}
