using UnityEngine;
using UnityEngine.UI;

namespace HeroName.WordPuzzle
{
    [RequireComponent(typeof(RectTransform))]
    public class LetterPlayArea : MonoBehaviour
    {
        public static LetterPlayArea Instance { get; private set; }

        RectTransform rectTransform;

        public RectTransform RectTransform => rectTransform;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("Multiple LetterPlayArea instances found.");
                return;
            }

            Instance = this;
            rectTransform = GetComponent<RectTransform>();
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void EnsureLayoutReady()
        {
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        }

        public bool ContainsScreenPoint(Vector2 screenPoint, Camera eventCamera)
        {
            EnsureLayoutReady();
            return RectTransformUtility.RectangleContainsScreenPoint(
                rectTransform,
                screenPoint,
                eventCamera);
        }

        public bool ContainsRectTransform(RectTransform target, Camera eventCamera)
        {
            EnsureLayoutReady();

            var corners = new Vector3[4];
            target.GetWorldCorners(corners);

            var center = (corners[0] + corners[2]) * 0.5f;
            var screenPoint = RectTransformUtility.WorldToScreenPoint(eventCamera, center);
            return ContainsScreenPoint(screenPoint, eventCamera);
        }

        public Vector2 ClampLocalPosition(RectTransform letter, Vector2 localAnchoredPosition)
        {
            GetLocalBounds(letter, 0f, out var min, out var max);

            return new Vector2(
                Mathf.Clamp(localAnchoredPosition.x, min.x, max.x),
                Mathf.Clamp(localAnchoredPosition.y, min.y, max.y));
        }

        public Vector2 GetRandomLocalPosition(RectTransform letter, float padding = 10f)
        {
            GetLocalBounds(letter, padding, out var min, out var max);

            if (min.x > max.x || min.y > max.y)
                return rectTransform.rect.center;

            return new Vector2(
                Random.Range(min.x, max.x),
                Random.Range(min.y, max.y));
        }

        public Vector2 CanvasAnchoredToLocal(Vector2 canvasAnchoredPosition, RectTransform canvasRect)
        {
            EnsureLayoutReady();
            var worldPoint = canvasRect.TransformPoint(canvasAnchoredPosition);
            return rectTransform.InverseTransformPoint(worldPoint);
        }

        void GetLocalBounds(RectTransform letter, float padding, out Vector2 min, out Vector2 max)
        {
            EnsureLayoutReady();

            var letterSize = GetLetterSize(letter);
            var halfWidth = letterSize.x * letter.pivot.x;
            var halfWidthRemain = letterSize.x * (1f - letter.pivot.x);
            var halfHeight = letterSize.y * letter.pivot.y;
            var halfHeightRemain = letterSize.y * (1f - letter.pivot.y);

            var corners = new Vector3[4];
            rectTransform.GetLocalCorners(corners);

            var minX = Mathf.Min(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
            var maxX = Mathf.Max(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
            var minY = Mathf.Min(corners[0].y, corners[1].y, corners[2].y, corners[3].y);
            var maxY = Mathf.Max(corners[0].y, corners[1].y, corners[2].y, corners[3].y);

            min = new Vector2(
                minX + halfWidth + padding,
                minY + halfHeight + padding);

            max = new Vector2(
                maxX - halfWidthRemain - padding,
                maxY - halfHeightRemain - padding);
        }

        static Vector2 GetLetterSize(RectTransform letter)
        {
            if (letter.sizeDelta.sqrMagnitude > 0.01f)
                return letter.sizeDelta;

            return letter.rect.size;
        }
    }
}
