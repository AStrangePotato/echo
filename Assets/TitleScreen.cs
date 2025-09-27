using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleScreen : MonoBehaviour
{
    [Header("UI Elements")]
    public Button playButton; // Assign PlayButton in Inspector
    public GameObject titleScreenCanvas; // Assign TitleScreenCanvas
    public GameObject player; // Assign Player with FPSController

    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        // Ensure player is disabled at start
        if (player != null)
        {
            player.SetActive(false);
        }

        // Hook up Play button
        if (playButton != null)
        {
            playButton.onClick.AddListener(StartGame);
        }
    }

    void StartGame()
    {   
        // Hide title screen
        if (titleScreenCanvas != null)
        {
            titleScreenCanvas.SetActive(false);
        }

        // Enable player
        if (player != null)
        {
            player.SetActive(true);
        }

        // Optional: If using a separate game scene, uncomment:
        // SceneManager.LoadScene("GameScene"); // Ensure your maze scene is named "GameScene"
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}