using System.Collections;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class PushBlock_Collab : Agent
{
    [SerializeField] private PushBlockCollabSettings settings;
    [SerializeField] private MeshRenderer groundRenderer;
    private Rigidbody agentRb;
    public PushBlockCollabEnvController envController;
    private Quaternion startRot;

    public override void Initialize()
    {
        base.Initialize();
        agentRb = GetComponent<Rigidbody>();
        startRot = transform.rotation;
        if (envController == null)
        {
            envController = GetComponentInParent<PushBlockCollabEnvController>();
            if (envController == null) Debug.LogError("EnvController not found");
        }

        // Ajan� grup i�ine kaydet
        //envController.agentGroup.RegisterAgent(this);
    }

    public override void OnEpisodeBegin()
    {
        ResetAgentPosition();
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        MoveAgent(actionBuffers.DiscreteActions);
        envController.agentGroup.AddGroupReward(-1f / 5000);
    }

    void MoveAgent(ActionSegment<int> act)
    {
        Vector3 dirToGo = Vector3.zero;
        float rotateDir = 0f;
        int action = act[0];
        switch (action)
        {
            case 1: dirToGo = transform.forward; break;
            case 2: dirToGo = -transform.forward; break;
            case 3: rotateDir = 1f; break;
            case 4: rotateDir = -1f; break;
            case 5: dirToGo = -transform.right * 0.75f; break;
            case 6: dirToGo = transform.right * 0.75f; break;
        }
        transform.Rotate(0f, rotateDir * settings.rotationSpeed * Time.fixedDeltaTime, 0f);
        agentRb.AddForce(dirToGo * settings.moveSpeed, ForceMode.VelocityChange);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = 0;
        if (Input.GetKey(KeyCode.W)) discreteActionsOut[0] = 1;
        else if (Input.GetKey(KeyCode.S)) discreteActionsOut[0] = 2;
        else if (Input.GetKey(KeyCode.D)) discreteActionsOut[0] = 3;
        else if (Input.GetKey(KeyCode.A)) discreteActionsOut[0] = 4;
    }

    public void ResetAgentPosition()
    {
        agentRb.linearVelocity = Vector3.zero;
        agentRb.angularVelocity = Vector3.zero;
        Vector3 pos = envController.GetRandomSpawnPosition();
        transform.position = pos;
        transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
    }
}
