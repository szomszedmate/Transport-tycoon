using UnityEngine;

public class WindMillFan : MonoBehaviour
{
    public float speed = 10f;

    public void Rotate(float deltaTime)
    {
        transform.Rotate(Vector3.forward * speed * deltaTime);
    }
}
