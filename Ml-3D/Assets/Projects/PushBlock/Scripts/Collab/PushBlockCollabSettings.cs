using UnityEngine;

[CreateAssetMenu(menuName = "PushBlock/CollabSettings")]
public class PushBlockCollabSettings : ScriptableObject
{
    [Header("Hareket Ayarlar�")]
    public float moveSpeed = 2f;
    public float rotationSpeed = 200f;

    [Header("Spawn Ayarlar�")]
    public float spawnAreaMultiplier = 0.9f;
    [Range(0.1f, 1f)]
    public float spawnAreaMarginMultiplier = 0.9f;

    [Header("�d�l / Ceza Materyalleri")]
    public Material rewardMaterial;
    public Material punishMaterial;

    [Header("Zemin Temel Renk")]
    public Color groundBaseColor = Color.gray;

    [Header("Max B�l�m S�resi (saniye)")]
    public float maxEpisodeSteps = 5000;

    [Header("Blok K�tleleri")]
    public float lightBlockMass = 10f;
    public float midBlockMass = 100f;
    public float heavyBlockMass = 150f;

    [Header("Sahnedeki Blok Say�lar�")]
    public int numberOfLightBlocks = 2;
    public int numberOfMidBlocks = 2;
    public int numberOfHeavyBlocks = 2;

    [Header("Ajan Say�s�")]
    public int numberOfAgents = 3;
}
