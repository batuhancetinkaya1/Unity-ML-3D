using UnityEngine;

[CreateAssetMenu(menuName = "PushBlock/CollabSettings")]
public class PushBlockCollabSettings : ScriptableObject
{
    [Header("Hareket Ayarlarý")]
    public float moveSpeed = 2f;
    public float rotationSpeed = 200f;

    [Header("Spawn Ayarlarý")]
    public float spawnAreaMultiplier = 0.9f;
    [Range(0.1f, 1f)]
    public float spawnAreaMarginMultiplier = 0.9f;

    [Header("Ödül / Ceza Materyalleri")]
    public Material rewardMaterial;
    public Material punishMaterial;

    [Header("Zemin Temel Renk")]
    public Color groundBaseColor = Color.gray;

    [Header("Max Bölüm Süresi (saniye)")]
    public float maxEpisodeSteps = 5000;

    [Header("Blok Kütleleri")]
    public float lightBlockMass = 10f;
    public float midBlockMass = 100f;
    public float heavyBlockMass = 150f;

    [Header("Sahnedeki Blok Sayýlarý")]
    public int numberOfLightBlocks = 2;
    public int numberOfMidBlocks = 2;
    public int numberOfHeavyBlocks = 2;

    [Header("Ajan Sayýsý")]
    public int numberOfAgents = 3;
}
