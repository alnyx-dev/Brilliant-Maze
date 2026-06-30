using UnityEngine;

public class DiamondSpawner : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private Transform[] points;
    [SerializeField] private int count = 10;

    public int SpawnedCount => count;

    private void Start()
    {
        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, points.Length - 1);

            Instantiate(
                prefab,
                Vector3.Lerp(points[index].position, points[index + 1].position, Random.value),
                Quaternion.identity
            );
        }
    }
}