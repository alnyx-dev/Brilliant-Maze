using UnityEngine;

public class DiamondCounter : MonoBehaviour
{
    [SerializeField] private GameUI _gameUI;
    private int count;

    public int Count => count;

    public void Increment()
    {
        count++;

        if (_gameUI != null)
            _gameUI.UpdateDiamondCount(count);
    }
}