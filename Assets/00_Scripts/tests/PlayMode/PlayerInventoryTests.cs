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
        ResourceEnum resource = (ResourceEnum)System.Enum.GetValues(typeof(ResourceEnum)).GetValue(0);

        player.UpdateInventory(resource, 10);
        player.UpdateInventory(resource, 25);

        Assert.AreEqual(25, player.Resources[resource]);
    }

    [Test]
    public void SellResource_DoesNotChangeInventory_WhenNotEnoughResource()
    {
        ResourceEnum resource = (ResourceEnum)System.Enum.GetValues(typeof(ResourceEnum)).GetValue(0);

        player.UpdateInventory(resource, 3);

        player.SellResource(resource, 5);

        Assert.AreEqual(3, player.Resources[resource]);
        Assert.AreEqual(500, player.Money);
    }

    [Test]
    public void SellResource_FiresInventoryChangedEvent_WhenResourceIsSold()
    {
        ResourceEnum resource = (ResourceEnum)System.Enum.GetValues(typeof(ResourceEnum)).GetValue(0);

        player.UpdateInventory(resource, 10);

        bool eventFired = false;

        player.InventoryChanged += (sender, e) =>
        {
            eventFired = true;
            Assert.AreEqual(resource, e.Resource);
            Assert.AreEqual(4, e.NewAmount);
        };

        player.SellResource(resource, 4);

        Assert.IsTrue(eventFired, "Az InventoryChanged eseménynek le kellett volna futnia eladáskor.");
    }
}