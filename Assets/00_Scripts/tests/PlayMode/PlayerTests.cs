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
        Assert.AreEqual(5000, player.Money, "A kezd� p�nznek 5000-nak kell lennie.");
        Assert.AreEqual(0, player.TaxToPay, "A kezd� ad�nak 0-nak kell lennie.");
        Assert.AreEqual(7, player.NextTaxDay, "Az els� ad�napnak a 7. napnak kell lennie.");
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
        player.IncreaseTax(100); // 500 p�nz, 100 ad�
        player.PayTaxes();

        Assert.AreEqual(400, player.Money, "A p�nzb�l le kellett volna vonni az ad�t.");
        Assert.AreEqual(0, player.TaxToPay, "Az ad�nak null�z�dnia kell fizet�s ut�n.");
        Assert.AreEqual(14, player.NextTaxDay, "A k�vetkez� ad�napnak 7 nappal k�s�bbre kell tol�dnia.");
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

    [Test]
    public void UpdateInventory_SetsResourceAmount_AndFiresInventoryChangedEvent()
    {
        ResourceEnum resource = ResourceEnum.Wheat;

        bool eventFired = false;

        player.InventoryChanged += (sender, e) =>
        {
            eventFired = true;
            Assert.AreEqual(resource, e.Resource);
            Assert.AreEqual(10, e.NewAmount);
        };

        player.UpdateInventory(resource, 10);

        Assert.AreEqual(10, player.Resources[resource]);
        Assert.IsTrue(eventFired, "Az InventoryChanged eventnek le kellett volna futnia.");
    }

    [Test]
    public void SellResource_IncreasesMoney_AndFiresInventoryChangedEvent()
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

        Assert.AreEqual(540, player.Money);
        Assert.IsTrue(eventFired);
    }
}