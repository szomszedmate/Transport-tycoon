using NUnit.Framework;
using UnityEngine;
using System.Reflection;

[TestFixture]
public class BusTypeTests
{
    private Bus bus;

    [SetUp]
    public void SetUp()
    {
        GameObject gameObject = new GameObject("TestBus");
        bus = gameObject.AddComponent<Bus>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(bus.gameObject);
    }

    private void SetBusModel(VehicleModel model)
    {
        FieldInfo modelField = typeof(VehicleBase).GetField(
            "model",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
        );

        modelField.SetValue(bus, model);
    }

    [Test]
    public void BuildCategory_ReturnsBus()
    {
        Assert.AreEqual(BuildCategory.BUS, bus.BuildCategory);
    }

    [Test]
    public void BusType_ReturnsUnknown_WhenModelIsNull()
    {
        SetBusModel(null);

        Assert.AreEqual(BusType.UNKNOWN, bus.BusType);
    }

    [Test]
    public void BusType_ReturnsBasic_WhenModelIsBusBasicModel()
    {
        BusBasicModel model = new GameObject("BusBasicModel").AddComponent<BusBasicModel>();

        SetBusModel(model);

        Assert.AreEqual(BusType.BASIC, bus.BusType);

        Object.DestroyImmediate(model.gameObject);
    }

    [Test]
    public void BusType_ReturnsAdvanced_WhenModelIsBusAdvancedModel()
    {
        BusAdvancedModel model = new GameObject("BusAdvancedModel").AddComponent<BusAdvancedModel>();

        SetBusModel(model);

        Assert.AreEqual(BusType.ADVANCED, bus.BusType);

        Object.DestroyImmediate(model.gameObject);
    }

    [Test]
    public void BusType_ReturnsPremium_WhenModelIsBusPremiumModel()
    {
        BusPremiumModel model = new GameObject("BusPremiumModel").AddComponent<BusPremiumModel>();

        SetBusModel(model);

        Assert.AreEqual(BusType.PREMIUM, bus.BusType);

        Object.DestroyImmediate(model.gameObject);
    }
}