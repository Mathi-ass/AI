using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

[RequireComponent(typeof(Rigidbody))]
public class SeekerAgent : Agent {
    private Rigidbody rb;
    public float moveSpeed = 5f;
    public float turnSpeed = 200f;
    public HiderAgent hider;

    public override void Initialize() {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        if (hider == null) hider = FindFirstObjectByType<HiderAgent>();
    }

    public override void CollectObservations(VectorSensor sensor) {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(rb.linearVelocity);
        if (hider != null)
            sensor.AddObservation(hider.transform.localPosition - transform.localPosition);
        else
            sensor.AddObservation(Vector3.zero);

        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(hider.transform.localPosition - transform.localPosition);
        sensor.AddObservation(GetComponent<Rigidbody>().linearVelocity);

    }

    public override void OnActionReceived(ActionBuffers actions) {
        float move = actions.ContinuousActions[0];
        float turn = actions.ContinuousActions[1];

        Vector3 forward = transform.forward * move * moveSpeed * Time.deltaTime;
        rb.MovePosition(rb.position + forward);

        Quaternion turnRot = Quaternion.Euler(0f, turn * turnSpeed * Time.deltaTime, 0f);
        rb.MoveRotation(rb.rotation * turnRot);

        // Small negative reward per step to encourage efficiency
        AddReward(-0.001f);

        // Bonus if getting close to Hider
        if (hider != null) {
            float dist = Vector3.Distance(transform.position, hider.transform.position);
            AddReward(Mathf.Clamp01(1f - dist / 15f) * 0.001f);
        }

        if (StepCount > MaxStep) {
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut) {
        var ca = actionsOut.ContinuousActions;
        ca[0] = Input.GetAxis("Vertical");
        ca[1] = Input.GetAxis("Horizontal");
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.collider.CompareTag("Hider")) {
            AddReward(+1.0f);
            hider.OnCaught();
            EndEpisode();
            FindFirstObjectByType<EnvironmentManager>()?.ResetEnvironment();
        }
    }

    public void ResetAgent() {
        rb.linearVelocity = Vector3.zero;
        transform.localPosition = new Vector3(Random.Range(25f, 25f), 1f, Random.Range(2f, 2f));
    }
}



