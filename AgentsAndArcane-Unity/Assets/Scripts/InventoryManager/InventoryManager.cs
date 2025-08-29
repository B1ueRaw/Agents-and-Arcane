using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InventoryManager : MonoBehaviour, IDataPersistence
{
    public GameObject InventoryMenu;
    public GameObject EquipmentMenu;
    public ItemSlot[] itemSlot; // slot in the inventory menu
    public EquipmentSlot[] equipmentSlot; // slot in the equipment menu
    public ItemSO[] itemSOs;
    //public EquipmentSlot[] EquipmentSlot;
    public EquippedSlot equippedSlot;
    public GameObject backButton;

    public Equipment equipment;

    //class for load
    [SerializeField]
    //public Bow bow;
    // sprite for load
    
    // fields for unit info
    public TMP_Text unitNameText;
    public TMP_Text itemDamageText;
    public TMP_Text itemHealthText;
    public TMP_Text unitBackgroundText;


    // Start is called before the first frame update
    void Start()
    {
        // reference for unit info
		unitNameText = GameObject.Find("UnitNameText").GetComponent<TMP_Text>();
		itemDamageText = GameObject.Find("DamageNumberText").GetComponent<TMP_Text>();
		itemHealthText = GameObject.Find("ActualHealthText").GetComponent<TMP_Text>();
		unitBackgroundText = GameObject.Find("BackgroundText").GetComponent<TMP_Text>();

        // EquipmentMenu.SetActive(false);
        // backButton.SetActive(false);
        //DataPersistenceManager.instance.LoadGame();

    }
    

    // Update is called once per frame
    void Update()
    {
        // Open and close the inventory menu with TAB
        // input button can be changed in edit->Project Settings->Input Manager->InventoryMenu
        if (Input.GetButtonDown("InventoryMenu"))
        {
            // Inventory();
        }
    }

    void Inventory() {
        if (InventoryMenu.activeSelf)
        {
            Time.timeScale = 1;
            InventoryMenu.SetActive(false);
            EquipmentMenu.SetActive(false);
        } else {
            Time.timeScale = 0;
            InventoryMenu.SetActive(true);
            EquipmentMenu.SetActive(false);
        }
    }


    // Add an item to the inventory, test only
    public void AddItem(Equipment equipment, Sprite sprite, int quantity)
    {
        if (equipment.itemBelongsTo == ItemBelongsTo.NONE) {
            // add the item to the item slot in the inventory,
            // eventhough we dont have any non-equipment items in sprint 1
            for (int i = 0; i < itemSlot.Length; i++)
            {
                if (itemSlot[i].quantity != 0)
                {
                    // check if the item is already in the inventory
                    if (itemSlot[i].equipment != null && itemSlot[i].equipment.equipName == equipment.equipName) { // so far not check stacks for resources
                        itemSlot[i].AddItem(equipment, sprite, quantity);
                        return;
                    }
                } else {
                    itemSlot[i].AddItem(equipment, sprite, quantity);
                    return;
                }
            }
        } else {
            // add the item to the inventory, if we have non-equipment items passed to inventory
            for (int i = 0; i < equipmentSlot.Length; i++)
            {
                // add equipments in the equipment slot of equipment menu

                /*
                if (equipmentSlot[i].quantity != 0)
                {
                    // check if the item is already in the inventory
                    if (equipmentSlot[i].equipment != null && equipmentSlot[i].equipment.equipName == equipment.equipName) { // so far not check stacks for resources
                        equipmentSlot[i].AddItem(equipment, sprite, quantity);
                        return;
                    }
                } else {
                    equipmentSlot[i].AddItem(equipment, sprite, quantity);
                    return;
                }
                */

                // item slots are no longer stackable for loading purposes
                if (equipmentSlot[i].quantity == 0) {
                    equipmentSlot[i].AddItem(equipment, sprite, quantity);
                    return;
                }
            }
        }
    }


    public void DeselectAllSlots()
    {
        for (int i = 0; i < itemSlot.Length; i++)
        {
            itemSlot[i].isSelected = false;
            itemSlot[i].selectedShader.SetActive(false);
        }

        //equippedSlot.selectedShader.SetActive(false);
        //equippedSlot.thisItemSelected = false;

        for (int i = 0; i < equipmentSlot.Length; i++)
        {
            equipmentSlot[i].isSelected = false;
            equipmentSlot[i].selectedShader.SetActive(false);
        }
    }

    public void LoadData(GameData data)
    {
        // Equipments are added to the inventory in each equipment scripts
    }

    public void SaveData(GameData data)
    {
        //for (int i = 0; i < itemSlot.Length; i++)
        //Debug.Log("item slot length: " + data.itemSlot.Length);
        //Debug.Log("data length: " + data.equipmentNames.Length);
        //Debug.Log("equipment slot length: " + equipmentSlot.Length);
        for (int i = 0; i < equipmentSlot.Length; i++)
        {
            // Debug.Log(i);
            if (equipmentSlot[i].equipment != null) {
                //======DATA IN ITEM SLOT======//
                data.isFull[i] = equipmentSlot[i].isFull;
                data.itemQuantity[i] = equipmentSlot[i].quantity;
                //======DATA IN EQUIPMENT======//
                data.id[i] = equipmentSlot[i].equipment.id;

                //Debug.Log("datae name: " + equipmentSlot[i].equipment.equipName);
                //Debug.Log("equipment slot: " + equipmentSlot[i].equipment.equipName);
                data.equipmentNames[i] = equipmentSlot[i].equipment.equipName;
                data.rarity[i] = equipmentSlot[i].equipment.rarity;
                data.isEquipped[i] = equipmentSlot[i].equipment.isEquipped;
                data.itemBelongsTo[i] = equipmentSlot[i].equipment.itemBelongsTo;
                data.damage[i] = equipmentSlot[i].equipment.damage;
                data.weight[i] = equipmentSlot[i].equipment.weight;
            } else {
                data.id[i] = -1;
                // also modify the quantity and isFull, names
            }

            // temporary nothing to save for the item slot in inventory menu
        }
        
    }
}
