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
    private EntityID ID;
    private int RoomNameFormatOption;
    private bool coversScreen;
    private bool onlyOnce;
    private bool legacyTimelines;
    private int AutomaticFormatDetection;

    public TimeSwitchControlTrigger(EntityData data, Vector2 offset, EntityID id) : base(data, offset) 
    {
        ID = id;
        RoomNameFormatOption = data.Int("RoomNameFormat");
        coversScreen = data.Bool("coversScreen", false);
        onlyOnce = data.Bool("onlyOnce", true);
        legacyTimelines = data.Bool("legacyTimelines", false);
        AutomaticFormatDetection = data.Int("AutomaticFormatDetection");
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
    


    public override void OnEnter(Player player)
    {
        base.OnEnter(player);

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
            SceneAs<Level>().Session.DoNotLoad.Add(ID);
        }

        if (legacyTimelines)
        {
            Time_SwitchModule.Settings.LegacyTimelines = true;
        }
        else if (!legacyTimelines && Time_SwitchModule.Settings.LegacyTimelines == true)
        {
            Time_SwitchModule.Settings.LegacyTimelines = false; //this only takes effect next time the map is loaded
        }

        if (AutomaticFormatDetection == 1)
        {
            //does nothing
        }
        else if (AutomaticFormatDetection == 2)
        {
            Time_SwitchModule.Settings.AutomaticFormatDetection = Time_Switch.AutoFormat.off;
            Time_SwitchModule.SaveData.AutomaticFormatDetection = Time_Switch.AutoFormat.off;
        }
        else if (AutomaticFormatDetection == 3)
        {
            Time_SwitchModule.Settings.AutomaticFormatDetection = Time_Switch.AutoFormat.light;
            Time_SwitchModule.SaveData.AutomaticFormatDetection = Time_Switch.AutoFormat.light;
        }
        else if (AutomaticFormatDetection == 4)
        {
            Time_SwitchModule.Settings.AutomaticFormatDetection = Time_Switch.AutoFormat.always;
            Time_SwitchModule.SaveData.AutomaticFormatDetection = Time_Switch.AutoFormat.always;
        }
    }
}

