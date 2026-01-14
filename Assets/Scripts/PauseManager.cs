using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    [Header("UI Ёлементы")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Slider sensitivitySlider;

    [Header("—сылки")]
    [SerializeField] private PlayerController playerController;

    private bool isPaused = false;

    private void Start()
    {

        if (playerController != null && sensitivitySlider != null)
        {

            sensitivitySlider.value = playerController.GetSensitivity();

            sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
        }

        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        if (pausePanel) pausePanel.SetActive(true);

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        isPaused = false;
        if (pausePanel) pausePanel.SetActive(false);

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnSensitivityChanged(float value)
    {
        if (playerController != null)
        {
            playerController.SetSensitivity(value);
        }
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}