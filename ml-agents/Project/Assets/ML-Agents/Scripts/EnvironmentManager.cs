using UnityEngine;

public class EnvironmentManager : MonoBehaviour {
    [Header("Agent References")]
    public HiderAgent hider;
    public SeekerAgent seeker;

    [Header("Spawn Positions")]
    public Transform hiderStart;
    public Transform seekerStart;

    // Called by agents when episodes end
    public void ResetEnvironment() {
        if (hider != null && hiderStart != null) {
            hider.transform.position = hiderStart.position;
            hider.transform.rotation = hiderStart.rotation;
            if (hider.TryGetComponent<Rigidbody>(out var hrb))
                hrb.linearVelocity = Vector3.zero;
        }

        if (seeker != null && seekerStart != null) {
            seeker.transform.position = seekerStart.position;
            seeker.transform.rotation = seekerStart.rotation;
            if (seeker.TryGetComponent<Rigidbody>(out var srb))
                srb.linearVelocity = Vector3.zero;
        }
    }
}

