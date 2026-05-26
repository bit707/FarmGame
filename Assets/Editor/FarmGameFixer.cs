using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FarmGameFixer : EditorWindow
{
    [MenuItem("FarmGame/修复渲染和输入系统")]
    public static void FixAll()
    {
        FixURP();
        FixInputSystem();
        FixMaterials();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("修复完成",
            "1. URP 渲染管线已创建并分配\n" +
            "2. Input System 已设为 Both 模式\n" +
            "3. 场景材质已修复\n\n" +
            "请停止 Play 模式，然后重新 Play 测试。", "好的");
    }

    static void FixURP()
    {
        // Check if a pipeline asset already exists
        var existing = GraphicsSettings.currentRenderPipeline;
        if (existing != null)
        {
            Debug.Log("URP Pipeline Asset already assigned.");
            return;
        }

        EnsureFolder("Assets/Settings");

        // Create URP Renderer Data
        var rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
        rendererData.name = "URP_Renderer";
        AssetDatabase.CreateAsset(rendererData, "Assets/Settings/URP_Renderer.asset");

        // Create URP Pipeline Asset via ScriptableObject and link renderer
        var pipelineAsset = ScriptableObject.CreateInstance<UniversalRenderPipelineAsset>();
        pipelineAsset.name = "URP_PipelineAsset";
        AssetDatabase.CreateAsset(pipelineAsset, "Assets/Settings/URP_PipelineAsset.asset");

        // Link renderer to pipeline via SerializedObject
        var so = new SerializedObject(pipelineAsset);
        var rendererListProp = so.FindProperty("m_RendererDataList");
        if (rendererListProp != null)
        {
            rendererListProp.arraySize = 1;
            rendererListProp.GetArrayElementAtIndex(0).objectReferenceValue = rendererData;
            so.ApplyModifiedProperties();
        }

        // Assign to Graphics Settings
        GraphicsSettings.defaultRenderPipeline = pipelineAsset;
        QualitySettings.renderPipeline = pipelineAsset;

        EditorUtility.SetDirty(pipelineAsset);
        Debug.Log("URP Pipeline Asset created and assigned.");
    }

    static void FixInputSystem()
    {
        // Set active input handling to "Both" via PlayerSettings serialized property
        var playerSettings = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
        foreach (var obj in playerSettings)
        {
            var so = new SerializedObject(obj);
            var prop = so.FindProperty("activeInputHandler");
            if (prop != null && prop.intValue != 2)
            {
                prop.intValue = 2; // 0=Old, 1=New, 2=Both
                so.ApplyModifiedProperties();
                Debug.Log("Input System set to Both mode. Unity will need to restart.");
                EditorUtility.DisplayDialog("需要重启",
                    "Input System 已改为 Both 模式，Unity 需要重启才能生效。\n点确定后 Unity 会自动重启。", "确定");
                EditorApplication.OpenProject(System.IO.Directory.GetCurrentDirectory());
                return;
            }
        }
        Debug.Log("Input System already set correctly.");
    }

    static void FixMaterials()
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            Debug.LogError("Cannot find URP/Lit shader. Make sure URP is properly installed.");
            return;
        }

        // Fix all materials in Assets/Materials
        string[] guids = AssetDatabase.FindAssets("t:Material", new[] { "Assets/Materials" });
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null && (mat.shader == null || mat.shader.name.Contains("Error") || mat.shader.name == "Hidden/InternalErrorShader" || mat.shader.name == "Standard"))
            {
                Color color = mat.HasProperty("_Color") ? mat.color : Color.white;
                mat.shader = shader;
                mat.color = color;
                EditorUtility.SetDirty(mat);
                Debug.Log($"Fixed material: {path}");
            }
        }

        // Also fix materials on scene objects
        var renderers = GameObject.FindObjectsOfType<Renderer>();
        foreach (var r in renderers)
        {
            foreach (var mat in r.sharedMaterials)
            {
                if (mat != null && (mat.shader.name.Contains("Error") || mat.shader.name == "Hidden/InternalErrorShader"))
                {
                    Color color = mat.color;
                    mat.shader = shader;
                    mat.color = color;
                    EditorUtility.SetDirty(mat);
                }
            }
        }

        Debug.Log("Materials fixed.");
    }

    static void EnsureFolder(string path)
    {
        string[] parts = path.Replace("\\", "/").Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }
}
