using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeGenerator
{
    ShapeSettings settings;
    INoiseFilter[] noisefilters;
    public MinMax elevationMinMax;
    public void UpdateSettings(ShapeSettings settings)
    {
        this.settings = settings;
        noisefilters = new INoiseFilter[settings.noiseLayers.Length];
        for (int i = 0; i < noisefilters.Length; i++)
        {
            noisefilters[i] = NoiseFilterFactory.CreateNoiseFilter(settings.noiseLayers[i].noiseSettings);
        }
        elevationMinMax = new MinMax();
    }

    public float CalculateUnescaledElevation(Vector3 pointOnUnitSphere)
    {
        float firstLayerValue = 0f;
        float elevation = 0;

        if(noisefilters.Length > 0)
        {
            firstLayerValue = noisefilters[0].Evaluate(pointOnUnitSphere);
            if (settings.noiseLayers[0].enable)
            {
                elevation = firstLayerValue;
            }
        }

        for(int i = 1; i < noisefilters.Length; i++)
        {
            if (settings.noiseLayers[i].enable)
            {
                float mask = (settings.noiseLayers[i].useFirstLayerAsMask) ? firstLayerValue : 1;
                elevation += noisefilters[i].Evaluate(pointOnUnitSphere) * mask;

            }
        }
        elevationMinMax.addValue(elevation);
        return elevation;
    }

    public float GetScaledElevation(float unscaledElevation)
    {
        float elevation = Mathf.Max(0, unscaledElevation);
        elevation = settings.planetRadius * (1 + elevation);
        return elevation;   
    }
}
