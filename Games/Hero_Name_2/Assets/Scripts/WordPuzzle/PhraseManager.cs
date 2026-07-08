using UnityEngine;

namespace HeroName.WordPuzzle
{
    public class PhraseManager : MonoBehaviour
    {
        public static PhraseManager Instance { get; private set; }

        [SerializeField] int totalLettersRequired = WordPuzzlePhraseData.TotalLetters;

        int correctCount;

        public int CorrectCount => correctCount;
        public int TotalRequired => totalLettersRequired;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void RegisterCorrectPlacement()
        {
            if (correctCount >= totalLettersRequired)
                return;

            correctCount++;
            GameManager.Instance?.OnLetterPlaced(correctCount, totalLettersRequired);

            if (correctCount >= totalLettersRequired)
                GameManager.Instance?.TriggerWin();
        }

        public void ResetProgress()
        {
            correctCount = 0;
        }
    }
}
