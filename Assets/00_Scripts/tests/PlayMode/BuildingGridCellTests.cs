using NUnit.Framework;

[TestFixture]
public class BuildingGridCellTests
{
    private BuildingGrid.BuildingGridCell cell;

    [SetUp]
    public void SetUp()
    {
        cell = new BuildingGrid.BuildingGridCell();
    }

    [Test]
    public void NewCell_IsLandByDefault()
    {
        Assert.AreEqual(BuildingGrid.TerrainType.Land, cell.GetTerrainType());
        Assert.IsTrue(cell.IsLand());
        Assert.IsFalse(cell.IsWater());
    }

    [Test]
    public void SetTerrainType_ToWater_ChangesCellToWater()
    {
        cell.SetTerrainType(BuildingGrid.TerrainType.Water);

        Assert.AreEqual(BuildingGrid.TerrainType.Water, cell.GetTerrainType());
        Assert.IsTrue(cell.IsWater());
        Assert.IsFalse(cell.IsLand());
    }

    [Test]
    public void NewCell_HasNoTreesByDefault()
    {
        Assert.AreEqual(0, cell.GetTreeCount());
        Assert.IsFalse(cell.HasTrees());
        Assert.IsFalse(cell.CanSpreadTrees());
    }

    [Test]
    public void SetTreeCount_ValidValue_UpdatesTreeCount()
    {
        cell.SetTreeCount(3);

        Assert.AreEqual(3, cell.GetTreeCount());
        Assert.IsTrue(cell.HasTrees());
        Assert.IsTrue(cell.CanSpreadTrees());
    }

    [Test]
    public void SetTreeCount_InvalidNegativeValue_DoesNotChangeTreeCount()
    {
        cell.SetTreeCount(2);

        cell.SetTreeCount(-1);

        Assert.AreEqual(2, cell.GetTreeCount());
    }

    [Test]
    public void SetTreeCount_InvalidTooLargeValue_DoesNotChangeTreeCount()
    {
        cell.SetTreeCount(2);

        cell.SetTreeCount(5);

        Assert.AreEqual(2, cell.GetTreeCount());
    }

    [Test]
    public void IncreaseTreeCount_IncreasesTreeCountByOne()
    {
        cell.SetTreeCount(2);

        cell.IncreaseTreeCount();

        Assert.AreEqual(3, cell.GetTreeCount());
    }

    [Test]
    public void IncreaseTreeCount_DoesNotIncreaseAboveFour()
    {
        cell.SetTreeCount(4);

        cell.IncreaseTreeCount();

        Assert.AreEqual(4, cell.GetTreeCount());
    }

    [Test]
    public void ClearTrees_RemovesAllTrees()
    {
        cell.SetTreeCount(4);

        cell.ClearTrees();

        Assert.AreEqual(0, cell.GetTreeCount());
        Assert.IsFalse(cell.HasTrees());
        Assert.IsFalse(cell.CanSpreadTrees());
    }

    [Test]
    public void CanSpreadTrees_ReturnsTrueOnlyWhenTreeCountIsGreaterThanTwo()
    {
        cell.SetTreeCount(2);
        Assert.IsFalse(cell.CanSpreadTrees());

        cell.SetTreeCount(3);
        Assert.IsTrue(cell.CanSpreadTrees());
    }
}