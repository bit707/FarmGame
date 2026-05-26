using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class FarmGameSceneSetup : EditorWindow
{
    [MenuItem("FarmGame/一键搭建场景")]
    public static void SetupScene()
    {
        if (!EditorUtility.DisplayDialog("搭建场景", "将在当前场景中创建所有游戏对象，是否继续？", "确定", "取消"))
            return;

        CreateLighting();
        CreateGround();
        var player = CreatePlayer();
        CreateCamera(player.transform);
        CreateGameManager();
        CreateFarmArea();
        CreateUI();
        CreateShopNPC();

        EditorUtility.DisplayDialog("完成", "场景搭建完成！按 Play 即可测试。\n\nWASD移动，鼠标左键使用工具，右键旋转相机，E打开背包。", "好的");
    }

    static void CreateLighting()
    {
        var existing = GameObject.Find("Directional Light");
        if (existing != null) DestroyImmediate(existing);

        var sun = new GameObject("Sun");
        var light = sun.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        light.color = new Color(1f, 0.95f, 0.85f);
        light.shadows = LightShadows.Soft;
        light.shadowStrength = 0.6f;
        sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.53f, 0.71f, 0.95f);
        RenderSettings.ambientEquatorColor = new Color(0.6f, 0.6f, 0.5f);
        RenderSettings.ambientGroundColor = new Color(0.3f, 0.25f, 0.2f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.75f, 0.85f, 0.95f);
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 30f;
        RenderSettings.fogEndDistance = 80f;
    }

    static void CreateGround()
    {
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(10f, 1f, 10f);
        ground.isStatic = true;

        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0.35f, 0.55f, 0.2f);
        ground.GetComponent<Renderer>().sharedMaterial = mat;
        SaveAsset(mat, "Assets/Materials/Ground.mat");
    }

    static GameObject CreatePlayer()
    {
        var player = new GameObject("Player");
        player.transform.position = new Vector3(5f, 0f, 5f);
        player.layer = LayerMask.NameToLayer("Default");

        var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(player.transform);
        body.transform.localPosition = new Vector3(0f, 1f, 0f);
        DestroyImmediate(body.GetComponent<CapsuleCollider>());
        var bodyMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        bodyMat.color = new Color(0.3f, 0.5f, 0.8f);
        body.GetComponent<Renderer>().sharedMaterial = bodyMat;
        SaveAsset(bodyMat, "Assets/Materials/Player.mat");

        var hat = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        hat.name = "Hat";
        hat.transform.SetParent(player.transform);
        hat.transform.localPosition = new Vector3(0f, 2.1f, 0f);
        hat.transform.localScale = new Vector3(0.6f, 0.3f, 0.6f);
        DestroyImmediate(hat.GetComponent<SphereCollider>());
        var hatMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        hatMat.color = new Color(0.6f, 0.3f, 0.1f);
        hat.GetComponent<Renderer>().sharedMaterial = hatMat;
        SaveAsset(hatMat, "Assets/Materials/Hat.mat");

        var cc = player.AddComponent<CharacterController>();
        cc.center = new Vector3(0f, 1f, 0f);
        cc.height = 2f;
        cc.radius = 0.4f;

        player.AddComponent<PlayerController>();
        player.AddComponent<PlayerInteraction>();
        player.AddComponent<PlayerStamina>();

        var starter = player.AddComponent<StarterInventory>();
        var hoe = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/ScriptableObjects/Items/Hoe.asset");
        var can = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/ScriptableObjects/Items/WateringCan.asset");
        var scythe = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/ScriptableObjects/Items/Scythe.asset");
        if (hoe != null || can != null || scythe != null)
        {
            var starterSO = new SerializedObject(starter);
            if (hoe != null) starterSO.FindProperty("hoe").objectReferenceValue = hoe;
            if (can != null) starterSO.FindProperty("wateringCan").objectReferenceValue = can;
            if (scythe != null) starterSO.FindProperty("scythe").objectReferenceValue = scythe;

            string[] seedGuids = AssetDatabase.FindAssets("_Seed t:ItemData", new[] { "Assets/ScriptableObjects/Items" });
            var seedsProp = starterSO.FindProperty("starterSeeds");
            seedsProp.arraySize = seedGuids.Length;
            for (int i = 0; i < seedGuids.Length; i++)
            {
                var seed = AssetDatabase.LoadAssetAtPath<ItemData>(AssetDatabase.GUIDToAssetPath(seedGuids[i]));
                seedsProp.GetArrayElementAtIndex(i).objectReferenceValue = seed;
            }
            starterSO.ApplyModifiedProperties();
        }

        var highlight = GameObject.CreatePrimitive(PrimitiveType.Quad);
        highlight.name = "TileHighlight";
        highlight.transform.localScale = new Vector3(1f, 1f, 1f);
        highlight.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        highlight.transform.position = new Vector3(0f, 0.02f, 0f);
        DestroyImmediate(highlight.GetComponent<MeshCollider>());
        var hlMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        hlMat.color = new Color(1f, 1f, 0f, 0.3f);
        SetMaterialTransparent(hlMat);
        highlight.GetComponent<Renderer>().sharedMaterial = hlMat;
        SaveAsset(hlMat, "Assets/Materials/TileHighlight.mat");
        highlight.SetActive(false);

        var interaction = player.GetComponent<PlayerInteraction>();
        var so = new SerializedObject(interaction);
        so.FindProperty("tileHighlight").objectReferenceValue = highlight;
        so.ApplyModifiedProperties();

        return player;
    }

    static void CreateCamera(Transform target)
    {
        var existing = GameObject.Find("Main Camera");
        if (existing != null) DestroyImmediate(existing);

        var cam = new GameObject("Main Camera");
        cam.tag = "MainCamera";
        cam.transform.position = new Vector3(5f, 8f, -3f);
        cam.transform.rotation = Quaternion.Euler(45f, 0f, 0f);

        var camera = cam.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.5f, 0.7f, 1f);
        camera.nearClipPlane = 0.3f;
        camera.farClipPlane = 100f;

        cam.AddComponent<AudioListener>();

        var tpc = cam.AddComponent<ThirdPersonCamera>();
        var so = new SerializedObject(tpc);
        so.FindProperty("target").objectReferenceValue = target;
        so.ApplyModifiedProperties();
    }

    static void CreateGameManager()
    {
        var gm = new GameObject("GameManager");

        gm.AddComponent<GameManager>();

        var timeObj = new GameObject("TimeSystem");
        timeObj.transform.SetParent(gm.transform);
        var timeSys = timeObj.AddComponent<TimeSystem>();
        var timeSO = new SerializedObject(timeSys);
        var sun = GameObject.Find("Sun");
        if (sun != null)
            timeSO.FindProperty("directionalLight").objectReferenceValue = sun.GetComponent<Light>();
        timeSO.ApplyModifiedProperties();

        var weatherObj = new GameObject("WeatherSystem");
        weatherObj.transform.SetParent(gm.transform);
        weatherObj.AddComponent<WeatherSystem>();

        var invObj = new GameObject("InventorySystem");
        invObj.transform.SetParent(gm.transform);
        invObj.AddComponent<InventorySystem>();

        var shopObj = new GameObject("ShopSystem");
        shopObj.transform.SetParent(gm.transform);
        shopObj.AddComponent<ShopSystem>();

        var saveObj = new GameObject("SaveSystem");
        saveObj.transform.SetParent(gm.transform);
        saveObj.AddComponent<SaveSystem>();
    }

    static void CreateFarmArea()
    {
        var farmArea = new GameObject("FarmArea");
        farmArea.transform.position = new Vector3(10f, 0f, 10f);

        var visual = GameObject.CreatePrimitive(PrimitiveType.Plane);
        visual.name = "FarmSoil";
        visual.transform.SetParent(farmArea.transform);
        visual.transform.localPosition = new Vector3(10f, 0.005f, 10f);
        visual.transform.localScale = new Vector3(2f, 1f, 2f);
        visual.layer = 6;

        var soilMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        soilMat.color = new Color(0.4f, 0.28f, 0.15f);
        visual.GetComponent<Renderer>().sharedMaterial = soilMat;
        SaveAsset(soilMat, "Assets/Materials/FarmSoil.mat");

        var grid = farmArea.AddComponent<FarmGrid>();

        // fence posts around farm
        for (int i = 0; i <= 20; i += 2)
        {
            CreateFencePost(farmArea.transform, new Vector3(i, 0, 0));
            CreateFencePost(farmArea.transform, new Vector3(i, 0, 20));
            CreateFencePost(farmArea.transform, new Vector3(0, 0, i));
            CreateFencePost(farmArea.transform, new Vector3(20, 0, i));
        }
    }

    static void CreateFencePost(Transform parent, Vector3 localPos)
    {
        var post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        post.name = "FencePost";
        post.transform.SetParent(parent);
        post.transform.localPosition = localPos + new Vector3(0f, 0.4f, 0f);
        post.transform.localScale = new Vector3(0.1f, 0.4f, 0.1f);
        post.isStatic = true;
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0.5f, 0.35f, 0.15f);
        post.GetComponent<Renderer>().sharedMaterial = mat;
    }

    static void CreateShopNPC()
    {
        var npc = new GameObject("Shopkeeper");
        npc.transform.position = new Vector3(-5f, 0f, 5f);

        var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(npc.transform);
        body.transform.localPosition = new Vector3(0f, 1f, 0f);
        DestroyImmediate(body.GetComponent<CapsuleCollider>());
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0.8f, 0.4f, 0.2f);
        body.GetComponent<Renderer>().sharedMaterial = mat;
        SaveAsset(mat, "Assets/Materials/NPC.mat");

        var sign = new GameObject("InteractPrompt");
        sign.transform.SetParent(npc.transform);
        sign.transform.localPosition = new Vector3(0f, 2.5f, 0f);
        sign.SetActive(false);

        npc.AddComponent<NPCShopkeeper>();
        var so = new SerializedObject(npc.GetComponent<NPCShopkeeper>());
        so.FindProperty("interactPrompt").objectReferenceValue = sign;
        so.ApplyModifiedProperties();
    }

    static void CreateUI()
    {
        var canvas = new GameObject("Canvas");
        var c = canvas.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvas.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        canvas.AddComponent<GraphicRaycaster>();

        // HUD
        var hud = new GameObject("HUD");
        hud.transform.SetParent(canvas.transform, false);
        var hudRect = hud.AddComponent<RectTransform>();
        hudRect.anchorMin = Vector2.zero;
        hudRect.anchorMax = Vector2.one;
        hudRect.offsetMin = Vector2.zero;
        hudRect.offsetMax = Vector2.zero;

        // Time display
        var timeObj = CreateUIText(hud.transform, "TimeText", "06:00", TextAlignmentOptions.TopRight,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-20f, -10f), new Vector2(200f, 40f));

        // Date display
        var dateObj = CreateUIText(hud.transform, "DateText", "Spring Day 1", TextAlignmentOptions.TopRight,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-20f, -50f), new Vector2(200f, 30f));

        // Gold display
        var goldObj = CreateUIText(hud.transform, "GoldText", "500G", TextAlignmentOptions.TopLeft,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -10f), new Vector2(150f, 40f));

        // Hotbar
        var hotbar = new GameObject("Hotbar");
        hotbar.transform.SetParent(hud.transform, false);
        var hotbarRect = hotbar.AddComponent<RectTransform>();
        hotbarRect.anchorMin = new Vector2(0.5f, 0f);
        hotbarRect.anchorMax = new Vector2(0.5f, 0f);
        hotbarRect.pivot = new Vector2(0.5f, 0f);
        hotbarRect.anchoredPosition = new Vector2(0f, 20f);
        hotbarRect.sizeDelta = new Vector2(540f, 60f);
        var hlg = hotbar.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 4f;
        hlg.childAlignment = TextAnchor.MiddleCenter;

        // Stamina bar
        var staminaBg = new GameObject("StaminaBar");
        staminaBg.transform.SetParent(hud.transform, false);
        var staminaRect = staminaBg.AddComponent<RectTransform>();
        staminaRect.anchorMin = new Vector2(0f, 1f);
        staminaRect.anchorMax = new Vector2(0f, 1f);
        staminaRect.pivot = new Vector2(0f, 1f);
        staminaRect.anchoredPosition = new Vector2(20f, -60f);
        staminaRect.sizeDelta = new Vector2(200f, 20f);
        var bgImg = staminaBg.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        var fill = new GameObject("Fill");
        fill.transform.SetParent(staminaBg.transform, false);
        var fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        var fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.2f, 0.8f, 0.3f);
        staminaBg.AddComponent<StaminaBarUI>();

        // Pause menu panel
        var pausePanel = new GameObject("PausePanel");
        pausePanel.transform.SetParent(canvas.transform, false);
        var ppRect = pausePanel.AddComponent<RectTransform>();
        ppRect.anchorMin = Vector2.zero;
        ppRect.anchorMax = Vector2.one;
        ppRect.offsetMin = Vector2.zero;
        ppRect.offsetMax = Vector2.zero;
        var ppImg = pausePanel.AddComponent<Image>();
        ppImg.color = new Color(0f, 0f, 0f, 0.7f);
        pausePanel.SetActive(false);

        CreateUIText(pausePanel.transform, "PauseTitle", "PAUSED", TextAlignmentOptions.Center,
            new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), Vector2.zero, new Vector2(300f, 60f));

        var pauseMenu = canvas.AddComponent<PauseMenu>();
        var pmSO = new SerializedObject(pauseMenu);
        pmSO.FindProperty("pausePanel").objectReferenceValue = pausePanel;
        pmSO.ApplyModifiedProperties();

        // Inventory panel
        var invPanel = new GameObject("InventoryPanel");
        invPanel.transform.SetParent(canvas.transform, false);
        var ipRect = invPanel.AddComponent<RectTransform>();
        ipRect.anchorMin = new Vector2(0.5f, 0.5f);
        ipRect.anchorMax = new Vector2(0.5f, 0.5f);
        ipRect.sizeDelta = new Vector2(600f, 400f);
        var ipImg = invPanel.AddComponent<Image>();
        ipImg.color = new Color(0.15f, 0.15f, 0.15f, 0.95f);
        invPanel.SetActive(false);

        var invUI = canvas.AddComponent<InventoryUI>();
        var invSO = new SerializedObject(invUI);
        invSO.FindProperty("panel").objectReferenceValue = invPanel;
        invSO.ApplyModifiedProperties();

        // EventSystem
        if (GameObject.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
    }

    static GameObject CreateUIText(Transform parent, string name, string text, TextAlignmentOptions alignment,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = anchorMin;
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = size;
        var tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 24;
        tmp.alignment = alignment;
        tmp.color = Color.white;
        return obj;
    }

    static void SetMaterialTransparent(Material mat)
    {
        mat.SetFloat("_Surface", 1);
        mat.SetFloat("_Blend", 0);
        mat.SetOverrideTag("RenderType", "Transparent");
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
    }

    static void SaveAsset(Object asset, string path)
    {
        string dir = System.IO.Path.GetDirectoryName(path);
        if (!AssetDatabase.IsValidFolder(dir))
        {
            string[] parts = dir.Replace("\\", "/").Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
        AssetDatabase.CreateAsset(asset, path);
    }
}
