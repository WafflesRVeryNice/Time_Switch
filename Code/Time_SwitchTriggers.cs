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
    private int mode;
    private int TimelineType;
    private bool coversScreen;
    private bool onlyOnce;

    public TimeSwitchControlTrigger(EntityData data, Vector2 offset, EntityID id) : base(data, offset) 
    {
        ID = id;
        RoomNameFormatOption = data.Int("RoomNameFormat");
        mode = data.Int("Activate");
        TimelineType = data.Int("Timelines");
        coversScreen = data.Bool("coversScreen", false);
        onlyOnce = data.Bool("onlyOnce", true);
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

        if (mode == 1)
        {
            Trigger();
        }
    }

    public override void OnLeave(Player player)
    {
        base.OnLeave(player);

        if (mode == 2)
        {
            Trigger();
        }
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);

        if (mode == 3)
        {
            Trigger();
        }
    }

    private void Trigger()
    {
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

        if (TimelineType == 1)
        {
            //does nothing
        }
        else if (TimelineType == 2)
        {
            Time_SwitchModule.Settings.LegacyTimelines = Time_Switch.TimelineTypes.auto;
            Time_SwitchModule.SaveData.LegacyTimelines = Time_Switch.TimelineTypes.auto;
        }
        else if (TimelineType == 3)
        {
            Time_SwitchModule.Settings.LegacyTimelines = Time_Switch.TimelineTypes.legacySaveFile;
            Time_SwitchModule.SaveData.LegacyTimelines = Time_Switch.TimelineTypes.legacySaveFile;
        }
        else if (TimelineType == 4)
        {
            Time_SwitchModule.Settings.LegacyTimelines = Time_Switch.TimelineTypes.legacySaveSession;
            Time_SwitchModule.SaveData.LegacyTimelines = Time_Switch.TimelineTypes.legacySaveSession;
        }
    }
}

