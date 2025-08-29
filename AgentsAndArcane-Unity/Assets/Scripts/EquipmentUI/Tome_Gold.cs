using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tome_Gold : Equipment
{
    public Mage mage;
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
        Tome_Gold tome;
        for (int i = 0; i < data.equipmentNames.Length; i++)
        {
            if (data.equipmentNames[i] == "Gold Tome")
            {
                tome = Instantiate(this);
                tome.SetEquipmentID(data.id[i]);
                inventoryManager.AddItem(tome, sprite, data.itemQuantity[i]);
            }
        }
    }
}
