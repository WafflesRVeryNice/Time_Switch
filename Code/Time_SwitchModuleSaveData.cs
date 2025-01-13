namespace Celeste.Mod.Time_Switch;

public class Time_SwitchModuleSaveData : EverestModuleSaveData 
{
    internal bool firstLoad = true;

    public Time_Switch.FormatMode RoomNameFormat { get; set; }

    internal bool defaultRoomNameFormat = true;

    public Time_Switch.TimelineTypes LegacyTimelines { get; set; }

    internal bool defaultLegacyTimelines = true;
}