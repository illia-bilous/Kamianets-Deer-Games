using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HeroName.WordPuzzle
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] GameObject winPanel;
        [SerializeField] TextMeshProUGUI winMessageText;
        [SerializeField] Text winMessageLegacyText;

        public bool IsWon { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (winPanel != null)
                winPanel.SetActive(false);
        }

        public void OnLetterPlacedCorrectly(DraggableLetter letter, LetterSlot slot)
        {
        }

        public void OnLetterPlaced(int currentCount, int totalRequired)
        {
        }

        public void TriggerWin()
        {
            if (IsWon)
                return;

            IsWon = true;
            SetWinMessage(WordPuzzlePhraseData.Phrase);

            if (winPanel != null)
                winPanel.SetActive(true);

            Debug.Log($"Win! Phrase complete: {WordPuzzlePhraseData.Phrase}");
        }

        void SetWinMessage(string message)
        {
            if (winMessageText != null)
                winMessageText.text = message;

            if (winMessageLegacyText != null)
                winMessageLegacyText.text = message;
        }

        public void ResetGame()
        {
            IsWon = false;

            if (winPanel != null)
                winPanel.SetActive(false);

            PhraseManager.Instance?.ResetProgress();

            foreach (var letter in FindObjectsByType<DraggableLetter>(FindObjectsSortMode.None))
                letter.UnlockAndReset();

            foreach (var slot in FindObjectsByType<LetterSlot>(FindObjectsSortMode.None))
                slot.ResetSlot();
        }
    }
}
