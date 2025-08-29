using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bow_Gold : Equipment, IDataPersistence
{
    // Start is called before the first frame update
    public Archer archer;
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
        Bow_Gold bow;
        for (int i = 0; i < data.equipmentNames.Length; i++)
        {
            if (data.equipmentNames[i] == "Gold Bow")
            {
                bow = Instantiate(this);
                bow.SetEquipmentID(data.id[i]);
                inventoryManager.AddItem(bow, sprite, data.itemQuantity[i]);
            }
        }
    }
}
