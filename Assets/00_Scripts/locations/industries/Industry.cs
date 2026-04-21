using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Industry : MonoBehaviour, ILocation
{
    public Vector3 Position => transform.position;
    public abstract StopType Type { get; }
    [SerializeField] protected IndustryModel model;

    [Header("Production")]
    [SerializeField] protected float productivityFactor = 1f;
    [SerializeField] protected float productivityTarget = 1f;
    [SerializeField] protected float productivityChangeInterval = 20f;
    [SerializeField] protected float productivityLerpSpeed = 0.05f;

    [Header("Storage")]
    [SerializeField] protected int defaultStorageCapacity = 50;

    [Header("Workers")]
    [SerializeField] protected int maxWorkers = 10;
    [SerializeField] protected int currentWorkers = 10; // csak azért nem 0, hogy lehessen vizsgálni a termelékenységet buszok nélkül

    protected Dictionary<ResourceEnum, int> inventory = new();
    protected float productionTimer;
    protected float productivityTimer;

    protected Recipe recipe;

    public virtual List<Vector3> GetAllBuildingPositions()
    {
        if (model == null)
        {
            Debug.LogWarning($"{name}: no IndustryModel assigned, using transform.position.");
            return new List<Vector3> { transform.position };
        }

        return model.GetAllBuildingPositions();
    }
    protected virtual void Start()
    {
        InitializeRecipe();
    }

    protected virtual void Update()
    {
        UpdateProductivity(Time.deltaTime);
        UpdateProduction(Time.deltaTime);
    }

    protected abstract void InitializeRecipe();

    protected virtual void UpdateProductivity(float deltaTime)
    {
        productivityTimer += deltaTime;

        if (productivityTimer >= productivityChangeInterval)
        {
            productivityTimer = 0f;
            productivityTarget = Random.Range(0.7f, 1.3f);
        }

        productivityFactor = Mathf.MoveTowards(
            productivityFactor,
            productivityTarget,
            productivityLerpSpeed * deltaTime);
    }

    protected virtual void UpdateProduction(float deltaTime)
    {
        if (recipe == null)
        {
            return;
        }

        if (!HasRequiredInputs() || !HasOutputCapacity())
        {
            return;
        }

        float workerFactor = GetWorkerFactor();
        float effectiveProductivity = productivityFactor * workerFactor;

        if (effectiveProductivity <= 0f)
        {
            return;
        }

        productionTimer += deltaTime * effectiveProductivity;

        if (productionTimer >= recipe.CycleTime)
        {
            productionTimer -= recipe.CycleTime;
            ConsumeInputs();
            ProduceOutputs();

            //Debug.Log($"{name} produced: {string.Join(", ", recipe.Outputs.Select(o => $"{o.Key} x{o.Value}"))}");
        }
    }

    protected virtual float GetWorkerFactor()
    {
        if (maxWorkers <= 0)
        {
            return 1f;
        }

        return Mathf.Clamp01((float)currentWorkers / maxWorkers);
    }

    protected bool HasRequiredInputs()
    {
        foreach (var input in recipe.Inputs)
        {
            if (GetStoredAmount(input.Key) < input.Value)
            {
                return false;
            }
        }

        return true;
    }

    protected bool HasOutputCapacity()
    {
        foreach (var output in recipe.Outputs)
        {
            if (GetStoredAmount(output.Key) + output.Value > defaultStorageCapacity)
            {
                return false;
            }
        }

        return true;
    }

    protected void ConsumeInputs()
    {
        foreach (var input in recipe.Inputs)
        {
            inventory[input.Key] -= input.Value;
        }
    }

    protected void ProduceOutputs()
    {
        foreach (var output in recipe.Outputs)
        {
            AddResource(output.Key, output.Value);
        }
    }

    protected void AddResource(ResourceEnum type, int amount)
    {
        if (type == ResourceEnum.None || amount <= 0)
        {
            return;
        }

        if (!inventory.ContainsKey(type))
        {
            inventory[type] = 0;
        }

        inventory[type] += amount;
        //Debug.Log(inventory[type]);
    }

    protected bool RemoveResource(ResourceEnum type, int amount)
    {
        if (type == ResourceEnum.None || amount <= 0)
        {
            return false;
        }

        if (!inventory.ContainsKey(type) || inventory[type] < amount)
        {
            return false;
        }

        inventory[type] -= amount;
        return true;
    }

    public virtual int GetStoredAmount(ResourceEnum type)
    {
        if (!inventory.ContainsKey(type))
        {
            return 0;
        }

        return inventory[type];
    }

    public virtual bool Accepts(ResourceEnum type)
    {
        return recipe != null && recipe.Inputs.ContainsKey(type);
    }

    public virtual bool Produces(ResourceEnum type)
    {
        return recipe != null && recipe.Outputs.ContainsKey(type);
    }

    public virtual int Accept(ResourceEnum type, int amount)
    {
        if (!Accepts(type) || amount <= 0)
        {
            return 0;
        }

        int current = GetStoredAmount(type);
        int freeSpace = defaultStorageCapacity - current;
        int accepted = Mathf.Max(0, Mathf.Min(amount, freeSpace));

        if (accepted > 0)
        {
            AddResource(type, accepted);
        }

        return accepted;
    }

    public virtual int Pickup(ResourceEnum type, int amount)
    {
        if (!Produces(type) || amount <= 0)
        {
            return 0;
        }

        int available = GetStoredAmount(type);
        int pickedUp = Mathf.Min(amount, available);

        if (pickedUp > 0)
        {
            RemoveResource(type, pickedUp);
        }

        return pickedUp;
    }

    #region WorkerMethods
    public virtual int AddWorkers(int amount)
    {
        if (amount <= 0)
        {
            return 0;
        }

        int accepted = Mathf.Min(amount, maxWorkers - currentWorkers);
        currentWorkers += accepted;
        return accepted;
    }

    public virtual int RemoveWorkers(int amount)
    {
        if (amount <= 0)
        {
            return 0;
        }

        int removed = Mathf.Min(amount, currentWorkers);
        currentWorkers -= removed;
        return removed;
    }


    public int GetMaxWorkers()
    {
        return maxWorkers;
    }

    public int GetCurrentWorkers()
    {
        return currentWorkers;
    }

    public int GetMissingWorkers()
    {
        return Mathf.Max(0, maxWorkers - currentWorkers);
    }
    #endregion
}
