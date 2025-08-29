using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dagger_Gold : Equipment
{
    public Thief thief;
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
        Dagger_Gold dagger;
        for (int i = 0; i < data.equipmentNames.Length; i++)
        {
            if (data.equipmentNames[i] == "Gold Dagger")
            {
                dagger = Instantiate(this);
                dagger.SetEquipmentID(data.id[i]);
                inventoryManager.AddItem(dagger, sprite, data.itemQuantity[i]);
            }
        }
    }
}
