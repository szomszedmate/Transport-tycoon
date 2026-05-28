using UnityEngine;

public class BusStopModel : MonoBehaviour
{
    public GameObject Visual;
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

    public void AdjustVisualToGround(Vector3 surfacePoint, Vector3 sideOffset, Quaternion rotation)
    {
        if (Visual != null)
        {
            // Y eltolás: talaj és az égi szülõ különbsége
            float yOffset = surfacePoint.y - transform.position.y;

            // localPosition = [Oldalra tolás X, Talajra tolás Y, Oldalra tolás Z]
            Visual.transform.localPosition = new Vector3(sideOffset.x, yOffset + 0.01f, sideOffset.z);
            Visual.transform.rotation = rotation;
        }
    }
}
