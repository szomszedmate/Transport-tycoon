using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using System.Reflection;

[TestFixture]
public class IndustryStorageTests
{
    private TestIndustry industry;

    private class TestIndustry : Industry
    {
        public override StopType Type => (StopType)0;

        protected override void InitializeRecipe()
        {
        }

        public void SetRecipeForTest(Recipe testRecipe)
        {
            recipe = testRecipe;
        }

        public void SetWorkerValuesForTest(int current, int max)
        {
            FieldInfo currentWorkersField = typeof(Industry).GetField(
                "currentWorkers",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            FieldInfo maxWorkersField = typeof(Industry).GetField(
                "maxWorkers",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            currentWorkersField.SetValue(this, current);
            maxWorkersField.SetValue(this, max);
        }
    }

    [SetUp]
    public void SetUp()
    {
        GameObject gameObject = new GameObject("TestIndustry");
        industry = gameObject.AddComponent<TestIndustry>();

        Recipe recipe = new Recipe(
            new Dictionary<ResourceEnum, int>
            {
                { ResourceEnum.Wheat, 2 }
            },
            new Dictionary<ResourceEnum, int>
            {
                { ResourceEnum.Flour, 1 }
            },
            5f
        );

        industry.SetRecipeForTest(recipe);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(industry.gameObject);
    }

    [Test]
    public void Accepts_ReturnsTrue_ForInputResource()
    {
        Assert.IsTrue(industry.Accepts(ResourceEnum.Wheat));
    }

    [Test]
    public void Accepts_ReturnsFalse_ForNonInputResource()
    {
        Assert.IsFalse(industry.Accepts(ResourceEnum.Flour));
    }

    [Test]
    public void Produces_ReturnsTrue_ForOutputResource()
    {
        Assert.IsTrue(industry.Produces(ResourceEnum.Flour));
    }

    [Test]
    public void Produces_ReturnsFalse_ForNonOutputResource()
    {
        Assert.IsFalse(industry.Produces(ResourceEnum.Wheat));
    }

    [Test]
    public void Accept_AddsInputResource_ToInventory()
    {
        int accepted = industry.Accept(ResourceEnum.Wheat, 10);

        Assert.AreEqual(10, accepted);
        Assert.AreEqual(10, industry.GetStoredAmount(ResourceEnum.Wheat));
    }

    [Test]
    public void Accept_ReturnsZero_ForNonInputResource()
    {
        int accepted = industry.Accept(ResourceEnum.Flour, 10);

        Assert.AreEqual(0, accepted);
        Assert.AreEqual(0, industry.GetStoredAmount(ResourceEnum.Flour));
    }

    [Test]
    public void Accept_ReturnsZero_ForNegativeAmount()
    {
        int accepted = industry.Accept(ResourceEnum.Wheat, -5);

        Assert.AreEqual(0, accepted);
        Assert.AreEqual(0, industry.GetStoredAmount(ResourceEnum.Wheat));
    }

    [Test]
    public void Accept_DoesNotExceedDefaultStorageCapacity()
    {
        int accepted = industry.Accept(ResourceEnum.Wheat, 60);

        Assert.AreEqual(50, accepted);
        Assert.AreEqual(50, industry.GetStoredAmount(ResourceEnum.Wheat));
    }

    [Test]
    public void Pickup_RemovesOutputResource_FromInventory()
    {
        MethodInfo addResourceMethod = typeof(Industry).GetMethod(
            "AddResource",
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        addResourceMethod.Invoke(industry, new object[] { ResourceEnum.Flour, 8 });

        int pickedUp = industry.Pickup(ResourceEnum.Flour, 3);

        Assert.AreEqual(3, pickedUp);
        Assert.AreEqual(5, industry.GetStoredAmount(ResourceEnum.Flour));
    }

    [Test]
    public void Pickup_ReturnsZero_ForNonOutputResource()
    {
        industry.Accept(ResourceEnum.Wheat, 10);

        int pickedUp = industry.Pickup(ResourceEnum.Wheat, 3);

        Assert.AreEqual(0, pickedUp);
        Assert.AreEqual(10, industry.GetStoredAmount(ResourceEnum.Wheat));
    }

    [Test]
    public void Pickup_DoesNotRemoveMoreThanAvailable()
    {
        MethodInfo addResourceMethod = typeof(Industry).GetMethod(
            "AddResource",
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        addResourceMethod.Invoke(industry, new object[] { ResourceEnum.Flour, 4 });

        int pickedUp = industry.Pickup(ResourceEnum.Flour, 10);

        Assert.AreEqual(4, pickedUp);
        Assert.AreEqual(0, industry.GetStoredAmount(ResourceEnum.Flour));
    }

    [Test]
    public void WorkerQueries_ReturnCorrectValues()
    {
        industry.SetWorkerValuesForTest(current: 25, max: 100);

        Assert.AreEqual(75, industry.SpaceLeft());
        Assert.AreEqual(100, industry.GetMaxWorkers());
        Assert.AreEqual(25, industry.GetCurrentWorkers());
        Assert.AreEqual(75, industry.GetMissingWorkers());
    }

    [Test]
    public void RemoveWorkers_RemovesAtMostCurrentWorkers()
    {
        industry.SetWorkerValuesForTest(current: 10, max: 100);

        int removed = industry.RemoveWorkers(20);

        Assert.AreEqual(10, removed);
        Assert.AreEqual(0, industry.GetCurrentWorkers());
    }

    [Test]
    public void RemoveWorkers_ReturnsZero_ForNegativeAmount()
    {
        industry.SetWorkerValuesForTest(current: 10, max: 100);

        int removed = industry.RemoveWorkers(-5);

        Assert.AreEqual(0, removed);
        Assert.AreEqual(10, industry.GetCurrentWorkers());
    }
}