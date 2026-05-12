using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class PlayerMoneyEventTests
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
    public void AddMoney_FiresMoneyChangedEvent_WithNewAmount()
    {
        bool eventFired = false;

        player.MoneyChanged += (sender, e) =>
        {
            eventFired = true;
            Assert.AreEqual(650, e.NewAmount);
        };

        player.AddMoney(150);

        Assert.IsTrue(eventFired, "A MoneyChanged eseménynek le kellett volna futnia pénz hozzáadásakor.");
    }

    [Test]
    public void LoseMoney_FiresMoneyChangedEvent_WithNewAmount()
    {
        bool eventFired = false;

        player.MoneyChanged += (sender, e) =>
        {
            eventFired = true;
            Assert.AreEqual(300, e.NewAmount);
        };

        player.LoseMoney(200);

        Assert.IsTrue(eventFired, "A MoneyChanged eseménynek le kellett volna futnia pénz levonásakor.");
    }

    [Test]
    public void MultipleMoneyChanges_ResultInCorrectFinalBalance()
    {
        player.AddMoney(100);
        player.LoseMoney(50);
        player.AddMoney(25);

        Assert.AreEqual(575, player.Money);
    }
}