using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ItemSO : ScriptableObject
{
    public string itemName;
    public string description;
    public Rarity rarity;
    public int damage;
    public int weight;
    public HexUnit heldBy;
    public bool isEquipped;

    public void EquipItem() {
        isEquipped = true;
        //heldBy.damage += damage;
        Debug.Log(itemName + " equipped and damage is now: ");
    }
}
