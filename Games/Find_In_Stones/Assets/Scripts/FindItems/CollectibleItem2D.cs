using UnityEngine;

namespace FindInStones
{
    [RequireComponent(typeof(Collider2D))]
    public class CollectibleItem2D : Draggable2D
    {
        [SerializeField] ItemType itemType;

        bool _isPlaced;
        Transform _spawnParent;
        Vector3 _spawnLocalPosition;

        public ItemType ItemType => itemType;
        public override bool CanBeDragged => !_isPlaced;

        protected override void Awake()
        {
            base.Awake();
            _spawnParent = startParent;
            _spawnLocalPosition = startLocalPosition;
        }

        protected override void OnDragEnded(Vector2 screenPoint)
        {
            var slot = FindSlotUnderScreenPoint(screenPoint);
            if (slot != null && slot.TryPlace(this))
            {
                _isPlaced = true;
                gameObject.SetActive(false);

                if (FindItemsGameController.Instance != null)
                    FindItemsGameController.Instance.RegisterPlacedItem();

                return;
            }

            var area = PlayAreaBounds.FindInScene();
            if (area != null)
                transform.position = area.ClampPosition(transform.position);

            RememberStartTransform();
        }

        InventorySlot FindSlotUnderScreenPoint(Vector2 screenPoint)
        {
            var slots = FindObjectsByType<InventorySlot>(FindObjectsSortMode.None);

            foreach (var slot in slots)
            {
                if (slot.IsFilled)
                    continue;

                if (RectTransformUtility.RectangleContainsScreenPoint(
                        slot.RectTransform,
                        screenPoint,
                        null))
                    return slot;
            }

            return null;
        }

        public void ResetItem()
        {
            _isPlaced = false;
            isDragging = false;
            gameObject.SetActive(true);
            transform.SetParent(_spawnParent, false);
            transform.localPosition = _spawnLocalPosition;

            var spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null)
                spriteRenderer.sortingOrder = SortingOrderManager.ItemOrder;

            RememberStartTransform();
        }
    }
}
