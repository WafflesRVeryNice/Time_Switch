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
using static Celeste.Mod.Time_Switch.Time_Switch;


namespace Celeste.Mod.Time_Switch;


[CustomEntity("Time_Switch/TimeSwitchControlTrigger")]
public class TimeSwitchControlTrigger : Trigger
{
    private EntityID ID;
    private int RoomNameFormatOption;
    private int mode;
    private int TimelineType;
    private bool onlyOnce;

    public TimeSwitchControlTrigger(EntityData data, Vector2 offset, EntityID id) : base(data, offset) 
    {
        ID = id;
        RoomNameFormatOption = data.Int("RoomNameFormat");
        mode = data.Int("Activate");
        TimelineType = data.Int("Timelines");
        onlyOnce = data.Bool("onlyOnce", true);
    }
    


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
        //save user settings
        if (Time_SwitchModule.SaveData.defaultRoomNameFormat == true)
        {
            Time_SwitchModule.Settings.userRoomNameFormat = Time_SwitchModule.Settings.RoomNameFormat;
        }
        if (Time_SwitchModule.SaveData.defaultLegacyTimelines == true && Time_SwitchModule.Session.defaultLegacyTimelines == true)
        {
            Time_SwitchModule.Settings.userLegacyTimelines = Time_SwitchModule.Settings.LegacyTimelines;
        }
        

        if (RoomNameFormatOption == 1)
        {
            //does nothing
        }
        else if (RoomNameFormatOption == 2)
        {
            Time_SwitchModule.Settings.RoomNameFormat = Time_Switch.FormatMode.off;
            Time_SwitchModule.SaveData.RoomNameFormat = Time_Switch.FormatMode.off;
            Time_SwitchModule.SaveData.defaultRoomNameFormat = false;
        }
        else if (RoomNameFormatOption == 3)
        {
            Time_SwitchModule.Settings.RoomNameFormat = Time_Switch.FormatMode.Format1;
            Time_SwitchModule.SaveData.RoomNameFormat = Time_Switch.FormatMode.Format1;
            Time_SwitchModule.SaveData.defaultRoomNameFormat = false;
        }
        else if (RoomNameFormatOption == 4)
        {
            Time_SwitchModule.Settings.RoomNameFormat = Time_Switch.FormatMode.Format2;
            Time_SwitchModule.SaveData.RoomNameFormat = Time_Switch.FormatMode.Format2;
            Time_SwitchModule.SaveData.defaultRoomNameFormat = false;
        }


        if (TimelineType == 1)
        {
            //does nothing
        }
        else if (TimelineType == 2)
        {
            Time_SwitchModule.Settings.LegacyTimelines = Time_Switch.TimelineTypes.auto;
            Time_SwitchModule.SaveData.LegacyTimelines = Time_Switch.TimelineTypes.auto;
            Time_SwitchModule.Session.LegacyTimelines = Time_Switch.TimelineTypes.auto;
            Time_SwitchModule.SaveData.defaultLegacyTimelines = false;
            Time_SwitchModule.Session.defaultLegacyTimelines = false;
        }
        else if (TimelineType == 3)
        {
            Time_SwitchModule.Settings.LegacyTimelines = Time_Switch.TimelineTypes.legacySaveFile;
            Time_SwitchModule.SaveData.LegacyTimelines = Time_Switch.TimelineTypes.legacySaveFile;
            Time_SwitchModule.Session.LegacyTimelines = Time_Switch.TimelineTypes.legacySaveFile;
            Time_SwitchModule.SaveData.defaultLegacyTimelines = false;
            Time_SwitchModule.Session.defaultLegacyTimelines = false;
        }
        else if (TimelineType == 4)
        {
            Time_SwitchModule.Settings.LegacyTimelines = Time_Switch.TimelineTypes.legacySaveSession;
            Time_SwitchModule.SaveData.LegacyTimelines = Time_Switch.TimelineTypes.legacySaveSession;
            Time_SwitchModule.Session.LegacyTimelines = Time_Switch.TimelineTypes.legacySaveSession;
            Time_SwitchModule.SaveData.defaultLegacyTimelines = false;
            Time_SwitchModule.Session.defaultLegacyTimelines = false;
        }

        if (onlyOnce)
        {
            RemoveSelf();
            SceneAs<Level>().Session.DoNotLoad.Add(ID);
        }
    }
}



[CustomEntity("Time_Switch/SwitchTimelineTrigger")]
public class SwitchTimelineTrigger : Trigger
{
    private EntityID ID;
    private int mode;
    private bool onlyOnce;

    public SwitchTimelineTrigger(EntityData data, Vector2 offset, EntityID id) : base(data, offset)
    {
        ID = id;
        mode = data.Int("Activate");
        onlyOnce = data.Bool("onlyOnce", true);
    }


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
        Time_SwitchModule.Session.forceTimeSwitch = true;

        if (onlyOnce)
        {
            RemoveSelf();
            SceneAs<Level>().Session.DoNotLoad.Add(ID);
        }
    }
}



[CustomEntity("Time_Switch/DisableInputTrigger")]
public class DisableInputTrigger : Trigger
{
    private EntityID ID;
    private int mode;
    private bool onlyOnce;
    private int input;

    public DisableInputTrigger(EntityData data, Vector2 offset, EntityID id) : base(data, offset)
    {
        ID = id;
        mode = data.Int("Activate");
        onlyOnce = data.Bool("onlyOnce", true);
        input = data.Int("PlayerInput");
    }


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
        if (input == 1)
        {
            Time_SwitchModule.Settings.DisableKeyBind = true;
            Time_SwitchModule.Session.disableKeybind = true;
        }
        else
        {
            Time_SwitchModule.Settings.DisableKeyBind = false;
            Time_SwitchModule.Session.disableKeybind = false;
        }

        if (onlyOnce)
        {
            RemoveSelf();
            SceneAs<Level>().Session.DoNotLoad.Add(ID);
        }
    }
}
