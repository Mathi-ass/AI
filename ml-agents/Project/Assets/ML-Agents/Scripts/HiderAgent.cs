using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class HiderAgent : Agent {
    private Rigidbody rb;
    public SeekerAgent seeker;

    public override void Initialize() {
        rb = GetComponent<Rigidbody>();
        seeker = FindFirstObjectByType<SeekerAgent>();

        if (seeker == null) {
            Debug.LogError("SeekerAgent is not found in the scene!");
        }
    }

    public override void CollectObservations(VectorSensor sensor) {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(rb.linearVelocity);

        if (seeker != null)
            sensor.AddObservation(seeker.transform.localPosition);
        else
            sensor.AddObservation(Vector3.zero);
    }

    public override void OnActionReceived(ActionBuffers actions) {
        float move = actions.ContinuousActions[0];
        float turn = actions.ContinuousActions[1];

        rb.linearVelocity = transform.forward * move * 5f;
        transform.Rotate(Vector3.up, turn * 180f * Time.fixedDeltaTime);

        AddReward(0.001f); // survive reward
    }

    public void OnCaught() {
        SetReward(-1f); // negative reward for getting caught
        EndEpisode();

        // optional: reset the environment
        var env = FindFirstObjectByType<EnvironmentManager>();
        if (env != null)
            env.ResetEnvironment();
    }


    public override void Heuristic(in ActionBuffers actionsOut) {
        var ca = actionsOut.ContinuousActions;
        ca[0] = Input.GetAxis("Vertical");
        ca[1] = Input.GetAxis("Horizontal");
    }
}


