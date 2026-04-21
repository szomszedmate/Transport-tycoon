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
}
