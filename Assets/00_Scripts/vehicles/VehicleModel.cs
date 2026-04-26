using UnityEngine;

public abstract class VehicleModel : MonoBehaviour
{
    public float Rotation => transform.eulerAngles.y;
    public void Rotate(float degree)
    {
        Vector3 angles = transform.eulerAngles;
        angles.y = degree;
        transform.eulerAngles = angles;
    }
    public Vector3 GetPosition()
    {
        return transform.position;
    }
}
