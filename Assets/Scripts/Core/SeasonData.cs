using UnityEngine;

[CreateAssetMenu(fileName = "NewSeasonData", menuName = "Farm/Season Data")]
public class SeasonData : ScriptableObject
{
    public Season season;
    public Color ambientColor = Color.white;
    public Color fogColor = new Color(0.7f, 0.8f, 0.9f);
    public float fogDensity = 0.01f;
    public Material skyboxMaterial;
    public AudioClip ambientSound;
    [Range(0f, 1f)] public float rainChance = 0.2f;
    public GameObject[] seasonalDecorations;
}
