using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherManager : MonoBehaviour
{

    public Color closeAtmosphereColor = new Color(0.637f, 0.877f, 0.950f, 1f);
    public Color farAtmosphereColor = new Color(0.419f, 0.705f, 0.910f, 1f);

    void Awake()
    {
        RenderSettings.ambientLight = closeAtmosphereColor; // Currently I think the shader overshoots the lerp, so farAtmosphere is actually the midrange. 
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = farAtmosphereColor;
        RenderSettings.fogDensity = 0.02f;
    }
}
