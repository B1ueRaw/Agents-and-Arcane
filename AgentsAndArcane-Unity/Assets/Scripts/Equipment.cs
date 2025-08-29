using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Rarity
{
    BRONZE,
    SILVER,
    GOLD
}

public enum ItemBelongsTo
{
    thief,
    dragonrider,
    cavelry,
    mage,
    archer,
    infantry,
    gargoyle,
    NONE
}

public class Equipment : MonoBehaviour, IDataPersistence
{
    public static int equipment_count = 0;
    public InventoryManager inventoryManager;
    // Description info
    public int id;
    public string equipName;
    public string description;
    public Rarity rarity;
    public HexUnit heldBy;
    public bool isEquipped;
    public ItemBelongsTo itemBelongsTo;
    public Sprite sprite;

    // Equipment Stats
    public int damage;
    public int weight;

    static Equipment()
    {
        Equipment.equipment_count = 0;
    }

    [Header("Equipment Sprite")]
    public Sprite BronzeBowSprite;
    // Start is called before the first frame update
    void Awake()
    {
        inventoryManager = GameObject.Find("InventoryCanvas").GetComponent<InventoryManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetEquipmentID(int id)
    {
        this.id = id;
    }

    public string ToJSON()
    {
        string _json = $"{{\"equip_id\": {id},\"tier\": {(int)rarity},\"description\": {description},}}";
        return _json;
    }

    public virtual void LoadData(GameData data)
    {
        Equipment.equipment_count = data.equipment_count;
        // load for each equipment slot
        // Debug.Log("Loading in Equipment class");
        // for (int i = 0; i < data.equipmentNames.Length; i++)
        // {
        //     if (data.equipmentNames[i] == "Bronze Bow")
        //     {
        //         Debug.Log("Bronze Bow Added");
        //         inventoryManager.AddItem(this, BronzeBowSprite, data.itemQuantity[i]);
        //         SetEquipmentID(data.id[i]);
        //     }
        // }
    }

    public void SaveData(GameData data)
    {
        // Debug.Log("Saving in Equipment class");
        data.equipment_count = Equipment.equipment_count;
    }
}
