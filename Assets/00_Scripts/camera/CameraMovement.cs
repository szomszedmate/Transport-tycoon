using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float speed = 10f;

    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            transform.position += Vector3.forward * speed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.S))
        {
            transform.position += Vector3.back * speed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.A))
        {
            transform.position += Vector3.left * speed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.D))
        {
            transform.position += Vector3.right * speed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.E))
        {
            transform.position += Vector3.up * speed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.Q))
        {
            transform.position += Vector3.down * speed * Time.deltaTime;
        }
    }
}