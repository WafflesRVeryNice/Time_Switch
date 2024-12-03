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

    private string currentLevelNumber;

    private string nextLevelName;

    private Vector2 nextLevelPos;

    private Vector2 nextPlayerPosRelativeToRoom;

    private LevelData nextPotentialLevel;

    private Level levelToTP;

    private int frameCounter = 0;

    private int secondCounter = 0;


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

        if (self.InControl && Time_SwitchModule.Settings.TimeSwitchBind.Pressed)
        {
            Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "teleported");

            TeleportPlayer(self);

            Time_SwitchModule.Settings.TimeSwitchBind.ConsumePress();
        }
        else
        {
            Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "did not teleport");
        }

        if (frameCounter >= 60)
        {
            secondCounter++;
            Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "-----------------" + secondCounter + "-----------------");
            frameCounter = 0;
        }
        frameCounter++;
        //Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "---" + frameCounter + "---");
    }

    
    void TeleportPlayer(Player self)
    {
        Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "started teleporting player");


        Level level = (Level)self.Scene;

        Player player = level.Tracker.GetEntity<Player>();
        playerPosition = player.Position;


        currentLevelPos = level.LevelOffset;

        playerPosRelativeToLevel = new Vector2(playerPosition.X - currentLevelPos.X, playerPosition.Y - currentLevelPos.Y);

        Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "current player pos " + playerPosRelativeToLevel);


        currentLevelName = level.Session.Level;

        if (currentLevelName.StartsWith("a"))
        {
            tpToLevel = "b";
        }
        else if (currentLevelName.StartsWith("b"))
        {
            tpToLevel = "a";
        }

        //Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "current room " + currentLevelName);


        string currentLevelNumberDigit1 = currentLevelName[1].ToString();
        string currentLevelNumberDigit2 = currentLevelName[2].ToString();
        currentLevelNumber = currentLevelNumberDigit1 + currentLevelNumberDigit2;

        //Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "num num " + currentLevelNumber);
        //Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "num string " + currentLevelNumber);


        MapData currentMapData = level.Session.MapData;

        nextPotentialLevel = currentMapData.Levels.Find(item => item.Name[0] == tpToLevel[0] && item.Name.Contains(currentLevelNumber));

        if (level != null)
        {
            nextLevelName = nextPotentialLevel.Name;

            Logger.Log(LogLevel.Warn, "Waffles - TimeSwitch", "chosen next level " + nextLevelName);

            nextLevelPos = new Vector2(nextPotentialLevel.Bounds.Left, nextPotentialLevel.Bounds.Top);

            //Level levelToTP = ;

            //Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "next level pos " + nextLevelPos);
        }



        nextPlayerPosRelativeToRoom = new Vector2(nextLevelPos.X + playerPosition.X, nextLevelPos.Y + playerPosition.Y);

        Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "next player pos " + nextPlayerPosRelativeToRoom);

        //Level levelToTP = currentMapData.Levels.Find(item => item.Name == nextLevelName);


        //Level.TeleportTo(self, nextLevelName, Player.IntroTypes.None, new Vector2(playerPosRelativeToRoom.X, playerPosRelativeToRoom.Y + 800));
        //Level.TeleportTo(self, nextLevelName, Player.IntroTypes.None, nextPlayerPosRelativeToRoom);
        level.OnEndOfFrame += () => { level.TeleportTo(self, nextLevelName, Player.IntroTypes.Transition, nextPlayerPosRelativeToRoom); };

        Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "finished teleport");
        Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "----------------------------------");

    }

    /*
        for (int i = 0; i < currentMapData.LevelCount; i++) 
        {
            nextPotentialLevel = currentMapData.Levels.Find(item => item.Name[0] == tpToLevel[0]);

            //Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "potential level " + nextPotentialLevel.Name);

            if (nextPotentialLevel.Name.Contains(currentLevelNumber))
            {
                nextLevelName = nextPotentialLevel.Name;

                Logger.Log(LogLevel.Warn, "Waffles - TimeSwitch", "chosen next level " + nextLevelName);

                nextLevelPos = new Vector2(nextPotentialLevel.Bounds.Left, nextPotentialLevel.Bounds.Top);

                Level levelToTP = ;

                //Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "next level pos " + nextLevelPos);
            }
        }
        */
}