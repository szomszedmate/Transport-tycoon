using TMPro;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    private readonly Quaternion baseRotation = Quaternion.Euler(0f, 0f, 0f);
    private readonly float baseSpeed = 40f;
    private readonly float speedMultiplier = 3f;

    public float speed;
    public float mindist;
    public float maxdist;
    public float mousescrollmultiplier;
    public float rotationSensitivity = 5f;
    [SerializeField] private float verticalRotation;
    [SerializeField] private float horizontalRotation;
    public Camera minimapcam;

    public void RotateFreeLook(float mouseX, float mouseY)
    {
        // 1. K�l�n v�ltoz�kban adjuk hozz� az elmozdul�st
        horizontalRotation += mouseX * rotationSensitivity;
        verticalRotation -= mouseY * rotationSensitivity;

        // 2. Clampelj�k a f�gg�legest (a -148 �s 28 tartom�nyod marad)
        verticalRotation = Mathf.Clamp(verticalRotation, -148f, 28f);

        // 3. EGYBEN �ll�tjuk be a rot�ci�t, nem r�szenk�nt
        // Ez megakad�lyozza, hogy a Unity "okoskodni" akarjon a tengelyekkel
        transform.localRotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0f);

        // Debug, hogy l�sd: most m�r nincsenek 180 fokos ugr�sok az Y-ban (horizontalRotation)
        // Debug.Log($"X: {verticalRotation}, Y: {horizontalRotation}");
    }

    public void MoveCameraHorizontally(Direction direction)
    {
        Vector3 forward = transform.forward;
        forward.y = 0;
        Vector3 right = transform.right;
        right.y = 0; // only move horizontally

        switch (direction)
        {
            case Direction.N:
                transform.position += forward * speed * Time.unscaledDeltaTime;
                break;
            case Direction.W:
                transform.position += -right * speed * Time.unscaledDeltaTime;
                break;
            case Direction.S:
                transform.position += -forward * speed * Time.unscaledDeltaTime;
                break;
            case Direction.E:
                transform.position += right * speed * Time.unscaledDeltaTime;
                break;
            default:
                return;
        }
    }

    public void MoveCameraVertically(bool up, float minHeight = 0)
    {
        float effectiveMin = minHeight;
        if (up) // up
        {
            if (transform.position.y + speed * mousescrollmultiplier * Time.unscaledDeltaTime < maxdist)
            {
                transform.position += Vector3.up * speed * mousescrollmultiplier * Time.unscaledDeltaTime;

            }
            else
            {
                transform.position = new Vector3(transform.position.x, maxdist, transform.position.z);
            }
        }
        else // down
        {
            float nextY = transform.position.y - speed * mousescrollmultiplier * Time.unscaledDeltaTime;
            if (nextY > effectiveMin)
                transform.position += Vector3.down * speed * mousescrollmultiplier * Time.unscaledDeltaTime;
            else if (transform.position.y > effectiveMin)
                transform.position = new Vector3(transform.position.x, effectiveMin, transform.position.z);
        }
    }

    public void SnapToHeight(float height)
    {
        transform.position = new Vector3(transform.position.x, height, transform.position.z);
    }

    public Vector3 NextPos(Direction direction)
    {
        Vector3 forward = transform.forward;
        forward.y = 0;
        Vector3 right = transform.right;
        right.y = 0; // only move horizontally

        Vector3 newPos = transform.position;

        switch (direction)
        {
            case Direction.N:
                newPos = transform.position + forward * speed * Time.unscaledDeltaTime;
                break;
            case Direction.W:
                newPos = transform.position + -right * speed * Time.unscaledDeltaTime;
                break;
            case Direction.S:
                newPos = transform.position + -forward * speed * Time.unscaledDeltaTime;
                break;
            case Direction.E:
                newPos = transform.position + right * speed * Time.unscaledDeltaTime;
                break;
            default:
                break;
        }
        return newPos;
    }

    public void ResetRotation()
    {
        transform.rotation = baseRotation;
        verticalRotation = transform.localEulerAngles.x;
        horizontalRotation = transform.localEulerAngles.y;
    }

    public void IncreaseSpeed()
    {
        speed = baseSpeed * speedMultiplier;
    }

    public void ResetSpeed()
    {
        speed = baseSpeed;
    }

    void Update()
    {

    }

    private void Start()
    {
        speed = baseSpeed;
        verticalRotation = transform.localEulerAngles.x;
        horizontalRotation = transform.localEulerAngles.y;
    }

    private bool TerrainHit(float terrainHeight, float newHeight)
    {
        return (terrainHeight >= newHeight);
    }
}