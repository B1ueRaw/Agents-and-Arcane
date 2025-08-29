using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

public class CraftingUI : MonoBehaviour
{
    public Player player;
    public GameObject craftingContent;
    // Baseline Prefabs
    public TMP_Text recipeName;
    public TMP_Text recipeStats;
    public TMP_Text recipeCost;
    public Toggle toggleButton;

    // Constants
    private int craftingGap = -130;
    public string costTag = "RecipeCost";

    // Start is called before the first frame update
    void Start()
    {
        Recipe[] recipes = player.recipes;
        Debug.Log(recipes.Length);
        for (int i = 0; i < recipes.Length; i++)
        {
            Recipe recipe = recipes[i];
            // Set Recipe Title
            TMP_Text newRecipeName = Instantiate(recipeName);
            newRecipeName.transform.SetParent(craftingContent.transform, false);
            newRecipeName.transform.Translate(new Vector3(0, i * craftingGap));
            if (recipe.result != null)
            {
                newRecipeName.text = recipe.result.equipName;
            }
            else
            {
                newRecipeName.text = "Unit";
            }
            //Debug.Log(newRecipeName.text);
            // Set Recipe Name Color based on rarity
            if (recipe.result != null)
            {
                switch (recipe.result.rarity)
                {
                    case Rarity.BRONZE:
                        newRecipeName.color = new Color((float)205 / 255, (float)127 / 255, (float)50 / 255);
                        break;
                    case Rarity.SILVER:
                        newRecipeName.color = Color.white;
                        break;
                    case Rarity.GOLD:
                        newRecipeName.color = Color.yellow;
                        break;
                }
            }
            else
            {
                newRecipeName.color = Color.blue;
            }

            Toggle selectThisRecipe = Instantiate(toggleButton);
            // Set name as index to be recognized later
            selectThisRecipe.name = i.ToString();
            selectThisRecipe.transform.SetParent(craftingContent.transform, false);
            selectThisRecipe.transform.Translate(new Vector3(0, i * craftingGap));
            selectThisRecipe.group = craftingContent.GetComponent<ToggleGroup>();

            TMP_Text newRecipeStats = Instantiate(recipeStats);
            newRecipeStats.transform.SetParent(craftingContent.transform, false);
            newRecipeStats.transform.Translate(new Vector3(0, i * craftingGap));
            if (recipe.result != null)
            {
                string statsString = "Stats\nDamage: ";
                statsString = statsString + recipe.result.damage + "\nWeight: ";
                statsString = statsString + recipe.result.weight;
                newRecipeStats.text = statsString;
            } else
            {
                newRecipeStats.text = "Create new\nunit for your army";
            }

            TMP_Text newRecipeCost = Instantiate(recipeCost);
            newRecipeCost.transform.SetParent(craftingContent.transform, false);
            newRecipeCost.transform.Translate(new Vector3(0, i * craftingGap));
            newRecipeCost.tag = costTag;
            newRecipeCost.name = i.ToString();
            string costString = "Cost\n";
            // Set Cost values to be displayed
            for (Resources resource = 0; resource < Resources.LENGTH; resource++)
            {
                if (recipe.resourceCost[(int) resource] > 0)
                {
                    costString += recipe.resourceCost[(int) resource] + " ";
                    switch (resource)
                    {
                        case Resources.MANA:
                            costString += "Mana";
                            break;
                        case Resources.COPPER:
                            costString += "Copper";
                            break;
                        case Resources.IRON:
                            costString += "Iron";
                            break;
                        case Resources.DIAMOND:
                            costString += "Diamond";
                            break;
                        case Resources.WOOD:
                            costString += "Wood";
                            break;
                    }
                    costString += "\n";
                }
            }
            newRecipeCost.text = costString;

            // Set color based on ability to craft
            if (player.CanCraft(recipe))
            {
                newRecipeCost.color = Color.green;
            }
            else
            {
                newRecipeCost.color = Color.red;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (gameObject.activeSelf)
        {
            // Find all text labels that are related to recipe costs
            GameObject[] costs = GameObject.FindGameObjectsWithTag(costTag);
            foreach (GameObject cost in costs)
            {
                TMP_Text costText = cost.GetComponent<TMP_Text>();
                if (costText != null)
                {
                    int index = Int32.Parse(costText.name);
                    // Set color based on whether player can craft or not
                    if (player.CanCraft(player.recipes[index]))
                    {
                        costText.color = Color.green;
                    }
                    else
                    {
                        costText.color = Color.red;
                    }
                }
            }
        }
    }
}
