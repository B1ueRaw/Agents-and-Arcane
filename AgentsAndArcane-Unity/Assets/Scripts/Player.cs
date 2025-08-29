using Group3d.Notifications;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour, IDataPersistence
{
    // UI Game Objects
    public GameObject craftingUI;
    public GameObject unitCreateUI;
    public GameObject craftingContent;
    public TileInfoManager tileInfoManager;
    public InventoryManager inventoryManager;
    // Array of craftable recipes used to populate crafting UI
    public Recipe[] recipes;
    private int[] ownedResources;

    public TextMeshProUGUI[] resourceCountText;
    public UIManager uiManager;


    // Start is called before the first frame update
    void Start()
    {
        // resources initialization moved to LoadData
    }

    // Update is called once per frame
    void Update()
    {
        // if (Input.GetKeyDown(KeyCode.R))
        // {
        //     craftingUI.SetActive(!craftingUI.activeSelf);
        // }
        // if (Input.GetKeyDown(KeyCode.I))
        // {
        //     // Open tile info for hovered tile
        //     tileInfoManager.Open();
        // }
    }

    /**
     * Checks and returns whether the player can craft the given recipe
     */
    public bool CanCraft(Recipe recipe)
    {
        return recipe.CanCraft(ownedResources);
    }

    /**
     * Called by button in Crafting UI
     * Checks if selected item can be crafted and adds it to the player's inventory
     */
    public void Craft()
    {
        // Find item to craft and perform necessary calcs/movements
        ToggleGroup craftingSelection = craftingContent.GetComponent<ToggleGroup>();
        Toggle selectionToggle = craftingSelection.GetFirstActiveToggle();
        Recipe selection = recipes[Int32.Parse(selectionToggle.name)];

        Equipment equipment;
        try
        {
            equipment = selection.Craft(this, ownedResources);
            if (equipment != null)
            {
                equipment = Instantiate(equipment);
                equipment.SetEquipmentID(Equipment.equipment_count);
                Equipment.equipment_count++;
            }
        } catch (NoCraftException) {
            Notifications.Send("You do not have the resources to craft that item", NotificationType.Warning);
            return;
        }

        if (equipment == null)
        {
            craftingUI.SetActive(false);
            unitCreateUI.SetActive(true);
            uiManager.currentMenu = unitCreateUI;
            uiManager.DisableUIActions();
            HexMapCamera.Locked = true;
            return;
        }

        // Add equipment to inventory
        inventoryManager.AddItem(equipment, equipment.sprite, 1);
    }

    private void SetResource(int resource_index, int count)
    {
        ownedResources[resource_index] = count;
        if (resource_index > resourceCountText.Length || resourceCountText[resource_index] != null)
            resourceCountText[resource_index].text = count.ToString();
    }

    public bool UseResource(int resource_index, int count)
    {
        if (ownedResources[resource_index] < count)
        {
            return false;
        }

        ownedResources[resource_index] -= count;
        if (resource_index > resourceCountText.Length || resourceCountText[resource_index] != null)
            resourceCountText[resource_index].text = ownedResources[resource_index].ToString();
        return true;
    }

    public void AddResource(int resource_index, int count)
    {
        ownedResources[resource_index] += count;
        if (resource_index > resourceCountText.Length || resourceCountText[resource_index] != null)
            resourceCountText[resource_index].text = ownedResources[resource_index].ToString();
    }

    public string InventoryToJSON()
    {
        List<string> _jsonInventory = new List<string>((int)Resources.LENGTH);
        for (int i = 0; i < (int)Resources.LENGTH; i++)
        {
            _jsonInventory.Add($"{{\"resource_id\":{i},\"quantity\":{ownedResources[i]}}}");
        }
        string _json = "\"inventory\": [" + string.Join(",", _jsonInventory) + "]";
        return _json;
    }

    public void LoadData(GameData data)
    {
        Debug.Log("Loading Player Data");
        if (data.firstTimePlayed) {
            ownedResources = new int[(int)Resources.LENGTH];
            SetResource((int)Resources.MANA, 20000);
            SetResource((int)Resources.COPPER, 20000);
            SetResource((int)Resources.WOOD, 20000);
        } else {
            ownedResources = new int[(int)Resources.LENGTH];
            SetResource((int)Resources.MANA, data.ownedResources[(int)Resources.MANA]);
            SetResource((int)Resources.COPPER, data.ownedResources[(int)Resources.COPPER]);
            SetResource((int)Resources.WOOD, data.ownedResources[(int)Resources.WOOD]);
            SetResource((int)Resources.IRON, data.ownedResources[(int)Resources.IRON]);
            SetResource((int)Resources.DIAMOND, data.ownedResources[(int)Resources.DIAMOND]);
        }
    }

    public void SaveData(GameData data)
    {
        data.ownedResources[(int)Resources.COPPER] = ownedResources[(int)Resources.COPPER];
        data.ownedResources[(int)Resources.MANA] = ownedResources[(int)Resources.MANA];
        data.ownedResources[(int)Resources.WOOD] = ownedResources[(int)Resources.WOOD];
        data.ownedResources[(int)Resources.IRON] = ownedResources[(int)Resources.IRON];
        data.ownedResources[(int)Resources.DIAMOND] = ownedResources[(int)Resources.DIAMOND];
        data.firstTimePlayed = false;
    }
}
