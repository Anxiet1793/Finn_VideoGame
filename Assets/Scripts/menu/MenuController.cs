// 14/12/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public void StartGame()
    {
        // Load the game scene
        SceneManager.LoadScene("Game");
    }

    public void QuitGame()
    {
        Debug.Log("cerrando juego");
            // Quit the application
        Application.Quit();
    }
}
