using Group3d.Notifications;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

// Serializable struct to send
[System.Serializable]
public struct ClassInfoCollection
{
    [System.Serializable]
    public struct ClassInfoFlask {
        public int class_id;
        public int base_hp;
        public float base_ms;
        public string ability;
    } 
    public ClassInfoFlask[] classes;
}

public class UnitManager : MonoBehaviour
{
    public HexUnit[] unitPrefabs;
    public HexGrid grid;
    public GameObject creationUI;
    
    // Start is called before the first frame update
    void Start()
    {
        // Initialize Dropdown based on array contents
        TMP_Dropdown classDropdown = creationUI.GetComponentInChildren<TMP_Dropdown>();
        if (classDropdown == null || classDropdown.name != "Class Select")
        {
            Debug.Log("Could not find dropdown menu");
            Debug.Log(classDropdown ? classDropdown.name : classDropdown);
            return;
        }
        List<string> optionsList = new();
        foreach (HexUnit go in unitPrefabs)
        {
            optionsList.Add(go.name);
        }
        classDropdown.AddOptions(optionsList);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateUnit()
    {

        // Locate proper UI Elements
        TMP_InputField[] textInputs = creationUI.GetComponentsInChildren<TMP_InputField>();
        TMP_Dropdown classDropdown = creationUI.GetComponentInChildren<TMP_Dropdown>();
        // Check that UI was found
        if (textInputs == null || textInputs.Length != 2 || classDropdown == null || classDropdown.name != "Class Select")
        {
            Debug.Log("Could not find all input fields");
            Debug.Log(classDropdown ? classDropdown.name : classDropdown);
            Debug.Log(textInputs.Length);
            foreach (TMP_InputField input in textInputs)
            {
                Debug.Log(input.name);
            }
            return;
        }
        string name = null;
        string background = null;
        // Populate correct variable w/ correct value
        foreach (TMP_InputField field in textInputs)
        {
            if (field.name == "Name Field")
            {
                name = field.text;
            } 
            else if (field.name == "Background Field")
            {
                background = field.text;
            }
            else
            {
                Debug.Log("Unknown field name found");
            }
        }
        HexUnit selection = unitPrefabs[classDropdown.value];

        // Check name validity
        if (string.IsNullOrWhiteSpace(name) ||
            string.Equals(name, "Enter Unit Name"))
        {
            Notifications.Send
                ("Entered Name is blank or invalid\nEnsure that you have entered a non-blank name that is different from the default text.", NotificationType.Error);
            return;
        }

        // Check background info validity
        if (string.IsNullOrWhiteSpace(background) ||
            string.Equals(background, "Enter Unit Background and Personality")) 
        {
            Notifications.Send
                ("Entered Background Information is blank or invalid\nEnsure that you have entered a non-blank description that is different from the default text.", NotificationType.Error);
            return;
        }

        // Check that enough information was entered
        if (background.Length < 20)
        {
            Notifications.Send(
                "Entered Background Information is too short. Please enter at least a sentence of detailed information.", NotificationType.Error);
            return;
        }

        HexCoordinates hexCoordinates = HexCoordinates.FromOffsetCoordinates(grid.CellCountX / 2, grid.CellCountZ / 2);
        CreateUnit(name, background, classDropdown.captionText.text, classDropdown.value, hexCoordinates);
    }

    public void CreateUnit(string name, string background, string unit_class, int prefabIndex, HexCoordinates hexCoordinates, bool isPlayerUnit=true)
    {
        // Create and initialize unit
        HexUnit createdUnit = Instantiate(unitPrefabs[prefabIndex]);

        HexCell hexCell = grid.GetCell(hexCoordinates);

        Queue<HexCell> hexQ = new Queue<HexCell>();
        List<int> visited = new List<int>();
        visited.Add(hexCell.Index);
        while (hexCell && hexCell.Unit)
        {

            for (int i = 0; i < 6; i++)
            {
                HexCell neighborCell = hexCell.GetNeighbor((HexDirection)i);
                if (neighborCell && !visited.Contains(neighborCell.Index))
                {
                    hexQ.Enqueue(neighborCell);
                    visited.Add(neighborCell.Index);
                }
            }
            if (hexQ.Count > 0)
            {
                hexCell = hexQ.Dequeue();
            }
            else break;
        }
        if (!hexCell)
        {
            Notifications.Send(
                "Could not find a viable location to spawn the unit.", NotificationType.Error);
            return;
        }
        createdUnit.Initialize(name, background, unit_class, prefabIndex, isPlayerUnit);
        grid.AddUnit(
            createdUnit, hexCell, Random.Range(0f, 360f)
        );

        creationUI.SetActive(false);

        // TODO Save unit
    }

    public string UnitsToJSON(bool resetMovement)
    {
        List<string > _jsonUnits = new List<string>(grid.CellUnits.Length);
        for (int i=0; i < grid.units.Count; i++)
        {
            _jsonUnits.Add(grid.units.ElementAt(i).ToJSON(resetMovement));
        }
        string _json = "\"units\": [" + string.Join(",", _jsonUnits) + "]";
        return _json;
    }

    public string EquipmentToJSON()
    {
        List<string> _jsonEquipment = new List<string>(grid.CellUnits.Length);
        for (int i = 0; i < grid.units.Count; i++)
        {
            if (grid.units.ElementAt(i).equipment != null)
            {
                _jsonEquipment.Add(grid.units.ElementAt(i).equipment.ToJSON());
            }
        }
        string _json = "\"equipment\": [" + string.Join(",", _jsonEquipment) + "]";
        return _json;
    }

    public string ClassesToJSON()
    {
        ClassInfoCollection classes = new()
        {
            classes = new ClassInfoCollection.ClassInfoFlask[unitPrefabs.Length]
        };
        for (int i = 0; i < unitPrefabs.Length; i++)
        {
            classes.classes[i] = new()
            {
                class_id = i,
                base_hp = unitPrefabs[i].health,
                base_ms = unitPrefabs[i].MovementSpeed,
                ability = ""
            };
        }
        string result = JsonUtility.ToJson(classes);
        return result.Substring(1, result.Length - 2);
    }

    public void EndTurnMovement()
    {
        for (int i = 0; i < grid.units.Count; i++)
        {
            Debug.Log("Has unit " + grid.units.ElementAt(i).name + " moved: " + grid.units.ElementAt(i).HasMoved());
            if (!grid.units.ElementAt(i).isPlayerUnit) continue;
            if (!grid.units.ElementAt(i).HasMoved()) continue;

            grid.units.ElementAt(i).Travel();
        }
    }
}
