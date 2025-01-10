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
using Celeste.Mod.Entities;


namespace Celeste.Mod.Time_Switch;


[CustomEntity("Time_Switch/TimeSwitchControlTrigger")]
public class TimeSwitchControlTrigger : Trigger
{
    private bool coversScreen;

    public TimeSwitchControlTrigger(EntityData data, Vector2 offset) : base(data, offset) 
    {
        int RoomNameFormatOption = data.Int("RoomNameFormat");
        coversScreen = data.Bool("coversScreen", false);
        bool onlyOnce = data.Bool("onlyOnce", true);
        bool legacyTimelines = data.Bool("legacyTimelines", false);
        bool alwaysUseAutomaticFormatDetection = data.Bool("alwaysUseAutomaticFormatDetection", false);

        if (RoomNameFormatOption == 1)
        {
            //does nothing
        }
        else if (RoomNameFormatOption == 2)
        {
            Time_SwitchModule.Settings.RoomNameFormat = Time_Switch.FormatMode.off;
        }
        else if (RoomNameFormatOption == 3)
        {
            Time_SwitchModule.Settings.RoomNameFormat = Time_Switch.FormatMode.Format1;
        }
        else if (RoomNameFormatOption == 4)
        {
            Time_SwitchModule.Settings.RoomNameFormat = Time_Switch.FormatMode.Format2;
        }

        if (onlyOnce)
        {
            RemoveSelf();
        }

        if (legacyTimelines)
        {
            Time_SwitchModule.Settings.LegacyTimelines = true;
        }
        else if (!legacyTimelines && Time_SwitchModule.Settings.LegacyTimelines == true)
        {
            Time_SwitchModule.Settings.LegacyTimelines = false; //this only takes effect next time the map is loaded
        }

        if (alwaysUseAutomaticFormatDetection)
        {
            Time_SwitchModule.Settings.AutomaticFormatDetectionEverytime = true;
        }
        else if (!alwaysUseAutomaticFormatDetection && Time_SwitchModule.Settings.AutomaticFormatDetectionEverytime == true)
        {
            Time_SwitchModule.Settings.AutomaticFormatDetectionEverytime = false;
        }
    }

    //+++solution from extended variant mode+++ https://github.com/maddie480/ExtendedVariantMode/blob/07e7f2e48cec484613a414bb2dd6aa6e87775db9/Entities/ForMappers/AbstractExtendedVariantTrigger.cs#L69
    public override void Added(Scene scene)
    {
        base.Added(scene);

        Rectangle bounds = (scene as Level).Bounds;

        if (coversScreen)
        {
            Position = new Vector2(bounds.X, bounds.Y - 24f);
            Collider.Width = bounds.Width;
            Collider.Height = bounds.Height + 32f;
        }
    }
    //+++
}

