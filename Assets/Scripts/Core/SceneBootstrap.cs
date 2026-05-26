using UnityEngine;

public class SceneBootstrap : MonoBehaviour
{
    [Header("Auto-generate scene for testing")]
    [SerializeField] bool autoGenerate = true;
    [SerializeField] Material groundMaterial;
    [SerializeField] Material farmSoilMaterial;

    void Awake()
    {
        if (!autoGenerate) return;
        CreateGround();
        CreatePlayer();
        CreateLighting();
        CreateFarmArea();
    }

    void CreateGround()
    {
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(10f, 1f, 10f);
        ground.layer = LayerMask.NameToLayer("Default");
        if (groundMaterial != null)
            ground.GetComponent<Renderer>().material = groundMaterial;
    }

    void CreatePlayer()
    {
        var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.transform.position = new Vector3(5f, 1f, 5f);
        Destroy(player.GetComponent<CapsuleCollider>());

        player.AddComponent<CharacterController>();
        player.AddComponent<PlayerController>();
        player.AddComponent<PlayerInteraction>();

        var cam = new GameObject("MainCamera");
        cam.tag = "MainCamera";
        cam.AddComponent<Camera>();
        cam.AddComponent<AudioListener>();
        cam.AddComponent<ThirdPersonCamera>();
    }

    void CreateLighting()
    {
        var sun = new GameObject("Sun");
        var light = sun.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        light.color = new Color(1f, 0.95f, 0.85f);
        light.shadows = LightShadows.Soft;
        sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.5f, 0.7f, 1f);
        RenderSettings.ambientEquatorColor = new Color(0.6f, 0.6f, 0.5f);
        RenderSettings.ambientGroundColor = new Color(0.3f, 0.25f, 0.2f);
    }

    void CreateFarmArea()
    {
        var farmArea = GameObject.CreatePrimitive(PrimitiveType.Plane);
        farmArea.name = "FarmArea";
        farmArea.transform.position = new Vector3(10f, 0.01f, 10f);
        farmArea.transform.localScale = new Vector3(2f, 1f, 2f);
        farmArea.layer = 6;
        if (farmSoilMaterial != null)
            farmArea.GetComponent<Renderer>().material = farmSoilMaterial;

        var grid = farmArea.AddComponent<FarmGrid>();
    }
}
