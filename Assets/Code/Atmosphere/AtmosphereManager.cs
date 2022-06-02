using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtmosphereManager : MonoBehaviour
{
    public Color currentBackgroundColor;
    public Color morningColor;
    public Color dayColor;
    public Color eveningColor;
    public Color nightColor; 
    public Gradient dayNightGradient;

    public Material sunGlowMat;

    public Material[] backgroundElementMats;

    public Light environmentLight; // Directional light tilted fully downwards to create environmental lighting.
    public SunLightScript sunLightScript; // The script controlling the Sunlight directional light angle.
    public SunScript sunScript;
    public Camera skyCamera;
    public float fogIntensity;
    public Material volFogMat;
    private Color eLightColor;

    public float dayCycleDuration = 60f; // Duration of a full 24 hour cycle in seconds.

    public float updateTime = 1f; // Update the elements every this often, in seconds.
    private float lastUpdateTime = 0f;

    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time > lastUpdateTime + updateTime) {
            Vector2 XZ = CalculateXZ();
            if(XZ.x > 0f) {
                float p = 1f - Mathf.Pow(XZ.x, 2f);
                currentBackgroundColor = Color.Lerp(morningColor, dayColor, p);
                eLightColor = Color.Lerp(morningColor, dayColor, p);
            }
            else {
                float p = -XZ.x;
                currentBackgroundColor = Color.Lerp(dayColor, eveningColor, p);
                eLightColor = Color.Lerp(dayColor, eveningColor, p);
            }
            if(XZ.y > 0f) {
                float p = XZ.y * 2f;
                currentBackgroundColor = Color.Lerp(currentBackgroundColor, nightColor, p);
                eLightColor = Color.Lerp(eLightColor, nightColor, p);
            }
            foreach(Material mat in backgroundElementMats) {
                mat.SetColor("_BackgroundColor", currentBackgroundColor);
                mat.SetVector("_FresnelXZ", new Vector4(XZ.x, XZ.y));
            }
            Color finalLightColor = Color.Lerp(eLightColor, new Color(0.8f, 0.8f, 0.8f, 1f), 1f - (Mathf.Abs(XZ.x) * 0.75f));
            float finalLightIntensity = Mathf.Clamp(((-XZ.y + 1f) / 3f) + 0.1f, 0f, 0.35f);
            environmentLight.color = currentBackgroundColor;
            environmentLight.intensity = finalLightIntensity * 0.5f;
            float H;
            float S;
            float V;
            Color.RGBToHSV(currentBackgroundColor, out H, out S, out V);
            Color sunGlowColor = sunScript.GetColor();

            sunGlowMat.color = new Color(sunGlowColor.r, sunGlowColor.g, sunGlowColor.b, 0.12f);

            volFogMat.SetFloat("_Intensity", fogIntensity);
            volFogMat.SetColor("_Color", currentBackgroundColor);
            Vector4 sunPos = skyCamera.WorldToScreenPoint(sunScript.transform.position);
            volFogMat.SetVector("_SunPos", sunPos);
            lastUpdateTime = Time.time;
        }
    }

    Vector2 CalculateXZ () { // Calculates X and Z values for background material fresnel effects. This mimics day/night (Z) and sunrise/sunset light (X) direction.
        Vector2 XZ = new Vector2();
        XZ.x = Mathf.Sin(((Time.time + dayCycleDuration / 4) / dayCycleDuration) * Mathf.PI * 2f);
        XZ.y = Mathf.Sin(((Time.time + dayCycleDuration / 2) / dayCycleDuration) * Mathf.PI * 2f);
        return XZ;
    }
}
