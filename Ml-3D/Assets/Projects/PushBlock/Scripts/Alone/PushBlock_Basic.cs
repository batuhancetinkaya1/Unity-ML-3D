using System.Collections;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class PushBlock_Basic : Agent
{
    [Header("Ayarlar ve Referanslar")]
    [SerializeField] private SettingsPushBlocks settings;
    [SerializeField] private Transform blockTransform;
    [SerializeField] private Transform goalTransform;
    [SerializeField] private MeshRenderer groundRenderer;

    private Rigidbody agentRb;
    private Rigidbody blockRb;
    private Transform platformTransform;

    // Başlangıç konum/rotasyonlarını tutmak için
    private Vector3 agentStartPos;
    private Quaternion agentStartRot;
    private Vector3 blockStartPos;
    private Quaternion blockStartRot;
    private Quaternion platformStartRot;

    public override void Initialize()
    {
        // Rigidbody referanslarını alalım
        agentRb = GetComponent<Rigidbody>();
        if (blockTransform != null)
        {
            blockRb = blockTransform.GetComponent<Rigidbody>();
        }

        // Platform Transform’una ulaş (genellikle Agent’ın parent’ı)
        platformTransform = transform.parent;

        // Başlangıç konumlarını kaydet
        agentStartPos = transform.localPosition;
        agentStartRot = transform.localRotation;
        if (blockTransform != null)
        {
            blockStartPos = blockTransform.localPosition;
            blockStartRot = blockTransform.localRotation;
        }
        platformStartRot = platformTransform.localRotation;

        // Blok scriptine bu ajanın referansını ver (tekli ajan senaryosu).
        if (blockTransform != null)
        {
            var blockScript = blockTransform.GetComponent<BlockScript>();
            if (blockScript != null)
            {
                blockScript.singleAgent = this;
            }
        }
    }

    public override void OnEpisodeBegin()
    {
        // Platformu rastgele 0, 90, 180, 270 derece döndür
        float[] possibleAngles = { 0f, 90f, 180f, 270f };
        float randomAngle = possibleAngles[Random.Range(0, possibleAngles.Length)];
        platformTransform.localRotation = platformStartRot * Quaternion.Euler(0f, randomAngle, 0f);

        // Ajanı sıfırla
        agentRb.linearVelocity = Vector3.zero;
        agentRb.angularVelocity = Vector3.zero;

        // Bloğu sıfırla
        if (blockRb != null)
        {
            blockRb.linearVelocity = Vector3.zero;
            blockRb.angularVelocity = Vector3.zero;
        }

        // Rastgele konumlar al
        Vector3 randomAgentPosOffset = GetRandomSpawnOffset();
        Vector3 randomBlockPosOffset = GetRandomSpawnOffset();

        // Ajanı yeni konuma koy
        transform.localPosition = agentStartPos + randomAgentPosOffset;
        transform.localRotation = agentStartRot * Quaternion.Euler(0, Random.Range(0f, 360f), 0);

        // Bloğu yeni konuma koy
        if (blockTransform != null)
        {
            blockTransform.localPosition = blockStartPos + randomBlockPosOffset;
            blockTransform.localRotation = blockStartRot;
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // DiscreteActions ile hareket ettir
        MoveAgent(actionBuffers.DiscreteActions);

        // Her adım ufak bir ceza
        AddReward(-1f / MaxStep);
    }

    /// <summary>
    /// Birinci koddaki "MoveAgent" mantığına benzer şekilde
    /// ayrık aksiyonları işleyen fonksiyon.
    /// </summary>
    private void MoveAgent(ActionSegment<int> act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = 0f;

        int action = act[0]; // 0-6 arasındaki aksiyon değeri

        switch (action)
        {
            case 1: // İleri
                dirToGo = transform.forward;
                break;
            case 2: // Geri
                dirToGo = -transform.forward;
                break;
            case 3: // Sağa dön
                rotateDir = 1f;
                break;
            case 4: // Sola dön
                rotateDir = -1f;
                break;
            case 5: // Sola doğru yana kay
                dirToGo = -transform.right * 0.75f;
                break;
            case 6: // Sağa doğru yana kay
                dirToGo = transform.right * 0.75f;
                break;
                // 0 → Hiçbir şey yapma
        }

        // Rotasyonu uygula
        transform.Rotate(0f, rotateDir * settings.rotationSpeed * Time.fixedDeltaTime, 0f);

        // İleri-geri / yana kayma kuvvetini uygula
        agentRb.AddForce(dirToGo * settings.moveSpeed, ForceMode.VelocityChange);
    }

    /// <summary>
    /// Klavye ile manuel test (Heuristic).
    /// Ayrık aksiyonlarda 0 = "hiçbir şey yapma".
    /// </summary>
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = 0; // Varsayılan: hiç hareket yok

        if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = 1; // ileri
        }
        else if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[0] = 2; // geri
        }
        else if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[0] = 3; // sağa dön
        }
        else if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[0] = 4; // sola dön
        }
    }

    // Rastgele spawn offset'i üretmek için basit bir fonksiyon.
    private Vector3 GetRandomSpawnOffset()
    {
        float range = 6.5f * settings.spawnAreaMultiplier;
        return new Vector3(
            Random.Range(-range, range),
            0f,
            Random.Range(-range, range)
        );
    }

    /// <summary>
    /// Kısa süreliğine zeminin rengini değiştiren Coroutine.
    /// </summary>
    IEnumerator ShowTempColor(Material mat, float duration)
    {
        if (groundRenderer == null || mat == null)
            yield break;

        var originalMat = groundRenderer.material;
        groundRenderer.material = mat;
        yield return new WaitForSeconds(duration);
        groundRenderer.material = originalMat;
    }

    /// <summary>
    /// Blok hedefe ulaştığında çağrılır.
    /// </summary>
    internal void ScoredAGoal()
    {
        AddReward(5f);
        EndEpisode();

        StopAllCoroutines();
        StartCoroutine(ShowTempColor(settings.rewardMaterial, 0.25f));
    }
}
