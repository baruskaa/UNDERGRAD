using UnityEngine;
using UnityEngine.UI;

public class SavePoint : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject savePanelUI;
    [SerializeField] private Button saveButton;

    [Header("Player Feedback")]
    [Tooltip("The '!' alert GameObject above the Player.")]
    [SerializeField] private GameObject playerAlertIcon;

    private bool isPlayerInRange = false;

    private void Start()
    {
        if (savePanelUI != null) savePanelUI.SetActive(false);
        if (playerAlertIcon != null) playerAlertIcon.SetActive(false);

        if (saveButton != null)
        {
            saveButton.onClick.AddListener(OnSaveButtonClicked);
        }

        Debug.Log($"Save File Location: {Application.persistentDataPath}");
    }

    private void Update()
    {
        if (!isPlayerInRange) return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (savePanelUI != null)
            {
                bool isPanelActive = !savePanelUI.activeSelf;
                savePanelUI.SetActive(isPanelActive);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;
            if (playerAlertIcon != null) playerAlertIcon.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;

            if (playerAlertIcon != null) playerAlertIcon.SetActive(false);
            if (savePanelUI != null) savePanelUI.SetActive(false);
        }
    }

    public void OnSaveButtonClicked()
    {
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.SaveGame();
        }

        if (savePanelUI != null)
        {
            savePanelUI.SetActive(false);
        }
    }
}