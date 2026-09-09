using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CanvasGroup))]
public class GameOverManager : MonoBehaviour
{
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
        SceneManager.LoadScene(1);
    }

    // Attached to Main Menu Button
    public void GoToMainMenu()
    {
        Time.timeScale = 1f; // Reset time scale before scene load
        SceneManager.LoadScene(0);
    }
}