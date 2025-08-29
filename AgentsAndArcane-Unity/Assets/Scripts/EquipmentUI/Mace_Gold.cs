using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mace_Gold : Equipment
{
    public Gargoyle gargoyle;
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
        Mace_Gold mace;
        for (int i = 0; i < data.equipmentNames.Length; i++)
        {
            if (data.equipmentNames[i] == "Gold Mace")
            {
                mace = Instantiate(this);
                mace.SetEquipmentID(data.id[i]);
                inventoryManager.AddItem(mace, sprite, data.itemQuantity[i]);
            }
        }
    }
}
