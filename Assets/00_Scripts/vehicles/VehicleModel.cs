using UnityEngine;
using UnityEngine.AI;

public abstract class VehicleModel : MonoBehaviour
{
    public float floatingHeight = 2f;
    public VehicleVisual Visual;
    public LayerMask terrainLayer;
    protected float frontOffset;
    protected float backOffset;
    public BuildingGrid grid;
    [SerializeField] private float heightSmoothSpeed = 5f;
    private float smoothedY = float.MinValue;

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

    public void CalculateOffsets()
    {
        frontOffset = 2f;
        backOffset = 2f;
    }

    public bool adjustEnabled = true;

    public void AdjustVisualPosition()
    {
        if (!adjustEnabled) return;
        if (Visual == null) return;
        Vector3 currentPos = transform.position;
        Vector3 forward = Quaternion.Euler(0, transform.eulerAngles.y, 0) * Vector3.forward;

        float finalY;
        Road roadAtPos = grid.GetRoad(currentPos);

        if (roadAtPos != null && roadAtPos.IsBridge)
        {
            Vector3 frontPoint = currentPos + forward * frontOffset;
            Vector3 backPoint = currentPos - forward * backOffset;

            float yFront = grid.CalculateBridgeHeight(frontPoint, roadAtPos.Model.Rotation, roadAtPos.data);
            float yBack = grid.CalculateBridgeHeight(backPoint, roadAtPos.Model.Rotation, roadAtPos.data);

            finalY = (yFront + yBack) / 2f;

            Vector3 direction = new Vector3(frontPoint.x, yFront, frontPoint.z) -
                                new Vector3(backPoint.x, yBack, backPoint.z);

            if (direction.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(direction.normalized, Vector3.up);
                Visual.transform.rotation = Quaternion.Slerp(Visual.transform.rotation, targetRot, 5f * Time.deltaTime);
            }
            if (smoothedY == float.MinValue) smoothedY = finalY;
            smoothedY = Mathf.Lerp(smoothedY, finalY, heightSmoothSpeed * Time.deltaTime);
            Visual.transform.position = new Vector3(currentPos.x, smoothedY, currentPos.z);
        }
        else
        {
            Vector3 frontPoint = currentPos + forward * frontOffset;
            Vector3 backPoint  = currentPos - forward * backOffset;

            bool hitFront = Physics.Raycast(frontPoint  + Vector3.up * 10f, Vector3.down, out RaycastHit frontHit, 1000f, terrainLayer);
            bool hitBack  = Physics.Raycast(backPoint   + Vector3.up * 10f, Vector3.down, out RaycastHit backHit,  1000f, terrainLayer);
            bool hitCenter = Physics.Raycast(currentPos + Vector3.up * 10f, Vector3.down, out RaycastHit centerHit, 1000f, terrainLayer);

            if (hitFront && hitBack)
            {
                finalY = (frontHit.point.y + backHit.point.y) / 2f;

                Vector3 direction = new Vector3(frontPoint.x, frontHit.point.y, frontPoint.z) -
                                    new Vector3(backPoint.x,  backHit.point.y,  backPoint.z);

                if (direction.sqrMagnitude > 0.0001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(direction.normalized, Vector3.up);
                    Visual.transform.rotation = Quaternion.Slerp(Visual.transform.rotation, targetRot, 5f * Time.deltaTime);
                }
            }
            else if (hitCenter)
            {
                finalY = centerHit.point.y;
                Quaternion flatRotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
                Visual.transform.rotation = Quaternion.Slerp(Visual.transform.rotation, flatRotation, 5f * Time.deltaTime);
            }
            else
            {
                finalY = currentPos.y; // Fail-safe
            }
            smoothedY = float.MinValue; // reset, hogy h�d ut�n ne sim�tson
            Visual.transform.position = new Vector3(currentPos.x, finalY + floatingHeight, currentPos.z);
        }

    }
}
