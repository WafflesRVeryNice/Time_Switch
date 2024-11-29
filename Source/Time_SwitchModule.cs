using System;
using Microsoft.Xna.Framework.Input;
using Celeste;
using On.Celeste;
using On.Celeste.Mod;
using IL.Celeste;

namespace Celeste.Mod.Time_Switch;

public class Time_SwitchModule : EverestModule {
    public static Time_SwitchModule Instance { get; private set; }

    public override Type SettingsType => typeof(Time_SwitchModuleSettings);
    public static Time_SwitchModuleSettings Settings => (Time_SwitchModuleSettings) Instance._Settings;

    public override Type SessionType => typeof(Time_SwitchModuleSession);
    public static Time_SwitchModuleSession Session => (Time_SwitchModuleSession) Instance._Session;

    public override Type SaveDataType => typeof(Time_SwitchModuleSaveData);
    public static Time_SwitchModuleSaveData SaveData => (Time_SwitchModuleSaveData) Instance._SaveData;


    public Time_SwitchModule() {
        Instance = this;
#if DEBUG
        // debug builds use verbose logging
        Logger.SetLogLevel(nameof(Time_SwitchModule), LogLevel.Verbose);
#else
        // release builds use info logging to reduce spam in log files
        Logger.SetLogLevel(nameof(Time_SwitchModule), LogLevel.Info);
#endif
    }

    public override void Load() 
    {
        On.Celeste.Player.Update += Player_Update;
    }

    private void Player_Update(On.Celeste.Player.orig_Update orig, Player self)
    {
        orig(self);

        if (self.InControl && Time_SwitchModule.Settings.TimeSwitchBind)
        {
            Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "jumped");
        }
        else
        {
            Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "did not jump");
        }
    }

    public override void Unload() 
    {
        On.Celeste.Player.Update -= Player_Update;
    }
}