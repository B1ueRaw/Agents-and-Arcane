using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Networking;
using Group3d.Notifications;
using System.Linq;
using TMPro;
using UnityEditor.VersionControl;

// Structs used to easily create/parse data for Flask server
[System.Serializable]
struct SendBackgroundInfo
{
	public string agent_name;
	public string agent_description;
	public string agent_class;
}

[System.Serializable]
struct ReceiveTraits
{
	public string[] assigned_traits;
	public string thread_id;
	public string unit_id;
}

/// <summary>
/// Component representing a unit that occupies a cell of the hex map.
/// </summary>
public class HexUnit : MonoBehaviour
{
    [SerializeField]
    int movementRange = 50;
	public int MovementRange => movementRange;
    [SerializeField]
    int visionRange = 3;
    [SerializeField]
	float rotationSpeed = 180f;
	[SerializeField]
	protected float movementSpeed = 4f;
    [SerializeField]
    protected float attackSpeed;
    [SerializeField]
    float maxHealth = 100f;
	[SerializeField]
	public MessageManager messageManager;
    // Battle Information
    public int health;
    public int damage; // make it from protected to public so that it can be accessed by equipment classes
    public Equipment equipment; // for now just one equipment, add more later if needed (if we have armor)
	private HexCell currentMovement;
	private string nextCommand = "";
	// TODO Battalion

	// Identifying Information
	public string unitName;
	public string background;
    public string agentId { get; private set; }
    private string threadId;
	private string agentClass;
	private string[] traits = new string[0];

    public bool isPlayerUnit { get; private set; }
    private int prefabIndex;

	public UIManager uiManager;
	public GameObject ghostPrefab;
	private GameObject ghostUnit;
	public int moveCost = 0;

	//=====FILEDS FOR STATS IN EQUIPMENT MENU=====//
    public TMP_Text unitNameText;
    public TMP_Text itemDamageText;
    public TMP_Text itemHealthText;
	public TMP_Text unitBackgroundText;
	public GameObject equipmentMenu;
	public EquippedSlot equippedSlot;
	public EquipmentSlot[] equipmentSlot;
	public ItemBelongsTo itemBelongsTo;
	public GameObject backButton;

	//=====FIELDS FOR UNIT THINKING=====//
	public GameObject thinkingSign;
	public bool isThinking = true; // currently set to true for testing purposes

	void Start(){
		//Debug.Log("Start");
		// equipmentMenu = GameObject.Find("EquipmentMenu");
		// unitNameText = GameObject.Find("UnitNameText").GetComponent<TMP_Text>();
		// itemDamageText = GameObject.Find("DamageNumberText").GetComponent<TMP_Text>();
		// itemHealthText = GameObject.Find("ActualHealthText").GetComponent<TMP_Text>();
		// unitBackgroundText = GameObject.Find("BackgroundText").GetComponent<TMP_Text>();
		// equippedSlot = GameObject.Find("EquippedSlot").GetComponent<EquippedSlot>();
		// Debug.Log("Equipment Slot length: " + equipmentSlot.Length);
	}

    public virtual void Initialize(string name, string background, string unitClass, int prefabIndex, bool isPlayerUnit = true)
    {
        this.unitName = name;
        this.background = background;
		this.agentClass = unitClass;

		this.isPlayerUnit = isPlayerUnit;
		this.prefabIndex = prefabIndex;

        currentMovement = default;

        // Assign traits w/ AI w/ call to Flask server
        SendBackgroundInfo postInfo = new()
        {
            agent_name = name,
            agent_description = background,
			agent_class = unitClass
        };

		// Make Web Request to set traits and ID
		StartCoroutine(GetTraits(postInfo));
    }

