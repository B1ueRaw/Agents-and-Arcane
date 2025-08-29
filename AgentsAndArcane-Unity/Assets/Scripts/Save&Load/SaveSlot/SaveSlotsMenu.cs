using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SaveSlotsMenu : Menu
{

    [Header("Menu Buttons")]
    [SerializeField] private Button backButton;

    [Header("Confirmation Popup")]
    [SerializeField] private ConfirmationPopupMenu confirmationPopupMenu;

    private SaveSlot[] saveSlots;

    private bool isLoadingGame = false;

    GameData gameData;

    private void Awake() 
    {
        saveSlots = this.GetComponentsInChildren<SaveSlot>();
    }

    public void OnSaveSlotClicked(SaveSlot saveSlot) 
    {
        // disable all buttons
        DisableMenuButtons();

        // case - loading game
        if (isLoadingGame) 
        {
            DataPersistenceManager.instance.ChangeSelectedProfileId(saveSlot.GetProfileId());
            SaveGameAndLoadScene(0);
        }
        // case - new game, but the save slot has data
        else if (saveSlot.hasData) 
        {
            confirmationPopupMenu.ActivateMenu(
                "Data will be overrided. Are you sure?",
                // function to execute if we select 'yes'
                () => {
                    DataPersistenceManager.instance.ChangeSelectedProfileId(saveSlot.GetProfileId());
                    DataPersistenceManager.instance.NewGame();
                    SaveGameAndLoadScene(1);
                },
                // function to execute if we select 'cancel'
                () => {
                    this.ActivateMenu(isLoadingGame);
                }
            );
        }
        // case - new game, and the save slot has no data
        else 
        {
            DataPersistenceManager.instance.ChangeSelectedProfileId(saveSlot.GetProfileId());
            DataPersistenceManager.instance.NewGame();
            SaveGameAndLoadScene(1);
        }
    }

    private void SaveGameAndLoadScene(int sceneIndex) 
    {
        // save the game anytime before loading a new scene
        //DataPersistenceManager.instance.SaveGame();
        //Debug.Log("Index of level: " + DataPersistenceManager.instance.currentSceneIndex);
        // load the scene
        //SceneManager.LoadSceneAsync("Start");
        

        if (sceneIndex == 0)
        { // case - loading game
            //SceneManager.LoadSceneAsync(DataPersistenceManager.instance.currentSceneIndex);
            SceneManager.LoadSceneAsync("Hex Map Scene");
        }
        else if (sceneIndex == 1)
        { // case - new game, but the save slot has data
            SceneManager.LoadSceneAsync("Hex Map Scene");
        }
        else if (sceneIndex == 2)
        { // case - new game, and the save slot has no data
            SceneManager.LoadSceneAsync("Hex Map Scene");
        }
    }

    public void OnClearClicked(SaveSlot saveSlot) 
    {
        DisableMenuButtons();

        confirmationPopupMenu.ActivateMenu(
            "Are you sure you want to delete this saved data?",
            // function to execute if we select 'yes'
            () => {
                DataPersistenceManager.instance.DeleteProfileData(saveSlot.GetProfileId());
                ActivateMenu(isLoadingGame);
            },
            // function to execute if we select 'cancel'
            () => {
                ActivateMenu(isLoadingGame);
            }
        );
    }

    public void ActivateMenu(bool isLoadingGame) 
    {
        // set this menu to be active
        this.gameObject.SetActive(true);

        // set mode
        this.isLoadingGame = isLoadingGame;

        // load all of the profiles that exist
        Dictionary<string, GameData> profilesGameData = DataPersistenceManager.instance.GetAllProfilesGameData();

        // ensure the back button is enabled when we activate the menu
        backButton.interactable = true;

        // loop through each save slot in the UI and set the content appropriately
        GameObject firstSelected = backButton.gameObject;
        foreach (SaveSlot saveSlot in saveSlots) 
        {
            GameData profileData = null;
            profilesGameData.TryGetValue(saveSlot.GetProfileId(), out profileData);
            saveSlot.SetData(profileData);
            if (profileData == null && isLoadingGame) 
            {
                saveSlot.SetInteractable(false);
            }
            else 
            {
                saveSlot.SetInteractable(true);
                if (firstSelected.Equals(backButton.gameObject))
                {
                    firstSelected = saveSlot.gameObject;
                }
            }
        }

        // set the first selected button
        Button firstSelectedButton = firstSelected.GetComponent<Button>();
        //this.SetFirstSelected(firstSelectedButton);
    }

    public void DeactivateMenu() 
    {
        this.gameObject.SetActive(false);
    }

    private void DisableMenuButtons() 
    {
        foreach (SaveSlot saveSlot in saveSlots) 
        {
            saveSlot.SetInteractable(false);
        }
        backButton.interactable = false;
    }
}
