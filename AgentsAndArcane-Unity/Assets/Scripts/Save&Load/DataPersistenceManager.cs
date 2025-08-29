using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class DataPersistenceManager : MonoBehaviour
{
    [Header("Debugging")]
    [SerializeField] private bool initializeDataIfNull = false;
    [SerializeField] private bool disableDataPersistense = false;
    [SerializeField] private bool overrrideSelectedProfileId = false;
    [SerializeField] private string testSelectedProfileId = "test";


    [Header("File Storage Config")]
    [SerializeField] private string fileName;
    [SerializeField] private bool Encryption;

    [Header("Auto Saving Config")]
    [SerializeField] public float autoSaveInterval = 60.0f;

    private GameData gameData;
    private List<IDataPersistence> dataPersistenceObjects;
    private FileDataHandler dataHandler;

    private Coroutine autoSaveCoroutine;

    public string selectedProfileId { get; private set; } // keep track of current profile selected
    public static DataPersistenceManager instance { get; private set; }
    public int currentSceneIndex;

    private void Awake()
    {
        if (instance != null)
        {
            //Debug.Log("Found more than one Data Persistence Manager in the scene. Destroying the newest one.");
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this.gameObject);

        if (disableDataPersistense)
        {
            //Debug.LogWarning("Data Persistence is disabled. No data will be saved or loaded.");
        }

        this.dataHandler = new FileDataHandler(Application.persistentDataPath, fileName, Encryption);

        InitializeSelectedProfileId();
    }

    public void NewGame()
    {
        this.gameData = new GameData();
    }

    public void LoadGame()
    {
        if (disableDataPersistense)
        {
            return;
        }

        // load any saved data from a file using the data handler
        this.gameData = dataHandler.Load(selectedProfileId);

        // start a new game if no data was found and config to set initialized data for debugging
        if (this.gameData == null && initializeDataIfNull)
        {
            //Debug.Log("No data was found. Initializing a new game.");
            NewGame();
            //return;
        }

        // if no data can be loaded, initialize a new game
        if (this.gameData == null)
        {
            //Debug.Log("No data was found. A New Game needs to be started before data can be loaded.");
            return;
        }

        // push loaded data to all other scripts that need it
        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            try
            {
                dataPersistenceObj.LoadData(gameData);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error loading data for object: " + dataPersistenceObj + "\n");
                Debug.LogException(e);
            }
        }
        //Debug.Log("Loaded gameData Index = " + gameData.currentSceneIndex);
        currentSceneIndex = gameData.currentSceneIndex;
    }

    public void SaveGame()
    {
        if (disableDataPersistense)
        {
            return;
        }
        if (gameData == null)
        {
            NewGame();
            Debug.Log("New Game Data Created");
        }

        // pass the data to other scripts so they can update it
        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            try
            {
                dataPersistenceObj.SaveData(gameData);
            } catch (System.Exception e)
            {
                Debug.LogError("Error saving data for object: " + dataPersistenceObj + "\n");
                Debug.LogException(e);
            }
        }
        
        // timestamp data so know when it was last saved
        gameData.lastUpdated = System.DateTime.Now.ToBinary();

        // save that data to a file using the data handler
        dataHandler.Save(gameData, selectedProfileId);
    }

    private List<IDataPersistence> FindAllDataPersistenceObjects()
    {
        // FindObjectsofType takes in an optional boolean to include inactive gameobjects
        IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>(true)
            .OfType<IDataPersistence>();

        return new List<IDataPersistence>(dataPersistenceObjects); // initialize this list
    }

    // called first
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // called when game is terminated
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // called second
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode) 
    {
        if (mode != LoadSceneMode.Additive)
        {
            Debug.Log("OnSceneLoaded Called");
            this.dataPersistenceObjects = FindAllDataPersistenceObjects();
            LoadGame();

            // start auto save coroutine
            if (autoSaveCoroutine != null)
            {
                StopCoroutine(autoSaveCoroutine);
            }
            autoSaveCoroutine = StartCoroutine(AutoSave());
        }
    }

    public void DeleteProfileData(string profileId) 
    {
        // delete the data for this profile id
        dataHandler.Delete(profileId);
        // initialize the selected profile id
        InitializeSelectedProfileId();
        // reload the game so that our data matches the newly selected profile id
        LoadGame();
    }

    private void InitializeSelectedProfileId() 
    {
        this.selectedProfileId = dataHandler.GetMostRecentUpdatedProfileId();
        if (overrrideSelectedProfileId) 
        {
            this.selectedProfileId = testSelectedProfileId;
            //Debug.LogWarning("Overrode selected profile id with test id: " + testSelectedProfileId);
        }
    }

    public void ChangeSelectedProfileId(string newProfileId) 
    {
        // update the profile to use for saving and loading
        this.selectedProfileId = newProfileId;
        // load the game, which will use that profile, updating our game data accordingly
        LoadGame();
    }


    // called third;below for simple testing purpose
    private void Start()
    {
        //this.dataHandler = new FileDataHandler(Application.persistentDataPath, fileName, Encryption);
        // this.dataPersistenceObjects = FindAllDataPersistenceObjects();
        // LoadGame();
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    public bool HasGameData()
    {
        return this.gameData != null;
    }

    public Dictionary<string, GameData> GetAllProfilesGameData() 
    {
        return dataHandler.LoadAllProfiles();
    }

    private IEnumerator AutoSave() 
    {
        while (true) 
        {
            yield return new WaitForSeconds(autoSaveInterval);
            SaveGame();
            Debug.Log("Auto saved game data.");
        }
    }
}
