using UnityEditorInternal;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Component that manages the game UI.
/// </summary>
public class HexGameUI : MonoBehaviour
{
	[SerializeField]
	HexGrid grid;

	[SerializeField]
	TurnManager turnManager;

    int currentCellIndex = -1;
	bool isVRInteraction = false;
    public GameObject rightGlove;
	bool isEditMode = false;

	HexUnit selectedUnit;

	/// <summary>
	/// Set whether map edit mode is active.
	/// </summary>
	/// <param name="toggle">Whether edit mode is enabled.</param>
	public void SetEditMode(bool toggle)
	{
        isEditMode = toggle;
		if (toggle)
		{
			Shader.EnableKeyword("_HEX_MAP_EDIT_MODE");
		}
		else
		{
			Shader.DisableKeyword("_HEX_MAP_EDIT_MODE");
		}
	}

    void Start()
    {
        grid.ShowUI(true);
        grid.ClearPath();
        SetEditMode(true);
    }

    void Update()
	{
		if (!EventSystem.current.IsPointerOverGameObject())
		{
			if (Input.GetMouseButtonDown(0))
			{
				isVRInteraction = false;

                DoSelection();
			}
			else if (selectedUnit)
			{
				if (Input.GetMouseButtonDown(1))
                {
                    isVRInteraction = false;
                    DoMove();
				}
				else
				{
					DoPathfinding();
				}
			}
		}

		if (Input.GetKeyDown(KeyCode.V))
        {
            SetEditMode(!isEditMode);
        }
    }

	public void HandleVRSelect(Ray ray)
    {
		isVRInteraction = true;

        if (selectedUnit == null)
		{
			grid.ClearPath();
			UpdateCurrentCell(ray);
			if (currentCellIndex >= 0)
			{
				selectedUnit = grid.GetCell(currentCellIndex).Unit;
			}
		} else
		{
			DoMove();
            selectedUnit = null;
        }
    }

	void DoSelection()
	{
		grid.ClearPath();
		UpdateCurrentCell();
		if (currentCellIndex >= 0)
		{
			selectedUnit = grid.GetCell(currentCellIndex).Unit;
			//if (!selectedUnit.isPlayerUnit) selectedUnit = null;
        }
	}

	void DoPathfinding()
	{
		if (UpdateCurrentCell())
		{
			if (currentCellIndex >= 0 &&
				selectedUnit.IsValidDestination(grid.GetCell(currentCellIndex)))
			{
				grid.FindPath(
					selectedUnit.Location,
					grid.GetCell(currentCellIndex),
					selectedUnit);
			}
			else
			{
				grid.ClearPath();
			}
		}
	}

	void DoMove()
	{
		if (grid.HasPath)
		{
			turnManager.ChangeMana(selectedUnit.MovePlaceholder(grid.PathToLocation, grid.PathDistance));
			grid.ClearPath();
		}
	}

	bool UpdateCurrentCell()
	{
		if (isVRInteraction)
        {
            return UpdateCurrentCell(new Ray(rightGlove.transform.position, rightGlove.transform.forward));
        }
        else
        {
            return UpdateCurrentCell(Camera.main.ScreenPointToRay(Input.mousePosition));
        }

    }

	bool UpdateCurrentCell(Ray ray)
    {
        HexCell cell = grid.GetCell(ray);
        int index = cell ? cell.Index : -1;
        if (index != currentCellIndex)
        {
            currentCellIndex = index;
            return true;
        }
        return false;
    }
}
