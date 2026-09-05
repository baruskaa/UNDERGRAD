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

    public Sprite image;
    public string title, description;

    private bool isOpen = false;


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
            uiItem.OnItemBeginDrag += HandleBeginDrag;
            uiItem.OnItemDroppedOn += HandleSwap;
            uiItem.OnItemEndDrag += HandleEndDrag;
            uiItem.OnRightMouseBtnClick += HandleShowItemActions;
        }
    }

    private void HandleShowItemActions(InventoryItem item)
    {
    }

    private void HandleEndDrag(InventoryItem item)
    {
    }

    private void HandleSwap(InventoryItem item)
    {
    }

    private void HandleBeginDrag(InventoryItem item)
    {
    }

    private void HandleItemSelection(InventoryItem item)
    {
        itemDescription.SetDescription(image, title, description);
        listOfUIItems[0].Select();
    }

    public void Show()
    {
        isOpen = true;
        itemDescription.ResetDescription();

        listOfUIItems[0].SetData(image);
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