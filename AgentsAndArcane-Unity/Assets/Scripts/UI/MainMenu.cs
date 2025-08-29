using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Animator transition;

    public float transitionTime = 1.0f;

    private bool playGameCalled = false;

    [Header("Menu Navigation")]
    [SerializeField] private SaveSlotsMenu saveSlotsMenu;

    [Header("Menu Buttons")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button LoadGameButton;

    private void Start()
    {
        this.ActivateMenu();
        saveSlotsMenu.DeactivateMenu();
        DisableButtonsDependingOnData();
    }

    private void DisableButtonsDependingOnData()
    {
        if(!DataPersistenceManager.instance.HasGameData())
        {
            continueButton.interactable = false;
            LoadGameButton.interactable = false;
        }
        else
        {
            continueButton.interactable = true;
            LoadGameButton.interactable = true;
        }
    }

    void Update()
    {
        if (playGameCalled)
        {
            LoadNextLevel();
            playGameCalled = false; // in case it always run
        }
    }

    public void OnPlayClicked ()
    {
        saveSlotsMenu.ActivateMenu(false);
        this.DeactivateMenu();
    }

    public void OnLoadGameClicked ()
    {
        saveSlotsMenu.ActivateMenu(true);
        this.DeactivateMenu();
    }

    public void QuitGame()
    {
        Debug.Log("Quit!"); // enable see message in console but not close the program
        Application.Quit();
    }


    public void LoadNextLevel()
    {
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        SceneManager.LoadSceneAsync("Hex Map Scene");
    }

    IEnumerator LoadLevel(int levelIndex)
    {
        // play animator
        transition.SetTrigger("Start");

        //wait
        yield return new WaitForSeconds(transitionTime);

        //load scene
        SceneManager.LoadScene(levelIndex); // might need to do SceneManager.LoadSceneAsync("Hex Map Scene");
    }


    // When continue is clicked
    public void OnContinueGameClicked() 
    {
        DisableMenuButtons();
        // save the game anytime before loading a new scene
        DataPersistenceManager.instance.SaveGame();
        // load the next scene - which will in turn load the game because of 
        // OnSceneLoaded() in the DataPersistenceManager
        SceneManager.LoadSceneAsync("Hex Map Scene");
    }

    private void DisableMenuButtons()
    {
        newGameButton.interactable = false;
        continueButton.interactable = false;
    }

    public void ActivateMenu() 
    {
        this.gameObject.SetActive(true);
        DisableButtonsDependingOnData();
    }

    public void DeactivateMenu() 
    {
        this.gameObject.SetActive(false);
    }
}
