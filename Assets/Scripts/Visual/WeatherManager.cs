using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherManager : MonoBehaviour
{
    public Light lightSource;
    public Color closeAtmosphereColor = new Color(0.637f, 0.877f, 0.950f, 1f);
    public Color farAtmosphereColor = new Color(0.419f, 0.705f, 0.910f, 1f);

    void Awake()
    {
        RenderSettings.ambientLight = closeAtmosphereColor; // Currently I think the shader overshoots the lerp, so farAtmosphere is actually the midrange.
        RenderSettings.ambientIntensity = 0f;
        RenderSettings.fog = false;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = farAtmosphereColor;
        RenderSettings.fogDensity = 0.02f;
    }

    // TODO: add plane underneath to block light near nighttime
    void FixedUpdate()
    {
        lightSource.transform.rotation *= Quaternion.AngleAxis(0.1f, Vector3.right);
        if (lightSource.transform.eulerAngles.x > -10f && lightSource.transform.eulerAngles.x < 190f)
        {
            lightSource.intensity = 4f;
        } 
        else 
        {
            lightSource.intensity = 0f;
        }
    }
}
