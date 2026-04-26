using JetBrains.Annotations;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[Serializable]
public struct BuildHotkey
{
    public KeyCode Key; 
    public ScriptableObject Data;
}

public class InputManager : MonoBehaviour
{
    [SerializeField] private BuildingSystem buildingSystem;
    [SerializeField] private GameUiFunctions ui;
    [SerializeField] private CameraMovement MainCamera;

    [SerializeField] private List<BuildHotkey> hotkeys;

    private const float doubleClickTime = 0.3f;
    private float lastClickTime;
    private RaycastHit hit;
    private Dictionary<KeyCode, IData> buildShortcuts = new Dictionary<KeyCode, IData>();
    public BuildingSystem BuildingSystem { get => buildingSystem; private set => buildingSystem = value; }



    public event EventHandler moneyDebugEvent;

    private void Awake()
    {
        if (buildingSystem == null) buildingSystem = GetComponentInChildren<BuildingSystem>();
        foreach (var hotkey in hotkeys)
        {
            if (hotkey.Data is IData data)
                buildShortcuts[hotkey.Key] = data;
        }
    }
    void Update()
    {
#if UNITY_EDITOR
        if (EditorWindow.focusedWindow == null || EditorWindow.focusedWindow.titleContent.text != "Game") return;
#endif
        Vector2 mousePos = Mouse.current.position.ReadValue();
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            bool leftClicked = Mouse.current.leftButton.wasPressedThisFrame;
            bool rightClicked = Mouse.current.rightButton.wasPressedThisFrame;
            bool leftHeld = Mouse.current.leftButton.isPressed;
            bool rightHeld = Mouse.current.rightButton.isPressed;

            buildingSystem.InputUpdate(mousePos, leftClicked, rightClicked, leftHeld, rightHeld);
        }


        // for debug
        #region Debug
        if (Keyboard.current.digit9Key.wasPressedThisFrame) //  press 9 to draw ilocation tiles
        {
            for (int i = 0; i < BuildingSystem.Grid.Width; i++)
            {
                for (int j = 0; j < BuildingSystem.Grid.Height; j++)
                {
                    if (BuildingSystem.Grid.Grid[i, j].IsLocation())
                    {
                        Vector3 gridOrigin = BuildingSystem.Grid.transform.position;
                        Vector3 pos = gridOrigin + new Vector3(i * 10f + 5f, 8.0f, j * 10f + 5f);

                        Debug.DrawLine(pos + Vector3.left * 4, pos + Vector3.right * 4, Color.magenta, 10f);
                        Debug.DrawLine(pos + Vector3.forward * 4, pos + Vector3.back * 4, Color.magenta, 10f);
                    }
                }
            }
        }
        
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            moneyDebugEvent.Invoke(this, EventArgs.Empty);
        }
        #endregion


        if (Keyboard.current.anyKey.wasPressedThisFrame)
        {
            foreach (var key in buildShortcuts.Keys)
            {
                string keyName = key.ToString().ToLower();

                if (keyName.Contains("alpha"))
                {
                    keyName = keyName.Replace("alpha", "");
                }
                var control = Keyboard.current[keyName];

                if (control is UnityEngine.InputSystem.Controls.KeyControl keyControl && keyControl.wasPressedThisFrame)
                {
                    buildingSystem.CreatePreview(buildShortcuts[key]);
                    break;
                }
            }
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ui.OpenMenu();
        }

        #region Camera

        if (Keyboard.current.leftShiftKey.wasPressedThisFrame)
        {
            MainCamera.IncreaseSpeed();
        } else if (Keyboard.current.leftShiftKey.wasReleasedThisFrame)
        {
            MainCamera.ResetSpeed();
        }

        if (Keyboard.current.wKey.isPressed)
        {
            MainCamera.MoveCameraHorizontally(Direction.N);
        }
        if (Keyboard.current.aKey.isPressed)
        {
            MainCamera.MoveCameraHorizontally(Direction.W);
        }
        if (Keyboard.current.sKey.isPressed)
        {
            MainCamera.MoveCameraHorizontally(Direction.S);
        }
        if (Keyboard.current.dKey.isPressed)
        {
            MainCamera.MoveCameraHorizontally(Direction.E);
        }
        
        if (Mouse.current.scroll.ReadValue().y < 0 || Keyboard.current.eKey.isPressed)
        {
            MainCamera.MoveCameraVertically(true); // up
        } else if (Mouse.current.scroll.ReadValue().y > 0 || Keyboard.current.qKey.isPressed)
        {
            MainCamera.MoveCameraVertically(false); // down
        }

        if (Mouse.current.middleButton.wasPressedThisFrame) // reset angle on double click
        {
            if (Time.time - lastClickTime < doubleClickTime)
            {
                MainCamera.ResetRotation();
            }
            lastClickTime = Time.time;
        }

        if (Mouse.current.middleButton.isPressed) // rotate camera
        {
            float mX = Mouse.current.delta.ReadValue().x * 0.05f;
            float mY = Mouse.current.delta.ReadValue().y * 0.05f;

            if (mX != 0 || mY != 0)
            {
                MainCamera.RotateFreeLook(mX, mY);
            }
        }

        #endregion


        if (Keyboard.current.xKey.wasPressedThisFrame)
        {
            BuildingSystem.DestroyMode();
        }

        if (BuildingSystem.Preview != null)
        {
            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                BuildingSystem.RotatePreview(90);
            }
        }

        if (Keyboard.current.rKey.wasPressedThisFrame && BuildingSystem.RoutePlanning)
        {
            BuildingSystem.ResetRoute();
        }

        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out hit))
            {
                BuildingSystem.SelectVehicleByHit(hit);
            }
        }

        if (Keyboard.current.enterKey.wasPressedThisFrame && BuildingSystem.RoutePlanning)
        {
            BuildingSystem.BS_ConfirmRoute();
        }
        
    }
}
