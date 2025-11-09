using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

[RequireComponent(typeof(Rigidbody))]
public class SeekerAgent : Agent {
    private Rigidbody rb;
    private HiderAgent hider;

    public override void Initialize() {
        rb = GetComponent<Rigidbody>();
        hider = FindFirstObjectByType<HiderAgent>(); // new API, Unity 2022+

        if (rb == null)
            Debug.LogError("SeekerAgent missing Rigidbody!");

        if (hider == null)
            Debug.LogError("HiderAgent not found in scene!");
    }

    public override void CollectObservations(VectorSensor sensor) {
        if (rb == null) return; // safety guard

        // Observations
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(rb.linearVelocity);

        if (hider != null)
            sensor.AddObservation(hider.transform.localPosition);
        else
            sensor.AddObservation(Vector3.zero);
    }

    public override void OnActionReceived(ActionBuffers actions) {
        if (rb == null) return;

        float move = actions.ContinuousActions[0];
        float turn = actions.ContinuousActions[1];

        rb.linearVelocity = transform.forward * move * 6f;
        transform.Rotate(Vector3.up, turn * 180f * Time.fixedDeltaTime);
    }

    public override void Heuristic(in ActionBuffers actionsOut) {
        var ca = actionsOut.ContinuousActions;
        ca[0] = Input.GetAxis("Vertical");
        ca[1] = Input.GetAxis("Horizontal");
    }
}


