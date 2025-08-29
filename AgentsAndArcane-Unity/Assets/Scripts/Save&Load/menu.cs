using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class menu : MonoBehaviour
{
    public void OnNewGameClicked()
    {
        DataPersistenceManager.instance.NewGame();
    }

    public void OnSaveGameClicked()
    {
       DataPersistenceManager.instance.SaveGame();
       Debug.Log("Button Clicked");
    }

    public void OnLoadGameClicked()
    {
        DataPersistenceManager.instance.LoadGame();
    }

    public void OnClicked()
    {
        Debug.Log("Button Clicked");
    }
}
