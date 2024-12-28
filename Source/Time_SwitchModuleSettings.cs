using System.Collections.Generic;
using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.Time_Switch;

public class Time_SwitchModuleSettings : EverestModuleSettings
{
    //---Time Switch Keybind---
    [DefaultButtonBinding(button: Buttons.Y, key: Keys.Q)]
    public ButtonBinding TimeSwitchBind { get; set; }
    //---



    //---on/off + room name format setting---
    public int RoomNameFormat { get; set; } = 0;

    public void CreateRoomNameFormatEntry(TextMenu menu, bool inGame)
    {
        string roomNameFormatSubtext = "error - description not set";

        int selected = 0;

        //+++solution from xaphan helper+++ https://github.com/Xaphan67/XaphanHelper/blob/dev/Code/XaphanModuleSettings.cs#L40
        TextMenu.Slider roomNameFormatSlider = (TextMenu.Slider)new TextMenu.Slider(label: Dialog.Clean("Setting_RoomNameFormat_Name"), (int selected) => selected switch
        {
            0 => Dialog.Clean("Setting_RoomNameFormat_Option_0"),
            1 => Dialog.Clean("Setting_RoomNameFormat_Option_1"),
            _ => Dialog.Clean("Setting_RoomNameFormat_Option_2"),
        }
        , 0, 2, RoomNameFormat)
            .Change(delegate {RoomNameFormat = selected;} );
        //+++

        roomNameFormatSlider.OnValueChange += val => roomNameFormatOption = (Time_SwitchModuleSettings.RoomNameFormatOptions)val;

        roomNameFormatSubtext = $"Setting_RoomNameFormat_Description_{selected}";

        menu.Add(roomNameFormatSlider);

        //+++solution from everest+++ https://github.com/EverestAPI/Everest/blob/dev/Celeste.Mod.mm/Mod/Core/CoreModuleSettings.cs#L559
        TextMenuExt.EaseInSubHeaderExt descrText = new TextMenuExt.EaseInSubHeaderExt(Dialog.Clean($"Setting_RoomNameFormat_Description_{Enum.GetName(roomNameFormatOption)}"), false, menu)
        {
            TextColor = Color.Gray,
            HeightExtra = 0f
        };

        descrText.IncludeWidthInMeasurement = false;

        menu.Insert(menu.Items.IndexOf(roomNameFormatSlider) + 1, descrText);

        roomNameFormatSlider.OnEnter += () => descrText.FadeVisible = true;
        roomNameFormatSlider.OnLeave += () => descrText.FadeVisible = false;
        roomNameFormatSlider.OnValueChange += val => {
            descrText.Title = Dialog.Clean($"Setting_RoomNameFormat_Description_{Enum.GetName((Time_SwitchModuleSettings.RoomNameFormatOptions)val)}");
            menu.RecalculateSize();
        };
        //+++
    }



    Time_SwitchModuleSettings.RoomNameFormatOptions roomNameFormatOption { get; set; }

    public enum RoomNameFormatOptions
    {
        off,
        Format1,
        Format2
    }

    //---

}