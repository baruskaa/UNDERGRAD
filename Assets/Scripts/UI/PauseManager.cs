using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CanvasGroup))]
public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    [Header("Pause Controls")]
    [Tooltip("If true, allows pressing Escape/P to toggle pause menu.")]
    public bool allowKeyToggle = true;

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
    [SerializeField] private float fadeDuration = 0.25f;

    [Header("Transition Settings (Optional)")]
    [Tooltip("Optional Animator transition for fading out when reloading or transitioning scenes.")]
    public Animator transition;
    public float transitionTime = 1f;

    public bool IsPaused { get; private set; } = false;

    private CanvasGroup canvasGroup;
    private Coroutine fadeCoroutine;

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

    private void Update()
    {
        if (!allowKeyToggle) return;

        // Block pause menu if Dialogue or Game Over is active
        if (DialogueManager.Instance != null && DialogueManager.Instance.isDialogueActive) return;
        if (GameOverManager.Instance != null && GameOverManager.Instance.GetComponent<CanvasGroup>().alpha > 0f) return;

        // Input check for Escape or P key
        bool pausePressed = (Keyboard.current != null && (Keyboard.current.escapeKey.wasPressedThisFrame))
                            || Input.GetKeyDown(KeyCode.Escape);

        if (pausePressed)
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (IsPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        IsPaused = true;
        Time.timeScale = 0f; // Freeze gameplay time

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeCanvasGroup(1f, true));
    }

    // Attached to Resume Button
    public void ResumeGame()
    {
        IsPaused = false;
        Time.timeScale = 1f; // Unfreeze gameplay time

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeCanvasGroup(0f, false));
    }

    // Attached to Restart Button
    public void RestartGame()
    {
        Time.timeScale = 1f; // Reset time scale before scene load


        // 1. Check if a save file exists -> Load Save State
        if (SaveSystem.Instance != null && SaveSystem.Instance.HasSaveFile())
        {
            Debug.Log("[PauseManager] Save file found. Loading saved game...");
            StartCoroutine(LoadSavedGameRoutine());
        }
        else
        {
            // 2. If no save exists -> Restart active scene
            Debug.Log("[PauseManager] No save file found. Restarting current scene...");
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

    private IEnumerator FadeCanvasGroup(float targetAlpha, bool enableInteraction)
    {
        float startAlpha = canvasGroup.alpha;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime; // Unscaled time runs while timeScale is 0
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        canvasGroup.interactable = enableInteraction;
        canvasGroup.blocksRaycasts = enableInteraction;
        fadeCoroutine = null;
    }
}