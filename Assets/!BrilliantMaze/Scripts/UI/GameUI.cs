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

    private void Start()
    {
        if (_deathPanel != null) _deathPanel.SetActive(false);
        if (_winPanel != null) _winPanel.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        UpdateDiamondCount(0);
    }

    public void UpdateDiamondCount(int count)
    {
        if (_diamondCountText != null)
            _diamondCountText.text = $"Собрано бриллиантов: {count}";
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
