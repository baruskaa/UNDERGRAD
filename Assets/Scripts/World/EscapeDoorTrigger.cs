using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class EscapeDoorTrigger : MonoBehaviour
{
    [Header("Scene Settings")]
    public string sceneToLoad = "BossFight";

    [Header("UI Alert (Optional)")]
    [Tooltip("I-drag dito ang Alert icon (hal. 'Q' prompt) na lalabas pag lumapit ang player.")]
    public GameObject interactionAlert;

    private bool isPlayerInRange = false;

    private void Awake()
    {
        // Siguraduhing naka-trigger ang collider
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void Start()
    {
        if (interactionAlert != null)
        {
            interactionAlert.SetActive(false);
        }
    }

    private void Update()
    {
        // Kapag nakatapat ang player at pinindot ang Q
        if (isPlayerInRange && Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;
            if (interactionAlert != null)
            {
                interactionAlert.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
            if (interactionAlert != null)
            {
                interactionAlert.SetActive(false);
            }
        }
    }
}