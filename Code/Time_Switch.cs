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


//+++Solution from everest+++ https://github.com/EverestAPI/Everest/blob/dev/Celeste.Mod.mm/Mod/Everest/Everest.cs#L957
namespace Celeste.Mod.Time_Switch;

public static partial class Time_Switch
{
    //+++ https://github.com/EverestAPI/Everest/blob/dev/Celeste.Mod.mm/Mod/Everest/Everest.cs#L115
    public static FormatMode RoomNameFormat { get; internal set; }

    public enum FormatMode
    {
        off,
        Format1,
        Format2
    }

    //+++solution also from everest (see above)+++
    public static AutoFormat AutomaticFormatDetection { get; internal set; }

    public enum AutoFormat
    {
        off,
        light,
        always
    }
}
//+++
