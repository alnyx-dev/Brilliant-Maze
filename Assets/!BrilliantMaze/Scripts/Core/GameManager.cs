using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public enum GameState { Playing, Won, Lost }

    [Header("Ссылки")]
    [SerializeField] private DiamondCounter _diamondCounter;
    [SerializeField] private GameUI _gameUI;
    [SerializeField] private DiamondSpawner _diamondSpawner;

    private GameState _state = GameState.Playing;
    private int _totalDiamonds;

    public GameState State => _state;

    private void Start()
    {
        _totalDiamonds = _diamondSpawner.SpawnedCount;
        _diamondCounter.Total = _totalDiamonds;
        _gameUI.UpdateDiamondCount(0, _totalDiamonds);
    }

    public void ReportDiamondCollected()
    {
        if (_state != GameState.Playing) return;
        _gameUI.UpdateDiamondCount(_diamondCounter.Count, _totalDiamonds);
    }

    public void ReportDeath()
    {
        if (_state != GameState.Playing) return;
        _state = GameState.Lost;
        _gameUI.ShowDeath();
    }

    public void ReportExitReached()
    {
        if (_state != GameState.Playing) return;

        if (_diamondCounter.Count >= _totalDiamonds)
        {
            _state = GameState.Won;
            _gameUI.ShowWin();
        }
        else
        {
            int needed = _totalDiamonds - _diamondCounter.Count;
            _gameUI.ShowExitBlocked(needed);
        }
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
