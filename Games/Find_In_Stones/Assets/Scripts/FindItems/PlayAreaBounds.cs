using UnityEngine;

namespace FindInStones
{
    [RequireComponent(typeof(BoxCollider2D))]
    [DefaultExecutionOrder(-100)]
    public class PlayAreaBounds : MonoBehaviour
    {
        static PlayAreaBounds _instance;

        BoxCollider2D _collider;

        [SerializeField] float spawnEdgePadding = 0.15f;

        public float SpawnEdgePadding => spawnEdgePadding;

        public static PlayAreaBounds Instance => _instance;

        public bool HasBounds => _collider != null;

        void Awake()
        {
            _instance = this;
            CacheCollider();
        }

        void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        void CacheCollider()
        {
            _collider = GetComponent<BoxCollider2D>();
            if (_collider != null)
                _collider.isTrigger = true;
        }

        public static PlayAreaBounds FindInScene()
        {
            if (Instance != null)
                return Instance;

            return FindAnyObjectByType<PlayAreaBounds>();
        }

        public static bool TryGetSpawnArea(out Vector2 min, out Vector2 max)
        {
            min = Vector2.zero;
            max = Vector2.zero;

            var area = FindInScene();
            if (area == null || !area.HasBounds)
                return false;

            var padding = area.SpawnEdgePadding;
            var bounds = area.GetWorldBounds();
            min = new Vector2(bounds.min.x + padding, bounds.min.y + padding);
            max = new Vector2(bounds.max.x - padding, bounds.max.y - padding);

            if (min.x >= max.x || min.y >= max.y)
            {
                min = bounds.min;
                max = bounds.max;
            }

            return true;
        }

        public Bounds GetWorldBounds()
        {
            if (_collider == null)
                CacheCollider();

            if (_collider == null)
                return new Bounds(transform.position, Vector3.zero);

            Physics2D.SyncTransforms();
            return _collider.bounds;
        }

        public bool Contains(Vector2 worldPoint)
        {
            if (_collider == null)
                return false;

            Physics2D.SyncTransforms();
            return _collider.OverlapPoint(worldPoint);
        }

        public Vector3 ClampPosition(Vector3 worldPosition)
        {
            if (_collider == null)
                return worldPosition;

            var bounds = GetWorldBounds();
            return new Vector3(
                Mathf.Clamp(worldPosition.x, bounds.min.x, bounds.max.x),
                Mathf.Clamp(worldPosition.y, bounds.min.y, bounds.max.y),
                worldPosition.z);
        }

        public Vector2 GetRandomPointInside()
        {
            if (!TryGetSpawnArea(out var min, out var max))
                return transform.position;

            return new Vector2(
                Random.Range(min.x, max.x),
                Random.Range(min.y, max.y));
        }

        void OnValidate()
        {
            CacheCollider();
        }

#if UNITY_EDITOR
        [ContextMenu("Виправити: Scale = 1, зберегти розмір зони")]
        void NormalizeColliderScale()
        {
            var col = GetComponent<BoxCollider2D>();
            if (col == null)
                return;

            Physics2D.SyncTransforms();
            var worldSize = col.bounds.size;
            var worldCenter = col.bounds.center;

            transform.localScale = Vector3.one;
            col.size = worldSize;
            col.offset = Vector2.zero;

            transform.position = new Vector3(worldCenter.x, worldCenter.y, transform.position.z);
        }
#endif

        void OnDrawGizmos()
        {
            var col = GetComponent<BoxCollider2D>();
            if (col == null)
                return;

            Physics2D.SyncTransforms();
            var bounds = col.bounds;

            Gizmos.color = new Color(0.2f, 0.9f, 0.4f, 0.35f);
            Gizmos.DrawCube(bounds.center, bounds.size);
            Gizmos.color = new Color(0.2f, 0.9f, 0.4f, 1f);
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
    }
}