	IEnumerator GetTraits(SendBackgroundInfo objectInfo)
	{
		using (UnityWebRequest createUnitRequest = UnityWebRequest.Post(TurnManager.BASE_URI + "create_agent", JsonUtility.ToJson(objectInfo), "application/json"))
		{
			yield return createUnitRequest.SendWebRequest();

			if (createUnitRequest.result == UnityWebRequest.Result.Success)
			{
				Debug.Log("Received: " + createUnitRequest.downloadHandler.text);
				// Set attributes
				ReceiveTraits receiveTraits = JsonUtility.FromJson<ReceiveTraits>(createUnitRequest.downloadHandler.text);
				this.traits = receiveTraits.assigned_traits;
				this.agentId = receiveTraits.unit_id;
				this.threadId = receiveTraits.thread_id;

				Notifications.Send("Unit " + unitName + " created with traits: " + string.Join(", ", this.traits));
			} else
			{
				Debug.LogError("Error: " + createUnitRequest.error);
				Notifications.Send("Error connecting to Flask Server. Please relaunch later", NotificationType.Error);
				Application.Quit();
			}
		}
	}

    public HexGrid Grid { get; set; }

	/// <summary>
	/// Cell that the unit occupies.
	/// </summary>
	public HexCell Location
	{
		get => Grid.GetCell(locationCellIndex);
		set
		{
			if (locationCellIndex >= 0)
			{
				HexCell location = Grid.GetCell(locationCellIndex);
				Grid.DecreaseVisibility(location, VisionRange);
				location.Unit = null;
			}
			locationCellIndex = value.Index;
			value.Unit = this;
			Grid.IncreaseVisibility(value, VisionRange);
			transform.localPosition = value.Position;
			Grid.MakeChildOfColumn(transform, value.Coordinates.ColumnIndex);
		}
	}

	int locationCellIndex = -1, currentTravelLocationCellIndex = -1;

	/// <summary>
	/// Orientation that the unit is facing.
	/// </summary>
	public float Orientation
	{
		get => orientation;
		set
		{
			orientation = value;
			transform.localRotation = Quaternion.Euler(0f, value, 0f);
		}
	}

	/// <summary>
	/// Speed of the unit, in cells per turn.
	/// </summary>
	public int Speed => movementRange;

	/// <summary>
	/// Vision range of the unit, in cells.
	/// </summary>
	public int VisionRange => visionRange;

	public float MovementSpeed => movementSpeed;

	float orientation;

	List<int> pathToTravel;

	public void SetNextCommand(string nextCommand)
	{
		this.nextCommand = nextCommand;
		Debug.Log("Unit " + unitName + " got command: " + this.nextCommand);
	}

	/// <summary>
	/// Validate the position of the unit.
	/// </summary>
	public void ValidateLocation() =>
		transform.localPosition = Grid.GetCell(locationCellIndex).Position;

	/// <summary>
	/// Check whether a cell is a valid destination for the unit.
	/// </summary>
	/// <param name="cell">Cell to check.</param>
	/// <returns>Whether the unit could occupy the cell.</returns>
	public bool IsValidDestination(HexCell cell) =>
		((this.isPlayerUnit && cell.Flags.HasAll(HexFlags.Explored | HexFlags.Explorable)) || !this.isPlayerUnit) &&
		!(cell.Unit && cell.Unit.isPlayerUnit == this.isPlayerUnit);

	/// <summary>
	/// Travel along a path.
	/// </summary>
	/// <param name="path">List of cells that describe a valid path.</param>
	public void Travel(List<int> path)
	{
		HexCell location = Grid.GetCell(locationCellIndex);
		location.Unit = null;
		location = Grid.GetCell(path[^1]);
		locationCellIndex = location.Index;
		location.Unit = this;
		pathToTravel = path;
		StopAllCoroutines();
		StartCoroutine(TravelPath());
        RemovePlaceholder();
    }

	public int MovePlaceholder(HexCell toCell, int distance)
	{
        currentMovement = toCell;
		int costDiff = distance - moveCost;
        moveCost = distance;
        if (ghostPrefab == null) return costDiff;
		RemovePlaceholder();

        ghostUnit = Instantiate(ghostPrefab, toCell.Position, Quaternion.identity);
        return costDiff;
    }

