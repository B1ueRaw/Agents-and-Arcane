using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.PlayerLoop;

public class UnitClickHandler : MonoBehaviour
{

    public Material highlightMaterial;
    private Material originalMaterial;
    private Renderer soldierRenderer;    // Renderer to change material

    public Button equipmentButton;  // Reference to the equipment button that appears when a soldier is selected
    public Button userInputButton;

    public bool isSoldierSelected = false;
    public bool isFocused = false;
    public TMP_InputField inputField;
    public UIManager uiManager;
    public TMP_Text userInput;
    // Start is called before the first frame update
    void Start()
    {
        equipmentButton.gameObject.SetActive(false);
        userInputButton.gameObject.SetActive(false);

        // Get the soldier's renderer and its original material
        soldierRenderer = this.gameObject.GetComponentInChildren<Renderer>();
        originalMaterial = soldierRenderer.material;
        uiManager = GameObject.Find("UIManager").GetComponent<UIManager>();
        // if (!GetComponent<HexUnit>().isPlayerUnit)
        // {
        //     GetComponent<UnitClickHandler>().enabled = false;
        //     return;
        // }

        //equipmentButton.onClick.AddListener(() => { Debug.Log("Button Clicked!"); });
    }

    // Update is called once per frame
    void Update()
    {
        UpdateButtonPosition();
        if (isSoldierSelected)
        {
            SelectSoldier();
            inputField.Select();
        }
        // Detect if we clicked on the soldier
        if (Input.GetMouseButtonDown(0) && (uiManager.currentMenu == null || uiManager.currentMenu == uiManager.UnitMenu)) // Left-click
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // Check if the ray hits the soldier unit
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.gameObject == this.gameObject)
                {
                    SelectSoldier();
                }
                else
                {
                    DeselectSoldier();  // Deselect if clicking outside
                }
            }
        }
    }

    void SelectSoldier()
    {
        uiManager.OpenUnitMenu();
        isSoldierSelected = true;
        // Highlight material
        soldierRenderer.material = highlightMaterial;

        equipmentButton.gameObject.SetActive(true);
        equipmentButton.interactable = true;

        userInputButton.gameObject.SetActive(true);
        userInputButton.interactable = true;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(this.gameObject.transform.position);
        equipmentButton.transform.position = screenPos + new Vector3(50, 50, 0);
        userInputButton.transform.position = screenPos + new Vector3(-50, 50, 0);

        UpdateButtonPosition();
        // after 5 seconds, hide the button
        // StartCoroutine(HideButtonAfterDelay(5f));
    }

    // Function to deselect the soldier, making it back to its original material and deactive the equipment button
    void DeselectSoldier()
    {
        // if (isSoldierSelected)
        // {
            soldierRenderer.material = originalMaterial;
            isSoldierSelected = false;
            // BUG here: if set it to false, the button will not be interactable
            //equipmentButton.gameObject.SetActive(false);
            
            StartCoroutine(HideButtonAfterDelay(0.6f));
            isSoldierSelected = false;
        // }
    }

    void UpdateButtonPosition()
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(this.gameObject.transform.position);
        equipmentButton.transform.position = screenPos + new Vector3(0, 50, 0);
        userInputButton.transform.position = screenPos + new Vector3(0, 80, 0);
    }

    public void OnInputButtonClicked()
    {
        // send to the backend
        Debug.Log("User input: " + userInput.text);
        HexUnit hexUnit = gameObject.GetComponent<HexUnit>();
        if (hexUnit == null )
        {
            return;
        }
        hexUnit.SetNextCommand(userInput.text);
    }

    private IEnumerator HideButtonAfterDelay(float delay)
    {
        if (inputField.isFocused) {
            yield break;
        }
        yield return new WaitForSeconds(delay);
        equipmentButton.gameObject.SetActive(false);
        userInputButton.gameObject.SetActive(false);
        userInputButton.gameObject.SetActive(false);
        if (uiManager.currentMenu == uiManager.UnitMenu){
            uiManager.CloseUnitMenu();
        }
    }
}
