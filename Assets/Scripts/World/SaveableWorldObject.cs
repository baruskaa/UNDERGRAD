using UnityEngine;

public class SaveableWorldObject : MonoBehaviour
{
    [SerializeField]
    private string uniqueID;
    public string UniqueID => uniqueID;

    public bool IsStateChanged { get; private set; } = false;

    [ContextMenu("Generate Unique ID")]
    public void GenerateID()
    {
        uniqueID = System.Guid.NewGuid().ToString();
    }

    private void Awake()
    {
        if (string.IsNullOrEmpty(uniqueID))
        {
            uniqueID = gameObject.name;
        }
    }

    public void SetStateChanged(bool changed)
    {
        IsStateChanged = changed;
    }

    public void RestoreState(WorldObjectState state)
    {
        if (state == null) return;

        IsStateChanged = state.isInteractableStateChanged;

        // 1. Hallway Door restoring
        UnlockHallwayDoor hallwayDoor = GetComponent<UnlockHallwayDoor>();
        if (hallwayDoor != null)
        {
            hallwayDoor.ApplyLoadedState(state.isInteractableStateChanged);
            return;
        }

        // 2. Bathroom Door restoring
        BathroomDoor bathroomDoor = GetComponent<BathroomDoor>();
        if (bathroomDoor != null)
        {
            bathroomDoor.ApplyLoadedState(state.isInteractableStateChanged);
            return;
        }

        // 3. Dialogue Trigger restoring
        DialogueTrigger dialogueTrigger = GetComponent<DialogueTrigger>();
        if (dialogueTrigger != null)
        {
            dialogueTrigger.ApplyLoadedState(state.isInteractableStateChanged, state.isActive);
            return;
        }

        // 4. Default fallback for scriptless objects/barricades
        gameObject.SetActive(state.isActive);
    }
}