    public void RemovePlaceholder()
    {
        if (ghostUnit != null)
        {
            Destroy(ghostUnit);
        }
    }

	public int ResetMovement()
	{
		currentMovement = default;
		int costDiff = moveCost;
		moveCost = 0;
		if (ghostPrefab != null)
        {
            RemovePlaceholder();
        }
        return costDiff;
    }

    /// <summary>
    /// Travel to the actual location.
    /// </summary>
    /// <param name="path">List of cells that describe a valid path.</param>
    public void Travel()
    {
		if (currentMovement == default || Location == currentMovement)
		{
			return;
        }
		Grid.FindPath(Location, currentMovement, this);
		Travel(Grid.GetPath());
        //HexCell location = Grid.GetCell(locationCellIndex);
        Grid.ClearPath();
    }

    IEnumerator TravelPath()
	{
		Vector3 a, b, c = Grid.GetCell(pathToTravel[0]).Position;
		yield return LookAt(Grid.GetCell(pathToTravel[1]).Position);

		if (currentTravelLocationCellIndex < 0)
		{
			currentTravelLocationCellIndex = pathToTravel[0];
		}
		HexCell currentTravelLocation = Grid.GetCell(
			currentTravelLocationCellIndex);
		Grid.DecreaseVisibility(currentTravelLocation, VisionRange);
		int currentColumn = currentTravelLocation.Coordinates.ColumnIndex;

		float t = Time.deltaTime * movementSpeed;
		for (int i = 1; i < pathToTravel.Count; i++)
		{
			currentTravelLocation = Grid.GetCell(pathToTravel[i]);
			currentTravelLocationCellIndex = currentTravelLocation.Index;
			a = c;
			b = Grid.GetCell(pathToTravel[i - 1]).Position;

			int nextColumn = currentTravelLocation.Coordinates.ColumnIndex;
			if (currentColumn != nextColumn)
			{
				if (nextColumn < currentColumn - 1)
				{
					a.x -= HexMetrics.innerDiameter * HexMetrics.wrapSize;
					b.x -= HexMetrics.innerDiameter * HexMetrics.wrapSize;
				}
				else if (nextColumn > currentColumn + 1)
				{
					a.x += HexMetrics.innerDiameter * HexMetrics.wrapSize;
					b.x += HexMetrics.innerDiameter * HexMetrics.wrapSize;
				}
				Grid.MakeChildOfColumn(transform, nextColumn);
				currentColumn = nextColumn;
			}

			c = (b + currentTravelLocation.Position) * 0.5f;
			Grid.IncreaseVisibility(Grid.GetCell(pathToTravel[i]), VisionRange);

			for (; t < 1f; t += Time.deltaTime * movementSpeed)
			{
				transform.localPosition = Bezier.GetPoint(a, b, c, t);
				Vector3 d = Bezier.GetDerivative(a, b, c, t);
				d.y = 0f;
				transform.localRotation = Quaternion.LookRotation(d);
				yield return null;
			}
			Grid.DecreaseVisibility(Grid.GetCell(pathToTravel[i]), VisionRange);
			t -= 1f;
		}
		currentTravelLocationCellIndex = -1;

		HexCell location = Grid.GetCell(locationCellIndex);
		a = c;
		b = location.Position;
		c = b;
		Grid.IncreaseVisibility(location, VisionRange);
		for (; t < 1f; t += Time.deltaTime * movementSpeed)
		{
			transform.localPosition = Bezier.GetPoint(a, b, c, t);
			Vector3 d = Bezier.GetDerivative(a, b, c, t);
			d.y = 0f;
			transform.localRotation = Quaternion.LookRotation(d);
			yield return null;
		}

		transform.localPosition = location.Position;
		orientation = transform.localRotation.eulerAngles.y;
		ListPool<int>.Add(pathToTravel);
		pathToTravel = null;
	}

