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

        if (buildingSystem.destroy)
        {
            Debug.Log("Handling destroy mode");
            buildingSystem.HandleDestroyMode();
            return;
        }

        if (buildingSystem.Preview != null)
        {
            buildingSystem.HandlePreview(mousePos);
            return;
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
    }
}
