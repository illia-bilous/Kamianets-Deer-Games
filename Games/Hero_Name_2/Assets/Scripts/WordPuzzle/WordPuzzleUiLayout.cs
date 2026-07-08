using UnityEngine;
using UnityEngine.UI;

namespace HeroName.WordPuzzle
{
    [RequireComponent(typeof(RectTransform))]
    public class WordPuzzleUiLayout : MonoBehaviour
    {
        [SerializeField] RectTransform podilskyiPanel;
        [SerializeField] RectTransform robinPanel;
        [SerializeField] RectTransform goodPanel;
        [SerializeField] RectTransform letterPlayArea;
        [SerializeField] float minSlotSize = 28f;

        public float ReferenceSlotSize { get; private set; }

        public float GetReferenceSlotSize()
        {
            if (ReferenceSlotSize > minSlotSize)
                return ReferenceSlotSize;

            if (podilskyiPanel != null)
            {
                ReferenceSlotSize = CalculateSquareSlotSize(podilskyiPanel);
                return ReferenceSlotSize;
            }

            return minSlotSize;
        }

        public static float FindReferenceSlotSize()
        {
            var layout = FindFirstObjectByType<WordPuzzleUiLayout>();
            if (layout != null)
                return layout.GetReferenceSlotSize();

            foreach (var slot in FindObjectsByType<LetterSlot>(FindObjectsSortMode.None))
            {
                var size = Mathf.Max(slot.RectTransform.rect.width, slot.RectTransform.rect.height);
                if (size > 1f)
                    return size;
            }

            return 90f;
        }

        Canvas rootCanvas;

        void Awake()
        {
            rootCanvas = GetComponentInParent<Canvas>();
            CacheReferencesIfMissing();
            ApplyLayout();
        }

        void OnRectTransformDimensionsChange()
        {
            ApplyLayout();
        }

        void CacheReferencesIfMissing()
        {
            if (podilskyiPanel == null)
                podilskyiPanel = FindPanelBySlotCount(WordPuzzlePhraseData.PodilskyiLetters.Length);

            if (robinPanel == null)
                robinPanel = FindPanelBySlotCount(WordPuzzlePhraseData.RobinLetters.Length);

            if (goodPanel == null)
                goodPanel = FindPanelBySlotCount(WordPuzzlePhraseData.GoodLetters.Length);

            if (letterPlayArea == null)
            {
                var playArea = FindFirstObjectByType<LetterPlayArea>();
                if (playArea != null)
                    letterPlayArea = playArea.RectTransform;
            }
        }

        RectTransform FindPanelBySlotCount(int slotCount)
        {
            if (rootCanvas == null)
                rootCanvas = GetComponentInParent<Canvas>();

            if (rootCanvas == null)
                return null;

            foreach (var rect in rootCanvas.GetComponentsInChildren<RectTransform>(true))
            {
                if (rect.GetComponentsInChildren<LetterSlot>(true).Length == slotCount)
                    return rect;
            }

            return null;
        }

        public void ApplyLayout()
        {
            CacheReferencesIfMissing();

            ConfigurePanel(podilskyiPanel, new Vector2(0.02f, 0.74f), new Vector2(0.98f, 0.92f));
            ConfigurePanel(robinPanel, new Vector2(0.02f, 0.60f), new Vector2(0.62f, 0.73f));
            ConfigurePanel(goodPanel, new Vector2(0.64f, 0.60f), new Vector2(0.98f, 0.73f));

            if (letterPlayArea != null)
                StretchRect(letterPlayArea, new Vector2(0.02f, 0.04f), new Vector2(0.98f, 0.58f));

            ReferenceSlotSize = podilskyiPanel != null
                ? CalculateSquareSlotSize(podilskyiPanel)
                : ReferenceSlotSize;
        }

        void ConfigurePanel(RectTransform panel, Vector2 anchorMin, Vector2 anchorMax)
        {
            if (panel == null)
                return;

            StretchRect(panel, anchorMin, anchorMax);
            ConfigureHorizontalLayout(panel);
            ApplySquareSlotSize(panel, CalculateSquareSlotSize(panel));
        }

        float CalculateSquareSlotSize(RectTransform panel)
        {
            var slotCount = CountSlotChildren(panel);
            if (slotCount == 0)
                return minSlotSize;

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(panel);

            var layout = panel.GetComponent<HorizontalLayoutGroup>();
            var paddingHorizontal = layout != null ? layout.padding.horizontal : 0;
            var paddingVertical = layout != null ? layout.padding.vertical : 0;
            var spacing = layout != null ? layout.spacing : 4f;

            var availableWidth = panel.rect.width - paddingHorizontal - spacing * (slotCount - 1);
            var availableHeight = panel.rect.height - paddingVertical;

            var slotSize = Mathf.Min(availableWidth / slotCount, availableHeight);
            return Mathf.Max(slotSize, minSlotSize);
        }

        static int CountSlotChildren(RectTransform panel)
        {
            var slotCount = 0;
            foreach (Transform child in panel)
            {
                if (child.GetComponent<LetterSlot>() != null || child.GetComponent<Image>() != null)
                    slotCount++;
            }

            return slotCount;
        }

        static void StretchRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            rect.localScale = Vector3.one;
        }

        static void ConfigureHorizontalLayout(RectTransform panel)
        {
            var layout = panel.GetComponent<HorizontalLayoutGroup>();
            if (layout == null)
                layout = panel.gameObject.AddComponent<HorizontalLayoutGroup>();

            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 4f;
            layout.padding = new RectOffset(4, 4, 4, 4);
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
        }

        static void ApplySquareSlotSize(RectTransform panel, float slotSize)
        {
            foreach (Transform child in panel)
            {
                if (child.GetComponent<LetterSlot>() == null && child.GetComponent<Image>() == null)
                    continue;

                var childRect = child as RectTransform;
                if (childRect == null)
                    continue;

                var layoutElement = child.GetComponent<LayoutElement>();
                if (layoutElement == null)
                    layoutElement = child.gameObject.AddComponent<LayoutElement>();

                layoutElement.minWidth = slotSize;
                layoutElement.minHeight = slotSize;
                layoutElement.preferredWidth = slotSize;
                layoutElement.preferredHeight = slotSize;
                layoutElement.flexibleWidth = 0f;
                layoutElement.flexibleHeight = 0f;

                childRect.localScale = Vector3.one;

                var aspectFitter = child.GetComponent<AspectRatioFitter>();
                if (aspectFitter != null)
                    aspectFitter.enabled = false;
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(panel);
        }
    }
}
