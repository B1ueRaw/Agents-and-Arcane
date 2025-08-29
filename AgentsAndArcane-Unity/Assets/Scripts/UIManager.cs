using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject HexMapEditor;
    public GameObject NewMapMenu;
    public GameObject SaveLoadMenu;
    public GameObject TileInfoMenu;
    public GameObject EndTurn;
    public GameObject ResoucesPanel;
    public GameObject CraftingUIMenu;
    public GameObject UnitCreationMenu;
    public GameObject InventoryMenu;
    public GameObject EquipmentMenu;
    public GameObject BackButton;
    public GameObject PauseMenu;
    public GameObject MessageMenu; 
    public GameObject UnitMenu;

    public GameObject currentMenu;
    public GameObject[] hiddenMenus; // menus to be hidden
    public GameObject[] allMenus;

    public GameObject gameUI; // Contains some scripts that run to maintain Game UI

    // Start is called before the first frame update
    void Start()
    {
        // NO NEED FOR AUTO SAVE NOTIFICATION MENU
        hiddenMenus = new GameObject[13];
        allMenus = new GameObject[] {/* HexMapEditor, NewMapMenu, */ SaveLoadMenu, 
        TileInfoMenu, EndTurn, ResoucesPanel, CraftingUIMenu, UnitCreationMenu, 
        InventoryMenu, EquipmentMenu, BackButton, PauseMenu, MessageMenu};

        EquipmentMenu.SetActive(false);
        BackButton.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (currentMenu) {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                ShowMenu();
                Time.timeScale = 1;
            } else if (Input.GetKeyDown(KeyCode.Tab) && currentMenu == InventoryMenu) {
                ShowMenu();
            } else if (Input.GetKeyDown(KeyCode.R) && currentMenu == CraftingUIMenu) {
                ShowMenu();
            } else if (Input.GetKeyDown(KeyCode.I) && currentMenu == TileInfoMenu) {
                ShowMenu();
            }
        } else if (Input.GetKeyDown(KeyCode.Escape)) {
            HideAllActivedMenu();
            PauseMenu.SetActive(true);
            Time.timeScale = 0;
            currentMenu = PauseMenu;
        } else if (Input.GetButtonDown("InventoryMenu")) {
            HideAllActivedMenu();
            InventoryMenu.SetActive(true);
            currentMenu = InventoryMenu;
        } else if (Input.GetKeyDown(KeyCode.R)){
            HideAllActivedMenuExceptResource(CraftingUIMenu);
            CraftingUIMenu.SetActive(true);
            currentMenu = CraftingUIMenu;
        } else if (Input.GetKeyDown(KeyCode.I)) {
            HideAllActivedMenuExceptResource(TileInfoMenu);
            TileInfoMenu.GetComponent<TileInfoManager>().Open();
            currentMenu = TileInfoMenu;
        }
    }

    /*
     * Hide all active menus when one mune is going to be opened
     * store the hidden menus in hiddenMenus array
     */
    public void HideAllActivedMenu() {
        int index = 0;
        HexMapCamera.Locked = true;
        for (int i = 0; i < allMenus.Length; i++) {
            if (allMenus[i].activeSelf) {
                hiddenMenus[index] = allMenus[i];
                index++;
                allMenus[i].SetActive(false);
            }
        }
    }

    /*
     * Hide all active menus when one mune is going to be opened,
     * except resourcesPanel
     * This is for showing the crafting UI
     * Store the hidden menus in hiddenMenus array
     */
    public void HideAllActivedMenuExceptResource(GameObject currentMenu) {
        int index = 0;
        HexMapCamera.Locked = true;
        for (int i = 0; i < allMenus.Length; i++) {
            if (allMenus[i].activeSelf && allMenus[i] != ResoucesPanel && allMenus[i] != currentMenu) {
                hiddenMenus[index] = allMenus[i];
                index++;
                allMenus[i].SetActive(false);
            }
        }
    }

    /*
     * Show all hidden menus and clear the hiddenMenus array
     */
    public void ShowMenu() {
        HexMapCamera.Locked = false;
        if (BackButton.activeSelf) {
            BackButton.SetActive(false);
        }
        currentMenu.SetActive(false);
        currentMenu = null;
        for (int i = 0; i < hiddenMenus.Length; i++) {
            if (hiddenMenus[i] != null) {
                hiddenMenus[i].SetActive(true);
                hiddenMenus[i] = null;
            }
        }
        EnableUIActions();
    }

    public void OpenEquipmentMenu() {
        HideAllActivedMenu();
        EquipmentMenu.SetActive(true);
		BackButton.SetActive(true);
        currentMenu = EquipmentMenu;
    }

    public void OpenSaveLoadMenu() {
        SaveLoadMenu.SetActive(true);
        currentMenu = SaveLoadMenu;
        HexMapCamera.Locked = true;
    }

    public void CloseSaveLoadMenu() {
        SaveLoadMenu.SetActive(false);
        currentMenu = null;
        HexMapCamera.Locked = false;
    }

    public void OpenNewMapMenu() {
        NewMapMenu.SetActive(true);
        currentMenu = NewMapMenu;
        HexMapCamera.Locked = true;
    }

    public void CloseNewMapMenu() {
        NewMapMenu.SetActive(false);
        currentMenu = null;
        HexMapCamera.Locked = false;
    }

    public void OpenUnitMenu() {
        // UnitMenu.SetActive(true);
        // Debug.Log("Open Unit Menu");
        currentMenu = UnitMenu;
        HexMapCamera.Locked = true;
    }

    public void CloseUnitMenu() {
        // UnitMenu.SetActive(false);
        currentMenu = null;
        HexMapCamera.Locked = false;
    }

    public void EnableUIActions()
    {
        gameUI.GetComponent<HexMapEditor>().enabled = true;
        gameUI.GetComponent<NewMapMenu>().enabled = true;
        gameUI.GetComponent<HexGameUI>().enabled = true;
    }

    public void DisableUIActions()
    {
        gameUI.GetComponent<HexMapEditor>().enabled = false;
        gameUI.GetComponent<NewMapMenu>().enabled = false;
        gameUI.GetComponent <HexGameUI>().enabled = false;
    }
}