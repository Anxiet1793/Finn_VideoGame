using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject root; // Panel/Canvas principal del GameOver

    private void Awake()
    {
        if (root == null) root = gameObject;
    }

    public void Show()
    {
        if (root != null) root.SetActive(true);
    }

    public void Hide()
    {
        if (root != null) root.SetActive(false);
    }
    private void Start()
{
    Hide();
}

    // Estos 3 se asignan directo a botones (OnClick)
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
