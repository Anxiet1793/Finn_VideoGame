using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Over UI")]
    [SerializeField] private GameOverUI gameOverUI;

    [Header("Flow")]
    [SerializeField] private string mainMenuSceneName = "Menu";
    [SerializeField] private bool freezeTimeOnGameOver = true;

    private bool isGameOver;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // cada escena nueva: resetea flags y encuentra UI si existe
        isGameOver = false;
        Time.timeScale = 1f;

        gameOverUI = FindObjectOfType<GameOverUI>(true);
        if (gameOverUI != null) gameOverUI.Hide();
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }

    public void TriggerGameOver()
{
    // Asegura que el juego no quede congelado
    Time.timeScale = 1f;

    // Carga menú directo
    GoToMainMenu();
}


    public void RestartScene()
    {
        Time.timeScale = 1f;
        isGameOver = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        isGameOver = false;

        if (!string.IsNullOrEmpty(mainMenuSceneName))
            SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}
