using SecretHistories.Fucine;
using UnityEngine;

namespace TheWheelBoH;

public class FSpeedSettingTracker:ISettingSubscriber
{
    private static float[] factorMap = new float[9]
    {
        0.2f,
        0.5f,
        2f,
        4f,
        6f,
        8f,
        13f,
        21f,
        34f
    };

    public void WhenSettingUpdated(object newValue)
    {
        if (!(newValue is int num1))
            num1 = 1;
        int num2 = num1;
        int index = Mathf.Min(FSpeedSettingTracker.factorMap.Length - 1, Mathf.Max(num2, 0));
        TheWheel.fspeedStep = FSpeedSettingTracker.factorMap[index];
    }

    public void BeforeSettingUpdated(object newValue)
    {
    }
}