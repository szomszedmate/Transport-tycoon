using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
public class DayNighCycle : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Game game;
    [SerializeField] private Light sunLight;
    [SerializeField] private GameObject sun;
    [SerializeField] private Light earthLight;
    [SerializeField] private GameObject earth;

    [Header("Settings")]
    [SerializeField] private float rotationOffset = -90f;
    [SerializeField] private Gradient sunColor;
    [FormerlySerializedAs("moonColor")]
    [SerializeField] private Gradient earthColor;
    [SerializeField] private float distance = 1000f;
    public float nightExposure = 0.7f; // for the skybox
    public float dayExposure = 1.5f;

    void Start()
    {

        game.TimeChanged += UpdateAtmosphere;        
    }

    private void UpdateAtmosphere(object sender, TimeChangedEventArgs e)
    {
        float dayPercent = (float)(e.NewTime / 86400.0);
        float sunRotation = (dayPercent * 360f)  % 360 + rotationOffset;
        float earthRotation = dayPercent * 360f % 360;

        RotateEarth(earthRotation);

        // Rotating the lights
        sunLight.transform.rotation = Quaternion.Euler(sunRotation, 170f, 0f);
        earthLight.transform.rotation = Quaternion.Euler(sunRotation + 180f, 170f, 0f);

        Vector3 camPos = Camera.main.transform.position;
        if (sun != null) sun.transform.position = camPos + (sunLight.transform.forward * -distance);
        if (earth != null) earth.transform.position = camPos + (earthLight.transform.forward * -distance);

        float rawHeight = -sunLight.transform.forward.y;
        float sunHeight = Mathf.Clamp01(rawHeight / 0.34f);

        float currentExposure = Mathf.Lerp(nightExposure, dayExposure, sunHeight);
        RenderSettings.skybox.SetFloat("_Exposure", currentExposure);

        float thickness = Mathf.Lerp(3.0f, 1.0f, sunHeight);
        RenderSettings.skybox.SetFloat("_AtmosphereThickness", thickness);

        sunLight.intensity = (sunHeight > 0) ? 1.2f : 0f;
        earthLight.intensity = (sunHeight == 0) ? 0.3f : 0f;

        sunLight.color = sunColor.Evaluate(dayPercent);
        earthLight.color = earthColor.Evaluate(dayPercent);

        RenderSettings.ambientLight = (sunHeight > 0) ? sunColor.Evaluate(dayPercent) : earthColor.Evaluate(dayPercent);


        RenderSettings.ambientIntensity = Mathf.Lerp(1.2f, 0.5f, sunHeight);

        Color currentSkyColor = (sunHeight > 0) ? sunColor.Evaluate(dayPercent) : earthColor.Evaluate(dayPercent);
        RenderSettings.skybox.SetColor("_SkyTint", currentSkyColor);

        RenderSettings.sun = (sunHeight > 0) ? sunLight : earthLight;

        if (e.NewTime % 600 == 0)
        {
            DynamicGI.UpdateEnvironment();
        }
    }

    public void RotateEarth(float earthRotation)
    {
        earth.transform.rotation = Quaternion.Euler(0f, earthRotation, -23.5f);
    }
}
