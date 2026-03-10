using UnityEngine;

public class Building : MonoBehaviour
{
    public string Description => data.Description;
    public int Cost => data.Cost;
    private BuildingModel model;
    private BuildingData data;
    public void Setup(BuildingData data, float rotation)
    {
        this.data = data;
        model = Instantiate(data.Model, transform.position, Quaternion.identity, transform);
        model.Rotate(rotation);
    }
}
