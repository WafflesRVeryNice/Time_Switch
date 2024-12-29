using System;
using Microsoft.Xna.Framework.Input;
using Celeste;
using On.Celeste;
using On.Celeste.Mod;
using IL.Celeste;
using static MonoMod.InlineRT.MonoModRule;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using System.Reflection;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using static Celeste.Mod.Time_Switch.Time_SwitchModuleSettings;



//WARNING I'm not very experienced at coding, I needed a lot of help to make this, so the comments may not be 100% accurate/correct



namespace Celeste.Mod.Time_Switch;

public class Time_SwitchModule : EverestModule
{
    //+++Mod Template+++ https://github.com/EverestAPI/CelesteModTemplate/blob/main/CelesteMod/Source/CelesteModModule.cs
    //---Setting up mod---
    public static Time_SwitchModule Instance { get; private set; }

    public override Type SettingsType => typeof(Time_SwitchModuleSettings);
    public static Time_SwitchModuleSettings Settings => (Time_SwitchModuleSettings)Instance._Settings;

    public override Type SessionType => typeof(Time_SwitchModuleSession);
    public static Time_SwitchModuleSession Session => (Time_SwitchModuleSession)Instance._Session;

    public override Type SaveDataType => typeof(Time_SwitchModuleSaveData);
    public static Time_SwitchModuleSaveData SaveData => (Time_SwitchModuleSaveData)Instance._SaveData;
    //---



    //---assigning the instance---
    public Time_SwitchModule()
    {
        Instance = this;
#if DEBUG
        Logger.SetLogLevel(nameof(Time_SwitchModule), LogLevel.Verbose);
#else

#endif
    }



    public override void Load()
    {
        Time_SwitchHooks.Load();
    }



    //unloads hooks
    public override void Unload()
    {
        Time_SwitchHooks.Unload();
    }
}

