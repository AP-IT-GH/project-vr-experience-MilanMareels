using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class PetanqueAgent : Agent
{
    public Rigidbody ballRb;
    public Transform target;
    public Transform ballStartPos;

    public override void OnEpisodeBegin()
    {
        // Reset de bal naar beginpositie
        ballRb.transform.position = ballStartPos.position;
        ballRb.linearVelocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Positieverschil tussen bal en target
        sensor.AddObservation(target.position - ballRb.position);
        // Snelheid van de bal
        sensor.AddObservation(ballRb.linearVelocity);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Alleen bij eerste actie gooien
        if (ballRb.linearVelocity.magnitude < 0.01f)
        {
            float x = actions.ContinuousActions[0];
            float z = actions.ContinuousActions[1];
            float power = actions.ContinuousActions[2];

            Vector3 direction = new Vector3(x, 0.5f, z).normalized;
            ballRb.AddForce(direction * power * 10f, ForceMode.Impulse);
        }

        float distance = Vector3.Distance(ballRb.position, target.position);
        float reward = Mathf.Clamp(1.0f - distance / 10f, 0, 1);
        AddReward(reward);

        // Be�indig episode als de bal stil ligt
        if (ballRb.linearVelocity.magnitude < 0.01f && ballRb.position != ballStartPos.position)
        {
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Random.Range(-1f, 1f);
        continuousActionsOut[1] = Random.Range(-1f, 1f);
        continuousActionsOut[2] = Random.Range(0.5f, 1.0f);
    }
}
