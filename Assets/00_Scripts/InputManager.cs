using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEditor.Rendering.BuiltIn.ShaderGraph;

public class InputManager : MonoBehaviour
{
    [SerializeField] private BuildingSystem buildingSystem;
    private RaycastHit hit;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePos = buildingSystem.GetMouseWorldPosition();
        // Right-click destroys preview
        if (Input.GetMouseButtonDown(1))
        {
            buildingSystem.DestroyPreview();
            return;
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            buildingSystem.DestroyMode();
            Debug.Log("X pressed");
        }

        if (Input.GetMouseButtonDown(0) && buildingSystem.destroy)
        {
            Debug.Log("Left clicked and destroy enabled");
            buildingSystem.Destroy();
            return;
        }

        if (buildingSystem.Preview != null)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                buildingSystem.Preview.Rotate(90);
            }
            else if (Input.GetMouseButtonDown(0))
            {
                buildingSystem.HandlePreview(mousePos, true);
            }
            else
            {
                buildingSystem.HandlePreview(mousePos, false);
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.R) && buildingSystem.RoutePlanning)
        {
            buildingSystem.ResetRoute();
        }

        if (Input.GetMouseButtonDown(0) && buildingSystem.RoutePlanning)
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out hit))
            {
                buildingSystem.AddToRoute(hit);
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.Return) && buildingSystem.RoutePlanning)
        {
            buildingSystem.BS_ConfirmRoute();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            buildingSystem.CreatePreview(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            buildingSystem.CreatePreview(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            buildingSystem.CreatePreview(3);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            buildingSystem.CreatePreview(4);
        }
        else if (Input.GetKeyDown(KeyCode.B))
        {
            buildingSystem.CreatePreview(5);
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out hit))
            {
                buildingSystem.SelectBusForPlanning(hit);
            }
        }
    }
}
