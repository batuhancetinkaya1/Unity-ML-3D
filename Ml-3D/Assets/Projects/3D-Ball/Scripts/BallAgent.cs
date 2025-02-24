using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;
using Unity.MLAgents.Sensors.Reflection;

public class BallAgent : Agent
{
    [SerializeField] private GameObject ball;

    private Rigidbody m_BallRb;
    private const float ALIVE_REWARD_MULTIPLIER = 0.01f;
    private float startTime;

    public override void Initialize()
    {
        m_BallRb = ball.GetComponent<Rigidbody>();
        ResetSystem();
    }

    public override void OnEpisodeBegin()
    {
        ResetSystem();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = -Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.rotation.x);
        sensor.AddObservation(transform.rotation.z);
        sensor.AddObservation(m_BallRb.position - transform.position);
        sensor.AddObservation(m_BallRb.linearVelocity);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        float actionX = 2f * Mathf.Clamp(actionBuffers.ContinuousActions[1], -1f, 1f);
        float actionZ = 2f * Mathf.Clamp(actionBuffers.ContinuousActions[0], -1f, 1f);

        float rotationX = transform.rotation.eulerAngles.x;
        if (rotationX > 180f) rotationX -= 360f; 
        float rotationZ = transform.rotation.eulerAngles.z;
        if (rotationZ > 180f) rotationZ -= 360f;  

        //Rotate the Agent
        if ((actionX > 0f && rotationX < 25f) || (actionX < 0f && rotationX > -25f))
        {
            transform.Rotate(new Vector3(1, 0, 0), actionX);
        }
        if ((actionZ > 0f && rotationZ < 25f) || (actionZ < 0f && rotationZ > -25f))
        {
            transform.Rotate(new Vector3(0, 0, 1), actionZ);
        }

        // Check if the Agent Failed
        if (ball.transform.position.y - transform.position.y < -0.5f ||
            Mathf.Abs(ball.transform.position.x - transform.position.x) > 1f ||
            Mathf.Abs(ball.transform.position.z - transform.position.z) > 1f)
        {
            SetReward(-1f); //Give Punishment
            EndEpisode(); //Finish curretn episode
        }

        m_BallRb.WakeUp(); // Preventing the calculation of Rigidbody stops

        CalculateReward();
    }

    private void CalculateReward()
    {
        float elapsedTime = Time.time - startTime;
        SetReward(elapsedTime * ALIVE_REWARD_MULTIPLIER); // Time dependence Reward
    }

    private void ResetSystem()
    {
        transform.rotation = Quaternion.identity; // Resets cube rotation
        transform.Rotate(Vector3.right, Random.Range(-10f, 10f)); // Set random rotation
        transform.Rotate(Vector3.forward, Random.Range(-10f, 10f)); // Set random rotation

        ball.transform.position = transform.position + new Vector3(Random.Range(-0.3f, 0.3f), 1f, Random.Range(-0.3f, 0.3f)); // Resets ball position
        m_BallRb.linearVelocity = Vector3.zero; // Resets ball linear movement
        m_BallRb.angularVelocity = Vector3.zero; // Resets ball angular movement

        startTime = Time.time; // Reset start time
    }
}
