using UnityEngine;

namespace FindInStones
{
    public class FindItemsGameController : MonoBehaviour
    {
        const int TotalItems = 3;

        [SerializeField] GameObject victoryPanel;
        [SerializeField] UnityEngine.UI.Text progressText;

        int _placedCount;

        public static FindItemsGameController Instance { get; private set; }

        void Awake()
        {
            Instance = this;

            if (FindAnyObjectByType<WorldDragInput>() == null)
                gameObject.AddComponent<WorldDragInput>();
        }

        void Start()
        {
            if (victoryPanel != null)
                victoryPanel.SetActive(false);

            UpdateProgress();
        }

        public void RegisterPlacedItem()
        {
            _placedCount++;
            UpdateProgress();

            if (_placedCount >= TotalItems && victoryPanel != null)
                victoryPanel.SetActive(true);
        }

        public void RestartGame()
        {
            _placedCount = 0;

            if (victoryPanel != null)
                victoryPanel.SetActive(false);

            UpdateProgress();

            foreach (var slot in FindObjectsByType<InventorySlot>(FindObjectsSortMode.None))
                slot.ResetSlot();

            var itemSpawner = FindAnyObjectByType<ItemSpawner>();
            if (itemSpawner != null)
                itemSpawner.RespawnItems();

            var stoneSpawner = FindAnyObjectByType<StoneSpawner>();
            if (stoneSpawner != null)
                stoneSpawner.RespawnStones();
        }

        void UpdateProgress()
        {
            if (progressText != null)
                progressText.text = $"Знайдено: {_placedCount} з {TotalItems}";
        }
    }
}
