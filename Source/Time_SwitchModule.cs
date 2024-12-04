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

    private LevelData nextPotentialLevel;

    private Vector2 nextPlayerPosRelativeToRoom;

    private Vector2 nextPlayerPosAbsolute;

    private Level levelToTP;

    private int frameCounter = 0;

    private int secondCounter = 0;

    private Vector2 playerTPPosition;

    private bool modActive;

    private Vector2 orig_playerPosition;


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
        IL.Celeste.Level.TeleportTo += Level_TeleportTo;
    }

    public override void Unload() 
    {
        On.Celeste.Player.Update -= Player_Update;
        IL.Celeste.Level.TeleportTo -= Level_TeleportTo;
    }


    private static void Level_TeleportTo(MonoMod.Cil.ILContext il)
    {
        Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "IL started");

        ILCursor cursor = new (il);

        cursor.GotoNext(MoveType.After, instr => instr.MatchCall<Vector2?>("get_Value"));

        cursor.EmitDelegate(GetPlayerPos);

        Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "IL context" + cursor.Context);

        Time_SwitchModule.Instance.modActive = false;
    }

    public static Vector2 GetPlayerPos(Vector2 orig_playerPos)
    {
        if (Time_SwitchModule.Instance.modActive)
        {
            return Time_SwitchModule.Instance.nextPlayerPosAbsolute;
        }
        else
        {
            return orig_playerPos;
        }
    }


    private void Player_Update(On.Celeste.Player.orig_Update orig, Player self)
    {
        orig(self);

        if (self.InControl && Time_SwitchModule.Settings.TimeSwitchBind.Pressed)
        {
            modActive = true;

            Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "teleported");

            TeleportPlayer(self);

            Time_SwitchModule.Settings.TimeSwitchBind.ConsumePress();
        }
        else
        {
            modActive = false;

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
        Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "----------------------------------");
        Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "started teleporting player");


        Level level = (Level)self.Scene;

        Player player = level.Tracker.GetEntity<Player>();
        playerPosition = player.Position;

        Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "current player pos" + playerPosition);



        currentLevelPos = level.LevelOffset;

        playerPosRelativeToLevel = new Vector2(playerPosition.X - currentLevelPos.X, playerPosition.Y - currentLevelPos.Y);

        Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "current relative player pos " + playerPosRelativeToLevel);


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



        //nextPlayerPosRelativeToRoom = new Vector2(nextLevelPos.X + playerPosition.X, nextLevelPos.Y + playerPosition.Y);

        nextPlayerPosAbsolute = new Vector2(nextLevelPos.X + playerPosRelativeToLevel.X, nextLevelPos.Y + playerPosRelativeToLevel.Y);

        Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "next player pos " + nextPlayerPosAbsolute);

        //Level levelToTP = currentMapData.Levels.Find(item => item.Name == nextLevelName);


        //Level.TeleportTo(self, nextLevelName, Player.IntroTypes.None, new Vector2(playerPosRelativeToRoom.X, playerPosRelativeToRoom.Y + 800));
        //Level.TeleportTo(self, nextLevelName, Player.IntroTypes.None, nextPlayerPosRelativeToRoom);
        level.OnEndOfFrame += () => { level.TeleportTo(self, nextLevelName, Player.IntroTypes.Transition, nextPlayerPosAbsolute); };

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