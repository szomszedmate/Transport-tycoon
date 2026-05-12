using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

[TestFixture]
public class BusStopBehaviorTests
{
    private BusStop busStop;

    [SetUp]
    public void SetUp()
    {
        GameObject gameObject = new GameObject("TestBusStop");
        busStop = gameObject.AddComponent<BusStop>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(busStop.gameObject);
    }

    [Test]
    public void BuildCategory_ReturnsBusStop()
    {
        Assert.AreEqual(BuildCategory.BUSSTOP, busStop.BuildCategory);
    }

    [Test]
    public void IsCityStop_ReturnsFalse_WhenLocationIsNull()
    {
        busStop.location = null;

        Assert.IsFalse(busStop.IsCityStop());
    }

    [Test]
    public void IsCityStop_ReturnsTrue_WhenLocationIsCity()
    {
        City city = new GameObject("TestCity").AddComponent<City>();

        busStop.location = city;

        Assert.IsTrue(busStop.IsCityStop());

        Object.DestroyImmediate(city.gameObject);
    }

    [Test]
    public void IsCityStop_ReturnsFalse_WhenLocationIsNotCity()
    {
        TestLocation location = new GameObject("TestLocation").AddComponent<TestLocation>();

        busStop.location = location;

        Assert.IsFalse(busStop.IsCityStop());

        Object.DestroyImmediate(location.gameObject);
    }

    [Test]
    public void CityProperty_CanStoreCityReference()
    {
        City city = new GameObject("TestCity").AddComponent<City>();

        busStop.City = city;

        Assert.AreEqual(city, busStop.City);

        Object.DestroyImmediate(city.gameObject);
    }

    private class TestLocation : MonoBehaviour, ILocation
    {
        public Vector3 Position => transform.position;
        public StopType Type => StopType.None;

        public bool Accepts(ResourceEnum resource) => false;
        public bool Produces(ResourceEnum resource) => false;
        public int Accept(ResourceEnum resource, int amount) => 0;
        public int Pickup(ResourceEnum resource, int amount) => 0;
        public int GetStoredAmount(ResourceEnum resource) => 0;

        public List<Vector3> GetAllBuildingPositions()
        {
            return new List<Vector3>();
        }
    }
}