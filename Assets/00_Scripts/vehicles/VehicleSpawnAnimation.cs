using System.Collections;
using UnityEngine;

public class VehicleSpawnAnimation : MonoBehaviour
{
    public static void Play(VehicleBase vehicle, Vector3 targetPosition, float spawnHeight = 500f, float duration = 1.2f)
    {
        vehicle.StartCoroutine(AnimateSpawn(vehicle, targetPosition, spawnHeight, duration));
    }

    private static IEnumerator AnimateSpawn(VehicleBase vehicle, Vector3 targetPosition, float spawnHeight, float duration)
    {
        VehicleModel model = vehicle.GetComponentInChildren<VehicleModel>();
        VehicleVisual visual = model?.Visual;
        if (model != null) model.adjustEnabled = false;
        if (visual == null)
        {
            if (model != null) model.adjustEnabled = true;
            yield break;
        }

        // Raycast to find actual ground Y at target position
        Vector3 groundTarget = targetPosition;
        if (Physics.Raycast(targetPosition + Vector3.up * 50f, Vector3.down, out RaycastHit hit, 500f, model.terrainLayer))
        {
            groundTarget = new Vector3(targetPosition.x, hit.point.y + model.floatingHeight, targetPosition.z);
        }

        Vector3 startPos = groundTarget + new Vector3(
            Random.Range(-450f, 450f),
            spawnHeight,
            Random.Range(-450f, 450f)
        );
        visual.transform.position = startPos;

        Vector3 dir = groundTarget - startPos;
        if (dir.sqrMagnitude > 0.0001f)
            visual.transform.rotation = Quaternion.LookRotation(dir);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            visual.transform.position = Vector3.Lerp(startPos, groundTarget, eased);
            yield return null;
        }

        visual.transform.position = groundTarget;

        if (model != null) model.adjustEnabled = true;
    }
}
