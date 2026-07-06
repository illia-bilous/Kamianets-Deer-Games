using System.Collections.Generic;
using UnityEngine;

namespace FindInStones
{
    [DefaultExecutionOrder(10)]
    public class StoneSpawner : MonoBehaviour
    {
        [Header("Префаб")]
        [SerializeField] DraggableStone2D stonePrefab;
        [SerializeField] Transform stonesParent;

        [Header("Область спавну")]
        [SerializeField] PlayAreaBounds playAreaBounds;
        [SerializeField] bool usePlayAreaBounds = true;
        [SerializeField] bool useCameraBounds = true;
        [SerializeField] Vector2 areaMin = new(-8f, -3.5f);
        [SerializeField] Vector2 areaMax = new(8f, 3.5f);
        [SerializeField] float areaPadding = 0.4f;

        [Header("Кількість каміння")]
        [Tooltip("1 = норма. 0.5 — рідше, 2 — дуже густо.")]
        [SerializeField, Range(0.3f, 2f)] float density = 1f;

        [Tooltip("Більше значення = менше каміння в сітці по всій мапі.")]
        [SerializeField, Min(0.4f)] float gridCellSize = 0.95f;

        [Tooltip("Додаткові каміння випадково поверх сітки.")]
        [SerializeField, Min(0)] int extraRandomStones = 40;

        [Tooltip("Скільки каміння накласти на кожен предмет.")]
        [SerializeField, Min(0)] int coverStonesPerItem = 4;

        [Tooltip("Радіус розкиду каміння над предметом.")]
        [SerializeField, Min(0f)] float coverStoneRadius = 0.55f;

        [Header("Вигляд")]
        [Tooltip("3 спрайти каміння — при спавні обирається один випадково.")]
        [SerializeField] Sprite[] stoneVisuals;

        [SerializeField] Vector2 scaleRange = new(0.85f, 1.3f);

        readonly List<DraggableStone2D> _spawnedStones = new();

        public int LastSpawnedCount => _spawnedStones.Count;
        public float Density
        {
            get => density;
            set => density = Mathf.Clamp(value, 0.3f, 2f);
        }

        void Start()
        {
            SpawnStones();
        }

        public void RespawnStones()
        {
            ClearStones();
            SpawnStones();
        }

        void ClearStones()
        {
            foreach (var stone in _spawnedStones)
            {
                if (stone != null)
                    Destroy(stone.gameObject);
            }

            _spawnedStones.Clear();
        }

        void SpawnStones()
        {
            if (stonePrefab == null)
            {
                Debug.LogWarning("StoneSpawner: не задано префаб каміння.");
                return;
            }

            if (!TryApplyPlayAreaBounds())
            {
                if (useCameraBounds)
                    CalculateAreaFromCamera();
                else
                    Debug.LogWarning("StoneSpawner: немає Play Area — каміння не заспавнено.");
                    return;
            }

            SortingOrderManager.Reset();

            var parent = stonesParent != null ? stonesParent : transform;
            var effectiveDensity = Mathf.Max(0.3f, density);
            var effectiveCellSize = gridCellSize / effectiveDensity;
            var effectiveExtra = Mathf.RoundToInt(extraRandomStones * effectiveDensity);
            var effectiveCover = Mathf.RoundToInt(coverStonesPerItem * effectiveDensity);

            SpawnGrid(parent, effectiveCellSize);
            SpawnRandomStones(parent, effectiveExtra);
            SpawnCoverStonesOverItems(parent, effectiveCover);
        }

        void SpawnGrid(Transform parent, float cellSize)
        {
            var width = areaMax.x - areaMin.x;
            var height = areaMax.y - areaMin.y;
            var columns = Mathf.Max(1, Mathf.CeilToInt(width / cellSize));
            var rows = Mathf.Max(1, Mathf.CeilToInt(height / cellSize));

            for (var row = 0; row < rows; row++)
            {
                for (var column = 0; column < columns; column++)
                {
                    var x = areaMin.x + (column + Random.Range(0.15f, 0.85f)) / columns * width;
                    var y = areaMin.y + (row + Random.Range(0.15f, 0.85f)) / rows * height;
                    SpawnStoneAt(new Vector3(x, y, 0f), parent);
                }
            }
        }

        void SpawnRandomStones(Transform parent, int count)
        {
            for (var i = 0; i < count; i++)
            {
                var position = new Vector3(
                    Random.Range(areaMin.x, areaMax.x),
                    Random.Range(areaMin.y, areaMax.y),
                    0f);

                SpawnStoneAt(position, parent);
            }
        }

        void SpawnCoverStonesOverItems(Transform parent, int stonesPerItem)
        {
            if (stonesPerItem <= 0)
                return;

            var items = FindObjectsByType<CollectibleItem2D>(FindObjectsSortMode.None);

            foreach (var item in items)
            {
                if (item == null)
                    continue;

                for (var i = 0; i < stonesPerItem; i++)
                {
                    var offset = Random.insideUnitCircle * coverStoneRadius;
                    var position = item.transform.position + new Vector3(offset.x, offset.y, 0f);
                    position = ClampToPlayArea(position);
                    SpawnStoneAt(position, parent);
                }
            }
        }

        Vector3 ClampToPlayArea(Vector3 position)
        {
            var area = playAreaBounds != null ? playAreaBounds : PlayAreaBounds.FindInScene();
            return area != null ? area.ClampPosition(position) : position;
        }

        bool TryApplyPlayAreaBounds()
        {
            if (!usePlayAreaBounds)
                return false;

            if (!PlayAreaBounds.TryGetSpawnArea(out var min, out var max))
            {
                Debug.LogWarning(
                    "StoneSpawner: додай об'єкт PlayArea з компонентом PlayAreaBounds (Box Collider 2D).");
                return false;
            }

            areaMin = min;
            areaMax = max;
            return true;
        }

        public void SetSpawnBounds(Vector2 min, Vector2 max)
        {
            areaMin = min;
            areaMax = max;
        }

        DraggableStone2D SpawnStoneAt(Vector3 worldPosition, Transform parent)
        {
            worldPosition = ClampToPlayArea(worldPosition);

            var stone = Instantiate(stonePrefab, worldPosition, Quaternion.identity, parent);
            stone.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

            var scale = Random.Range(scaleRange.x, scaleRange.y);
            stone.transform.localScale = Vector3.one * scale;

            ApplyRandomVisual(stone);

            var spriteRenderer = stone.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null)
                spriteRenderer.sortingOrder = SortingOrderManager.GetNext();

            stone.RememberStartTransform();
            _spawnedStones.Add(stone);
            return stone;
        }

        void ApplyRandomVisual(DraggableStone2D stone)
        {
            if (stoneVisuals == null || stoneVisuals.Length == 0)
                return;

            var sprite = stoneVisuals[Random.Range(0, stoneVisuals.Length)];
            if (sprite == null)
                return;

            var spriteRenderer = stone.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null)
                spriteRenderer.sprite = sprite;
        }

        void CalculateAreaFromCamera()
        {
            var camera = Camera.main;
            if (camera == null || !camera.orthographic)
                return;

            var height = camera.orthographicSize;
            var width = height * camera.aspect;
            var center = camera.transform.position;

            areaMin = new Vector2(
                center.x - width + areaPadding,
                center.y - height + areaPadding);
            areaMax = new Vector2(
                center.x + width - areaPadding,
                center.y + height - areaPadding);
        }
    }
}
