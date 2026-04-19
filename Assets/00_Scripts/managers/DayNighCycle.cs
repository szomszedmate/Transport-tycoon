using Unity.VisualScripting;
using UnityEngine;
public class DayNighCycle : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Game game;
    [SerializeField] private Light sunLight;
    [SerializeField] private GameObject sun;
    [SerializeField] private Light moonLight;
    [SerializeField] private GameObject moon;

    [Header("Settings")]
    [SerializeField] private float rotationOffset = -90f; // Éjfélkor a föld alatt legyen
    [SerializeField] private Gradient sunColor; // Nappali színek (narancs -> fehér -> narancs)
    [SerializeField] private Gradient moonColor; // Éjszakai színek (sötétkék -> kék)
    [SerializeField] private float distance = 1000f;
    public float nightExposure = 0.7f; // a skyboxnak
    public float dayExposure = 1.5f;

    void Start()
    {
        game.TimeChanged += UpdateAtmosphere;        
    }

    private void UpdateAtmosphere(object sender, TimeChangedEventArgs e)
    {
        float dayPercent = (float)(e.NewTime / 86400.0);
        float sunRotation = (dayPercent * 360f) + rotationOffset;

        // Lámpák forgatása
        sunLight.transform.rotation = Quaternion.Euler(sunRotation, 170f, 0f);
        moonLight.transform.rotation = Quaternion.Euler(sunRotation + 180f, 170f, 0f);

        Vector3 camPos = Camera.main.transform.position;
        if (sun != null) sun.transform.position = camPos + (sunLight.transform.forward * -distance);
        if (moon != null) moon.transform.position = camPos + (moonLight.transform.forward * -distance);

        // --- MAGASSÁG SZÁMÍTÁSA A FORGÓ LÁMPÁBÓL ---
        // A sunLight.transform.forward.y értéke -1 (ha pont lefelé néz) és 1 (ha felfelé) között van.
        // A negatív elõjel miatt a sunHeight 1 lesz délben és 0 éjszaka.
        // 1. Megnézzük, mennyire néz lefelé a nap. 
        // -1 = függõlegesen lefelé (dél), 0 = horizont (naplemente), 1 = felfelé (éjszaka)
        float rawHeight = -sunLight.transform.forward.y;

        // 2. Skálázzuk! Azt mondjuk, hogy ha a nap már 20 fokos szögben (0.34-es érték) fent van, 
        // az már legyen nekünk "teljes nappal" (sunHeight = 1).
        // Ezzel kényszerítjük, hogy a Lerp elérje a dayExposure-t (0.3).
        float sunHeight = Mathf.Clamp01(rawHeight / 0.34f);

        // --- INNENTÕL A KÓDOD TÖBBI RÉSZE MARAD ---
        float currentExposure = Mathf.Lerp(nightExposure, dayExposure, sunHeight);
        RenderSettings.skybox.SetFloat("_Exposure", currentExposure);

        // Nappal vastagabb atmoszféra (elmosódottabb), este vékonyabb (tisztább csillagok)
        float thickness = Mathf.Lerp(3.0f, 1.0f, sunHeight);
        RenderSettings.skybox.SetFloat("_AtmosphereThickness", thickness);

        // Intenzitás állítás (simább átmenet, ha a napHeight-et nézzük)
        sunLight.intensity = (sunHeight > 0) ? 1.2f : 0f;
        moonLight.intensity = (sunHeight == 0) ? 0.3f : 0f;

        sunLight.color = sunColor.Evaluate(dayPercent);
        moonLight.color = moonColor.Evaluate(dayPercent);

        // Ambient szín és intenzitás
        RenderSettings.ambientLight = (sunHeight > 0) ? sunColor.Evaluate(dayPercent) : moonColor.Evaluate(dayPercent);


        // Ambient Intensity: Este (1.2), Nappal (0.5)
        RenderSettings.ambientIntensity = Mathf.Lerp(1.2f, 0.5f, sunHeight);

        // Sky Tint frissítése
        Color currentSkyColor = (sunHeight > 0) ? sunColor.Evaluate(dayPercent) : moonColor.Evaluate(dayPercent);
        RenderSettings.skybox.SetColor("_SkyTint", currentSkyColor);

        RenderSettings.sun = (sunHeight > 0) ? sunLight : moonLight;

        if (e.NewTime % 600 == 0)
        {
            DynamicGI.UpdateEnvironment();
        }
    }
}
