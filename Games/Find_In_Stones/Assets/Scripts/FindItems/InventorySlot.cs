using UnityEngine;
using UnityEngine.UI;

namespace FindInStones
{
    public class InventorySlot : MonoBehaviour
    {
        [SerializeField] ItemType acceptedType;
        [SerializeField] Image silhouetteImage;
        [SerializeField] Image filledImage;

        bool _isFilled;

        public ItemType AcceptedType => acceptedType;
        public bool IsFilled => _isFilled;
        public RectTransform RectTransform => (RectTransform)transform;

        public bool TryPlace(CollectibleItem2D item)
        {
            if (_isFilled || item == null || item.ItemType != acceptedType)
                return false;

            return PlaceItem();
        }

        bool PlaceItem()
        {
            _isFilled = true;

            if (silhouetteImage != null)
            {
                silhouetteImage.enabled = false;
                silhouetteImage.gameObject.SetActive(false);
            }

            if (filledImage != null)
            {
                filledImage.gameObject.SetActive(true);
                filledImage.enabled = true;
            }

            return true;
        }

        public void ResetSlot()
        {
            _isFilled = false;

            if (silhouetteImage != null)
            {
                silhouetteImage.gameObject.SetActive(true);
                silhouetteImage.enabled = true;
            }

            if (filledImage != null)
            {
                filledImage.enabled = false;
                filledImage.gameObject.SetActive(false);
            }
        }
    }
}
