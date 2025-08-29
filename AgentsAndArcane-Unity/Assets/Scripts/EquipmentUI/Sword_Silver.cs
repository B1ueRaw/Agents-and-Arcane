using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword_Silver : Equipment
{
    public Infantry infantry;
    // Start is called before the first frame update
    void Start()
    {
        inventoryManager = GameObject.Find("InventoryCanvas").GetComponent<InventoryManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public new void LoadData(GameData data)
    {
        Sword_Silver sword;
        for (int i = 0; i < data.equipmentNames.Length; i++)
        {
            if (data.equipmentNames[i] == "Silver Sword")
            {
                sword = Instantiate(this);
                sword.SetEquipmentID(data.id[i]);
                inventoryManager.AddItem(sword, sprite, data.itemQuantity[i]);
            }
        }
    }
}
