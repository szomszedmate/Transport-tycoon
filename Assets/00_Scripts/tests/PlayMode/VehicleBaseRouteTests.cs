using System.Collections;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class VehicleBaseRouteTests
{
    private TestVehicle vehicle;

    private class TestVehicle : VehicleBase
    {
        public void InitializeForTest()
        {
            Route = new System.Collections.Generic.List<Road>();
        }

        public void ConfirmRouteForTest()
        {
            RouteConfirmed = true;
        }

        public override IEnumerator OnArrivedAtStop(BusStop stop)
        {
            yield break;
        }
    }

    [SetUp]
    public void SetUp()
    {
        GameObject gameObject = new GameObject("TestVehicle");
        vehicle = gameObject.AddComponent<TestVehicle>();
        vehicle.InitializeForTest();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(vehicle.gameObject);
    }

    [Test]
    public void GetLast_ReturnsNull_WhenRouteIsEmpty()
    {
        Road lastRoad = vehicle.GetLast();

        Assert.IsNull(lastRoad);
    }

    [Test]
    public void AddToRoute_AddsRoad_WhenRouteIsNotConfirmed()
    {
        Road road = new GameObject("Road").AddComponent<Road>();

        bool result = vehicle.AddToRoute(road);

        Assert.IsTrue(result);
        Assert.AreEqual(road, vehicle.GetLast());

        Object.DestroyImmediate(road.gameObject);
    }

    [Test]
    public void AddToRoute_ReturnsFalse_WhenRoadIsAlreadyInRoute()
    {
        Road road = new GameObject("Road").AddComponent<Road>();

        vehicle.AddToRoute(road);
        bool result = vehicle.AddToRoute(road);

        Assert.IsFalse(result);

        Object.DestroyImmediate(road.gameObject);
    }

    [Test]
    public void AddToRoute_ReturnsFalse_WhenRouteIsConfirmed()
    {
        Road road = new GameObject("Road").AddComponent<Road>();
        vehicle.ConfirmRouteForTest();

        bool result = vehicle.AddToRoute(road);

        Assert.IsFalse(result);
        Assert.IsNull(vehicle.GetLast());

        Object.DestroyImmediate(road.gameObject);
    }

    [Test]
    public void RemFromRoute_RemovesRoad_WhenRoadIsLast()
    {
        Road firstRoad = new GameObject("FirstRoad").AddComponent<Road>();
        Road secondRoad = new GameObject("SecondRoad").AddComponent<Road>();

        vehicle.AddToRoute(firstRoad);
        vehicle.AddToRoute(secondRoad);

        bool result = vehicle.RemFromRoute(secondRoad);

        Assert.IsTrue(result);
        Assert.AreEqual(firstRoad, vehicle.GetLast());

        Object.DestroyImmediate(firstRoad.gameObject);
        Object.DestroyImmediate(secondRoad.gameObject);
    }

    [Test]
    public void RemFromRoute_ReturnsFalse_WhenRoadIsNotLast()
    {
        Road firstRoad = new GameObject("FirstRoad").AddComponent<Road>();
        Road secondRoad = new GameObject("SecondRoad").AddComponent<Road>();

        vehicle.AddToRoute(firstRoad);
        vehicle.AddToRoute(secondRoad);

        bool result = vehicle.RemFromRoute(firstRoad);

        Assert.IsFalse(result);
        Assert.AreEqual(secondRoad, vehicle.GetLast());

        Object.DestroyImmediate(firstRoad.gameObject);
        Object.DestroyImmediate(secondRoad.gameObject);
    }

    [Test]
    public void RemFromRoute_ReturnsFalse_WhenRouteIsConfirmed()
    {
        Road road = new GameObject("Road").AddComponent<Road>();

        vehicle.AddToRoute(road);
        vehicle.ConfirmRouteForTest();

        bool result = vehicle.RemFromRoute(road);

        Assert.IsFalse(result);
        Assert.AreEqual(road, vehicle.GetLast());

        Object.DestroyImmediate(road.gameObject);
    }

    [Test]
    public void ChangeState_UpdatesVehicleState()
    {
        vehicle.ChangeState(VehicleBase.VehicleState.SELECTHOVER);

        Assert.AreEqual(VehicleBase.VehicleState.SELECTHOVER, vehicle.State);
    }

    [Test]
    public void Sold_FiresSoldEvent()
    {
        bool eventFired = false;

        vehicle.sold += (sender, e) =>
        {
            eventFired = true;
        };

        vehicle.Sold();

        Assert.IsTrue(eventFired);
    }
}