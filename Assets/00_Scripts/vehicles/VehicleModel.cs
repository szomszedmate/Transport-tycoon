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

        //// Megkeressük az összes MeshRenderer-t a modellben (lehet több darabból is)
        //Renderer[] renderers = Visual.GetComponentsInChildren<Renderer>();
        //if (renderers.Length == 0) return;

        //// Létrehozunk egy befoglaló téglalapot (Bounds)
        //Bounds combinedBounds = renderers[0].bounds;
        //foreach (Renderer r in renderers)
        //{
        //    combinedBounds.Encapsulate(r.bounds);
        //}

        //// A bounds.extents.z megadja a távolságot a középponttól az elejéig (Z tengely)
        //// Ez a világkoordinátákban van, így a skálázást is figyelembe veszi
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
            // --- HÍD LOGIKA ---
            Vector3 frontPoint = currentPos + forward * frontOffset;
            Vector3 backPoint = currentPos - forward * backOffset;

            float yFront = grid.CalculateBridgeHeight(frontPoint, roadAtPos.Model.Rotation, roadAtPos.data);
            float yBack = grid.CalculateBridgeHeight(backPoint, roadAtPos.Model.Rotation, roadAtPos.data);

            finalY = (yFront + yBack) / 2f;

            Vector3 direction = new Vector3(frontPoint.x, yFront, frontPoint.z) -
                                new Vector3(backPoint.x, yBack, backPoint.z);

            if (direction.sqrMagnitude > 0.0001f)
            {
                Visual.transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            }
            Visual.transform.position = new Vector3(currentPos.x, finalY, currentPos.z);
        }
        else
        {
            // --- TALÁJ LOGIKA (Raycast) ---
            // Két pontos raycast az út dõléséhez talajon is (opcionális, de ajánlott)
            if (Physics.Raycast(currentPos + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 1000f, terrainLayer))
            {
                finalY = hit.point.y;

                // Itt a talajon egyszerûen a jármû eredeti Y forgatását használjuk, 
                // vagy megtarthatod a régi két pontos raycastos dõlést is.
                Quaternion flatRotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
                Visual.transform.rotation = flatRotation;
            }
            else
            {
                finalY = currentPos.y; // Fail-safe
            }
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
    //        // 2. Magasság beállítása (a két pont átlaga)
    //        float groundY = (frontHit.point.y + backHit.point.y) / 2f + floatingHeight;
    //        Visual.transform.position = new Vector3(transform.position.x, groundY, transform.position.z);
    //        Debug.Log(Visual.transform.position);
    //        //Debug.Log(frontHit.point.y + ", " + backHit.point.y);

    //        Vector3 direction = frontHit.point - backHit.point;

    //        // sqrMagnitude-ot használunk, mert gyorsabb, mint a Distance
    //        if (direction.sqrMagnitude > 0.0001f)
    //        {
    //            // Normalizáljuk az irányt és a felfelé mutató vektort
    //            Vector3 upVector = (frontHit.normal + backHit.normal).normalized;

    //            // Csak akkor hívjuk meg, ha van érvényes irányunk
    //            Visual.transform.rotation = Quaternion.LookRotation(direction.normalized, upVector);
    //        }
    //        else
    //        {
    //            // Ha a két pont azonos, nézzen a jármû eredeti elõre-irányába
    //            Vector3 currentEuler = Visual.transform.eulerAngles;
    //            Visual.transform.rotation = Quaternion.Euler(currentEuler.x, transform.eulerAngles.y, currentEuler.z);
    //            Debug.LogWarning("Nem tudott forgatni, magnitude: " + direction.sqrMagnitude);
    //        }
    //    }
    //}
}
