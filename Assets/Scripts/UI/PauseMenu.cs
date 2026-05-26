using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject pausePanel;
    [SerializeField] KeyCode pauseKey = KeyCode.Escape;

    bool isPaused;

    void Update()
    {
        if (Input.GetKeyDown(pauseKey))
            Toggle();
    }

    public void Toggle()
    {
        isPaused = !isPaused;
        pausePanel.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.None;
    }

    public void OnSave()
    {
        GameManager.Instance.Save.SaveGame();
    }

    public void OnLoad()
    {
        GameManager.Instance.Save.LoadGame();
        Toggle();
    }

    public void OnQuit()
    {
        Time.timeScale = 1f;
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
