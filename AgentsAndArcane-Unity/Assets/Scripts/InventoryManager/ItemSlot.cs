using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemSlot : MonoBehaviour, IPointerClickHandler, IDataPersistence
{
    //======ITEM DATA======//
    public Equipment equipment;
    public int quantity;
    public bool isFull;
    [SerializeField]
    public Sprite itemSprite;
    public Sprite emptySprite;


    //======ITEM SLOT======//
    [SerializeField]
    private TMP_Text quantityText;
    [SerializeField]
    private Image itemImage;


    //======ITEM DESCRIPTION======//
    public Image itemDescriptionImage;
    public TMP_Text itemNameText;
    public TMP_Text itemDescriptionText;
    public TMP_Text itemDamageText;
    public TMP_Text itemWeightText;


    public GameObject selectedShader;
    public bool isSelected;


    private InventoryManager inventoryManager;
    public ItemBelongsTo itemBelongsTo;
    public EquippedSlot equippedSlot;

    private void Start()
    {
        inventoryManager = GameObject.Find("InventoryCanvas").GetComponent<InventoryManager>(); // name might be different
    }
    public void AddItem(Equipment equipment, Sprite itemSprite, int quantity)
    {
        this.equipment = equipment;
        this.quantity += quantity;
        this.itemSprite = itemSprite;
        this.itemBelongsTo = equipment.itemBelongsTo;
        //isFull = true;
        quantityText.text = this.quantity.ToString();
        quantityText.enabled = true;
        quantityText.gameObject.SetActive(true);
        itemImage.sprite = itemSprite;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnLeftClick();
        }
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnRightClick();
        }
    }

    public void OnLeftClick()
    {
        if (equipment == null) return;
        if (isSelected)
        {
            // may add more usable items here
            //EquipGear();
            //emptySlot();
        } else {
            inventoryManager.DeselectAllSlots();
            selectedShader.SetActive(true);
            isSelected = true;
            if (equipment != null) // not check for resources
            {
                itemNameText.text = equipment.equipName;
                itemDescriptionText.text = equipment.description;
                itemDescriptionImage.sprite = itemSprite;
                itemDamageText.text = "Damage: " + equipment.damage.ToString();
                itemWeightText.text = "Weight: " + equipment.weight.ToString();
            } else {
                itemNameText.text = "";
                itemDescriptionText.text = "";
                itemDescriptionImage.sprite = emptySprite;
                itemDamageText.text = "";
                itemWeightText.text = "";
                //emptySlot();
            }

            if(itemDescriptionImage.sprite == null)
            {
                itemDescriptionImage.sprite = emptySprite;
            }
        }
    }

    private void EquipGear()
    {
        // TODO: Add check unit types function to check if the item can be equipped by the unit
        // equippedSlot.EquipGear(itemSprite);
        // emptySlot();
    }


    // drop the item from the inventory by 1
    public void OnRightClick()
    {
        if (equipment == null) return;
        if (isSelected)
        {
            if (this.quantity - 1 == 0)
            {
                itemNameText.text = "";
                itemDescriptionText.text = "";
                itemDamageText.text = "";
                itemWeightText.text = "";
                itemDescriptionImage.sprite = emptySprite;
                emptySlot();
            }

            if (this.quantity > 0)
            {
                this.quantity -= 1;
                quantityText.text = this.quantity.ToString();
            }
        } else {
            inventoryManager.DeselectAllSlots();
            selectedShader.SetActive(true);
            isSelected = true;
        }
        
    }

    public void emptySlot()
    {
        equipment = null;
        isFull = false;
        itemSprite = emptySprite;
        itemImage.sprite = emptySprite;
        quantity = 0;
        quantityText.text = quantity.ToString();
        quantityText.enabled = false;
        quantityText.gameObject.SetActive(false);

    }

    public void LoadData(GameData data)
    {
        
    }

    public void SaveData(GameData data)
    {
        
    }
}
