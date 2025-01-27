namespace Celeste.Mod.Time_Switch;

public class Time_SwitchModuleSession : EverestModuleSession 
{
    public Time_Switch.TimelineTypes LegacyTimelines { get; set; }

    internal bool defaultLegacyTimelines = true;

    internal bool forceTimeSwitch = false; //TODO? make a custom component

    internal bool? disableKeybind = null;
}