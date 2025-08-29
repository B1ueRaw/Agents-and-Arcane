using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public struct MicroBattleInfo
{
    public HexUnit ally;
    public HexUnit enemy;
    public HexCoordinates coordinates;
    public bool isPlayerTurn;
}

public struct MicroBattleResult
{
    public MicroBattleInfo info;
    public int result;
}

public class TransitionManager : MonoBehaviour
{
    Queue<MicroBattleInfo> battlesQueue = new Queue<MicroBattleInfo> ();
    Queue<MicroBattleResult> resultsQueue = new Queue<MicroBattleResult> ();
    public TurnManager turnManager;
    
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Enqueue a battle between the given ally and enemy
    public void EnqueueBattle(HexUnit ally, HexUnit enemy, HexCoordinates coordinates, bool isPlayerTurn)
    {
        battlesQueue.Enqueue(new MicroBattleInfo { 
            ally = ally, 
            enemy = enemy,
            coordinates = coordinates,
            isPlayerTurn = isPlayerTurn
        });
    }

    // Retrieve the units involved in the next battle
    public MicroBattleInfo DequeueBattle()
    {
        return battlesQueue.Dequeue();
    }

    public void EnqueueResults(MicroBattleInfo info, int result)
    {
        resultsQueue.Enqueue(new MicroBattleResult { 
            info = info, 
            result = result});
    }

    public MicroBattleResult DequeueResult()
    {
        return resultsQueue.Dequeue();
    }

    public bool HasBattles()
    {
        return battlesQueue.Count > 0;
    }

    public bool HasResults()
    {
        return resultsQueue.Count > 0;
    }

    public void TransitionBackToMap()
    {
        if (HasBattles())
        {
            SceneManager.LoadScene("Scenes/Micro Battle", LoadSceneMode.Additive);
            return;
        }
        if (HasResults())
        {
            bool wasPlayerTurn = turnManager.HandleResults(this);
            if (wasPlayerTurn)
            {
                turnManager.StartEnemyTurn();
            }
        }
    }
}
