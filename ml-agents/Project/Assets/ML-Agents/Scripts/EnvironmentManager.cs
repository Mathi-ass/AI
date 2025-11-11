using UnityEngine;

public class EnvironmentManager : MonoBehaviour {
    public SeekerAgent seeker;
    public HiderAgent hider;
    public LockObjects[] boxes;

    public void ResetEnvironment() {
        seeker.ResetAgent();
        hider.ResetAgent();

        // Reset boxes
        foreach (var box in boxes) {
            if (box == null) continue;
            box.UnlockBox();
            box.transform.localPosition = new Vector3(
                Random.Range(35f, 10f), 0.5f, Random.Range(-10f, 4f));
        }
    }
}
