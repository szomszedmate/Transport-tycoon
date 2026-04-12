using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Rendering.BuiltIn.ShaderGraph;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [SerializeField] private BuildingSystem buildingSystem;
    [SerializeField] private GameUiFunctions ui;
    [SerializeField] private CameraMovement MainCamera;

    private const float doubleClickTime = 0.3f;
    private float lastClickTime;
    private RaycastHit hit;

    public BuildingSystem BuildingSystem { get => buildingSystem; private set => buildingSystem = value; }

    private void Awake()
    {
        if (buildingSystem == null) buildingSystem = GetComponentInChildren<BuildingSystem>();
    }
    void Update()
    {
        Vector3 mousePos = BuildingSystem.GetMouseWorldPosition();

        // for debug
        #region Debug
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            for (int i = 0; i < BuildingSystem.Grid.Width; i++)
            {
                for (int j = 0; j < BuildingSystem.Grid.Height; j++)
                {
                    if (BuildingSystem.Grid.Grid[i,j].IsLocation())
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

        #region Camera

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            MainCamera.IncreaseSpeed();
        } else if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            MainCamera.ResetSpeed();
        }

        if (Input.GetKey(KeyCode.W))
        {
            MainCamera.MoveCameraHorizontally(Direction.N);
        }
        if (Input.GetKey(KeyCode.A))
        {
            MainCamera.MoveCameraHorizontally(Direction.W);
        }
        if (Input.GetKey(KeyCode.S))
        {
            MainCamera.MoveCameraHorizontally(Direction.S);
        }
        if (Input.GetKey(KeyCode.D))
        {
            MainCamera.MoveCameraHorizontally(Direction.E);
        }

        if (Input.GetAxis("Mouse ScrollWheel") < 0 || Input.GetKey(KeyCode.E))
        {
            MainCamera.MoveCameraVertically(true); // up
        } else if (Input.GetAxis("Mouse ScrollWheel") > 0 || Input.GetKey(KeyCode.Q))
        {
            MainCamera.MoveCameraVertically(false); // down
        }

        if (Input.GetMouseButtonDown(2)) // reset angle on double click
        {
            if (Time.time - lastClickTime < doubleClickTime)
            {
                MainCamera.ResetRotation();
            }
            lastClickTime = Time.time;
        }

        if (Input.GetMouseButton(2)) // rotate camera
        {
            float mX = Input.GetAxis("Mouse X");
            float mY = Input.GetAxis("Mouse Y");

            if (mX != 0 || mY != 0)
            {
                MainCamera.RotateFreeLook(mX, mY);
            }
        }

        #endregion


        if (Input.GetKeyDown(KeyCode.X))
        {
            BuildingSystem.DestroyMode();
        }

        if (Input.GetMouseButtonDown(0) && BuildingSystem.destroy)
        {
            BuildingSystem.Destroy();
            return;
        }

        if (BuildingSystem.Preview != null)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                BuildingSystem.RotatePreview(90);
            }
            else if (!EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown(0)) // cant build if hovering ui
            {
                
                BuildingSystem.HandlePreview(mousePos, true);
            }
            else if (Input.GetMouseButtonDown(1)) // right click destroys preview
            {
                BuildingSystem.DestroyPreview();
                return;
            }
            else
            {
                BuildingSystem.HandlePreview(mousePos, false);
            }
        }

        if (Input.GetKeyDown(KeyCode.R) && BuildingSystem.RoutePlanning)
        {
            BuildingSystem.ResetRoute();
        }

        if (Input.GetMouseButton(0) && BuildingSystem.RoutePlanning) // click to add road to route
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out hit))
            {
                BuildingSystem.AddToRoute(hit);
            }
            return;
        } else if (Input.GetMouseButton(1) && BuildingSystem.RoutePlanning) // right click to remove road from route
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out hit))
            {
                BuildingSystem.RemFromRoute(hit);
            }
            return;
        }
        

        if (Input.GetKeyDown(KeyCode.Return) && BuildingSystem.RoutePlanning)
        {
            BuildingSystem.BS_ConfirmRoute();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)) // straight road
        {
            BuildingSystem.CreatePreview(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2)) // turning road
        {
            BuildingSystem.CreatePreview(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3)) // t road
        {
            BuildingSystem.CreatePreview(3);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4)) // cross road
        {
            BuildingSystem.CreatePreview(4);
        }
        else if (Input.GetKeyDown(KeyCode.B)) // bus
        {
            BuildingSystem.CreatePreview(5);
        }
        else if (Input.GetKeyDown(KeyCode.C)) // route planning
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out hit))
            {
                BuildingSystem.SelectBusForPlanning(hit);
            }
        } else if (Input.GetKeyDown(KeyCode.T)) // bus stop
        {
            BuildingSystem.CreatePreview(6);
        }
    }
}
