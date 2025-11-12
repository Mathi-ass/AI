using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class HiderAgent : Agent {
    private Rigidbody rb;
    public float moveSpeed = 5f;
    public float turnSpeed = 200f;
    public SeekerAgent seeker;

    [Header("Lock Settings")]
    public float lockRange = 3f;
    public LayerMask boxMask; // Assign to "Box" layer
    public float lockCooldown = 2f;
    private float lastLockTime = -999f;

    private List<LockObjects> nearbyBoxes = new List<LockObjects>();

    public override void Initialize() {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        if (seeker == null) seeker = FindFirstObjectByType<SeekerAgent>();
    }

    public override void CollectObservations(VectorSensor sensor) {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(rb.linearVelocity);

        // Observation: how many boxes nearby
        Collider[] hits = Physics.OverlapSphere(transform.position, lockRange, boxMask);
        sensor.AddObservation(hits.Length / 5.0f); // normalized count (max ~5)

        sensor.AddObservation(seeker.transform.localPosition - transform.localPosition);
        sensor.AddObservation(GetComponent<Rigidbody>().linearVelocity);

    }

    public override void OnActionReceived(ActionBuffers actions) {
        int moveAction = actions.DiscreteActions[0];  // 0: none, 1: forward, 2: back
        int turnAction = actions.DiscreteActions[1];  // 0: none, 1: left, 2: right
        int lockAction = actions.DiscreteActions[2];  // 0: none, 1: lock

        // Movement
        Vector3 moveDir = Vector3.zero;
        if (moveAction == 1) moveDir = transform.forward;
        else if (moveAction == 2) moveDir = -transform.forward;

        rb.MovePosition(rb.position + moveDir * moveSpeed * Time.deltaTime);

        float turn = 0f;
        if (turnAction == 1) turn = -1f;
        else if (turnAction == 2) turn = 1f;
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, turn * turnSpeed * Time.deltaTime, 0f));

        // Try to lock a box
        if (lockAction == 1 && Time.time > lastLockTime + lockCooldown) {
            TryLockNearbyBox();
            lastLockTime = Time.time;
        }

        // Time-based survival reward
        AddReward(+0.001f);

        if (StepCount > MaxStep) {
            EndEpisode();
        }
    }

    void TryLockNearbyBox() {
        Collider[] hits = Physics.OverlapSphere(transform.position, lockRange, boxMask);
        foreach (Collider col in hits) {
            LockObjects box = col.GetComponent<LockObjects>();
            if (box != null && !box.isLocked) {
                box.LockBox();
                AddReward(+0.2f); // reward for using box effectively
                break;
            }
        }
    }

    public void OnCaught() {
        AddReward(-1.0f);
        FindFirstObjectByType<EnvironmentManager>()?.ResetEnvironment();
        EndEpisode();
    }

    public void ResetAgent() {
        rb.linearVelocity = Vector3.zero;
        transform.localPosition = new Vector3(Random.Range(-3f, -3f), 1.5f, Random.Range(0f, 0f));
    }
}