	IEnumerator LookAt(Vector3 point)
	{
		if (HexMetrics.Wrapping)
		{
			float xDistance = point.x - transform.localPosition.x;
			if (xDistance < -HexMetrics.innerRadius * HexMetrics.wrapSize)
			{
				point.x += HexMetrics.innerDiameter * HexMetrics.wrapSize;
			}
			else if (xDistance > HexMetrics.innerRadius * HexMetrics.wrapSize)
			{
				point.x -= HexMetrics.innerDiameter * HexMetrics.wrapSize;
			}
		}

		point.y = transform.localPosition.y;
		Quaternion fromRotation = transform.localRotation;
		Quaternion toRotation =
			Quaternion.LookRotation(point - transform.localPosition);
		float angle = Quaternion.Angle(fromRotation, toRotation);

		if (angle > 0f)
		{
			float speed = rotationSpeed / angle;
			for (float t = Time.deltaTime * speed;
				t < 1f; t += Time.deltaTime * speed)
			{
				transform.localRotation = Quaternion.Slerp(
					fromRotation, toRotation, t);
				yield return null;
			}
		}

		transform.LookAt(point);
		orientation = transform.localRotation.eulerAngles.y;
	}

	/// <summary>
	/// Get the movement cost of moving from one cell to another.
	/// </summary>
	/// <param name="fromCell">Cell to move from.</param>
	/// <param name="toCell">Cell to move to.</param>
	/// <param name="direction">Movement direction.</param>
	/// <returns></returns>
	public int GetMoveCost(
		HexCell fromCell, HexCell toCell, HexDirection direction)
	{
		if (!IsValidDestination(toCell))
		{
			return -1;
		}
		HexEdgeType edgeType = HexMetrics.GetEdgeType(
			fromCell.Values.Elevation, toCell.Values.Elevation);
		if (edgeType == HexEdgeType.Cliff)
		{
			return -1;
		}
		int moveCost;
		if (fromCell.Flags.HasRoad(direction))
		{
			moveCost = 1;
		}
		else if (fromCell.Flags.HasAny(HexFlags.Walled) !=
			toCell.Flags.HasAny(HexFlags.Walled))
		{
			return -1;
		}
		else
		{
			moveCost = edgeType == HexEdgeType.Flat ? 5 : 10;
			HexValues v = toCell.Values;
			moveCost += v.UrbanLevel + v.FarmLevel + v.PlantLevel;
		}
		return moveCost;
    }

	/// <summary>
	/// Get whether the HexUnit has moved during this turn.
	/// </summary>
	/// <returns></returns>
	//public bool HasMoved() => previousLocation.Equals(Location);
	public bool HasMoved() => (currentMovement != default && currentMovement != Location);

    /// <summary>
    /// Terminate the unit.
    /// </summary>
    public void Die()
	{
		HexCell location = Grid.GetCell(locationCellIndex);
		RemovePlaceholder();
		Grid.DecreaseVisibility(location, VisionRange);
		location.Unit = null;
		Destroy(gameObject);
	}

	/// <summary>
	/// Save the unit data.
	/// </summary>
	/// <param name="writer"><see cref="BinaryWriter"/> to use.</param>
	public void Save(BinaryWriter writer)
	{
		//location.Coordinates.Save(writer);
		Grid.GetCell(locationCellIndex).Coordinates.Save(writer);
		writer.Write(orientation);
		writer.Write(health);
        writer.Write((equipment) ? equipment.id : -1);
        writer.Write(unitName);
        writer.Write(background);
		if (agentId != null)
	        writer.Write(agentId);
		else writer.Write("NULL");
        if (threadId != null)
            writer.Write(threadId);
        else writer.Write("NULL");
        writer.Write(agentClass);
        writer.Write(traits.Length);
		for (int i = 0; i < traits.Length; i++)
		{
			writer.Write(traits[i]);
		}
		writer.Write(isPlayerUnit);
        writer.Write(prefabIndex);
	}


