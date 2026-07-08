using UnityEngine;
using UnityEngine.UI;

namespace HeroName.WordPuzzle
{
    [RequireComponent(typeof(Image))]
    public class LetterSlot : MonoBehaviour
    {
        [SerializeField] char expectedChar;

        bool isFilled;

        public char ExpectedChar => expectedChar;
        public bool IsFilled => isFilled;
        public RectTransform RectTransform => (RectTransform)transform;

        public void Initialize(char character)
        {
            expectedChar = character;
            ResetSlot();
        }

        public bool CanAccept(DraggableLetter letter)
        {
            return !isFilled && letter != null && letter.Letter == expectedChar;
        }

        public bool TryPlace(DraggableLetter letter)
        {
            if (!CanAccept(letter))
                return false;

            isFilled = true;
            letter.SnapToSlot(this);
            return true;
        }

        public void ResetSlot()
        {
            isFilled = false;
        }
    }
}
