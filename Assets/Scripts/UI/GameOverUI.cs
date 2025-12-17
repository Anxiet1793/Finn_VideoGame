using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void OnRestartPressed()
    {
        if (GameManager.Instance != null) GameManager.Instance.RestartScene();
    }

    public void OnMenuPressed()
    {
        if (GameManager.Instance != null) GameManager.Instance.GoToMainMenu();
    }

    public void OnQuitPressed()
    {
        if (GameManager.Instance != null) GameManager.Instance.QuitGame();
    }
}
