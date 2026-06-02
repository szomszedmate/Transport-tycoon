using UnityEngine;
using UnityEngine.AI;

public abstract class VehicleModel : MonoBehaviour
{
    public float floatingHeight = 2f;
    public GameObject Visual;
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
        //if (Visual == null) return;

        //// Megkeress�k az �sszes MeshRenderer-t a modellben (lehet t�bb darabb�l is)
        //Renderer[] renderers = Visual.GetComponentsInChildren<Renderer>();
        //if (renderers.Length == 0) return;

        //// L�trehozunk egy befoglal� t�glalapot (Bounds)
        //Bounds combinedBounds = renderers[0].bounds;
        //foreach (Renderer r in renderers)
        //{
        //    combinedBounds.Encapsulate(r.bounds);
        //}

        //// A bounds.extents.z megadja a t�vols�got a k�z�ppontt�l az elej�ig (Z tengely)
        //// Ez a vil�gkoordin�t�kban van, �gy a sk�l�z�st is figyelembe veszi
        //frontOffset = combinedBounds.extents.z;
        //backOffset = combinedBounds.extents.z;

        frontOffset = 2f;
        backOffset = 2f;

        //Debug.Log("Kalkulalt: " + frontOffset + ", " + backOffset);
    }

    public void AdjustVisualPosition()
    {
        Vector3 currentPos = transform.position;
        Vector3 forward = Quaternion.Euler(0, transform.eulerAngles.y, 0) * Vector3.forward;

        float finalY;
        Road roadAtPos = grid.GetRoad(currentPos);

        if (roadAtPos != null && roadAtPos.IsBridge)
        {
            // --- H�D LOGIKA ---
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
            // --- TAL�J LOGIKA (k�t pontos raycast a d�l�s sz�g�hez) ---
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

    //public void AdjustVisualPosition()
    //{
    //    Vector3 currentPos = transform.position;
    //    (int col, int row) = grid.WorldToGridPosition(currentPos);

    //    Vector3 forward = Quaternion.Euler(0, transform.eulerAngles.y, 0) * Vector3.forward;

    //    Vector3 frontStart = transform.position + forward * frontOffset + Vector3.up * 10f;
    //    Vector3 backStart = transform.position - forward * backOffset + Vector3.up * 10f;

    //    bool hitFront = Physics.Raycast(frontStart, Vector3.down, out RaycastHit frontHit, 1000f, terrainLayer);
    //    bool hitBack = Physics.Raycast(backStart, Vector3.down, out RaycastHit backHit, 1000f, terrainLayer);

    //    Debug.Log("Front: " + frontStart + ", back: " + backStart);

    //    Debug.Log("Raycast talalt: " + frontHit.point + ", " + backHit.point);

    //    if (hitFront && hitBack)
    //    {
    //        // 2. Magass�g be�ll�t�sa (a k�t pont �tlaga)
    //        float groundY = (frontHit.point.y + backHit.point.y) / 2f + floatingHeight;
    //        Visual.transform.position = new Vector3(transform.position.x, groundY, transform.position.z);
    //        Debug.Log(Visual.transform.position);
    //        //Debug.Log(frontHit.point.y + ", " + backHit.point.y);

    //        Vector3 direction = frontHit.point - backHit.point;

    //        // sqrMagnitude-ot haszn�lunk, mert gyorsabb, mint a Distance
    //        if (direction.sqrMagnitude > 0.0001f)
    //        {
    //            // Normaliz�ljuk az ir�nyt �s a felfel� mutat� vektort
    //            Vector3 upVector = (frontHit.normal + backHit.normal).normalized;

    //            // Csak akkor h�vjuk meg, ha van �rv�nyes ir�nyunk
    //            Visual.transform.rotation = Quaternion.LookRotation(direction.normalized, upVector);
    //        }
    //        else
    //        {
    //            // Ha a k�t pont azonos, n�zzen a j�rm� eredeti el�re-ir�ny�ba
    //            Vector3 currentEuler = Visual.transform.eulerAngles;
    //            Visual.transform.rotation = Quaternion.Euler(currentEuler.x, transform.eulerAngles.y, currentEuler.z);
    //            Debug.LogWarning("Nem tudott forgatni, magnitude: " + direction.sqrMagnitude);
    //        }
    //    }
    //}
}
