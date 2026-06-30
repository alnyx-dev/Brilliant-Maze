using UnityEngine;

public class DiamondCounter : MonoBehaviour
{
    private int count;

    public int Count => count;
    public int Total { get; set; }

    public void Increment()
    {
        count++;
    }
}
