using NUnit.Framework;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

[TestFixture]
public class BuildingGridBehaviorTests
{
    private BuildingGrid buildingGrid;

    [SetUp]
    public void SetUp()
    {
        GameObject gameObject = new GameObject("TestBuildingGrid");
        buildingGrid = gameObject.AddComponent<BuildingGrid>();

        buildingGrid.Width = 5;
        buildingGrid.Height = 5;
        buildingGrid.Locations = new List<ILocation>();

        BuildingGrid.BuildingGridCell[,] grid = new BuildingGrid.BuildingGridCell[5, 5];

        for (int col = 0; col < 5; col++)
        {
            for (int row = 0; row < 5; row++)
            {
                grid[col, row] = new BuildingGrid.BuildingGridCell();
            }
        }

        SetPrivateProperty("Grid", grid);

        SetPrivateField("treeVisuals", new TreeVisual[5, 5]);
        SetPrivateField("waterVisuals", new GameObject[5, 5]);

        GameObject waterPrefab = new GameObject("WaterTilePrefab");
        GameObject waterContainer = new GameObject("WaterContainer");

        SetPrivateField("waterTilePrefab", waterPrefab);
        SetPrivateField("waterContainer", waterContainer.transform);

        GameObject treePrefabObject = new GameObject("TreeVisualPrefab");
        TreeVisual treeVisualPrefab = treePrefabObject.AddComponent<TreeVisual>();

        GameObject tree1 = new GameObject("tree1");
        GameObject tree2 = new GameObject("tree2");
        GameObject tree3 = new GameObject("tree3");
        GameObject tree4 = new GameObject("tree4");

        tree1.transform.parent = treePrefabObject.transform;
        tree2.transform.parent = treePrefabObject.transform;
        tree3.transform.parent = treePrefabObject.transform;
        tree4.transform.parent = treePrefabObject.transform;

        SetPrivateFieldOnObject(treeVisualPrefab, "tree1", tree1);
        SetPrivateFieldOnObject(treeVisualPrefab, "tree2", tree2);
        SetPrivateFieldOnObject(treeVisualPrefab, "tree3", tree3);
        SetPrivateFieldOnObject(treeVisualPrefab, "tree4", tree4);

        GameObject treeContainer = new GameObject("TreeContainer");

        SetPrivateField("treeVisualPrefab", treeVisualPrefab);
        SetPrivateField("treeContainer", treeContainer.transform);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(buildingGrid.gameObject);
    }

    [Test]
    public void WorldToGridPosition_ReturnsExpectedGridCoordinates()
    {
        Vector3 worldPosition = new Vector3(BuildingSystem.CellSize * 2.2f, 0, BuildingSystem.CellSize * 3.7f);

        (int x, int y) = buildingGrid.WorldToGridPosition(worldPosition);

        Assert.AreEqual(2, x);
        Assert.AreEqual(3, y);
    }

    [Test]
    public void GetWidthAndHeight_ReturnConfiguredSize()
    {
        Assert.AreEqual(5, buildingGrid.GetWidth());
        Assert.AreEqual(5, buildingGrid.GetHeight());
    }

    [Test]
    public void SetWater_ChangesTerrainToWater()
    {
        buildingGrid.SetWater(2, 2);

        Assert.IsTrue(buildingGrid.IsWater(2, 2));
        Assert.AreEqual(BuildingGrid.TerrainType.Water, buildingGrid.GetTerrainType(2, 2));
    }

    [Test]
    public void SetLand_ChangesTerrainBackToLand()
    {
        buildingGrid.SetWater(2, 2);

        buildingGrid.SetLand(2, 2);

        Assert.IsFalse(buildingGrid.IsWater(2, 2));
        Assert.AreEqual(BuildingGrid.TerrainType.Land, buildingGrid.GetTerrainType(2, 2));
    }

    [Test]
    public void SetWater_ClearsTreesOnCell()
    {
        buildingGrid.SetTreeCount(2, 2, 4);

        buildingGrid.SetWater(2, 2);

        Assert.AreEqual(0, buildingGrid.GetTreeCount(2, 2));
        Assert.IsTrue(buildingGrid.IsWater(2, 2));
    }

    [Test]
    public void SetTreeCount_SetsTreeCount_OnLandCell()
    {
        buildingGrid.SetTreeCount(1, 1, 3);

        Assert.AreEqual(3, buildingGrid.GetTreeCount(1, 1));
        Assert.IsTrue(buildingGrid.CanSpreadTrees(1, 1));
    }

