using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum Resources
{
    MANA,
    COPPER,
    IRON,
    DIAMOND,
    WOOD,
    LENGTH
}

// Class for error to throw when item can't be crafted
public class NoCraftException : Exception
{
    public NoCraftException() : base() { }

    public NoCraftException(string message) : base(message) { }
}

public class Recipe : MonoBehaviour
{
    public int[] resourceCost = new int[(int) Resources.LENGTH];
    // If null, creates a new Unit
    public Equipment result;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /**
     * Check if item can be crafted given the amount of resources currently owned by the player
     * @param ownedResources An array representing the amount of resources owned by the player
     * @return Whether the player has enough resources to craft this item or not
     */
    public bool CanCraft(int[] ownedResources)
    {
        if (ownedResources == null || ownedResources.Length != resourceCost.Length)
        {
            return false;
        }

        for (int i = 0; i < resourceCost.Length; i++)
        {
            if (ownedResources[i] < resourceCost[i])
            {
                return false;
            }
        }
        return true;
    }

    /**
     * Craft this recipe if the player pocesses the needed resources
     * @return The equipment that was crafted, or null if a unit is crafted
     * @throws NoCraftException If the item cannot be crafted
     */
    public Equipment Craft(Player player, int[] ownedResources)
    {
        if (!CanCraft(ownedResources))
        {
            throw new NoCraftException("Lack resources to craft");
        }
        for (int i = 0; i < resourceCost.Length; i++)
        {
            player.UseResource(i, resourceCost[i]);
        }
        return result;
    }
}
