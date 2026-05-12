using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class PlayerProductionTests
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
    public void IndustryProduced_UpdatesInventory()
    {
        ResourceEnum resource = ResourceEnum.Wheat;

        ProducedEventArgs producedEventArgs = new ProducedEventArgs
        {
            Resouce = resource,
            Amount = 12
        };

        player.Industry_Produced(this, producedEventArgs);

        Assert.AreEqual(12, player.Resources[resource]);
    }

    [Test]
    public void IndustryProduced_FiresInventoryChangedEvent()
    {
        ResourceEnum resource = ResourceEnum.Wheat;

        bool eventFired = false;

        player.InventoryChanged += (sender, e) =>
        {
            eventFired = true;
            Assert.AreEqual(resource, e.Resource);
            Assert.AreEqual(12, e.NewAmount);
        };

        ProducedEventArgs producedEventArgs = new ProducedEventArgs
        {
            Resouce = resource,
            Amount = 12
        };

        player.Industry_Produced(this, producedEventArgs);

        Assert.IsTrue(eventFired, "Az InventoryChanged eseménynek le kellett volna futnia termelés után.");
    }
}