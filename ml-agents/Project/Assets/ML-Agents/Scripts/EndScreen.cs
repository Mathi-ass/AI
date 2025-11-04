using UnityEngine;

public class EndScreen : MonoBehaviour
{
    [SerializeField] private GameObject defeatScreenPrefab;
    [SerializeField] private GameObject victoryScreenPrefab;

    public void ShowDefeatScreen()
    {
        Instantiate(defeatScreenPrefab, Vector3.zero, Quaternion.identity);
    }

    public void ShowVictoryScreen()
    {
        Instantiate(victoryScreenPrefab, Vector3.zero, Quaternion.identity);
    }
}
