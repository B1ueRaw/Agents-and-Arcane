using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
    public MicroBattleInfo currentBattleInfo;
    public HexUnit ally;
    public HexUnit enemy;

    public Minion minionPrefabAlly;
    public Minion minionPrefabEnemy;

    public Spell[] spells;
    public Button spellCastPrefab;
    public Canvas spellCanvas;

    private readonly int terrainCoord = 50;

    // Track whether the battle has started or not
    private bool battleStarted;
    // Getter/setter for the above variable
    public bool BattleStarted
    {
        get => battleStarted; set => battleStarted = value;
    }
        

    // Start is called before the first frame update
    void Start()
    {
        // Make sure its scene is the active scene
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("Scenes/Micro Battle"));
        float offset = Screen.width / (spells.Length + 1f);

        // Initialize Spell UI
        for (int i = 0; i < spells.Length; i++)
        {
            Button newPrefab = Instantiate(spellCastPrefab);
            newPrefab.image.sprite = spells[i].sprite;
            newPrefab.transform.SetParent(spellCanvas.transform);
            newPrefab.transform.position = new Vector2(offset * (i + 1), newPrefab.transform.position.y);
            // Move to memory separate from i
            Spell currSpell = spells[i];

            // Add functionality to cast spell to button
            newPrefab.onClick.AddListener(() => { currSpell.Cast(); });
        }

        
        // Get TransitionManager
        TransitionManager manager = FindFirstObjectByType<TransitionManager>();
        if (manager == null || !manager.HasBattles())
        {
            ReturnToMap();
        }

        currentBattleInfo = manager.DequeueBattle();
        this.ally = currentBattleInfo.ally;
        this.enemy = currentBattleInfo.enemy;

        // Set Unit positions
        if (ally != null)
        {
            ally.transform.position = new Vector3(0, terrainCoord, 0);
            SpawnMinions(ally, true);
        }
        if (enemy != null)
        {
            enemy.transform.position = new Vector3(100, terrainCoord, 100);
            SpawnMinions(enemy, false);
        }
    }

    void SpawnMinions(HexUnit owner, bool ownerIsAlly)
    {
        for (int i = 1; i <= 30; i++)
        {
            int multiplier = ownerIsAlly ? 1 : -1;
            Minion prefab = ownerIsAlly ? minionPrefabAlly : minionPrefabEnemy;
            // Spawn 30 minions 
            Minion newMinion = Instantiate(prefab, owner.transform.position + (multiplier * new Vector3(i, 0, 30 - i)), prefab.transform.rotation);
            newMinion.owner = owner;
            newMinion.alliedMinion = ownerIsAlly;
            newMinion.battleManager = this;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ReturnToMap()
    {
        if (ally != null && enemy != null)
        {
            TransitionManager manager = FindFirstObjectByType<TransitionManager>();
            if (manager != null)
            {
                manager.EnqueueResults(currentBattleInfo, SimulateBattle(ally, enemy));
                manager.TransitionBackToMap();
            }
            else
            {
                Debug.LogError("No transition manager found for return from " + ally.name + " vs. " + enemy.name);
            }
        }
        // HexMapEditor editor = FindAnyObjectByType<HexMapEditor>(FindObjectsInactive.Include);
        // if (editor != null) editor.gameObject.SetActive(true);
        SceneManager.UnloadSceneAsync("Scenes/Micro Battle");
    }


    /**
     * Simulate a battle between the two units
     * If they have equipment, use that to determine the result
     * Otherwise return a random result
     * @return 1 if the ally wins, 2 if the enemy wins
     */
    public int SimulateBattle(HexUnit ally, HexUnit enemy)
    {
        if (ally.equipment && enemy.equipment)
        {
            // Check who has better equipment
            if (ally.equipment.rarity > enemy.equipment.rarity) return 1;
            else if (ally.equipment.rarity < enemy.equipment.rarity) return 2;
            else return UnityEngine.Random.Range(1, 3);
        }
        else if (ally.equipment || enemy.equipment)
        {
            // Only one has equipment
            if (ally.equipment) return 1;
            else return 2;
        }
        else
        {
            // Toss up
            return UnityEngine.Random.Range(1, 3);
        }
    }
}
