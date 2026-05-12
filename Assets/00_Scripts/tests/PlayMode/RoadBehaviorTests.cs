using NUnit.Framework;
using UnityEngine;
using System.Reflection;

[TestFixture]
public class RoadBehaviorTests
{
    private Road road;

    [SetUp]
    public void SetUp()
    {
        GameObject gameObject = new GameObject("TestRoad");
        road = gameObject.AddComponent<Road>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(road.gameObject);
    }

    [Test]
    public void NewRoad_HasBuiltStateByDefault()
    {
        Assert.AreEqual(RoadState.BUILT, road.State);
    }

    [Test]
    public void BuildCategory_ReturnsRoad()
    {
        Assert.AreEqual(BuildCategory.ROAD, road.BuildCategory);
    }

    [Test]
    public void Type_ReturnsUnknown_WhenModelIsNull()
    {
        road.Model = null;

        Assert.AreEqual(RoadType.UNKNOWN, road.Type);
    }

    [Test]
    public void Type_ReturnsStraight_WhenModelIsRoadStraightModel()
    {
        RoadStraightModel model = new GameObject("RoadStraightModel").AddComponent<RoadStraightModel>();

        road.Model = model;

        Assert.AreEqual(RoadType.STRAIGHT, road.Type);

        Object.DestroyImmediate(model.gameObject);
    }

    [Test]
    public void Type_ReturnsTurn_WhenModelIsRoadLModel()
    {
        RoadLModel model = new GameObject("RoadLModel").AddComponent<RoadLModel>();

        road.Model = model;

        Assert.AreEqual(RoadType.TURN, road.Type);

        Object.DestroyImmediate(model.gameObject);
    }

    [Test]
    public void Type_ReturnsT_WhenModelIsRoadTModel()
    {
        RoadTModel model = new GameObject("RoadTModel").AddComponent<RoadTModel>();

        road.Model = model;

        Assert.AreEqual(RoadType.T, road.Type);

        Object.DestroyImmediate(model.gameObject);
    }

    [Test]
    public void Type_ReturnsCross_WhenModelIsRoadCrossModel()
    {
        RoadCrossModel model = new GameObject("RoadCrossModel").AddComponent<RoadCrossModel>();

        road.Model = model;

        Assert.AreEqual(RoadType.CROSS, road.Type);

        Object.DestroyImmediate(model.gameObject);
    }

    [Test]
    public void ChangeState_UpdatesState()
    {
        road.ChangeState(RoadState.SELECTED);

        Assert.AreEqual(RoadState.SELECTED, road.State);
    }

    [Test]
    public void ChangeState_DoesNothing_WhenNewStateIsSameAsCurrentState()
    {
        road.ChangeState(RoadState.BUILT);

        Assert.AreEqual(RoadState.BUILT, road.State);
    }

    [Test]
    public void ChangeState_DoesNotSetDestroyHover_WhenRoadIsCityRoad()
    {
        SetIsCityRoad(true);

        road.ChangeState(RoadState.DESTROYHOVER);

        Assert.AreEqual(RoadState.BUILT, road.State);
    }

    [Test]
    public void RoadHasBusStop_ReturnsFalse_WhenBusStopIsNull()
    {
        Assert.IsFalse(road.Road_HasBusStop());
    }

    [Test]
    public void GetStopType_ReturnsNone_WhenBusStopIsNull()
    {
        Assert.AreEqual(StopType.None, road.GetStopType());
    }

    [Test]
    public void Kind_ReturnsNormalRoad_WhenDataIsNull()
    {
        Assert.AreEqual(RoadKind.NormalRoad, road.Kind);
        Assert.IsFalse(road.IsBridge);
    }

    private void SetIsCityRoad(bool value)
    {
        FieldInfo field = typeof(Road).GetField(
            "isCityRoad",
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        field.SetValue(road, value);
    }
}