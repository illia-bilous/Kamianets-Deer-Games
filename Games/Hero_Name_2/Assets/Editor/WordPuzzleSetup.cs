#if UNITY_EDITOR
using HeroName.WordPuzzle;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HeroName.WordPuzzle.Editor
{
    public static class WordPuzzleSetup
    {
        const string LetterPrefabPath = "Assets/Prefabs/Square.prefab";

        const string PodilskyiPanelName = "Panel";
        const string RobinPanelName = "Panel (1)";
        const string GoodPanelName = "Panel (2)";
        const string PlayAreaName = "LetterPlayArea";

        [MenuItem("Hero Name/Setup Word Puzzle (All Steps)")]
        public static void SetupAll()
        {
            ConfigureResponsiveCanvas();
            AddManagersToScene();
            SetupSlotsInOpenScene();
            SetupPlayAreaAndSpawner();
            SetupResponsiveLayout();
            Debug.Log("Word puzzle setup complete. Press Play.");
        }

        [MenuItem("Hero Name/5. Make UI Fit All Screens")]
        public static void SetupResponsiveLayoutOnly()
        {
            ConfigureResponsiveCanvas();
            SetupResponsiveLayout();
            Debug.Log("Responsive UI configured. Test in Simulator with different devices.");
        }

        [MenuItem("Hero Name/1. Create UI Letter Prefab")]
        public static void CreateUiLetterPrefab()
        {
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(LetterPrefabPath);
        }

        [MenuItem("Hero Name/2. Setup Slot Images In Open Scene")]
        public static void SetupSlotsInOpenScene()
        {
            SetupPanelSlots(PodilskyiPanelName, WordPuzzlePhraseData.PodilskyiLetters);
            SetupPanelSlots(RobinPanelName, WordPuzzlePhraseData.RobinLetters);
            CleanupGoodPanel(GoodPanelName);
            SetupPanelSlots(GoodPanelName, WordPuzzlePhraseData.GoodLetters);

            Debug.Log("Slots: Panel=Подільський, Panel (1)=Робін, Panel (2)=Гуд.");
        }

        [MenuItem("Hero Name/3. Add Managers To Open Scene")]
        public static void AddManagersToScene()
        {
            if (Object.FindFirstObjectByType<PhraseManager>() == null)
            {
                var managersGo = new GameObject("Managers");
                managersGo.AddComponent<PhraseManager>();
                managersGo.AddComponent<GameManager>();
            }

            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                var eventSystemGo = new GameObject("EventSystem");
                eventSystemGo.AddComponent<EventSystem>();
                eventSystemGo.AddComponent<StandaloneInputModule>();
            }

            Debug.Log("Managers and EventSystem are ready.");
        }

        [MenuItem("Hero Name/4. Add Letter Spawner To Open Scene")]
        public static void SetupPlayAreaAndSpawner()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<DraggableLetter>(LetterPrefabPath);
            if (prefab == null)
            {
                Debug.LogWarning("Square.prefab with DraggableLetter not found.");
                return;
            }

            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("Scene needs a Canvas.");
                return;
            }

            var playAreaGo = GameObject.Find(PlayAreaName);
            if (playAreaGo == null)
            {
                playAreaGo = new GameObject(
                    PlayAreaName,
                    typeof(RectTransform),
                    typeof(Image),
                    typeof(LetterPlayArea));

                playAreaGo.transform.SetParent(canvas.transform, false);
            }

            ConfigurePlayArea(playAreaGo);

            RemoveLegacyLettersRoot(canvas.transform);

            var playArea = playAreaGo.GetComponent<LetterPlayArea>();
            if (playArea == null)
                playArea = playAreaGo.AddComponent<LetterPlayArea>();

            var existing = Object.FindFirstObjectByType<LetterSpawner>();
            LetterSpawner spawner;

            if (existing != null)
            {
                spawner = existing;
            }
            else
            {
                var spawnerGo = new GameObject("LetterSpawner");
                spawner = spawnerGo.AddComponent<LetterSpawner>();
            }

            var serializedSpawner = new SerializedObject(spawner);
            serializedSpawner.FindProperty("letterPrefab").objectReferenceValue = prefab;
            serializedSpawner.FindProperty("playArea").objectReferenceValue = playArea;
            serializedSpawner.ApplyModifiedPropertiesWithoutUndo();

            Debug.Log($"Letter spawn field: {PlayAreaName}. Spawner configured.");
        }

        static void RemoveLegacyLettersRoot(Transform canvasTransform)
        {
            var lettersRoot = canvasTransform.Find("LettersRoot");
            if (lettersRoot == null)
                return;

            Object.DestroyImmediate(lettersRoot.gameObject);
            Debug.Log("Removed old LettersRoot. Letters now spawn only inside LetterPlayArea.");
        }

        static void CleanupGoodPanel(string panelName)
        {
            var panel = GameObject.Find(panelName);
            if (panel == null)
                return;

            var playArea = panel.GetComponent<LetterPlayArea>();
            if (playArea != null)
                Object.DestroyImmediate(playArea);
        }

        static void ConfigurePlayArea(GameObject playAreaGo)
        {
            var layout = playAreaGo.GetComponent<HorizontalLayoutGroup>();
            if (layout != null)
                Object.DestroyImmediate(layout);

            var grid = playAreaGo.GetComponent<GridLayoutGroup>();
            if (grid != null)
                Object.DestroyImmediate(grid);

            var rect = playAreaGo.GetComponent<RectTransform>();
            // Position is driven by WordPuzzleUiLayout at runtime.
            rect.localScale = Vector3.one;

            var mask = playAreaGo.GetComponent<RectMask2D>();
            if (mask == null)
                playAreaGo.AddComponent<RectMask2D>();

            var image = playAreaGo.GetComponent<Image>();
            if (image == null)
                image = playAreaGo.AddComponent<Image>();

            image.color = new Color(1f, 1f, 1f, 0.2f);
            image.raycastTarget = false;

            if (playAreaGo.GetComponent<LetterPlayArea>() == null)
                playAreaGo.AddComponent<LetterPlayArea>();
        }

        static void SetupPanelSlots(string panelName, char[] expectedLetters)
        {
            var panel = GameObject.Find(panelName);
            if (panel == null)
            {
                Debug.LogWarning($"Panel '{panelName}' not found in scene.");
                return;
            }

            var panelImage = panel.GetComponent<Image>();
            if (panelImage != null)
                panelImage.raycastTarget = false;

            var slotIndex = 0;
            foreach (Transform child in panel.transform)
            {
                if (slotIndex >= expectedLetters.Length)
                    break;

                var image = child.GetComponent<Image>();
                if (image == null)
                    continue;

                image.raycastTarget = true;

                var slot = child.GetComponent<LetterSlot>();
                if (slot == null)
                    slot = child.gameObject.AddComponent<LetterSlot>();

                slot.Initialize(expectedLetters[slotIndex]);
                child.name = $"Slot_{expectedLetters[slotIndex]}";
                slotIndex++;
            }

            if (slotIndex != expectedLetters.Length)
            {
                Debug.LogWarning(
                    $"Panel '{panelName}' has {slotIndex} slot images, expected {expectedLetters.Length}.");
            }
        }

        static void ConfigureResponsiveCanvas()
        {
            var canvas = GetRootCanvas();
            if (canvas == null)
            {
                Debug.LogError("No Canvas found in scene.");
                return;
            }

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null)
                scaler = canvas.gameObject.AddComponent<CanvasScaler>();

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            var raycaster = canvas.GetComponent<GraphicRaycaster>();
            if (raycaster == null)
                canvas.gameObject.AddComponent<GraphicRaycaster>();

            Debug.Log($"Canvas Scaler set on '{canvas.name}' (1080x1920, Match 0.5).");
        }

        static void SetupResponsiveLayout()
        {
            var canvas = GetRootCanvas();
            if (canvas == null)
                return;

            var layoutRoot = canvas.GetComponent<WordPuzzleUiLayout>();
            if (layoutRoot == null)
                layoutRoot = canvas.gameObject.AddComponent<WordPuzzleUiLayout>();

            var serializedLayout = new SerializedObject(layoutRoot);
            AssignPanelBySlotCount(serializedLayout, "podilskyiPanel", WordPuzzlePhraseData.PodilskyiLetters.Length);
            AssignPanelBySlotCount(serializedLayout, "robinPanel", WordPuzzlePhraseData.RobinLetters.Length);
            AssignPanelBySlotCount(serializedLayout, "goodPanel", WordPuzzlePhraseData.GoodLetters.Length);
            AssignRectByName(serializedLayout, "letterPlayArea", PlayAreaName);
            serializedLayout.ApplyModifiedPropertiesWithoutUndo();

            layoutRoot.ApplyLayout();
        }

        static void AssignPanelBySlotCount(SerializedObject serializedLayout, string propertyName, int slotCount)
        {
            var canvas = GetRootCanvas();
            if (canvas == null)
                return;

            foreach (var rect in canvas.GetComponentsInChildren<RectTransform>(true))
            {
                if (rect.GetComponentsInChildren<LetterSlot>(true).Length != slotCount)
                    continue;

                serializedLayout.FindProperty(propertyName).objectReferenceValue = rect;
                return;
            }
        }

        static void AssignRectByName(SerializedObject serializedLayout, string propertyName, string objectName)
        {
            var canvas = GetRootCanvas();
            if (canvas == null)
                return;

            foreach (var transform in canvas.GetComponentsInChildren<Transform>(true))
            {
                if (transform.name != objectName)
                    continue;

                serializedLayout.FindProperty(propertyName).objectReferenceValue = transform;
                return;
            }
        }

        static Canvas GetRootCanvas()
        {
            var canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            Canvas rootCanvas = null;
            var smallestDepth = int.MaxValue;

            foreach (var canvas in canvases)
            {
                var depth = 0;
                var parent = canvas.transform.parent;
                while (parent != null)
                {
                    depth++;
                    parent = parent.parent;
                }

                if (depth >= smallestDepth)
                    continue;

                smallestDepth = depth;
                rootCanvas = canvas;
            }

            return rootCanvas;
        }
    }
}
#endif
