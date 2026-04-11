using UnityEngine;

public class BusStopModel : MonoBehaviour
{
    public float Rotation => transform.eulerAngles.y;
    public void Rotate(float degree)
    {
        Vector3 angles = transform.eulerAngles;
        angles.y = degree;
        transform.eulerAngles = angles;
    }
    public Vector3 GetBusStopPosition()
    {
        return transform.position;
    }
}
