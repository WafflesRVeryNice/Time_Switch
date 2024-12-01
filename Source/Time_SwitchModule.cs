using System;
using Microsoft.Xna.Framework.Input;
using Celeste;
using On.Celeste;
using On.Celeste.Mod;
using IL.Celeste;
using static MonoMod.InlineRT.MonoModRule;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Time_Switch;

public class Time_SwitchModule : EverestModule {
    public static Time_SwitchModule Instance { get; private set; }

    public override Type SettingsType => typeof(Time_SwitchModuleSettings);
    public static Time_SwitchModuleSettings Settings => (Time_SwitchModuleSettings) Instance._Settings;

    public override Type SessionType => typeof(Time_SwitchModuleSession);
    public static Time_SwitchModuleSession Session => (Time_SwitchModuleSession) Instance._Session;

    public override Type SaveDataType => typeof(Time_SwitchModuleSaveData);
    public static Time_SwitchModuleSaveData SaveData => (Time_SwitchModuleSaveData) Instance._SaveData;

    //vars
    private Vector2 playerPosition;

    public Scene Scene { get; private set; }

    private Vector2 currentLevelPos;

    private Vector2 playerPosRelativeToLevel;

    private string currentLevelName;

    private string tpToLevel;

    private int currentLevelNumber;

    private string currentLevelNumberString;

    private string nextLevelName;

    private Vector2 nextLevelPos;

    private Vector2 nextPlayerPosRelativeToRoom;


    public Time_SwitchModule() {
        Instance = this;
#if DEBUG
        // debug builds use verbose logging
        Logger.SetLogLevel(nameof(Time_SwitchModule), LogLevel.Verbose);
#else
        // release builds use info logging to reduce spam in log files
        Logger.SetLogLevel(nameof(Time_SwitchModule), LogLevel.Info);
#endif
    }

    public override void Load() 
    {
        On.Celeste.Player.Update += Player_Update;
    }

    public override void Unload() 
    {
        On.Celeste.Player.Update -= Player_Update;
    }


    private void Player_Update(On.Celeste.Player.orig_Update orig, Player self)
    {
        orig(self);

        if (self.InControl && Time_SwitchModule.Settings.TimeSwitchBind)
        {
            Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "teleported");

            TeleportPlayer(self);
        }
        else
        {
            Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "did not teleport");
        }
    }

    void TeleportPlayer(Player self)
    {
        Level level = (Level)self.Scene;

        Player player = level.Tracker.GetEntity<Player>();
        playerPosition = player.Position;


        currentLevelPos = level.LevelOffset;

        playerPosRelativeToLevel = new Vector2(playerPosition.X - currentLevelPos.X, playerPosition.Y - currentLevelPos.Y);


        currentLevelName = level.Session.Level;

        if (currentLevelName.StartsWith("a"))
        {
            tpToLevel = "b";
        }
        else if (currentLevelName.StartsWith("b"))
        {
            tpToLevel = "a";
        }

        currentLevelNumber = currentLevelName[1] + currentLevelName[2];
        currentLevelNumberString = currentLevelNumber.ToString();


        MapData currentMapData = level.Session.MapData;

        //LevelData nextLevelFirstChar = currentMapData.Levels.Find(item => item.Name[0] == tpToLevel[0]);

        for (int i = 0; i < currentMapData.LevelCount; i++) 
        {
            LevelData nextPotentiallevel = currentMapData.Levels.Find(item => item.Name[0] == tpToLevel[0]);

            if (nextPotentiallevel.Name.Contains(currentLevelNumberString))
            {
                nextLevelName = nextPotentiallevel.Name;

                nextLevelPos = nextPotentiallevel.LevelOffset;
            }
        }

        //nextLevelPos = ;

        nextPlayerPosRelativeToRoom = new Vector2(nextLevelPos.X + playerPosition.X, nextLevelPos.Y + playerPosition.Y);

        //Level.TeleportTo(self, nextLevelName, Player.IntroTypes.None, new Vector2(playerPosRelativeToRoom.X, playerPosRelativeToRoom.Y + 800));
        //Level.TeleportTo(self, nextLevelName, Player.IntroTypes.None, );

    }
}