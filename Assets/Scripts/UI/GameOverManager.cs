using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CanvasGroup))]
public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }

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

    [Header("Transition Settings (Optional)")]
    [Tooltip("Optional Animator transition for fading out when reloading or loading a save.")]
    public Animator transition;
    public float transitionTime = 1f;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        canvasGroup = GetComponent<CanvasGroup>();

        // Start hidden and un-interactable
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    /// <summary>
    /// Call this method when player health reaches 0 or player dies.
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

        // 1. Check if a save file exists -> Load Save State
        if (SaveSystem.Instance != null && SaveSystem.Instance.HasSaveFile())
        {
            Debug.Log("[GameOverManager] Save file found. Loading saved game...");
            StartCoroutine(LoadSavedGameRoutine());
        }
        else
        {
            // 2. If no save exists -> Restart active scene
            Debug.Log("[GameOverManager] No save file found. Restarting current scene...");
            RestartScene();
        }
    }

    private IEnumerator LoadSavedGameRoutine()
    {
        if (transition != null)
        {
            transition.SetTrigger("Start");
            yield return new WaitForSecondsRealtime(transitionTime);
        }

        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.LoadGameFromMainMenu();
        }
    }

    private void RestartScene()
    {
        if (useSceneName)
        {
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