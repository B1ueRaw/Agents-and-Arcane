using Group3d.Notifications;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Valve.Newtonsoft.Json;

[System.Serializable]
struct ReceiveInstruction
{
    public string decision_info;
    public string decision_type;
}

[System.Serializable]
struct InvokeAgentResponse
{
    // Instruction received from the server
    public ReceiveInstruction decision;
    /*
     * Maps reciving agent id as key to message sent
     */
    public Dictionary<string, string> messages;
}

public class TurnManager : MonoBehaviour
{
    private Player player;

    static int manaRequired = 0;
    public TextMeshProUGUI manaRequirementText;

    public UnitManager unitManager;
    public HexGrid hexGrid;

    // Prefab used to instantiate a transition manager when it's needed
    public TransitionManager transitionPrefab;

    public readonly static string BASE_URI = "http://localhost:5000/";

    public MessageManager messageManager;

    // Start is called before the first frame update
    void Start()
    {
        player = GetComponent<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeMana(int manaChange)
    {
        manaRequired += manaChange;
        if (manaRequired <= 0)
        {
            manaRequirementText.text = "";
        } else
        {
            manaRequirementText.text = manaRequired.ToString();
        }
    }

    private void ResetMana()
    {
        manaRequired = 0;
        manaRequirementText.text = "";
    }

    public void Initialize()
    {
        // Combine JSON from HexGrid and UnitManager
        string json = "{\"user_id\":\"AgentsAndArcane\"," + hexGrid.ToJSON() + "," + unitManager.ClassesToJSON() + "}";
        Debug.Log(json);
        StartCoroutine(InitializeFlask(json));
    }

    IEnumerator InitializeFlask(string gameInfo)
    {
        using (UnityWebRequest initGameRequest = UnityWebRequest.Post(TurnManager.BASE_URI + "init_game", gameInfo, "application/json"))
        {
            yield return initGameRequest.SendWebRequest();

            if (initGameRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + initGameRequest.error);
                Notifications.Send("Error connecting to Flask Server. Please relaunch later", NotificationType.Error);
                Application.Quit();
            }

            Debug.Log("Received: " + initGameRequest.downloadHandler.text);
        }
    }

    public void EndTurn()
    {

        if (!player.UseResource(0, manaRequired))
        {
            Notifications.Send("Not enough mana to perform all unit movement. Please reset unit positions.", NotificationType.Warning);
            return;
        }
        unitManager.EndTurnMovement();


        // Combine JSON from HexGrid and UnitManager
        string json = $"{{\"player_turn\":true,{unitManager.UnitsToJSON(true)},{unitManager.EquipmentToJSON()},{player.InventoryToJSON()}}}";
        Debug.Log(json);
        StartCoroutine(EndTurnFlask(json, true));
    }

    public void StartEnemyTurn()
    {
        string json = $"{{\"player_turn\":false,{unitManager.UnitsToJSON(true)},{unitManager.EquipmentToJSON()},{player.InventoryToJSON()}}}";
        StartCoroutine(EndTurnFlask(json, false));
    }

