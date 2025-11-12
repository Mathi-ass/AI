using UnityEngine;

public class ArenaSpawner : MonoBehaviour {
    public GameObject arenaPrefab;
    public int arenaCountX = 3;  // number of arenas along X
    public int arenaCountZ = 3;  // number along Z
    public float arenaSpacing = 25f;  // distance between arenas

    void Start() {
        for (int x = 0; x < arenaCountX; x++) {
            for (int z = 0; z < arenaCountZ; z++) {
                Vector3 pos = new Vector3(x * arenaSpacing, 0, z * arenaSpacing);
                Instantiate(arenaPrefab, pos, Quaternion.identity, transform);
            }
        }
    }
}

