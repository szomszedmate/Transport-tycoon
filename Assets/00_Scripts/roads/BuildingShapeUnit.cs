using UnityEngine;
using Unity.AI.Navigation;
public class BuildingShapeUnit : MonoBehaviour
{
    [SerializeField] private NavMeshSurface navMeshSurface;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        navMeshSurface = FindFirstObjectByType<NavMeshSurface>();
        navMeshSurface.BuildNavMesh();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
