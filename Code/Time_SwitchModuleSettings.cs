using System.Collections.Generic;
using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using static Celeste.Mod.Time_Switch.Time_SwitchModule;

namespace Celeste.Mod.Time_Switch;

public class Time_SwitchModuleSettings : EverestModuleSettings
{
    //---Time Switch Keybind---
    [DefaultButtonBinding(button: Buttons.Y, key: Keys.Q)]
    public ButtonBinding TimeSwitchBind { get; set; }
    //---


    //---on/off + room name format setting---
    public Time_Switch.FormatMode RoomNameFormat { get; set; } = Time_Switch.FormatMode.off;

    public void CreateRoomNameFormatEntry(TextMenu menu, bool inGame)
    {
        //+++solution from everest+++ https://github.com/EverestAPI/Everest/blob/dev/Celeste.Mod.mm/Mod/Core/CoreModuleSettings.cs#L559

        TextMenu.Slider roomNameFormatSlider = new TextMenu.Slider(label: Dialog.Clean("Time_Switch_Setting_RoomNameFormat_Name"),
            i => Dialog.Clean($"Time_Switch_Setting_RoomNameFormat_Option_{Enum.GetName((Time_Switch.FormatMode)i)}"),
            0, Enum.GetValues<Time_Switch.FormatMode>().Length - 1, (int)RoomNameFormat);

        roomNameFormatSlider.OnValueChange += val => RoomNameFormat = (Time_Switch.FormatMode)val;

        menu.Add(roomNameFormatSlider);

        if (inGame && !Time_SwitchModule.SaveData.defaultRoomNameFormat)
        {
            roomNameFormatSlider.Disabled = true;
        }


        TextMenuExt.EaseInSubHeaderExt descrText = new TextMenuExt.EaseInSubHeaderExt(Dialog.Clean($"Time_Switch_Setting_RoomNameFormat_Description_Menu_{Enum.GetName(RoomNameFormat)}"), false, menu)
        {
            TextColor = Color.Gray,
            HeightExtra = 0f
        };

        descrText.IncludeWidthInMeasurement = false;

        menu.Insert(menu.Items.IndexOf(roomNameFormatSlider) + 1, descrText);

        roomNameFormatSlider.OnEnter += () => descrText.FadeVisible = true;
        roomNameFormatSlider.OnLeave += () => descrText.FadeVisible = false;
        roomNameFormatSlider.OnValueChange += val => {
            descrText.Title = Dialog.Clean($"Time_Switch_Setting_RoomNameFormat_Description_Menu_{Enum.GetName((Time_Switch.FormatMode)val)}");
            menu.RecalculateSize();
        };
        //+++
    }



    //---setting for automatic room name format detection---
    //+++solution also from everest (see above)+++
    public Time_Switch.TimelineTypes LegacyTimelines { get; set; } = Time_Switch.TimelineTypes.auto;

    public void CreateLegacyTimelinesEntry(TextMenu menu, bool inGame)
    {
        TextMenu.Slider TimelineTypeSlider = new TextMenu.Slider(label: Dialog.Clean("Time_Switch_Setting_LegacyTimelines_Name"),
            i => Dialog.Clean($"Time_Switch_Setting_LegacyTimelines_Option_{Enum.GetName((Time_Switch.TimelineTypes)i)}"),
            0, Enum.GetValues<Time_Switch.TimelineTypes>().Length - 1, (int)LegacyTimelines);

        TimelineTypeSlider.OnValueChange += val => LegacyTimelines = (Time_Switch.TimelineTypes)val;

        menu.Add(TimelineTypeSlider);

        if (inGame)
        {
            TimelineTypeSlider.Disabled = true;
        }


        TextMenuExt.EaseInSubHeaderExt descrText = new TextMenuExt.EaseInSubHeaderExt(Dialog.Clean($"Time_Switch_Setting_LegacyTimelines_Description_{Enum.GetName(LegacyTimelines)}"), false, menu)
        {
            TextColor = Color.Gray,
            HeightExtra = 0f
        };

        descrText.IncludeWidthInMeasurement = false;

        menu.Insert(menu.Items.IndexOf(TimelineTypeSlider) + 1, descrText);

        TimelineTypeSlider.OnEnter += () => descrText.FadeVisible = true;
        TimelineTypeSlider.OnLeave += () => descrText.FadeVisible = false;
        TimelineTypeSlider.OnValueChange += val => {
            descrText.Title = Dialog.Clean($"Time_Switch_Setting_LegacyTimelines_Description_{Enum.GetName((Time_Switch.TimelineTypes)val)}");
            menu.RecalculateSize();
        };
    }

    //---


    [SettingIgnore]
    public bool DisableKeyBind { get; set; } = false; //TODO implement


    //settings to save user settings incase they are overriden by map
    [SettingIgnore]
    public Time_Switch.FormatMode userRoomNameFormat { get; set; }

    [SettingIgnore]
    public Time_Switch.TimelineTypes userLegacyTimelines { get; set; }
}