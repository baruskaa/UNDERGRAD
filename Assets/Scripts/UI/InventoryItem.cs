using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventoryItem : MonoBehaviour
{
    [SerializeField]
    private Image itemImage;

    [SerializeField]
    private Image borderImage;

    public event Action<InventoryItem> OnItemClicked,
        OnItemDroppedOn, OnItemBeginDrag, OnItemEndDrag, OnRightMouseBtnClick;

    private bool empty = true;

    public void Awake()
    {
        ResetData();
        Deselect();
    }

    // --- ADDED THIS METHOD TO PREVENT MISSING REFERENCE EXCEPTIONS ---
    private void OnDestroy()
    {
        OnItemClicked = null;
        OnItemDroppedOn = null;
        OnItemBeginDrag = null;
        OnItemEndDrag = null;
        OnRightMouseBtnClick = null;
    }

    public void ResetData()
    {
        if (itemImage != null)
            this.itemImage.gameObject.SetActive(false);

        empty = true;
    }

    public void Deselect()
    {
        if (borderImage != null)
            borderImage.enabled = false;
    }

    public void SetData(Sprite sprite)
    {
        if (itemImage != null)
        {
            this.itemImage.gameObject.SetActive(true);
            this.itemImage.sprite = sprite;
        }
        empty = false;
    }

    public void Select()
    {
        if (borderImage != null)
            borderImage.enabled = true;
    }

    public void OnBeginDrag(BaseEventData eventData)
    {
        if (empty)
            return;
        OnItemBeginDrag?.Invoke(this);
    }

    public void OnDrop(BaseEventData eventData)
    {
        OnItemDroppedOn?.Invoke(this);
    }

    public void OnEndDrag(BaseEventData eventData)
    {
        OnItemEndDrag?.Invoke(this);
    }

    public void OnPointerClick(BaseEventData data)
    {
        PointerEventData pointerData = (PointerEventData)data;
        if (pointerData.button == PointerEventData.InputButton.Right)
        {
            OnRightMouseBtnClick?.Invoke(this);
        }
        else
        {
            OnItemClicked?.Invoke(this);
        }
    }
}