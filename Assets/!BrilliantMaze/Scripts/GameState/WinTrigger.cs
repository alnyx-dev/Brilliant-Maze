using UnityEngine;

public class WinTrigger : MonoBehaviour
{
    [Header("Победа")]
    [SerializeField] private int _requiredDiamonds = 10;
    [SerializeField] private DiamondCounter _diamondCounter;
    [SerializeField] private GameUI _gameUI;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (_diamondCounter != null && _diamondCounter.Count < _requiredDiamonds)
            {
                Debug.Log($"Нужно ещё {_requiredDiamonds - _diamondCounter.Count} алмазов!");
                return;
            }

            if (_gameUI != null)
                _gameUI.ShowWin();
        }
    }
}
