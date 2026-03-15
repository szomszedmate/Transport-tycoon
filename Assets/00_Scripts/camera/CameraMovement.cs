using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float speed = 10f;
    public float mindist = 10;
    public float maxdist;
    public float mousescrollmultiplier;
    public Camera minimapcam;
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


        //up
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            if (transform.position.y+speed * mousescrollmultiplier * Time.deltaTime < maxdist)
            {
                transform.position += Vector3.up * speed* mousescrollmultiplier * Time.deltaTime;
            }
            else
            {
                transform.position = new Vector3(transform.position.x, maxdist, transform.position.z);
            }
        }

        if (Input.GetKey(KeyCode.E))
        {
            if (transform.position.y < maxdist)
            {
                transform.position += Vector3.up * speed * Time.deltaTime;
            }
            else
            {
                transform.position = new Vector3(transform.position.x, maxdist, transform.position.z);
            }

        }



        //down
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            if (transform.position.y- speed * mousescrollmultiplier * Time.deltaTime > mindist)
            {
                transform.position += Vector3.down * speed * mousescrollmultiplier * Time.deltaTime;
            }
            else
            {
                transform.position = new Vector3(transform.position.x,mindist,transform.position.z);
            }
        }
        if (Input.GetKey(KeyCode.Q))
        {
            if (transform.position.y>mindist)
            {
                transform.position += Vector3.down * speed * Time.deltaTime;
            }
            else
            {
                transform.position = new Vector3(transform.position.x, mindist, transform.position.z);
            }

        }
    }
}