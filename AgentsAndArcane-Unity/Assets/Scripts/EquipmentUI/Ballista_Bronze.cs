using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ballista_Bronze : Equipment
{
    // Start is called before the first frame update
    public DragonRider dragonRider;

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
        Ballista_Bronze ballista;
        for (int i = 0; i < data.equipmentNames.Length; i++)
        {
            if (data.equipmentNames[i] == "Bronze Ballista")
            {
                ballista = Instantiate(this);
                ballista.SetEquipmentID(data.id[i]);
                inventoryManager.AddItem(ballista, sprite, data.itemQuantity[i]);
            }
        }
    }
}
