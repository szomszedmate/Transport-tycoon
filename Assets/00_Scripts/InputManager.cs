using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEditor.Rendering.BuiltIn.ShaderGraph;

public class InputManager : MonoBehaviour
{
    [SerializeField] private BuildingSystem buildingSystem;
    [SerializeField] private GameUiFunctions ui;
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

        // for debug
        #region Debug
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            for (int i = 0; i < buildingSystem.Grid.Width; i++)
            {
                for (int j = 0; j < buildingSystem.Grid.Height; j++)
                {
                    if (buildingSystem.Grid.Grid[i,j].IsLocation())
                    {
                        Debug.Log(i + ", " + j);
                    }

                }
            }
        }
        #endregion

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ui.OpenMenu();
        }

        if (Input.GetMouseButtonDown(1))
        {
            buildingSystem.DestroyPreview();
            return;
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            buildingSystem.DestroyMode();
        }

        if (Input.GetMouseButtonDown(0) && buildingSystem.destroy)
        {
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

        if (Input.GetKeyDown(KeyCode.Alpha1)) // straight road
        {
            buildingSystem.CreatePreview(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2)) // turning road
        {
            buildingSystem.CreatePreview(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3)) // t road
        {
            buildingSystem.CreatePreview(3);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4)) // cross road
        {
            buildingSystem.CreatePreview(4);
        }
        else if (Input.GetKeyDown(KeyCode.B)) // bus
        {
            buildingSystem.CreatePreview(5);
        }
        else if (Input.GetKeyDown(KeyCode.C)) // route planning
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out hit))
            {
                buildingSystem.SelectBusForPlanning(hit);
            }
        } else if (Input.GetKeyDown(KeyCode.T)) // bus stop
        {
            buildingSystem.CreatePreview(6);
        }
    }
}
