using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]


public class GameData
{
    public const int NUMBER_SLOT = 20;

    public long lastUpdated;
    public int currentSceneIndex;

    public int clickCount; // temporary variable to test data persistance
    // add new variables here to save them

    //======PLAYER DATA======//
    public int[] ownedResources;
    public bool firstTimePlayed;
    
    //======INVENTORY======//
    //public ItemSlot[] itemSlot;
    //======DATA IN ITEM SLOT======//
    //public Equipment[] equipments;
    public bool[] isFull;
    public int[] itemQuantity;
    //public Sprite[] itemSprite;
    //======DATA IN EQUIPMENT======//
    public int[] id;
    public string[] equipmentNames;
    public Rarity[] rarity;
    //public HexUnit[] heldBy;
    public bool[] isEquipped;
    public ItemBelongsTo[] itemBelongsTo;
    public int[] damage;
    public int[] weight;


    public int equipment_count;
    // values in the class are default value when a new game starts
    public GameData() // initialize the values
    {
        this.clickCount = 0;
        equipment_count = 0;
        currentSceneIndex = 1;
        firstTimePlayed = true;

        //======PLAYER DATA======//
        this.ownedResources = new int[(int)Resources.LENGTH];

        //======INVENTORY======//
        //this.itemSlot = new ItemSlot[NUMBER_SLOT];
        //======DATA IN ITEM SLOT======//
        //this.equipments = new Equipment[NUMBER_SLOT];
        this.isFull = new bool[NUMBER_SLOT];
        this.itemQuantity = new int[NUMBER_SLOT];
        //this.itemSprite = new Sprite[NUMBER_SLOT];
        //======DATA IN EQUIPMENT======//
        this.id = new int[NUMBER_SLOT];
        this.equipmentNames = new string[NUMBER_SLOT];
        this.rarity = new Rarity[NUMBER_SLOT];
        //this.heldBy = new HexUnit[NUMBER_SLOT];
        this.isEquipped = new bool[NUMBER_SLOT];
        this.itemBelongsTo = new ItemBelongsTo[NUMBER_SLOT];
        this.damage = new int[NUMBER_SLOT];
        this.weight = new int[NUMBER_SLOT];
    }

    /*
    Remember to add the interface IDataPersistence to the class that you want to save data from
    for example:
    public class Click : MonoBehaviour, IDataPersistence {
        public void LoadData(GameData data) {
            this.clickCount = data.clickCount;
        }
        public void SaveData(GameData data) {
            data.clickCount = this.clickCount;
        }
    }
    */
}
