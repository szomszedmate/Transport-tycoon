using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [SerializeField] private BuildingSystem buildingSystem;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePos = buildingSystem.GetMouseWorldPosition();
        // Right-click destroys preview
        if (Input.GetMouseButtonDown(1) && buildingSystem.Preview != null)
        {
            buildingSystem.destroyPreview(); // Works for buses and roads
            return;
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            destroy = !destroy;

            // Reset last hovered road if turning destroy mode off
            if (!destroy && lastHovered != null)
            {
                lastHovered.ChangeState(Road.RoadState.BUILT);
                lastHovered = null;
            }
        }

        if (destroy)
        {
            HandleDestroyMode();
            return;
        }

        if (Preview != null)
        {
            HandlePreview(mousePos);
            return;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Preview = CreateRoadPreview(RoadData1, mousePos);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Preview = CreateRoadPreview(RoadData2, mousePos);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Preview = CreateRoadPreview(RoadData3, mousePos);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Preview = CreateRoadPreview(RoadData4, mousePos);
        }
        else if (Input.GetKeyDown(KeyCode.B))
        {
            Preview = CreateBusPreview(BusData1, mousePos);
        }
    }
}
