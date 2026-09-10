using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CanvasGroup))]
public class GameOverManager : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("If checked, uses Scene Names instead of Build Indices.")]
    [SerializeField] private bool useSceneName = false;

    [Header("Scene Names")]
    [SerializeField] private string restartSceneName = "";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Scene Indices (if useSceneName is unchecked)")]
    [SerializeField] private int restartSceneIndex = 1;
    [SerializeField] private int mainMenuSceneIndex = 0;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 1f;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        // Start hidden and un-interactable
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    /// <summary>
    /// Call this method from your player health script or enemy reach trigger when player dies.
    /// </summary>
    public void TriggerGameOver()
    {
        StartCoroutine(FadeInRoutine());
    }

    private IEnumerator FadeInRoutine()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime; // Uses unscaled time in case Time.timeScale is 0
            canvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    // Attached to Retry Button
    public void RetryGame()
    {
        Time.timeScale = 1f; // Reset time scale before scene load

        if (useSceneName)
        {
            // If restartSceneName is blank, reloads the current active scene automatically
            if (string.IsNullOrEmpty(restartSceneName))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            else
            {
                SceneManager.LoadScene(restartSceneName);
            }
        }
        else
        {
            SceneManager.LoadScene(restartSceneIndex);
        }
    }

    // Attached to Main Menu Button
    public void GoToMainMenu()
    {
        Time.timeScale = 1f; // Reset time scale before scene load

        if (useSceneName)
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            SceneManager.LoadScene(mainMenuSceneIndex);
        }
    }
}