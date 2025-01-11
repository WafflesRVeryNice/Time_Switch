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
        //TODO? make only available in-game
        if (inGame)
        {
            //+++solution from everest+++ https://github.com/EverestAPI/Everest/blob/dev/Celeste.Mod.mm/Mod/Core/CoreModuleSettings.cs#L559

            TextMenu.Slider roomNameFormatSlider = new TextMenu.Slider(label: Dialog.Clean("Time_Switch_Setting_RoomNameFormat_Name"),
                i => Dialog.Clean($"Time_Switch_Setting_RoomNameFormat_Option_{Enum.GetName((Time_Switch.FormatMode)i)}"),
                0, Enum.GetValues<Time_Switch.FormatMode>().Length - 1, (int)RoomNameFormat);

            roomNameFormatSlider.OnValueChange += val => RoomNameFormat = (Time_Switch.FormatMode)val;

            menu.Add(roomNameFormatSlider);


            TextMenuExt.EaseInSubHeaderExt descrText = new TextMenuExt.EaseInSubHeaderExt(Dialog.Clean($"Time_Switch_Setting_RoomNameFormat_Description_{Enum.GetName(RoomNameFormat)}"), false, menu)
            {
                TextColor = Color.Gray,
                HeightExtra = 0f
            };

            descrText.IncludeWidthInMeasurement = false;

            menu.Insert(menu.Items.IndexOf(roomNameFormatSlider) + 1, descrText);

            roomNameFormatSlider.OnEnter += () => descrText.FadeVisible = true;
            roomNameFormatSlider.OnLeave += () => descrText.FadeVisible = false;
            roomNameFormatSlider.OnValueChange += val => {
                descrText.Title = Dialog.Clean($"Time_Switch_Setting_RoomNameFormat_Description_{Enum.GetName((Time_Switch.FormatMode)val)}");
                menu.RecalculateSize();
            };
            //+++
        }
    }


    [SettingIgnore]
    public bool DisableRoomNameFormatInSettings { get; set; } = false; //TODO implement


    //---setting for automatic room name format detection---
    //+++solution also from everest (see above)+++
    public Time_Switch.AutoFormat AutomaticFormatDetection { get; set; } = Time_Switch.AutoFormat.light;

    public void CreateAutomaticFormatDetectionEntry(TextMenu menu, bool inGame)
    {
        TextMenu.Slider autoFormatDetectSlider = new TextMenu.Slider(label: Dialog.Clean("Time_Switch_Setting_AutomaticFormatDetection_Name"),
            i => Dialog.Clean($"Time_Switch_Setting_AutomaticFormatDetection_Option_{Enum.GetName((Time_Switch.AutoFormat)i)}"),
            0, Enum.GetValues<Time_Switch.AutoFormat>().Length - 1, (int)AutomaticFormatDetection);

        autoFormatDetectSlider.OnValueChange += val => AutomaticFormatDetection = (Time_Switch.AutoFormat)val;

        menu.Add(autoFormatDetectSlider);


        TextMenuExt.EaseInSubHeaderExt descrText = new TextMenuExt.EaseInSubHeaderExt(Dialog.Clean($"Time_Switch_Setting_AutomaticFormatDetection_Description_{Enum.GetName(AutomaticFormatDetection)}"), false, menu)
        {
            TextColor = Color.Gray,
            HeightExtra = 0f
        };

        descrText.IncludeWidthInMeasurement = false;

        menu.Insert(menu.Items.IndexOf(autoFormatDetectSlider) + 1, descrText);

        autoFormatDetectSlider.OnEnter += () => descrText.FadeVisible = true;
        autoFormatDetectSlider.OnLeave += () => descrText.FadeVisible = false;
        autoFormatDetectSlider.OnValueChange += val => {
            descrText.Title = Dialog.Clean($"Time_Switch_Setting_AutomaticFormatDetection_Description_{Enum.GetName((Time_Switch.AutoFormat)val)}");
            menu.RecalculateSize();
        };
    }

    //---


    [SettingInGame(false)]
    [SettingSubText("Time_Switch_Setting_LegacyTimelines_Description")]
    public bool LegacyTimelines { get; set; } = false;


    [SettingIgnore]
    public bool DisableKeyBind { get; set; } = false;

    //TODO add settings to dump user settings until exiting map
}