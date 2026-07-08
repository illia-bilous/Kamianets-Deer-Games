using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HeroName.WordPuzzle
{
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(RectTransform))]
    public class DraggableLetter : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] TextMeshProUGUI letterText;
        [SerializeField] Text letterLegacyText;
        [SerializeField] float returnDuration = 0.3f;
        [SerializeField] float shakeDuration = 0.22f;
        [SerializeField] float shakeStrength = 10f;
        [SerializeField] Color errorColor = Color.red;

        CanvasGroup canvasGroup;
        RectTransform rectTransform;
        RectTransform canvasRect;
        LetterPlayArea playArea;
        Transform restParent;
        Vector2 restAnchoredPosition;
        int restSiblingIndex;
        Vector2 dragOffset;
        bool isLocked;
        Coroutine motionCoroutine;
        Color originalColor;

        public char Letter
        {
            get
            {
                if (letterText != null && letterText.text.Length > 0)
                    return letterText.text[0];

                if (letterLegacyText != null && letterLegacyText.text.Length > 0)
                    return letterLegacyText.text[0];

                return '\0';
            }
        }

        public bool IsLocked => isLocked;

        void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            rectTransform = GetComponent<RectTransform>();

            if (letterText == null)
                letterText = GetComponentInChildren<TextMeshProUGUI>();

            if (letterLegacyText == null)
                letterLegacyText = GetComponentInChildren<Text>();

            originalColor = GetTextColor();
            RememberHome();
        }

        void Start()
        {
            playArea = LetterPlayArea.Instance;
            if (playArea == null)
                playArea = GetComponentInParent<LetterPlayArea>();

            var canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
                canvasRect = canvas.transform as RectTransform;
        }

        public void Setup(char letter)
        {
            var text = letter.ToString();

            if (letterText != null)
                letterText.text = text;

            if (letterLegacyText != null)
                letterLegacyText.text = text;
        }

        public void RememberHome()
        {
            restParent = transform.parent;
            restAnchoredPosition = rectTransform.anchoredPosition;
            restSiblingIndex = transform.GetSiblingIndex();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (isLocked || canvasRect == null)
                return;

            StopMotion();

            restParent = transform.parent;
            restAnchoredPosition = rectTransform.anchoredPosition;
            restSiblingIndex = transform.GetSiblingIndex();

            transform.SetParent(canvasRect, true);
            transform.SetAsLastSibling();

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect,
                    eventData.position,
                    eventData.pressEventCamera,
                    out var localPointer))
            {
                dragOffset = rectTransform.anchoredPosition - localPointer;
            }

            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0.9f;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (isLocked || canvasRect == null)
                return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect,
                    eventData.position,
                    eventData.pressEventCamera,
                    out var localPointer))
            {
                rectTransform.anchoredPosition = localPointer + dragOffset;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (isLocked)
                return;

            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;

            var slot = FindSlotUnderPointer(eventData);

            if (slot != null && slot.TryPlace(this))
            {
                isLocked = true;
                GameManager.Instance?.OnLetterPlacedCorrectly(this, slot);
                PhraseManager.Instance?.RegisterCorrectPlacement();
                return;
            }

            if (slot != null || !IsDropInsidePlayArea(eventData))
            {
                motionCoroutine = StartCoroutine(PlayErrorFeedbackAndReturn());
                return;
            }

            PlaceFreelyInsidePlayArea();
        }

        void PlaceFreelyInsidePlayArea()
        {
            if (playArea == null || canvasRect == null)
                return;

            var localInPlayArea = playArea.CanvasAnchoredToLocal(rectTransform.anchoredPosition, canvasRect);
            localInPlayArea = playArea.ClampLocalPosition(rectTransform, localInPlayArea);

            transform.SetParent(playArea.transform, false);
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = localInPlayArea;

            RememberHome();
        }

        bool IsDropInsidePlayArea(PointerEventData eventData)
        {
            if (playArea == null)
                return true;

            return playArea.ContainsRectTransform(rectTransform, eventData.pressEventCamera);
        }

        LetterSlot FindSlotUnderPointer(PointerEventData eventData)
        {
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            foreach (var result in results)
            {
                if (result.gameObject == gameObject)
                    continue;

                var slot = result.gameObject.GetComponent<LetterSlot>();
                if (slot != null)
                    return slot;
            }

            return null;
        }

        public void SnapToSlot(LetterSlot slot)
        {
            StopMotion();

            transform.SetParent(slot.transform, false);
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.localScale = Vector3.one;

            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 1f;
            SetTextColor(originalColor);
        }

        IEnumerator PlayErrorFeedbackAndReturn()
        {
            var from = rectTransform.anchoredPosition;
            var elapsed = 0f;

            while (elapsed < shakeDuration)
            {
                elapsed += Time.deltaTime;
                var dampening = 1f - elapsed / shakeDuration;
                var offsetX = Mathf.Sin(elapsed * 40f) * shakeStrength * dampening;
                rectTransform.anchoredPosition = from + new Vector2(offsetX, 0f);
                SetTextColor(errorColor);
                yield return null;
            }

            SetTextColor(originalColor);
            yield return ReturnToRestPosition();
        }

        IEnumerator ReturnToRestPosition()
        {
            if (restParent == null)
            {
                motionCoroutine = null;
                yield break;
            }

            transform.SetParent(restParent, true);

            var from = rectTransform.anchoredPosition;
            var elapsed = 0f;

            while (elapsed < returnDuration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.SmoothStep(0f, 1f, elapsed / returnDuration);
                rectTransform.anchoredPosition = Vector2.Lerp(from, restAnchoredPosition, t);
                yield return null;
            }

            transform.SetParent(restParent, false);
            transform.SetSiblingIndex(restSiblingIndex);
            rectTransform.anchoredPosition = restAnchoredPosition;
            motionCoroutine = null;
        }

        public void UnlockAndReset()
        {
            isLocked = false;
            StopMotion();

            if (restParent != null)
            {
                transform.SetParent(restParent, false);
                transform.SetSiblingIndex(restSiblingIndex);
                rectTransform.anchoredPosition = restAnchoredPosition;
            }

            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;
            SetTextColor(originalColor);
        }

        Color GetTextColor()
        {
            if (letterText != null)
                return letterText.color;

            if (letterLegacyText != null)
                return letterLegacyText.color;

            return Color.white;
        }

        void SetTextColor(Color color)
        {
            if (letterText != null)
                letterText.color = color;

            if (letterLegacyText != null)
                letterLegacyText.color = color;
        }

        void StopMotion()
        {
            if (motionCoroutine == null)
                return;

            StopCoroutine(motionCoroutine);
            motionCoroutine = null;
        }
    }
}
