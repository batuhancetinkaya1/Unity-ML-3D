using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;

public class PushBlockCollabEnvController : MonoBehaviour
{
    [SerializeField] public PushBlockCollabSettings settings;
    [SerializeField] private MeshRenderer groundRenderer;
    [SerializeField] private Collider spawnArea;
    [SerializeField] private Vector3 spawnCheckBoxSize = new Vector3(1.5f, 0.1f, 1.5f);
    private Quaternion initialRotation;
    public List<PushBlock_Collab> agents = new List<PushBlock_Collab>();
    public List<BlockScriptCollab> blocks = new List<BlockScriptCollab>();
    private int blocksReachedGoal;

    public SimpleMultiAgentGroup agentGroup = new SimpleMultiAgentGroup();
    private int maxStepCounter;

    void Awake()
    {
        agentGroup = new SimpleMultiAgentGroup(); // AgentGroup baþlatýldý
    }

    void Start()
    {
        if (spawnArea == null) Debug.LogError("Spawn area collider is not assigned!");
        initialRotation = transform.rotation;
        agents.AddRange(GetComponentsInChildren<PushBlock_Collab>());
        blocks.AddRange(GetComponentsInChildren<BlockScriptCollab>());

        // Ajanlarý gruba ekle
        foreach (var agent in agents)
        {
            agent.envController = this;
            agentGroup.RegisterAgent(agent);
        }

        foreach (var block in blocks)
        {
            block.envController = this;
            block.isCollab = true;
        }

        ResetEnvironment(); // Baþlangýçta spawn yap
    }

    void FixedUpdate()
    {
        maxStepCounter++;

        // Max Step aþýlýrsa tüm grubu resetle
        if (maxStepCounter >= settings.maxEpisodeSteps)
        {
            agentGroup.GroupEpisodeInterrupted();
            ResetEnvironment();
        }
    }

    public Vector3 GetRandomSpawnPosition()
    {
        Physics.SyncTransforms();
        if (spawnArea == null) return Vector3.zero;
        Bounds b = spawnArea.bounds;
        float marginX = b.extents.x * settings.spawnAreaMarginMultiplier;
        float marginZ = b.extents.z * settings.spawnAreaMarginMultiplier;
        bool found = false;
        Vector3 finalPos = Vector3.zero;

        int maxAttempts = 100; // Sonsuz döngüyü önlemek için max deneme sayýsý
        int attempts = 0;

        while (!found && attempts < maxAttempts)
        {
            attempts++;
            float randomX = Random.Range(-marginX, marginX);
            float randomZ = Random.Range(-marginZ, marginZ);
            float y = b.center.y + 2f;
            Vector3 attemptPos = new Vector3(b.center.x + randomX, y, b.center.z + randomZ);

            if (!Physics.CheckBox(attemptPos, spawnCheckBoxSize, spawnArea.transform.rotation))
            {
                finalPos = attemptPos;
                found = true;
            }
        }

        if (!found)
        {
            Debug.LogWarning("Could not find a valid spawn position after 100 attempts.");
            return b.center; // Default konum döndür
        }

        return finalPos;
    }

    public void ResetEnvironment()
    {
        blocksReachedGoal = 0;
        maxStepCounter = 0;

        float[] angles = { 0f, 90f, 180f, 270f };
        float randomAngle = angles[Random.Range(0, angles.Length)];
        transform.rotation = initialRotation * Quaternion.Euler(0f, randomAngle, 0f);
        Physics.SyncTransforms();

        // **Ajanlarý Resetle**
        foreach (var agent in agents) agent.ResetAgentPosition();

        // **Bloklarý Resetle**
        foreach (var block in blocks) block.ResetBlock();
    }

    public void NotifyBlockReachedGoal(BlockType type)
    {
        float reward = (float)type;
        agentGroup.AddGroupReward(reward);
        blocksReachedGoal++;

        if (blocksReachedGoal >= blocks.Count)
        {
            agentGroup.AddGroupReward(10f);
            agentGroup.EndGroupEpisode();
            StartCoroutine(DelayedReset());
        }
    }

    IEnumerator DelayedReset()
    {
        yield return new WaitForSeconds(0.1f);
        ResetEnvironment();
    }
}
