using System.Collections.Generic;
using UnityEngine;

namespace FindInStones
{
    [DefaultExecutionOrder(0)]
    public class ItemSpawner : MonoBehaviour
    {
        [SerializeField] Transform[] spawnPoints;
        [SerializeField] CollectibleItem2D[] itemPrefabs;

        readonly List<CollectibleItem2D> _spawnedItems = new();

        void Start()
        {
            SpawnItems();
        }

        public void RespawnItems()
        {
            ClearSpawnedItems();
            SpawnItems();
        }

        void ClearSpawnedItems()
        {
            foreach (var item in _spawnedItems)
            {
                if (item != null)
                    Destroy(item.gameObject);
            }

            _spawnedItems.Clear();

            var leftover = FindObjectsByType<CollectibleItem2D>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            foreach (var item in leftover)
            {
                if (item != null)
                    Destroy(item.gameObject);
            }
        }

        void SpawnItems()
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogWarning("ItemSpawner: не задано точки спавну.");
                return;
            }

            if (itemPrefabs == null || itemPrefabs.Length == 0)
            {
                Debug.LogWarning("ItemSpawner: не задано префаби предметів.");
                return;
            }

            var shuffledItems = new List<CollectibleItem2D>(itemPrefabs);
            Shuffle(shuffledItems);

            var shuffledPoints = new List<Transform>(spawnPoints);
            shuffledPoints.RemoveAll(point => point == null);
            Shuffle(shuffledPoints);

            var spawnCount = Mathf.Min(shuffledPoints.Count, shuffledItems.Count);

            for (var i = 0; i < spawnCount; i++)
            {
                var spawnPoint = shuffledPoints[i];
                var item = Instantiate(shuffledItems[i], spawnPoint);
                item.transform.localPosition = Vector3.zero;
                item.transform.localRotation = Quaternion.identity;
                item.transform.localScale = Vector3.one;

                if (PlayAreaBounds.TryGetSpawnArea(out var min, out var max))
                {
                    var position = item.transform.position;
                    var clamped = new Vector3(
                        Mathf.Clamp(position.x, min.x, max.x),
                        Mathf.Clamp(position.y, min.y, max.y),
                        position.z);

                    if (clamped != position)
                    {
                        Debug.LogWarning(
                            $"ItemSpawner: {spawnPoint.name} поза Play Area — предмет зміщено всередину зони.");
                        item.transform.position = clamped;
                    }
                }

                var spriteRenderer = item.GetComponentInChildren<SpriteRenderer>();
                if (spriteRenderer != null)
                    spriteRenderer.sortingOrder = SortingOrderManager.ItemOrder;

                item.RememberStartTransform();
                _spawnedItems.Add(item);
            }
        }

        static void Shuffle<T>(IList<T> list)
        {
            for (var i = list.Count - 1; i > 0; i--)
            {
                var randomIndex = Random.Range(0, i + 1);
                (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
            }
        }
    }
}
