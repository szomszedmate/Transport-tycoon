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
    [SerializeField] private CameraMovement MainCamera;

    private const float doubleClickTime = 0.3f;
    private float lastClickTime;
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
                buildingSystem.RotatePreview(90);
            }
            else if (Input.GetMouseButtonDown(0))
            {
                buildingSystem.HandlePreview(mousePos, true);
            } 
            else if (Input.GetMouseButtonDown(1))
            {
                buildingSystem.DestroyPreview();
                return;
            }
            else
            {
                buildingSystem.HandlePreview(mousePos, false);
            }
        }

        if (Input.GetKeyDown(KeyCode.R) && buildingSystem.RoutePlanning)
        {
            buildingSystem.ResetRoute();
        }

        if (Input.GetMouseButton(0) && buildingSystem.RoutePlanning) // click to add road to route
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out hit))
            {
                buildingSystem.AddToRoute(hit);
            }
            return;
        } else if (Input.GetMouseButton(1) && buildingSystem.RoutePlanning) // right click to remove road from route
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out hit))
            {
                buildingSystem.RemFromRoute(hit);
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
