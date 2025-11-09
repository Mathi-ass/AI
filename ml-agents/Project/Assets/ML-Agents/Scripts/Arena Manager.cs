using UnityEngine;

public class ArenaManager : MonoBehaviour {
    public HiderAgent hider;
    public SeekerAgent seeker;
    public LockObjects[] boxes;

    public void ResetArena() {
        hider.transform.localPosition = new Vector3(14, 0.5f, -2);
        seeker.transform.localPosition = new Vector3(35, 0.5f, -2);

        foreach (var box in boxes) {
            box.UnlockBox();
            box.transform.localPosition = new Vector3(Random.Range(-30f, 30f), 0.5f, Random.Range(-30f, 30f));
        }
    }
}
