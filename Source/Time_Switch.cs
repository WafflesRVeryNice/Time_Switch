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



namespace Celeste.Mod.Time_Switch;

public static partial class Time_Switch
{

    public static FormatMode RoomNameFormat { get; internal set; }

    public enum FormatMode
    {
        off,
        Format1,
        Format2
    }
}

