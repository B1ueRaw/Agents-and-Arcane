using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Valve.VR;

public class TileInfoManager : MonoBehaviour
{
    [SerializeField]
    HexGrid hexGrid;

    [SerializeField]
    TextMeshProUGUI resourceName;
    [SerializeField]
    TextMeshProUGUI resourceYield;
    [SerializeField]
    TextMeshProUGUI manaYield;
    [SerializeField]
    TextMeshProUGUI resourceDescription;

    public void Open()
    {
        if (SteamVR.initializedState != SteamVR.InitializedStates.InitializeSuccess)
        {
            HexCell hexCell = hexGrid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));
            if (hexCell != default)
            {
                Open(hexCell);
            }
        }
    }

    public void Open(HexCell hexCell)
    {
        resourceName.text = ((Resources)hexCell.ExtendedValues.ResourceTypeIndex).ToString();
        resourceYield.text = hexCell.ExtendedValues.ResourceLevel.ToString();
        manaYield.text = hexCell.ExtendedValues.ManaLevel.ToString();
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
