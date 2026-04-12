using UnityEngine;

[CreateAssetMenu(menuName = "Data/Bus Stops/Bus Stop")]
public class BusStopData : ScriptableObject, IData
{
    [field: SerializeField]
    public BusStopModel Model {  get; private set; }

    [field: SerializeField]
    public int Cost {  get; private set; }

}
