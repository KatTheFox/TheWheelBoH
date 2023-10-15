using BepInEx.Logging;
using SecretHistories.Fucine;

namespace TheWheelBoH;

public class ResetSpeedOnDaybreakTracker:ISettingSubscriber
{
    public void WhenSettingUpdated(object newValue)
    {
        TheWheel.speedreset = (bool)newValue;
    }

    public void BeforeSettingUpdated(object newValue)
    {
    }
}