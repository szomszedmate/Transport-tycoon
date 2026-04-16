using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public abstract class BusModel : MonoBehaviour
{
    public float Rotation => transform.eulerAngles.y;
    public void Rotate(float degree)
    {
        Vector3 angles = transform.eulerAngles;
        angles.y = degree;
        transform.eulerAngles = angles;
    }
    public Vector3 GetBusPosition()
    {
        return transform.position;
    }

}
