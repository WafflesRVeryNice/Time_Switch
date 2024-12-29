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



    public Time_Switch.FormatMode RoomNameFormat { get; set; } = Time_Switch.FormatMode.off;

    //---on/off + room name format setting---
    public void CreateRoomNameFormatEntry(TextMenu menu, bool inGame)
    {
        //+++solution from everest+++ https://github.com/EverestAPI/Everest/blob/dev/Celeste.Mod.mm/Mod/Core/CoreModuleSettings.cs#L559

        TextMenu.Slider roomNameFormatSlider = (TextMenu.Slider)new TextMenu.Slider(label: Dialog.Clean("Setting_RoomNameFormat_Name"), 
            i => Dialog.Clean($"Setting_RoomNameFormat_Option_{Enum.GetName((Time_Switch.FormatMode) i)}"), 
            0, Enum.GetValues<Time_Switch.FormatMode>().Length - 1, (int)RoomNameFormat);

        roomNameFormatSlider.OnValueChange += val => RoomNameFormat = (Time_Switch.FormatMode)val;

        menu.Add(roomNameFormatSlider);

        
        TextMenuExt.EaseInSubHeaderExt descrText = new TextMenuExt.EaseInSubHeaderExt(Dialog.Clean($"Setting_RoomNameFormat_Description_{Enum.GetName(RoomNameFormat)}"), false, menu)
        {
            TextColor = Color.Gray,
            HeightExtra = 0f
        };

        descrText.IncludeWidthInMeasurement = false;

        menu.Insert(menu.Items.IndexOf(roomNameFormatSlider) + 1, descrText);

        roomNameFormatSlider.OnEnter += () => descrText.FadeVisible = true;
        roomNameFormatSlider.OnLeave += () => descrText.FadeVisible = false;
        roomNameFormatSlider.OnValueChange += val => {
            descrText.Title = Dialog.Clean($"Setting_RoomNameFormat_Description_{Enum.GetName((Time_Switch.FormatMode)val)}");
            menu.RecalculateSize();
        };
        //+++
    }
}