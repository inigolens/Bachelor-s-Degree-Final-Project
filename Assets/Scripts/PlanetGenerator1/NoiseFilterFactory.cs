using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NoiseFilterFactory
{
    public static INoiseFilter CreateNoiseFilter(NoiseSettings settings)
    {
        switch (settings.filterType)
        {
            case NoiseSettings.FilterType.Simple:
                return new SimpleNoiseFilter(settings.simplenoisesettings);
            case NoiseSettings.FilterType.Rigid:
                return new RigidNoiseFilter(settings.rigidnoisesettings);
                
        }
        return null;

    }
}