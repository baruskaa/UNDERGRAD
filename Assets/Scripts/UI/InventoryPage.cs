using System.Collections.Generic;
using UnityEngine;

public class InventoryPage : MonoBehaviour
{
    [SerializeField]
    private InventoryItem itemPrefab;

    [SerializeField]
    private RectTransform contentPanel;

    [SerializeField]
    private Animator animator;

    List<InventoryItem> listOfUIItems = new List<InventoryItem>();

    private bool isOpen = false;

    public void InitializeInventoryUI(int inventorysize)
    {
        for (int i = 0; i < inventorysize; i++)
        {
            InventoryItem uiItem =
                Instantiate(itemPrefab, Vector3.zero, Quaternion.identity);
            uiItem.transform.SetParent(contentPanel);
            listOfUIItems.Add(uiItem);
        }
    }

    public void Show()
    {
        isOpen = true;
        animator.Play("Show");
    }

    public void Hide()
    {
        isOpen = false;
        animator.Play("Hide"); 
    }

    public bool IsOpen()
    {
        return isOpen;
    }
}