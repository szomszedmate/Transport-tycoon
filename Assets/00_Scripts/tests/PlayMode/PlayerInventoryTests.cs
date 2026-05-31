using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class PlayerInventoryTests
{
    private Player player;

    [SetUp]
    public void SetUp()
    {
        GameObject gameObject = new GameObject("TestPlayer");
        player = gameObject.AddComponent<Player>();
        player.Start();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(player.gameObject);
    }

    [Test]
    public void UpdateInventory_OverwritesPreviousResourceAmount()
    {
        ResourceEnum resource = ResourceEnum.Wheat;

        player.UpdateInventory(resource, 10);
        player.UpdateInventory(resource, 25);

        Assert.AreEqual(25, player.Resources[resource]);
    }

    [Test]
public void SellResource_FiresInventoryChangedEvent_WithNegativeAmount_WhenResourceIsSold()
{
    ResourceEnum resource = ResourceEnum.Wheat;
    player.UpdateInventory(resource, 10);

    bool eventFired = false;
    player.InventoryChanged += (sender, e) =>
    {
        eventFired = true;
        Assert.AreEqual(resource, e.Resource);
        Assert.AreEqual(-4, e.NewAmount);
    };

    player.SellResource(resource, 4);

    Assert.IsTrue(eventFired, "Az InventoryChanged eseménynek le kellett volna futnia eladáskor.");
}


    [Test]
    public void SellResource_FiresInventoryChangedEvent_WhenResourceIsSold()
    {
        ResourceEnum resource = ResourceEnum.Wheat;

        player.UpdateInventory(resource, 10);

        bool eventFired = false;

        player.InventoryChanged += (sender, e) =>
        {
            eventFired = true;
            Assert.AreEqual(resource, e.Resource);
            Assert.AreEqual(-4, e.NewAmount);
        };

        player.SellResource(resource, 4);

        Assert.IsTrue(eventFired, "Az InventoryChanged eseménynek le kellett volna futnia eladáskor.");
    }
}