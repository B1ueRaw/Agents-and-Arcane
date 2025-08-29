using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class EquippedSlot : MonoBehaviour, IPointerClickHandler
{
    //======SLOT APPEARANCE======//
    [SerializeField]
    public Image slotImage;
    [SerializeField]
    public Sprite emptySprite;

    //======SLOT DATA======//
    [SerializeField]
    private ItemBelongsTo itemBelongsTo;
    public Equipment equipment;


    private Sprite itemSprite;

    [SerializeField]
    public bool thisItemSelected;

    [SerializeField]
    public GameObject selectedShader;

    public bool slotInUse;

    private InventoryManager inventoryManager;

    public HexUnit hexUnit;

    private void Start()
    {
        inventoryManager = GameObject.Find("InventoryCanvas").GetComponent<InventoryManager>(); // name might be different
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnLeftClick();
        }
        // if (eventData.button == PointerEventData.InputButton.Right)
        // {
        //     OnRightClick();
        // }
    }


    void OnLeftClick()
    {
        if (thisItemSelected && slotInUse) {
            UnEquipGear(this.equipment, this.hexUnit);
        } else {
            //inventoryManager.DeselectAllSlots();
            selectedShader.SetActive(true);
            thisItemSelected = true;
            slotInUse = true;
        }
    }


    public void EquipGear(Sprite itemSprite, Equipment equipment, HexUnit hexUnit)
    {
        if (slotInUse) // if there is already an item in the slot, unequip it first, then equip the new item
        {
            UnEquipGear(this.equipment, this.hexUnit);
        }

        // update image
        this.itemSprite = itemSprite;
        slotImage.sprite = this.itemSprite;

        // update data
        this.equipment = equipment;
        this.hexUnit = hexUnit;

        // update hexUnit
        hexUnit.equipment = equipment;
        hexUnit.damage += equipment.damage;

        hexUnit.itemDamageText.text = hexUnit.damage.ToString();

        slotInUse = true;

    }

    public void UnEquipGear(Equipment equipment, HexUnit hexUnit)
    {
        //inventoryManager.DeselectAllSlots();

        if (equipment == null) return;
        // update hexUnit
        hexUnit.damage -= equipment.damage;
        hexUnit.itemDamageText.text = hexUnit.damage.ToString();
        hexUnit.equipment = null;

        inventoryManager.AddItem(this.equipment, this.itemSprite, 1);

        slotImage.sprite = emptySprite;
        slotInUse = false;
        equipment = null;
    }

    public void emptySlot()
    {
        slotImage.sprite = emptySprite;
        slotInUse = false;
        equipment = null;
    }

}
