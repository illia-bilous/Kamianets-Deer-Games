using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeroName.WordPuzzle
{
    public class LetterSpawner : MonoBehaviour
    {
        [SerializeField] DraggableLetter letterPrefab;
        [SerializeField] LetterPlayArea playArea;
        [SerializeField] bool spawnOnStart = true;
        [SerializeField] bool clearExistingLetters = true;
        [SerializeField] float spawnPadding = 8f;
        [SerializeField] float minDistanceMultiplier = 1.05f;
        [SerializeField] int maxPlacementAttempts = 40;

        readonly List<DraggableLetter> spawnedLetters = new();

        void Start()
        {
            if (spawnOnStart)
                StartCoroutine(SpawnLettersWhenReady());
        }

        IEnumerator SpawnLettersWhenReady()
        {
            yield return null;
            Canvas.ForceUpdateCanvases();

            var uiLayout = FindFirstObjectByType<WordPuzzleUiLayout>();
            if (uiLayout != null)
                uiLayout.ApplyLayout();

            SpawnLetters();
        }

        public void SpawnLetters()
        {
            if (letterPrefab == null)
            {
                Debug.LogError("LetterSpawner: assign Square.prefab with DraggableLetter.");
                return;
            }

            if (playArea == null)
                playArea = LetterPlayArea.Instance;

            if (playArea == null)
            {
                Debug.LogError("LetterSpawner: LetterPlayArea not found in scene.");
                return;
            }

            playArea.EnsureLayoutReady();

            if (clearExistingLetters)
                ClearSpawnedLetters();

            var slotSize = WordPuzzleUiLayout.FindReferenceSlotSize();
            var minDistanceBetweenLetters = slotSize * minDistanceMultiplier;

            var letters = new List<char>(WordPuzzlePhraseData.Letters);
            Shuffle(letters);

            var usedPositions = new List<Vector2>();

            foreach (var letterChar in letters)
            {
                var letter = Instantiate(letterPrefab, playArea.transform);
                var rect = letter.GetComponent<RectTransform>();

                PrepareLetterRect(rect, slotSize);
                letter.name = $"Letter_{letterChar}";
                letter.Setup(letterChar);

                var position = FindSpreadPosition(rect, usedPositions, minDistanceBetweenLetters);
                rect.anchoredPosition = position;
                rect.localRotation = Quaternion.identity;
                rect.localScale = Vector3.one;
                usedPositions.Add(position);

                letter.RememberHome();
                spawnedLetters.Add(letter);
            }
        }

        static void PrepareLetterRect(RectTransform rect, float slotSize)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
            rect.sizeDelta = new Vector2(slotSize, slotSize);
        }

        Vector2 FindSpreadPosition(
            RectTransform letterRect,
            List<Vector2> usedPositions,
            float minDistanceBetweenLetters)
        {
            for (var attempt = 0; attempt < maxPlacementAttempts; attempt++)
            {
                var candidate = playArea.GetRandomLocalPosition(letterRect, spawnPadding);
                candidate = playArea.ClampLocalPosition(letterRect, candidate);

                if (IsFarEnough(candidate, usedPositions, minDistanceBetweenLetters))
                    return candidate;
            }

            return playArea.ClampLocalPosition(
                letterRect,
                playArea.GetRandomLocalPosition(letterRect, spawnPadding));
        }

        bool IsFarEnough(Vector2 candidate, List<Vector2> usedPositions, float minDistanceBetweenLetters)
        {
            foreach (var used in usedPositions)
            {
                if (Vector2.Distance(candidate, used) < minDistanceBetweenLetters)
                    return false;
            }

            return true;
        }

        public void ClearSpawnedLetters()
        {
            for (var i = spawnedLetters.Count - 1; i >= 0; i--)
            {
                if (spawnedLetters[i] != null)
                    Destroy(spawnedLetters[i].gameObject);
            }

            spawnedLetters.Clear();

            if (playArea == null)
                playArea = LetterPlayArea.Instance;

            if (playArea == null)
                return;

            var strayLetters = playArea.GetComponentsInChildren<DraggableLetter>(true);
            foreach (var letter in strayLetters)
            {
                if (letter != null)
                    Destroy(letter.gameObject);
            }
        }

        static void Shuffle(IList<char> list)
        {
            for (var i = list.Count - 1; i > 0; i--)
            {
                var j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
