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
            GenerateID();
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

        // 1. If this object has UnlockHallwayDoor, run custom door logic
        UnlockHallwayDoor hallwayDoor = GetComponent<UnlockHallwayDoor>();
        if (hallwayDoor != null)
        {
            hallwayDoor.ApplyLoadedState(state.isInteractableStateChanged);
            return;
        }

        // 2. FOR PURE DESIGN OBJECTS (Barricades, Tilemaps, Props with no custom scripts):
        // Automatically restore whether this GameObject was active or disabled when saved!
        gameObject.SetActive(state.isActive);
    }
}