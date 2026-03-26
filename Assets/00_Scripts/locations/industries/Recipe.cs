using System.Collections.Generic;

public class Recipe
{
    public Dictionary<ResourceEnum, int> Inputs { get; }
    public Dictionary<ResourceEnum, int> Outputs { get; }
    public float CycleTime { get; }

    public Recipe(
        Dictionary<ResourceEnum, int> inputs,
        Dictionary<ResourceEnum, int> outputs,
        float cycleTime)
    {
        Inputs = inputs ?? new Dictionary<ResourceEnum, int>();
        Outputs = outputs ?? new Dictionary<ResourceEnum, int>();
        CycleTime = cycleTime;
    }
}