    /// <summary>
    /// Load the unit data.
    /// </summary>
    /// <param name="reader"><see cref="BinaryReader"/> to use.</param>
    /// <param name="grid"><see cref="HexGrid"/> to add the unit to.</param>
    public static void Load(BinaryReader reader, HexGrid grid, UnitManager unitManager)
	{
		HexCoordinates coordinates = HexCoordinates.Load(reader);
		float orientation = reader.ReadSingle();
		int health = reader.ReadInt32();

		int _equip_id = reader.ReadInt32();
		// TODO: Save/Load equipment and get equipment by id
		//Equipment equipment = (equip_id == -1) ? null : 
		string unitName = reader.ReadString();
		string background = reader.ReadString();
		string agentId = reader.ReadString();
		if (agentId.Equals("NULL"))
			agentId = null;
		string threadId = reader.ReadString();
		string agentClass = reader.ReadString();
		int _trait_count = reader.ReadInt32();
		List<string> _trait_list = new List<string>(_trait_count);
		for (int i = 0; i < _trait_count; i++)
		{
            _trait_list.Add(reader.ReadString());
        }
		string[] traits = _trait_list.ToArray();
		bool isPlayerUnit = reader.ReadBoolean();
		Debug.Log("Is Player Unit: " + isPlayerUnit);
        int prefabIndex = reader.ReadInt32();

		HexUnit createdUnit = Instantiate(unitManager.unitPrefabs[prefabIndex]);
		createdUnit.unitName = unitName;
		createdUnit.background = background;
		createdUnit.agentId = agentId;
		createdUnit.threadId = threadId;
		createdUnit.agentClass = agentClass;
		createdUnit.traits = traits;
		createdUnit.isPlayerUnit = isPlayerUnit;
		createdUnit.prefabIndex = prefabIndex;

        grid.AddUnit(
            createdUnit, grid.GetCell(coordinates), orientation);
    }

	void OnEnable()
	{
		if (locationCellIndex >= 0)
		{
			HexCell location = Grid.GetCell(locationCellIndex);
            currentMovement = default;
			transform.localPosition = location.Position;
			if (currentTravelLocationCellIndex >= 0)
			{
				HexCell currentTravelLocation =
					Grid.GetCell(currentTravelLocationCellIndex);
				Grid.IncreaseVisibility(location, VisionRange);
				Grid.DecreaseVisibility(currentTravelLocation, VisionRange);
				currentTravelLocationCellIndex = -1;
			}
		}
	}

	public string ToJSON(bool resetMovement)
	{
		string _equip_id = (!equipment) ? "\"NULL\"" : equipment.id.ToString();
		string _json = $"{{\"agent_id\": \"{agentId}\",\"thread_id\": \"{threadId}\",\"x_coord\": {Location.Coordinates.X},\"y_coord\": {Location.Coordinates.Z},\"hp\": {health}," +
			$"\"class_id\": {prefabIndex},\"equip_id\": {_equip_id},\"team\": {isPlayerUnit.ToString().ToLower()},\"order\": \"{nextCommand}\",\"has_moved\": {HasMoved().ToString().ToLower()}}}";
		if (resetMovement) {
            ResetMovement();
        }
        return _json;
	}

	public void SetUpEquipmentMenu() {
		unitNameText.text = unitName;
		itemDamageText.text = damage.ToString();
		itemHealthText.text = health.ToString();
		unitBackgroundText.text = background;	
	}

	public void OpenEquipmentMenu() {
		// equipmentMenu.SetActive(true);
		// backButton.SetActive(true);
		bool menuOpenCheck = true;
		for (int i = 0; i < uiManager.hiddenMenus.Length; i++) {
			if (uiManager.hiddenMenus[i] != null) {
				menuOpenCheck = false;
				break;
			}
		}
		if (menuOpenCheck) {
			uiManager.OpenEquipmentMenu();
		}
		
	}
}