    IEnumerator EndTurnFlask(string gameInfo, bool isPlayerTurn)
    {
        using (UnityWebRequest endTurnRequest = UnityWebRequest.Post(TurnManager.BASE_URI + "init_turn", gameInfo, "application/json"))
        {
            yield return endTurnRequest.SendWebRequest();

            if (endTurnRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + endTurnRequest.error);
                Notifications.Send("Error connecting to Flask Server. Please relaunch later", NotificationType.Error);
                Application.Quit();
            }

            Debug.Log("Received: " + endTurnRequest.downloadHandler.text);

            List<bool> finishedDecisions = new List<bool>();
            for (int i = 0; i < unitManager.grid.units.Count; i++)
            {
                if (isPlayerTurn && unitManager.grid.units[i].isPlayerUnit)
                {
                    finishedDecisions.Add(false);
                    StartCoroutine(InvokeAgentToFlask(unitManager.grid.units[i], finishedDecisions, finishedDecisions.Count - 1));
                } else if (!isPlayerTurn && !unitManager.grid.units[i].isPlayerUnit)
                {
                    finishedDecisions.Add(false);
                    StartCoroutine(InvokeAgentToFlask(unitManager.grid.units[i], finishedDecisions, finishedDecisions.Count - 1));
                }
            }

            // Wait until all previous coroutines are finished
            bool canMoveOn = false;
            while (!canMoveOn)
            {
                canMoveOn = true;
                foreach (bool finished in finishedDecisions)
                {
                    if (!finished)
                    {
                        canMoveOn = false;
                        break;
                    }
                }
                if (!canMoveOn) yield return null;
            }

            TransitionManager transManager = FindFirstObjectByType<TransitionManager>();
            if (transManager != null && transManager.HasBattles())
            {
                HandleBattles();
                yield break;
            }
            if (isPlayerTurn)
            {
                Debug.Log("Starting enemy's turn...");
                StartEnemyTurn();
            }
            else
            {
                Debug.Log("Ending enemy's turn...");
            }
        }
    }

    /**
     * Send the web request to the server to make a decision for this agent's turn
     * @param finishedDecisions List indicating whether each Coroutine has finished
     * @param i This agent's index in finishedDecisions
     */
    IEnumerator InvokeAgentToFlask(HexUnit unit, List<bool> finishedDecisions, int i)
    {
        string json = "{\"agent_id\":\"" + unit.agentId + "\"}";
        Debug.Log("Sending: " + json + "to flask");
        using (UnityWebRequest agentRequest = UnityWebRequest.Post(TurnManager.BASE_URI + "invoke_agent", json, "application/json"))
        {
            yield return agentRequest.SendWebRequest();

            if (agentRequest.result != UnityWebRequest.Result.Success)
            {
                unit.isThinking = false;
                Debug.LogError("Error: " + agentRequest.error);
                Notifications.Send("Error connecting to Flask Server. Please relaunch later", NotificationType.Error);
                finishedDecisions[i] = true;
                Application.Quit();
            }

            // start thinking after receiving response
            unit.isThinking = true;

            Debug.Log("Received " + agentRequest.downloadHandler.text + "for unit " + unit.unitName);


            InvokeAgentResponse invokeResponse = JsonConvert.DeserializeObject<InvokeAgentResponse>(agentRequest.downloadHandler.text);
            // Instruction information
            ReceiveInstruction instruction = invokeResponse.decision;
            
            // Dictionary of messages sent during turn
            Dictionary<string, string> messages = invokeResponse.messages;
            if (messages != null)
            {
                Debug.Log(messages.Count);
                if (messages.Count > 0)
                {
                    foreach (KeyValuePair<string, string> message in messages)
                    {
                        if (message.Value != " " && message.Value != "" && message.Value != null) {
                            HexUnit receiver = null;
                            for (int k = 0; k < unitManager.grid.units.Count; k++)
                            {
                                if (unitManager.grid.units[k].agentId.Equals(message.Key))
                                {
                                    receiver = unitManager.grid.units[k];
                                    break;
                                }
                            }
                            string msg = "To " + (receiver != null ? receiver.unitName : message.Key) + " from " +
                                unit.unitName + ": " + message.Value;
                            messageManager.AddMessage(msg);
                        }
                    }
                }
            } else
            {
                Debug.Log("Messages is null");
            }

            bool foundUnit;
            switch (instruction.decision_type)
            {
                case "move":
                    Debug.Log("Moving to " + instruction.decision_info);
                    string[] coords = instruction.decision_info.Split(',');
                    HexCoordinates coordinates;
                    try
                    {
                        coordinates = new HexCoordinates(int.Parse(coords[0]), int.Parse(coords[1]));
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                        Debug.LogError("Unable to move unit to " + instruction.decision_info);
                        break;
                    }
                    MoveUnit(unit, coordinates);
                    if (unit)
                        HarvestResource(unit, coordinates);
                    break;
                case "target":
                    Debug.Log("Targeting " + instruction.decision_info);
                    foundUnit = false;
                    for (int k = 0; k < unitManager.grid.units.Count; k++)
                    {
                        if (!unitManager.grid.units[k].agentId.Equals(instruction.decision_info)) continue;
                        foundUnit = true;
                        MoveUnit(unit, unitManager.grid.units[k].Location.Coordinates);
                    }
                    if (foundUnit == false) Debug.Log("Did not find unit " + instruction.decision_info);
                    break;
                case "retreat":
                    Debug.Log("Retreating " + instruction.decision_info);
                    //foundUnit = false;
                    //for (int k = 0; k < unitManager.grid.units.Count; k++)
                    //{
                    //    if (!unitManager.grid.units[k].agentId.Equals(instruction.decision_info)) continue;
                    //    foundUnit = true;
                    //    MoveUnit(unit, unitManager.grid.units[k].Location.Coordinates);
                    //}
                    break;
                default:
                    Debug.Log("Unknown decision type: " + instruction.decision_type);
                    break;
            }
            unit.isThinking = false;
            finishedDecisions[i] = true;
        }
    }

    private void MoveUnit(HexUnit selectedUnit, HexCoordinates coordinates)
    {
        Debug.Log("Moving unit " + selectedUnit.unitName + " to " + coordinates.ToString());
        if (selectedUnit.Location.Coordinates.DistanceTo(coordinates) == 0) return;
        if (unitManager.grid.GetCell(coordinates).Unit)
        {
            HexUnit otherUnit = unitManager.grid.GetCell(coordinates).Unit;

            Debug.Log("Info: Unit " + unitManager.grid.GetCell(coordinates).Unit.unitName + " is in the way");
            if (otherUnit.isPlayerUnit != selectedUnit.isPlayerUnit &&
                selectedUnit.Location.Coordinates.DistanceTo(coordinates) <= selectedUnit.MovementRange)
            {
                TransitionManager tranManager = FindFirstObjectByType<TransitionManager>();
                if (tranManager == null)
                {
                    tranManager = Instantiate(transitionPrefab);
                    tranManager.turnManager = this;
                }

                // Enqueue proper battle information
                if (selectedUnit.isPlayerUnit)
                {
                    tranManager.EnqueueBattle(selectedUnit, otherUnit, coordinates, true);
                } else
                {
                    tranManager.EnqueueBattle(otherUnit, selectedUnit, coordinates, false);
                }
                return;
            }
        }

        PathFindUnit(selectedUnit, coordinates);
    }

    public int SimulateBattle(HexUnit unit1, HexUnit unit2)
    {
        if (unit1.equipment && unit2.equipment)
        {
            // Check who has better equipment
            if (unit1.equipment.rarity > unit2.equipment.rarity) return 1;
            else if (unit1.equipment.rarity < unit2.equipment.rarity) return 2;
            else return UnityEngine.Random.Range(1, 3);
        }
        else if (unit1.equipment || unit2.equipment)
        {
            // Only one has equipment
            if (unit1.equipment) return 1;
            else return 2;
        }
        else
        {
            // Toss up
            return UnityEngine.Random.Range(1,3);
        }
    }

    public void HarvestResource(HexUnit harvester, HexCoordinates coordinates)
    {
        if (!harvester.isPlayerUnit) return;
        HexCell cell = unitManager.grid.GetCell(coordinates);
        if (cell && cell.ExtendedValues.ResourceLevel > 0)
        {
            if (cell.ExtendedValues.ResourceTypeIndex < 5)
                player.AddResource(cell.ExtendedValues.ResourceTypeIndex, cell.ExtendedValues.ResourceLevel);
        }
    }

    private void HandleBattles()
    {
        // Deactivate Editor UI
        // HexMapEditor editor = FindFirstObjectByType<HexMapEditor>();
        // if (editor != null) editor.gameObject.SetActive(false);
        SceneManager.LoadScene("Scenes/Micro Battle", LoadSceneMode.Additive);
    }

    /**
     * Handle the results contained in the transition manager queue
     * Return whether the last result was from a player's turn (true) or an enemy's turn (false)
     */
    public bool HandleResults(TransitionManager manager) {
        bool isPlayerTurn = true;
        while (manager.HasResults()) { 
            MicroBattleResult result = manager.DequeueResult();
            MicroBattleInfo battleInfo = result.info;
            HexUnit ally = result.info.ally;
            HexUnit enemy = result.info.enemy;
            int winner = result.result;
            isPlayerTurn = battleInfo.isPlayerTurn;

            // Simulate battle
            if (winner == 1)
            {
                // Allied unit wins
                Debug.Log("Info: Unit " + enemy.unitName + " has been defeated");
                unitManager.grid.RemoveUnit(enemy);
            }
            else if (winner == 2)
            {
                // Allied unit loses
                Debug.Log("Info: Unit " + ally.unitName + " has been defeated");
                unitManager.grid.RemoveUnit(ally);
            }
            else
            {
                Debug.LogError("Error: Invalid winner from SimulateBattle removing enemy unit");
                    Debug.Log("Info: Unit " + enemy.unitName + " has been defeated");
                    unitManager.grid.RemoveUnit(enemy);
            }
            if (winner != 2)
            {
                MoveUnitPostBattle(ally, battleInfo.coordinates);
            } else if (winner == 2)
            {
                MoveUnitPostBattle(enemy, battleInfo.coordinates);
            }
        }
        return isPlayerTurn;
    }

    private void MoveUnitPostBattle(HexUnit unit, HexCoordinates destination)
    {
        if (unit == null) return;

        bool ret = hexGrid.TryGetCell(destination, out HexCell destCell);
        if (!ret)
        {
            Debug.LogError("Unable to find cell from given coords");
            Debug.LogError(destination.ToString());
        }

        unit.transform.position = destCell.Position;
    }

    private void PathFindUnit(HexUnit unit, HexCoordinates coords)
    {
        unitManager.grid.FindPath(
        unit.Location,
        unitManager.grid.GetCell(coords), unit);
        unit.Travel(unitManager.grid.GetPath());
        unitManager.grid.ClearPath();
    }
}
