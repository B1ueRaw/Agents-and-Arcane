using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class Click : MonoBehaviour, IDataPersistence
{
    public Button tmpButton;         // Reference to the TMP button
    private int clickCount = 0;      // To track the number of clicks
    void Start()
    {
        // Ensure the button is not null, and add a listener to the button
        // if (tmpButton != null)
        // {
        //     tmpButton.onClick.AddListener(IncrementClickCount);
        // }
        // Initialize the counter text
    }

    // This method is called every time the button is clicked
    public void IncrementClickCount()
    {
        this.clickCount++;
        Debug.Log("click count: " + this.clickCount);
        //DataPersistenceManager.instance.SaveGame();
    }

    public void LoadData(GameData data)
    {
        this.clickCount = data.clickCount;
        //Debug.Log("loaded!");
    }

    public void SaveData(GameData data)
    {
        //Debug.Log("local click count: " + this.clickCount);
        data.clickCount = this.clickCount;
        //Debug.Log("data click count: " + data.clickCount);
    }

    // Updates the text to display the current click count
}
