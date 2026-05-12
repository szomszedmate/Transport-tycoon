using NUnit.Framework;
using UnityEngine;
using System.Reflection;

[TestFixture]
public class TruckTypeTests
{
    private Truck truck;

    [SetUp]
    public void SetUp()
    {
        GameObject gameObject = new GameObject("TestTruck");
        truck = gameObject.AddComponent<Truck>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(truck.gameObject);
    }

    private void SetTruckModel(VehicleModel model)
    {
        FieldInfo modelField = typeof(VehicleBase).GetField(
            "model",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
        );

        modelField.SetValue(truck, model);
    }

    [Test]
    public void BuildCategory_ReturnsTruck()
    {
        Assert.AreEqual(BuildCategory.TRUCK, truck.BuildCategory);
    }

    [Test]
    public void TruckType_ReturnsUnknown_WhenModelIsNull()
    {
        SetTruckModel(null);

        Assert.AreEqual(TruckType.UNKNOWN, truck.TruckType);
    }

    [Test]
    public void TruckType_ReturnsFarm_WhenModelIsFarmTruck()
    {
        FarmTruck model = new GameObject("FarmTruckModel").AddComponent<FarmTruck>();

        SetTruckModel(model);

        Assert.AreEqual(TruckType.FARM, truck.TruckType);

        Object.DestroyImmediate(model.gameObject);
    }

    [Test]
    public void TruckType_ReturnsWater_WhenModelIsWaterTruck()
    {
        WaterTruck model = new GameObject("WaterTruckModel").AddComponent<WaterTruck>();

        SetTruckModel(model);

        Assert.AreEqual(TruckType.WATER, truck.TruckType);

        Object.DestroyImmediate(model.gameObject);
    }

    [Test]
    public void TruckType_ReturnsCoal_WhenModelIsCoalTruck()
    {
        CoalTruck model = new GameObject("CoalTruckModel").AddComponent<CoalTruck>();

        SetTruckModel(model);

        Assert.AreEqual(TruckType.COAL, truck.TruckType);

        Object.DestroyImmediate(model.gameObject);
    }

    [Test]
    public void TruckType_ReturnsIron_WhenModelIsIronTruck()
    {
        IronTruck model = new GameObject("IronTruckModel").AddComponent<IronTruck>();

        SetTruckModel(model);

        Assert.AreEqual(TruckType.IRON, truck.TruckType);

        Object.DestroyImmediate(model.gameObject);
    }

    [Test]
    public void TruckType_ReturnsGold_WhenModelIsGoldTruck()
    {
        GoldTruck model = new GameObject("GoldTruckModel").AddComponent<GoldTruck>();

        SetTruckModel(model);

        Assert.AreEqual(TruckType.GOLD, truck.TruckType);

        Object.DestroyImmediate(model.gameObject);
    }

    [Test]
    public void TruckType_ReturnsBakery_WhenModelIsBakeryTruck()
    {
        BakeryTruck model = new GameObject("BakeryTruckModel").AddComponent<BakeryTruck>();

        SetTruckModel(model);

        Assert.AreEqual(TruckType.BAKERY, truck.TruckType);

        Object.DestroyImmediate(model.gameObject);
    }
}