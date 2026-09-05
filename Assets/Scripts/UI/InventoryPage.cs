using Inventory.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryPage : MonoBehaviour
{
    [SerializeField]
    private InventoryItem itemPrefab;

    [SerializeField]
    private RectTransform contentPanel;

    [SerializeField]
    private InventoryDescription itemDescription;

    [SerializeField]
    private Animator animator;

    List<InventoryItem> listOfUIItems = new List<InventoryItem>();

    private bool isOpen = false;

    public event Action<int> OnDescriptionRequested;
    public event Action<int> OnItemActionRequested;

    private void Awake()
    {
        Idle();
        itemDescription.ResetDescription();
    }

    public void InitializeInventoryUI(int inventorysize)
    {
        for (int i = 0; i < inventorysize; i++)
        {
            InventoryItem uiItem =
                Instantiate(itemPrefab, Vector3.zero, Quaternion.identity);
            uiItem.transform.SetParent(contentPanel);
            listOfUIItems.Add(uiItem);
            uiItem.OnItemClicked += HandleItemSelection;
            uiItem.OnRightMouseBtnClick += HandleShowItemActions;
        }
    }

    // Called by whatever owns the actual inventory data,
    // to push a sprite/quantity into a specific slot.
    public void UpdateData(int itemIndex, Sprite itemImage)
    {
        if (listOfUIItems.Count > itemIndex)
        {
            listOfUIItems[itemIndex].SetData(itemImage);
        }
    }

    // Called by whatever owns the actual inventory data,
    // to show the description panel for a specific slot.
    public void UpdateDescription(int itemIndex, Sprite itemImage, string name, string description)
    {
        itemDescription.SetDescription(itemImage, name, description);
        DeselectAllItems();
        listOfUIItems[itemIndex].Select();
    }

    public void ResetAllItems()
    {
        foreach (var item in listOfUIItems)
        {
            item.ResetData();
            item.Deselect();
        }
    }

    private void DeselectAllItems()
    {
        foreach (var item in listOfUIItems)
        {
            item.Deselect();
        }
    }

    private void HandleItemSelection(InventoryItem item)
    {
        int index = listOfUIItems.IndexOf(item);
        if (index == -1)
            return;
        OnDescriptionRequested?.Invoke(index);
    }

    private void HandleShowItemActions(InventoryItem item)
    {
        int index = listOfUIItems.IndexOf(item);
        if (index == -1)
            return;
        OnItemActionRequested?.Invoke(index);
    }

    public void ResetSelection()
    {
        itemDescription.ResetDescription();
        DeselectAllItems();
    }

    public void Show()
    {
        isOpen = true;
        itemDescription.ResetDescription();
        animator.Play("Show");
    }

    public void Hide()
    {
        isOpen = false;
        animator.Play("Hide");
    }

    public void Idle()
    {
        animator.Play("Idle");
    }

    public bool IsOpen()
    {
        return isOpen;
    }
}