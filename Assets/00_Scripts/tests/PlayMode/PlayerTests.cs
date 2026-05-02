using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class PlayerTests
{
    private Player player;

    [SetUp]
    public void SetUp()
    {

        GameObject gameObject = new GameObject();
        player = gameObject.AddComponent<Player>();

        player.Start();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(player.gameObject);
    }

    [Test]
    public void InitialValues_AreCorrect()
    {
        Assert.AreEqual(500, player.Money, "A kezdõ pénznek 500-nak kell lennie.");
        Assert.AreEqual(0, player.TaxToPay, "A kezdõ adónak 0-nak kell lennie.");
        Assert.AreEqual(7, player.NextTaxDay, "Az elsõ adónapnak a 7. napnak kell lennie.");
    }

    [Test]
    public void AddMoney_IncreasesBalance()
    {
        player.AddMoney(100);
        Assert.AreEqual(600, player.Money);
    }

    [Test]
    public void LoseMoney_DecreasesBalance()
    {
        player.LoseMoney(100);
        Assert.AreEqual(400, player.Money);
    }

    [Test]
    public void CanAfford_ReturnsTrue_WhenEnoughMoney()
    {
        Assert.IsTrue(player.CanAfford(500));
        Assert.IsTrue(player.CanAfford(100));
    }

    [Test]
    public void CanAfford_ReturnsFalse_WhenNotEnoughMoney()
    {
        Assert.IsFalse(player.CanAfford(501));
    }

    [Test]
    public void IncreaseTax_IncreasesTaxAmount()
    {
        player.IncreaseTax(50);
        Assert.AreEqual(50, player.TaxToPay);
    }

    [Test]
    public void PayTaxes_DeductsMoney_AndResetsTax()
    {
        player.IncreaseTax(100); // 500 pénz, 100 adó
        player.PayTaxes();

        Assert.AreEqual(400, player.Money, "A pénzbõl le kellett volna vonni az adót.");
        Assert.AreEqual(0, player.TaxToPay, "Az adónak nullázódnia kell fizetés után.");
        Assert.AreEqual(14, player.NextTaxDay, "A következõ adónapnak 7 nappal késõbbre kell tolódnia.");
    }

    [Test]
    public void MoneyChanged_Event_Fires()
    {
        bool eventFired = false;
        player.MoneyChanged += (sender, e) => {
            eventFired = true;
            Assert.AreEqual(600, e.NewAmount);
        };

        player.AddMoney(100);
        Assert.IsTrue(eventFired, "A MoneyChanged eventnek le kellene futnia.");
    }
}