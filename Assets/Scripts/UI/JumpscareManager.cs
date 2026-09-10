using System.Collections;
using UnityEngine;

public class JumpscareManager : MonoBehaviour
{
    [Header("Game Over Target")]
    [SerializeField] private GameOverManager gameOverManager;

    [Header("Timing Settings")]
    [Tooltip("Delay in seconds before triggering the Game Over screen after jumpscare displays.")]
    [SerializeField] private float delayBeforeGameOver = 2f;

    private void OnEnable()
    {
        StartCoroutine(JumpscareToGameOverSequence());
    }

    private IEnumerator JumpscareToGameOverSequence()
    {
        // Wait for the specified duration while jumpscare is visible
        yield return new WaitForSeconds(delayBeforeGameOver);

        // Trigger Game Over screen
        if (gameOverManager != null)
        {
            gameOverManager.TriggerGameOver();
        }
        else
        {
            Debug.LogWarning("GameOverManager reference is missing in JumpscareManager.");
        }
    }
}