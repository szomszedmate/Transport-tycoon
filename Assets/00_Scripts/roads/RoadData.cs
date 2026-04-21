using UnityEngine;

public enum RoadKind
{
    NormalRoad,
    Bridge
}

[CreateAssetMenu(menuName = "Data/Roads/Road")]
public class RoadData : ScriptableObject, IData
{
    [field: SerializeField]
    public RoadModel Model {  get; private set; }
    [field: SerializeField]
    public int Cost { get; private set; }
    [field: SerializeField]
    public string Description { get; private set; }
    [field: SerializeField]
    public RoadKind Kind { get; private set; }

    [SerializeField] private int maxBridgeLength = 1;
    public int MaxBridgeLength => maxBridgeLength;

    [SerializeField] private float speedModifier = 1f;
    public float SpeedModifier => speedModifier;
}
