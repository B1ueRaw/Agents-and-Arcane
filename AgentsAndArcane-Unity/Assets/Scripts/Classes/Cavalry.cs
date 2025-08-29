using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Cavalry : HexUnit
{
    public InventoryManager inventoryManager;
    void Start() {
        inventoryManager = GameObject.Find("InventoryCanvas").GetComponent<InventoryManager>();
        uiManager = GameObject.Find("UIManager").GetComponent<UIManager>();
        equipmentMenu = inventoryManager.EquipmentMenu;
        
		unitNameText = inventoryManager.unitNameText;
		itemDamageText = inventoryManager.itemDamageText;
		itemHealthText = inventoryManager.itemHealthText;
		unitBackgroundText = inventoryManager.unitBackgroundText;
		equippedSlot = inventoryManager.equippedSlot;
        equipmentSlot = inventoryManager.equipmentSlot;
        backButton = inventoryManager.backButton;
        for (int i = 0; i < equipmentSlot.Length; i++)
        {
            equipmentSlot[i].hexUnit = this;
        }
        backButton.SetActive(false);
        isThinking = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isThinking) {
			thinkingSign.SetActive(true);
            thinkingSign.transform.Rotate(0, 1, 0);
		} else {
			thinkingSign.SetActive(false);
		}
    }

    public void setHexUnitForEquipmentSlot()
    {
        equipmentSlot = inventoryManager.equipmentSlot;
        for (int i = 0; i < equipmentSlot.Length; i++)
        {
            equipmentSlot[i].hexUnit = this;
        }

        if (equipment != null)
        {
            equippedSlot.slotImage.sprite = this.equipment.sprite;
            equippedSlot.equipment = this.equipment;
        } else {
            equippedSlot.slotImage.sprite = null;
            equippedSlot.equipment = null;
            equippedSlot.emptySlot();
        }
    }
}
