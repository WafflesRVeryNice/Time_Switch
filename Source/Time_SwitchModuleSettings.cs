using Microsoft.Xna.Framework.Input;

namespace Celeste.Mod.Time_Switch;

public class Time_SwitchModuleSettings : EverestModuleSettings {

    //---Adding Keybind---
    [DefaultButtonBinding(button: Buttons.Y, key: Keys.Q)]
    public ButtonBinding TimeSwitchBind { get; set; }
    //---
}