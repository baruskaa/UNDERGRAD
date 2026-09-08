using UnityEngine;
using UnityEngine.InputSystem;
using Inventory.Model;

public class ItemQuest : MonoBehaviour
{
    [Header("Quest Settings")]
    public int questNumber;

    [Header("Inventory Data")]
    public ItemSO itemData;           // ScriptableObject asset for this item
    public InventorySO inventoryData;  // ScriptableObject asset for the player's inventory

    [Header("UI Prompt")]
    public GameObject alertIcon;      // Assign your "Q" key prompt / alert icon here

    private QuestManager theQM;
    private bool isPlayerInRange = false;

    void Start()
    {
        theQM = FindAnyObjectByType<QuestManager>();

        if (alertIcon != null)
        {
            alertIcon.SetActive(false);
        }
    }

    void Update()
    {
        if (!isPlayerInRange) return;

        // Reads 'Q' key press (supports New Input System and Legacy Input)
        bool qKeyPressed = (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame) || Input.GetKeyDown(KeyCode.Q);

        if (qKeyPressed)
        {
            TryPickUpItem();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Show alert icon only if the quest is active and not finished
            if (theQM != null && !theQM.questCompleted[questNumber] && theQM.quests[questNumber].gameObject.activeSelf)
            {
                isPlayerInRange = true;
                if (alertIcon != null) alertIcon.SetActive(true);
            }
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
        if (inventoryData == null || itemData == null)
        {
            Debug.LogError("Missing InventorySO or ItemSO on " + gameObject.name);
            return;
        }

        bool addedSuccessfully = inventoryData.AddItem(itemData);

        if (addedSuccessfully)
        {
            theQM.itemCollected = itemData.name;
            if (alertIcon != null) alertIcon.SetActive(false);
            gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("Inventory is full! Item cannot be picked up.");
        }
    }
}