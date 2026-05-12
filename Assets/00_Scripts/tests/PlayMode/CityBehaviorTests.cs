using NUnit.Framework;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

[TestFixture]
public class CityBehaviorTests
{
    private City city;

    [SetUp]
    public void SetUp()
    {
        GameObject gameObject = new GameObject("TestCity");
        city = gameObject.AddComponent<City>();

        SetPrivateField(city, "population", 8);

        InvokeStart(city);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(city.gameObject);
    }

    [Test]
    public void Type_ReturnsUniversal()
    {
        Assert.AreEqual(StopType.Universal, city.Type);
    }

    [Test]
    public void Position_ReturnsTransformPosition()
    {
        city.transform.position = new Vector3(3, 0, 7);

        Assert.AreEqual(new Vector3(3, 0, 7), city.Position);
    }

    [Test]
    public void Start_DividesPopulationIntoShifts()
    {
        Assert.AreEqual(3, city.DayShift.Count);
        Assert.AreEqual(3, city.EveningShift.Count);
        Assert.AreEqual(2, city.NightShift.Count);
    }

    [Test]
    public void Load_RemovesWorkerFromSelectedShift()
    {
        Worker worker = city.Load(DayPhase.DAY);

        Assert.IsNotNull(worker);
        Assert.AreEqual(2, city.DayShift.Count);
    }

    [Test]
    public void Load_ReturnsNull_WhenSelectedShiftIsEmpty()
    {
        city.Load(DayPhase.NIGHT);
        city.Load(DayPhase.NIGHT);

        Worker worker = city.Load(DayPhase.NIGHT);

        Assert.IsNull(worker);
        Assert.AreEqual(0, city.NightShift.Count);
    }

    [Test]
    public void Unload_AddsWorkerBackToItsShift()
    {
        Worker worker = city.Load(DayPhase.EVENING);

        city.Unload(worker);

        Assert.AreEqual(3, city.EveningShift.Count);
    }

    [Test]
    public void RegisterWaitingBus_AddsBusOnlyOnce()
    {
        Bus bus = new GameObject("TestBus").AddComponent<Bus>();

        city.RegisterWaitingBus(bus);
        city.RegisterWaitingBus(bus);

        Assert.AreEqual(1, city.buses.Count);

        Object.DestroyImmediate(bus.gameObject);
    }


    [Test]
    public void GetAllBuildingPositions_ReturnsEmptyList_WhenModelIsNull()
    {
        List<Vector3> positions = city.GetAllBuildingPositions();

        Assert.IsNotNull(positions);
        Assert.AreEqual(0, positions.Count);
    }


    private static void InvokeStart(City city)
    {
        MethodInfo startMethod = typeof(City).GetMethod(
            "Start",
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        startMethod.Invoke(city, null);
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        field.SetValue(target, value);
    }
}