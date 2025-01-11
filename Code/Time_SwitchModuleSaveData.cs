namespace Celeste.Mod.Time_Switch;

public class Time_SwitchModuleSaveData : EverestModuleSaveData 
{
    internal bool firstLoad = true;

    public Time_Switch.FormatMode RoomNameFormat { get; set; }

    public Time_Switch.AutoFormat AutomaticFormatDetection { get; set; }
}