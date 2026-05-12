using NUnit.Framework;
using UnityEngine;
using System.Reflection;
using Assets._00_Scripts.locations.industries;

[TestFixture]
public class IndustryRecipeTests
{
    [Test]
    public void Farm_AcceptsNoInput_AndProducesWheat()
    {
        Farm farm = CreateAndStartIndustry<Farm>("TestFarm");

        Assert.AreEqual(StopType.Farm, farm.Type);

        Assert.IsFalse(farm.Accepts(ResourceEnum.Wheat));
        Assert.IsFalse(farm.Accepts(ResourceEnum.Water));
        Assert.IsFalse(farm.Accepts(ResourceEnum.Coal));

        Assert.IsTrue(farm.Produces(ResourceEnum.Wheat));
        Assert.IsFalse(farm.Produces(ResourceEnum.Flour));

        DestroyIndustry(farm);
    }

    [Test]
    public void Well_AcceptsNoInput_AndProducesWater()
    {
        Well well = CreateAndStartIndustry<Well>("TestWell");

        Assert.AreEqual(StopType.Water, well.Type);

        Assert.IsFalse(well.Accepts(ResourceEnum.Water));
        Assert.IsFalse(well.Accepts(ResourceEnum.Wheat));
        Assert.IsFalse(well.Accepts(ResourceEnum.Coal));

        Assert.IsTrue(well.Produces(ResourceEnum.Water));
        Assert.IsFalse(well.Produces(ResourceEnum.Wheat));

        DestroyIndustry(well);
    }

    [Test]
    public void CoalMine_AcceptsNoInput_AndProducesCoal()
    {
        CoalMine coalMine = CreateAndStartIndustry<CoalMine>("TestCoalMine");

        Assert.AreEqual(StopType.Coal, coalMine.Type);

        Assert.IsFalse(coalMine.Accepts(ResourceEnum.Coal));
        Assert.IsFalse(coalMine.Accepts(ResourceEnum.Water));
        Assert.IsFalse(coalMine.Accepts(ResourceEnum.Wheat));

        Assert.IsTrue(coalMine.Produces(ResourceEnum.Coal));
        Assert.IsFalse(coalMine.Produces(ResourceEnum.Iron));

        DestroyIndustry(coalMine);
    }

    [Test]
    public void IronMine_AcceptsNoInput_AndProducesIron()
    {
        IronMine ironMine = CreateAndStartIndustry<IronMine>("TestIronMine");

        Assert.AreEqual(StopType.IronOre, ironMine.Type);

        Assert.IsFalse(ironMine.Accepts(ResourceEnum.Iron));
        Assert.IsFalse(ironMine.Accepts(ResourceEnum.Coal));
        Assert.IsFalse(ironMine.Accepts(ResourceEnum.Water));

        Assert.IsTrue(ironMine.Produces(ResourceEnum.Iron));
        Assert.IsFalse(ironMine.Produces(ResourceEnum.IronBar));

        DestroyIndustry(ironMine);
    }

    [Test]
    public void GoldMine_AcceptsNoInput_AndProducesGold()
    {
        GoldMine goldMine = CreateAndStartIndustry<GoldMine>("TestGoldMine");

        Assert.AreEqual(StopType.GoldOre, goldMine.Type);

        Assert.IsFalse(goldMine.Accepts(ResourceEnum.Gold));
        Assert.IsFalse(goldMine.Accepts(ResourceEnum.Coal));
        Assert.IsFalse(goldMine.Accepts(ResourceEnum.Water));

        Assert.IsTrue(goldMine.Produces(ResourceEnum.Gold));
        Assert.IsFalse(goldMine.Produces(ResourceEnum.GoldBar));

        DestroyIndustry(goldMine);
    }

    [Test]
    public void Mill_AcceptsWheat_AndProducesFlour()
    {
        Mill mill = CreateAndStartIndustry<Mill>("TestMill");

        Assert.AreEqual(StopType.Flour, mill.Type);

        Assert.IsTrue(mill.Accepts(ResourceEnum.Wheat));
        Assert.IsFalse(mill.Accepts(ResourceEnum.Flour));
        Assert.IsFalse(mill.Accepts(ResourceEnum.Water));

        Assert.IsTrue(mill.Produces(ResourceEnum.Flour));
        Assert.IsFalse(mill.Produces(ResourceEnum.Wheat));

        DestroyIndustry(mill);
    }

