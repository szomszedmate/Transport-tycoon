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
        if (model != null) model.adjustEnabled = false;

        Vector3 startPos = targetPosition + new Vector3(
            Random.Range(-150f, 150f),
            spawnHeight,
            Random.Range(-150f, 150f)
        );

        vehicle.transform.position = startPos;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            vehicle.transform.position = Vector3.Lerp(startPos, targetPosition, eased);
            yield return null;
        }

        vehicle.transform.position = targetPosition;

        if (model != null) model.adjustEnabled = true;
    }
}
