using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public abstract class Industry : MonoBehaviour, ILocation
{
    public event System.EventHandler<GetPhaseTimesEventArgs> GetPhaseTimes;
    public event System.EventHandler<GetTimeEventArgs> GetTime;
    public event System.EventHandler<ProducedEventArgs> Produced;
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
    [SerializeField] private int inputStorage;
    [SerializeField] private int outputStorage;

    [Header("Workers")]
    [SerializeField] protected int maxWorkers = 100;
    [SerializeField] protected int currentWorkers = 10; // csak az�rt nem 0, hogy lehessen vizsg�lni a termel�kenys�get buszok n�lk�l
    public List<Shift> shifts;
    public List<Worker> waitingForBus;
    public Shift DayShift; // ezt a 3at hasznaljak a linear munkasbuszok
    public Shift EveningShift;
    public Shift NightShift;

    [Header("Terrain")]
    [SerializeField] private GameObject visual;
    public GameObject Visual => visual;

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

        shifts = new List<Shift>();
        productionTimer = 0f;
        waitingForBus = new List<Worker>();
        SetupMainShifts();
    }

    private void SetupMainShifts()
    {
        GetPhaseTimesEventArgs e = new GetPhaseTimesEventArgs();
        GetPhaseTimes?.Invoke(this, e);

        DayShift = Shift.CreateNewShift(-1, e.DayStart, e.EveningStart, new List<Worker>());
        DayShift.enabled = false;
        DayShift.WorkerChanged += NewShift_WorkerChanged;
        shifts.Add(DayShift);

        EveningShift = Shift.CreateNewShift(-1, e.EveningStart, e.NightStart, new List<Worker>());
        EveningShift.enabled = false;
        EveningShift.WorkerChanged += NewShift_WorkerChanged;
        shifts.Add(EveningShift);

        NightShift = Shift.CreateNewShift(-1, e.NightStart, e.DayStart, new List<Worker>());
        NightShift.enabled = false;
        NightShift.WorkerChanged += NewShift_WorkerChanged;
        shifts.Add(NightShift);

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
        //Debug.Log("Prod time: " + productionTimer + ", cycle time: " + recipe.CycleTime + " workerFactor: " + workerFactor + ", name: " + name);
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
        UpdateStorageDebug();
    }

    protected void ProduceOutputs()
    {
        foreach (var output in recipe.Outputs)
        {
            AddResource(output.Key, output.Value);
            Produced?.Invoke(this, new ProducedEventArgs { Resouce = output.Key, Amount = output.Value });
        }
        UpdateStorageDebug();
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
    }

    public bool TakeResource(ResourceEnum type, int amount) => RemoveResource(type, amount);

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
        UpdateStorageDebug();
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
        UpdateStorageDebug();
        return pickedUp;
    }

    #region WorkerMethods
    public virtual void AddWorkers(List<Worker> workers, bool scheduled, DayPhase dayPhase)
    {
        
        
        if (scheduled)
        {
            switch (dayPhase)
            {
                case (DayPhase.DAY):
                    DayShift.RefillShift(workers);
                    break;
                case (DayPhase.EVENING):
                    EveningShift.RefillShift(workers);
                    break;
                case (DayPhase.NIGHT):
                    NightShift.RefillShift(workers);
                    break;
                default:
                    break;
            }
        } else
        {
            GetTimeEventArgs e = new GetTimeEventArgs { RoundToDayPhase = false };
            GetTime?.Invoke(this, e);
            float currTime = e.Time;
            float startTime = e.Time; // mas lesz munkasbuszoknal
            float endTime = e.Time + 28800; // = 8 oraval kesobb

            Shift newShift = Shift.CreateNewShift(currTime, startTime, endTime, workers);
            newShift.WorkerChanged += NewShift_WorkerChanged;
            shifts.Add(newShift);
        }
        
    }

    private void NewShift_WorkerChanged(object sender, WorkerChangedEventArgs e)
    {
        Shift shift = sender as Shift;
        if (e.Starts)
        {
            currentWorkers += e.WorkerCount;
        }
        else // Munkaid� v�ge
        {
            currentWorkers -= e.WorkerCount;

            // Csak akkor adjuk hozz� �ket, ha t�nyleg vannak benne munk�sok
            if (shift.Workers != null && shift.Workers.Count > 0)
            {
                waitingForBus.AddRange(shift.Workers);

                // FONTOS: Miut�n hazamentek, �r�ts�k ki a m�szak list�j�t, 
                // hogy a k�vetkez� v�lt�sn�l ne duplik�l�djanak!
                shift.Workers.Clear();
            }

            // Ha ez egy dinamikusan l�trehozott (nem fix) m�szak, takar�tsunk el
            if (shift != DayShift && shift != EveningShift && shift != NightShift)
            {
                shifts.Remove(shift);
                Destroy(shift.gameObject);
            }
        }
    }

    //private void NewShift_WorkerChanged(object sender, WorkerChangedEventArgs e)
    //{
    //    Shift shift = sender as Shift;
    //    if (e.Starts)
    //    {
    //        currentWorkers += e.WorkerCount;
    //    } else if (shift == DayShift || shift == EveningShift || shift == NightShift)
    //    {
    //        currentWorkers -= e.WorkerCount;
    //        waitingForBus.AddRange(shift.Workers);
    //    } else
    //    {
    //        currentWorkers -= e.WorkerCount;
    //        waitingForBus.AddRange(shift.Workers);
    //        shifts.Remove(shift);
    //        Destroy(shift);
    //    } 
    //}

    public int SpaceLeft()
    {
        return maxWorkers - currentWorkers;
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

    private void UpdateStorageDebug()
    {
        inputStorage = 0;
        outputStorage = 0;

        if (recipe == null) return;

        foreach (var item in inventory)
        {
            // Ha a nyersanyag benne van a recept bemenetei k�z�tt, akkor input
            if (recipe.Inputs.ContainsKey(item.Key))
            {
                inputStorage += item.Value;
            }

            // Ha a nyersanyag benne van a recept kimenetei k�z�tt, akkor output
            if (recipe.Outputs.ContainsKey(item.Key))
            {
                outputStorage += item.Value;
            }
        }
    }

    public List<Worker> GoingHome(List<BusStop> stops, int capacity)
    {
        List<City> cities = new List<City>();
        foreach (BusStop stop in stops)
        {
            if (stop.City != null)
            {
                cities.Add(stop.City);
            }
        }

        List<Worker> passangers = new List<Worker>();

        for (int i = waitingForBus.Count - 1; i >= 0; i--)
        {
            if (cities.Contains(waitingForBus[i].HomeCity) && capacity > 0) // ha a busz megall a szulovarosuknal
            {
                passangers.Add(waitingForBus[i]);
                capacity++;
                waitingForBus.Remove(waitingForBus[i]);
            }
        }


        return passangers;
    }

    public void UpdateShifts(float gameTime)
    {
        for (int i = shifts.Count - 1; i >= 0; i--)
        {
            if (i < shifts.Count && shifts[i].enabled)
            {
                shifts[i].ShiftUpdate(gameTime);
            }
        }
    }

    public void AdjustVisualToGround(Vector3 surfacePoint)
    {
        //if (Visual != null)
        //{
        //    float yOffset = surfacePoint.y - transform.position.y;

        //    Visual.transform.localPosition = new Vector3(0, yOffset + 0.01f, 0);
        //}
    }
}
