using UnityEngine;
using UnityEngine.InputSystem;
using Inventory.Model;

public class ItemQuest : MonoBehaviour
{
    [Header("Quest Settings")]
    public int questNumber = 1;

    [Header("Inventory Data")]
    public ItemSO itemData;
    public InventorySO inventoryData;

    [Header("UI Prompt")]
    public GameObject alertIcon;

    private QuestManager theQM;
    private bool isPlayerInRange = false;

    void Start()
    {
        theQM = FindAnyObjectByType<QuestManager>();
        if (alertIcon != null) alertIcon.SetActive(false);
    }

    void Update()
    {
        if (!isPlayerInRange) return;

        bool qKeyPressed = (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame) || Input.GetKeyDown(KeyCode.Q);

        if (qKeyPressed)
        {
            Debug.Log($"[ItemQuest] 'Q' pressed near '{gameObject.name}'. Attempting pickup...");
            TryPickUpItem();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            if (alertIcon != null) alertIcon.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            if (alertIcon != null) alertIcon.SetActive(false);
        }
    }

    private void TryPickUpItem()
    {
        if (itemData == null)
        {
            Debug.LogError($"[ItemQuest Error] 'Item Data' (ItemSO) is missing on '{gameObject.name}' in the Inspector!");
            return;
        }

        if (theQM == null) theQM = FindAnyObjectByType<QuestManager>();

        // Attempt inventory addition if scriptable object exists
        if (inventoryData != null)
        {
            inventoryData.AddItem(itemData);
        }

        // Send asset name directly to QuestManager
        if (theQM != null)
        {
            theQM.itemCollected = itemData.name;
            Debug.Log($"[ItemQuest Success] Assigned '{itemData.name}' to QuestManager.itemCollected!");
        }

        if (alertIcon != null) alertIcon.SetActive(false);
        gameObject.SetActive(false);
    }
}