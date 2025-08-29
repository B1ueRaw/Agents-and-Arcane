using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bow_Bronze : Equipment
{
    // Start is called before the first frame update
    public Archer archer;

    void Start()
    {
        //inventoryManager = GameObject.Find("InventoryPanel").GetComponent<InventoryManager>(); // the name might be different
        //inventoryManager.AddItem(this, sprite, 1);
        //Debug.Log("Bow Name: " + equipName);
    }

    // Update is called once per frame
    void Update()
    {
        // if this bow is being clicked
        
        // if (Input.GetKeyDown(KeyCode.Z))
        // {
        //     // add this bow to the inventory
        //     inventoryManager.AddItem(this, sprite, 1);
        //     //Destroy(gameObject);
        // }
        
    }

    // REMEMBER override keyword !!!!!!!!
    public override void LoadData(GameData data)
    {
        Bow_Bronze bow;
        for (int i = 0; i < data.equipmentNames.Length; i++)
        {
            if (data.equipmentNames[i] == "Bronze Bow")
            {
                bow = Instantiate(this);
                bow.SetEquipmentID(data.id[i]);
                inventoryManager.AddItem(bow, sprite, data.itemQuantity[i]);
            }
        }
    }
    // add all equipments scripts to equipment manager and set them accordingly
    // issues with loading ID, since adding this using one script, the ID will set all the same
    // might need to use crafting system to make a new instance of the equipment to properly set the ID

}
