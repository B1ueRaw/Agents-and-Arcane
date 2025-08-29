using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bow_Silver : Equipment, IDataPersistence
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
        // if this bow is being clicked
        if (Input.GetKeyDown(KeyCode.Z))
        {
            // add this bow to the inventory
            inventoryManager.AddItem(this, sprite, 1);
            //Destroy(gameObject);
        }
    }

    public new void LoadData(GameData data)
    {
        Bow_Silver bow;
        for (int i = 0; i < data.equipmentNames.Length; i++)
        {
            if (data.equipmentNames[i] == "Silver Bow")
            {
                bow = Instantiate(this);
                bow.SetEquipmentID(data.id[i]);
                inventoryManager.AddItem(bow, sprite, data.itemQuantity[i]);
            }
        }
    }
}
