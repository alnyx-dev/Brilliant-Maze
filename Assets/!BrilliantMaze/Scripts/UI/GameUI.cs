using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameUI : MonoBehaviour
{
    [Header("Панели")]
    [SerializeField] private GameObject _deathPanel;
    [SerializeField] private GameObject _winPanel;

    [Header("Счётчик")]
    [SerializeField] private TextMeshProUGUI _diamondCountText;

    [Header("Выход")]
    [SerializeField] private GameObject _exitBlockedPanel;
    [SerializeField] private float _exitBlockedDuration = 2.5f;

    private Coroutine _exitBlockedCoroutine;

    private void Start()
    {
        if (_deathPanel != null) _deathPanel.SetActive(false);
        if (_winPanel != null) _winPanel.SetActive(false);
        if (_exitBlockedPanel != null) _exitBlockedPanel.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UpdateDiamondCount(int count, int total)
    {
        if (_diamondCountText != null)
            _diamondCountText.text = $"Собрано: {count} / {total}";
    }

    public void ShowDeath()
    {
        if (_deathPanel != null) _deathPanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ShowWin()
    {
        if (_winPanel != null) _winPanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ShowExitBlocked(int needed)
    {
        if (_exitBlockedPanel == null) return;

        if (_exitBlockedCoroutine != null)
            StopCoroutine(_exitBlockedCoroutine);

        _exitBlockedCoroutine = StartCoroutine(ShowExitBlockedRoutine(needed));
    }

    private IEnumerator ShowExitBlockedRoutine(int needed)
    {
        var tmp = _exitBlockedPanel.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null)
            tmp.text = $"Нужно ещё {needed} бриллиантов!";

        _exitBlockedPanel.SetActive(true);
        yield return new WaitForSecondsRealtime(_exitBlockedDuration);
        _exitBlockedPanel.SetActive(false);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
