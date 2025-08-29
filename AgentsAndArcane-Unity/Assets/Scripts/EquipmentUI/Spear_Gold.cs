using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spear_Gold : Equipment
{
    public Cavalry cavalry;
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
        Spear_Gold spear;
        for (int i = 0; i < data.equipmentNames.Length; i++)
        {
            if (data.equipmentNames[i] == "Gold Spear")
            {
                spear = Instantiate(this);
                spear.SetEquipmentID(data.id[i]);
                inventoryManager.AddItem(spear, sprite, data.itemQuantity[i]);
            }
        }
    }
}