    [Test]
    public void SetTreeCount_DoesNotSetTrees_OnWaterCell()
    {
        buildingGrid.SetWater(1, 1);

        buildingGrid.SetTreeCount(1, 1, 3);

        Assert.AreEqual(0, buildingGrid.GetTreeCount(1, 1));
    }

    [Test]
    public void IncreaseTreeCount_IncreasesTrees_OnLandCell()
    {
        buildingGrid.SetTreeCount(1, 1, 1);

        buildingGrid.IncreaseTreeCount(1, 1);

        Assert.AreEqual(2, buildingGrid.GetTreeCount(1, 1));
    }

    [Test]
    public void ClearTrees_RemovesTrees()
    {
        Vector3 position = new Vector3(BuildingSystem.CellSize * 2.5f, 0, BuildingSystem.CellSize * 2.5f);
        buildingGrid.SetTreeCount(position, 4);

        buildingGrid.ClearTrees(position);

        Assert.AreEqual(0, buildingGrid.GetTreeCount(position));
        Assert.IsFalse(buildingGrid.HasTrees(position));
    }

    [Test]
    public void GetSpreadableNeighbors_ReturnsOnlyEmptyLandNeighbors()
    {
        buildingGrid.SetWater(1, 2);
        buildingGrid.SetTreeCount(3, 2, 2);

        List<(int, int)> neighbors = buildingGrid.GetSpreadableNeighbors(2, 2);

        Assert.IsFalse(neighbors.Contains((1, 2)), "Water cell should not be spreadable.");
        Assert.IsFalse(neighbors.Contains((3, 2)), "Cell with trees should not be spreadable.");
        Assert.IsTrue(neighbors.Contains((2, 1)));
        Assert.IsTrue(neighbors.Contains((2, 3)));
    }

    [Test]
    public void CanBuildRoad_ReturnsTrue_ForEmptyLandCell()
    {
        Vector3 position = new Vector3(BuildingSystem.CellSize * 1.5f, 0, BuildingSystem.CellSize * 1.5f);

        Assert.IsTrue(buildingGrid.CanBuildRoad(position));
    }

    [Test]
    public void CanBuildRoad_ReturnsFalse_ForWaterCell()
    {
        buildingGrid.SetWater(1, 1);
        Vector3 position = new Vector3(BuildingSystem.CellSize * 1.5f, 0, BuildingSystem.CellSize * 1.5f);

        Assert.IsFalse(buildingGrid.CanBuildRoad(position));
    }

    [Test]
    public void CanBuildRoad_ReturnsFalse_ForCellWithRoad()
    {
        Road road = new GameObject("Road").AddComponent<Road>();
        Vector3 position = new Vector3(BuildingSystem.CellSize * 1.5f, 0, BuildingSystem.CellSize * 1.5f);

        buildingGrid.SetRoad(road, position);

        Assert.IsFalse(buildingGrid.CanBuildRoad(position));

        Object.DestroyImmediate(road.gameObject);
    }

    [Test]
    public void SetRoad_DoesNotPlaceNormalRoad_OnWaterCell()
    {
        Road road = new GameObject("Road").AddComponent<Road>();
        Vector3 position = new Vector3(BuildingSystem.CellSize * 1.5f, 0, BuildingSystem.CellSize * 1.5f);

        buildingGrid.SetWater(1, 1);

        buildingGrid.SetRoad(road, position);

        Assert.IsFalse(buildingGrid.Grid[1, 1].IsRoad());

        Object.DestroyImmediate(road.gameObject);
    }

    [Test]
    public void SetVehicle_DoesNotPlaceVehicle_OnWaterCell()
    {
        TestVehicle vehicle = new GameObject("Vehicle").AddComponent<TestVehicle>();
        Vector3 position = new Vector3(BuildingSystem.CellSize * 1.5f, 0, BuildingSystem.CellSize * 1.5f);

        buildingGrid.SetWater(1, 1);

        buildingGrid.SetVehicle(vehicle, position);

        Assert.IsTrue(buildingGrid.Grid[1, 1].IsEmpty());

        Object.DestroyImmediate(vehicle.gameObject);
    }

    private class TestVehicle : MonoBehaviour, IVehicle
    {
        public BuildCategory BuildCategory => BuildCategory.BUS;
    }

    private void SetPrivateField(string fieldName, object value)
    {
        FieldInfo field = typeof(BuildingGrid).GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        field.SetValue(buildingGrid, value);
    }

    private void SetPrivateProperty(string propertyName, object value)
    {
        PropertyInfo property = typeof(BuildingGrid).GetProperty(
            propertyName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
        );

        property.SetValue(buildingGrid, value);
    }

    private void SetPrivateFieldOnObject(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        field.SetValue(target, value);
    }
}