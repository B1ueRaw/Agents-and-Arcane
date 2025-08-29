using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tome_Silver : Equipment
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
        Tome_Silver tome;
        for (int i = 0; i < data.equipmentNames.Length; i++)
        {
            if (data.equipmentNames[i] == "Silver Tome")
            {
                tome = Instantiate(this);
                tome.SetEquipmentID(data.id[i]);
                inventoryManager.AddItem(tome, sprite, data.itemQuantity[i]);
            }
        }
    }
}