    [Test]
    public void Bakery_AcceptsFlourAndWater_AndProducesBread()
    {
        Bakery bakery = CreateAndStartIndustry<Bakery>("TestBakery");

        Assert.AreEqual(StopType.Bakery, bakery.Type);

        Assert.IsTrue(bakery.Accepts(ResourceEnum.Flour));
        Assert.IsTrue(bakery.Accepts(ResourceEnum.Water));
        Assert.IsFalse(bakery.Accepts(ResourceEnum.Bread));
        Assert.IsFalse(bakery.Accepts(ResourceEnum.Wheat));

        Assert.IsTrue(bakery.Produces(ResourceEnum.Bread));
        Assert.IsFalse(bakery.Produces(ResourceEnum.Flour));
        Assert.IsFalse(bakery.Produces(ResourceEnum.Water));

        DestroyIndustry(bakery);
    }

    [Test]
    public void IronSmelter_AcceptsIronAndCoal_AndProducesIronBar()
    {
        IronSmelter ironSmelter = CreateAndStartIndustry<IronSmelter>("TestIronSmelter");

        Assert.AreEqual(StopType.IronBar, ironSmelter.Type);

        Assert.IsTrue(ironSmelter.Accepts(ResourceEnum.Iron));
        Assert.IsTrue(ironSmelter.Accepts(ResourceEnum.Coal));
        Assert.IsFalse(ironSmelter.Accepts(ResourceEnum.IronBar));
        Assert.IsFalse(ironSmelter.Accepts(ResourceEnum.Water));

        Assert.IsTrue(ironSmelter.Produces(ResourceEnum.IronBar));
        Assert.IsFalse(ironSmelter.Produces(ResourceEnum.Iron));
        Assert.IsFalse(ironSmelter.Produces(ResourceEnum.Coal));

        DestroyIndustry(ironSmelter);
    }

    [Test]
    public void GoldSmelter_AcceptsGoldAndCoal_AndProducesGoldBar()
    {
        GoldSmelter goldSmelter = CreateAndStartIndustry<GoldSmelter>("TestGoldSmelter");

        Assert.AreEqual(StopType.GoldBar, goldSmelter.Type);

        Assert.IsTrue(goldSmelter.Accepts(ResourceEnum.Gold));
        Assert.IsTrue(goldSmelter.Accepts(ResourceEnum.Coal));
        Assert.IsFalse(goldSmelter.Accepts(ResourceEnum.GoldBar));
        Assert.IsFalse(goldSmelter.Accepts(ResourceEnum.Water));

        Assert.IsTrue(goldSmelter.Produces(ResourceEnum.GoldBar));
        Assert.IsFalse(goldSmelter.Produces(ResourceEnum.Gold));
        Assert.IsFalse(goldSmelter.Produces(ResourceEnum.Coal));

        DestroyIndustry(goldSmelter);
    }

    [Test]
    public void Mint_AcceptsGoldBar_AndProducesCoin()
    {
        Mint mint = CreateAndStartIndustry<Mint>("TestMint");

        Assert.AreEqual(StopType.Mint, mint.Type);

        Assert.IsTrue(mint.Accepts(ResourceEnum.GoldBar));
        Assert.IsFalse(mint.Accepts(ResourceEnum.Gold));
        Assert.IsFalse(mint.Accepts(ResourceEnum.Coin));
        Assert.IsFalse(mint.Accepts(ResourceEnum.Coal));

        Assert.IsTrue(mint.Produces(ResourceEnum.Coin));
        Assert.IsFalse(mint.Produces(ResourceEnum.GoldBar));
        Assert.IsFalse(mint.Produces(ResourceEnum.Gold));

        DestroyIndustry(mint);
    }


    private static T CreateAndStartIndustry<T>(string objectName) where T : Industry
    {
        GameObject gameObject = new GameObject(objectName);
        T industry = gameObject.AddComponent<T>();

        InvokeStart(industry);

        return industry;
    }

    private static void DestroyIndustry(Industry industry)
    {
        Object.DestroyImmediate(industry.gameObject);
    }

    private static void InvokeStart(Industry industry)
    {
        MethodInfo startMethod = FindStartMethod(industry.GetType());
        startMethod.Invoke(industry, null);
    }

    private static MethodInfo FindStartMethod(System.Type type)
    {
        while (type != null)
        {
            MethodInfo method = type.GetMethod(
                "Start",
                BindingFlags.Instance |
                BindingFlags.NonPublic |
                BindingFlags.Public |
                BindingFlags.DeclaredOnly
            );

            if (method != null)
            {
                return method;
            }

            type = type.BaseType;
        }

        Assert.Fail("Start method was not found on Industry type hierarchy.");
        return null;
    }
}