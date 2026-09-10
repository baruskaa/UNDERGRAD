using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class JumpscareManager : MonoBehaviour
{
    [Header("Game Over Target")]
    [SerializeField] private GameOverManager gameOverManager;

    [Header("Audio Settings")]
    [Tooltip("The music or sound effect that plays immediately when the jumpscare panel is enabled.")]
    [SerializeField] private AudioClip jumpscareAudio;
    [Range(0f, 1f)][SerializeField] private float volume = 1f;

    [Header("Timing Settings")]
    [Tooltip("Delay in seconds before triggering the Game Over screen after jumpscare displays.")]
    [SerializeField] private float delayBeforeGameOver = 2f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        PlayJumpscareSound();
        StartCoroutine(JumpscareToGameOverSequence());
    }

    private void PlayJumpscareSound()
    {
        if (jumpscareAudio != null && audioSource != null)
        {
            audioSource.PlayOneShot(jumpscareAudio, volume);
        }
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