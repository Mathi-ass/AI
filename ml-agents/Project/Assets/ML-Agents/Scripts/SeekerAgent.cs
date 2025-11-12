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

    //private float freezeTime = 5f;     // Time (in seconds) to wait at start
    //private float freezeTimer = 0f;
    //private bool isFrozen = true;

    //private void Start() {
    //    freezeTimer = freezeTime;
    //    isFrozen = true;
    //}

    public override void Initialize() {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        if (hider == null) hider = FindFirstObjectByType<HiderAgent>();

        //freezeTimer = freezeTime;
        //isFrozen = true;
    }

    public override void CollectObservations(VectorSensor sensor) {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(rb.linearVelocity);
        sensor.AddObservation(hider.transform.localPosition - transform.localPosition);
        sensor.AddObservation(GetComponent<Rigidbody>().linearVelocity);

    }


    public override void OnActionReceived(ActionBuffers actions) {

        //if (isFrozen) {
        //    freezeTimer -= Time.deltaTime;
        //    rb.linearVelocity = Vector3.zero;
        //    rb.angularVelocity = Vector3.zero;
        //    if (freezeTimer <= 0f) {
        //        isFrozen = false;
        //    }
        //    return;
        //}

        float move = actions.ContinuousActions[0];
        float turn = actions.ContinuousActions[1];

        Vector3 forward = transform.forward * move * moveSpeed * Time.deltaTime;
        rb.MovePosition(rb.position + forward);

        Quaternion turnRot = Quaternion.Euler(0f, turn * turnSpeed * Time.deltaTime, 0f);
        rb.MoveRotation(rb.rotation * turnRot);

        // Small negative reward per step to encourage efficiency
        AddReward(-0.001f);

        if (rb.linearVelocity.magnitude > 0.1f)
            AddReward(0.0005f);

        // Bonus if getting close to Hider
        if (hider != null) {
            float dist = Vector3.Distance(transform.position, hider.transform.position);
            AddReward(Mathf.Clamp01(1f - dist / 7f) * 0.001f);
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
        transform.localPosition = new Vector3(Random.Range(3f, 3f), 1.5f, Random.Range(0f, 0f));
    }
}



