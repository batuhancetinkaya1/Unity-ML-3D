using System.Collections;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class BasicAgent : Agent
{
    [Header("References")]
    private Rigidbody agentRb;
    public MeshRenderer groundRenderer;
    public GameObject rewardBig;
    public GameObject rewardSmall;
    public GameObject punishment;

    [Header("Episode")]
    public float maxTime = 10f;
    private float startTime;

    [Header("Rewards")]
    public float bigReward = 1f;
    public float smallReward = 0.5f;
    public float punishmentReward = -2f;
    public float approachMultiplier = 0.01f;
    public float approachDistance = 2f;

    [Header("Movement")]
    public float moveSpeed = 4f;
    public float rotateSpeed = 200f;

    [Header("Ground/Visuals")]
    public Material bigRewardMaterial;
    public Material smallRewardMaterial;
    public Material punishmentMaterial;
    public Color groundBaseColor;

    // For normalization (ground is 15x15 => half is 7.5)
    private float halfSize = 7.5f;
    private float maxVelocity = 10f; // for velocity normalization

    public override void Initialize()
    {
        agentRb = GetComponent<Rigidbody>();
        groundBaseColor = groundRenderer.material.color;
    }

    public override void OnEpisodeBegin()
    {
        startTime = Time.time;

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        agentRb.linearVelocity = Vector3.zero;
        agentRb.angularVelocity = Vector3.zero;

        RelocateObjects();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var actions = actionsOut.ContinuousActions;
        actions[0] = Input.GetAxis("Horizontal"); // rotation
        actions[1] = Input.GetAxis("Vertical");   // forward / brake
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Agent position (normalized)
        Vector3 pos = transform.localPosition;
        sensor.AddObservation(Mathf.Clamp(pos.x / halfSize, -1f, 1f));
        sensor.AddObservation(Mathf.Clamp(pos.z / halfSize, -1f, 1f));
        sensor.AddObservation(Mathf.Clamp(pos.y / 2f, -1f, 1f));

        // Forward direction (already normalized, but add directly)
        sensor.AddObservation(transform.forward);

        // Objects positions (normalized, x/z only if they stay on ground)
        Vector3 bPos = rewardBig.transform.localPosition;
        sensor.AddObservation(Mathf.Clamp(bPos.x / halfSize, -1f, 1f));
        sensor.AddObservation(Mathf.Clamp(bPos.z / halfSize, -1f, 1f));

        Vector3 sPos = rewardSmall.transform.localPosition;
        sensor.AddObservation(Mathf.Clamp(sPos.x / halfSize, -1f, 1f));
        sensor.AddObservation(Mathf.Clamp(sPos.z / halfSize, -1f, 1f));

        Vector3 pPos = punishment.transform.localPosition;
        sensor.AddObservation(Mathf.Clamp(pPos.x / halfSize, -1f, 1f));
        sensor.AddObservation(Mathf.Clamp(pPos.z / halfSize, -1f, 1f));

        // Velocity (normalized by an assumed max speed)
        Vector3 vel = agentRb.linearVelocity;
        sensor.AddObservation(Mathf.Clamp(vel.x / maxVelocity, -1f, 1f));
        sensor.AddObservation(Mathf.Clamp(vel.y / maxVelocity, -1f, 1f));
        sensor.AddObservation(Mathf.Clamp(vel.z / maxVelocity, -1f, 1f));

        // Rotation (quaternion, raw)
        Quaternion rot = transform.localRotation;
        sensor.AddObservation(rot.x);
        sensor.AddObservation(rot.y);
        sensor.AddObservation(rot.z);
        sensor.AddObservation(rot.w);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        float rotateAction = Mathf.Clamp(actionBuffers.ContinuousActions[0], -1f, 1f);
        float moveAction = Mathf.Clamp(actionBuffers.ContinuousActions[1], -1f, 1f);

        // Rotation
        float rotationAmount = rotateAction * rotateSpeed * Time.fixedDeltaTime;
        transform.Rotate(0, rotationAmount, 0);

        // Forward (no actual reverse), negative => brake
        float forwardSpeed = (moveAction >= 0f)
            ? (moveAction * moveSpeed)
            : (moveSpeed * (1f + moveAction)); // if moveAction = -1 => speed=0

        Vector3 forwardMove = transform.forward * (forwardSpeed * Time.fixedDeltaTime);
        agentRb.MovePosition(agentRb.position + forwardMove);

        if (Time.time - startTime > maxTime)
        {
            AddReward(-0.01f);
            EndEpisode();
        }

        // Linear approach rewards
        float dBig = Vector3.Distance(transform.localPosition, rewardBig.transform.localPosition);
        if (dBig < approachDistance)
        {
            float normDist = dBig / approachDistance; // 0..1
            float val = (1f - normDist) * approachMultiplier;
            AddReward(val * Time.fixedDeltaTime);
        }

        float dSmall = Vector3.Distance(transform.localPosition, rewardSmall.transform.localPosition);
        if (dSmall < approachDistance)
        {
            float normDist = dSmall / approachDistance;
            float val = (1f - normDist) * approachMultiplier * 0.5f;
            AddReward(val * Time.fixedDeltaTime);
        }

        float dPunish = Vector3.Distance(transform.localPosition, punishment.transform.localPosition);
        if (dPunish < approachDistance)
        {
            float normDist = dPunish / approachDistance;
            float val = (1f - normDist) * approachMultiplier;
            AddReward(-val * Time.fixedDeltaTime);
        }

        if (transform.localPosition.y < -2f)
        {
            AddReward(-0.5f);
            EndEpisode();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        StopAllCoroutines();

        if (collision.gameObject.CompareTag("Big"))
        {
            StartCoroutine(GroundLighting(bigRewardMaterial));
            AddReward(bigReward);
            EndEpisode();
        }
        else if (collision.gameObject.CompareTag("Small"))
        {
            StartCoroutine(GroundLighting(smallRewardMaterial));
            AddReward(smallReward);
            EndEpisode();
        }
        else if (collision.gameObject.CompareTag("Punishment"))
        {
            StartCoroutine(GroundLighting(punishmentMaterial));
            AddReward(punishmentReward);
            EndEpisode();
        }
    }

    private void RelocateObjects()
    {
        float minX = -7.5f, maxX = 7.5f;
        float minZ = -7.5f, maxZ = 7.5f;
        float yPos = 0f;

        rewardBig.transform.localPosition = GetRandomPos(minX, maxX, minZ, maxZ, yPos);

        Vector3 smallPos;
        do
        {
            smallPos = GetRandomPos(minX, maxX, minZ, maxZ, yPos);
        } while (Vector3.Distance(smallPos, rewardBig.transform.localPosition) < 1.0f);
        rewardSmall.transform.localPosition = smallPos;

        Vector3 punishPos;
        do
        {
            punishPos = GetRandomPos(minX, maxX, minZ, maxZ, yPos);
        } while (
            Vector3.Distance(punishPos, rewardBig.transform.localPosition) < 1.0f ||
            Vector3.Distance(punishPos, rewardSmall.transform.localPosition) < 1.0f
        );
        punishment.transform.localPosition = punishPos;
    }

    private Vector3 GetRandomPos(float minX, float maxX, float minZ, float maxZ, float y)
    {
        float x = Random.Range(minX, maxX);
        float z = Random.Range(minZ, maxZ);
        return new Vector3(x, y, z);
    }

    private IEnumerator GroundLighting(Material rewardMat)
    {
        groundRenderer.material.color = rewardMat.color;
        yield return new WaitForSeconds(0.25f);
        groundRenderer.material.color = groundBaseColor;
    }
}
