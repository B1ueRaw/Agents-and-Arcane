using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TargetingType
{
    MOVE,
    ATTACK,
    RETREAT
}

public class Minion : MonoBehaviour
{
    private int maxHealth = 20;
    private int health = 20;
    private int damage = 5;
    private int attackSpeed = 10;
    private float attackRange = 2.5f;
    private int movementSpeed = 10;

    // Denotes unit this minion is associated with
    public HexUnit owner;
    // Denotes whether the minion is an ally or enemy to the player
    public bool alliedMinion;

    // Target to attack if ordered to attack
    public GameObject target = null;
    public Vector3 targetCoords = Vector3.zero;
    public TargetingType actionType = TargetingType.ATTACK;

    // Used to check status of ongoing battles
    public BattleManager battleManager;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (battleManager.BattleStarted)
        {
            if (target != null)
            {
                if (actionType == TargetingType.ATTACK)
                {
                    // Move unit if far enough away, otherwise attack
                    if (!MoveIfFar(target.transform.position))
                    {
                        // TODO Attack enemie
                    }
                }
            }
            if (actionType == TargetingType.MOVE)
            {
                MoveIfFar(targetCoords);
            }
        }
    }

    /* Move minion to given vector if it's far enough away and return true
     * Otherwise, return false
     */
    bool MoveIfFar(Vector3 destination)
    {
        Vector3 distance = destination - gameObject.transform.position;
        if (Vector3.Magnitude(distance) > attackRange)
        {
            gameObject.transform.Translate(movementSpeed * Time.deltaTime * Vector3.Normalize(distance));
            return true;
        }
        return false;
    }
}
