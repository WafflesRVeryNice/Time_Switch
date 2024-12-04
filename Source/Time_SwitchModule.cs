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



//WARNING I'm not very experienced at coding, I needed a lot of help to make this, so the comments may not be 100% accurate/correct



namespace Celeste.Mod.Time_Switch;

public class Time_SwitchModule : EverestModule {
    //---Setting up mod (from mod template)---
    public static Time_SwitchModule Instance { get; private set; }

    public override Type SettingsType => typeof(Time_SwitchModuleSettings);
    public static Time_SwitchModuleSettings Settings => (Time_SwitchModuleSettings) Instance._Settings;

    public override Type SessionType => typeof(Time_SwitchModuleSession);
    public static Time_SwitchModuleSession Session => (Time_SwitchModuleSession) Instance._Session;

    public override Type SaveDataType => typeof(Time_SwitchModuleSaveData);
    public static Time_SwitchModuleSaveData SaveData => (Time_SwitchModuleSaveData) Instance._SaveData;
    //---

    //---vars---

    //player's position in world space measured in pixels
    private Vector2 playerPosition;

    //gets the 'scene' scene can be MainMenu, Level etc - it's kind of like the camera
    public Scene Scene { get; private set; }

    //the position of the current room (position is at the top-left of the room) (room positions are also in pixels)
    private Vector2 currentLevelPos;

    //result of simple calculation to find difference between player's position and room's position
    private Vector2 playerPosRelativeToLevel;

    //the name of the current room
    private string currentLevelName;

    //first character of the room' name (assumes room name format as stated in ReadMe)
    private string tpToLevel;

    //second and third character of the room's name (assumes room name format as stated in ReadMe)
    private string currentLevelNumber;

    //name of the room the player should teleport to
    private string nextLevelName;

    //the position of the room the player should teleport to
    private Vector2 nextLevelPos;

    //list of rooms that begin with the character in tpToLevel
    private LevelData nextPotentialLevel;

    //the position in world space the player should teleport to (uses "Absolute" sufix due to the now remove variable "nextPlayerPosRelativeToRoom")
    private Vector2 nextPlayerPosAbsolute;

    //frame counter used for debugging
    //private int frameCounter = 0;

    //second sounter used for debugging
    //private int secondCounter = 0;

    //used to activate/deactivate the IL hook
    private bool modActive;

    //---

    //this came with the template
    
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
        //loads On hook to Player.Update - 'wraps mod's code around what it's hooking (Update)'
        On.Celeste.Player.Update += Player_Update;
        //loads IL hook to Level.TeleportTo - inserts this "IL_0000: call Microsoft.Xna.Framework.Vector2 Celeste.Mod.Time_Switch.Time_SwitchModule::GetPlayerPos(Microsoft.Xna.Framework.Vector2)" line under IL_008f
        IL.Celeste.Level.TeleportTo += Level_TeleportTo;
    }

    //unloads hooks
    public override void Unload() 
    {
        On.Celeste.Player.Update -= Player_Update;
        IL.Celeste.Level.TeleportTo -= Level_TeleportTo;
    }



    private static void Level_TeleportTo(MonoMod.Cil.ILContext il)
    {
        //Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "IL started");

        //creates new IL cursor
        ILCursor cursor = new (il);

        //places cursor bellow the line where MatchLdflda<Session>("RespawnPoint") - load feild address (ld fld a) (<type Vector2>) Celeste.Session :: RespawnPoint
        //GoToNext is used instead of TryGoToNext so it throws an exception instead of silently moving on
        cursor.GotoNext(MoveType.After, instr => instr.MatchLdflda<Session>("RespawnPoint"));

        //moves the cursor one line down
        cursor.Index++;

        //cursor 'prints' the delegate
        cursor.EmitDelegate(GetPlayerPos);

        //moves the cursor one line down again so it can continue
        cursor.Index++;

        //Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "IL context" + cursor.Context);

        //sets modActive false after exicution so it's not still true on the next frame
        //Time_SwitchModule.Instance. is added before modActive because this method is static so you have to specifiy the instance you want
        Time_SwitchModule.Instance.modActive = false;
    }

    public static Vector2 GetPlayerPos(Vector2 orig_playerPos)
    {
        //if statement checks whether the modded code should trigger or it it should use the vanilla version
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
        //this calls the original Update (which calls orig_Update - the actual vanilla update hooking Update instead of orig_Update is 'safer')
        orig(self);

        //checks that the player can perform actions and whether the input to time switch has been pressed (defined in settings)
        if (self.InControl && Time_SwitchModule.Settings.TimeSwitchBind.Pressed)
        {
            //activates IL
            modActive = true;

            //Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "teleported");

            //calls seperate method that actually handles the teleportation
            TeleportPlayer(self);

            //tells the game to stop checking for the input for a set amount of time
            Time_SwitchModule.Settings.TimeSwitchBind.ConsumePress();
        }
        else
        {
            //deactivates IL hook (only required on the first frame, safety after that)
            modActive = false;

            //Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "did not teleport");
        }

        //counter for debugging to see on what frame(s) something happens
        //assumes the frame rate is 60 because Celeste updates phystics 60 times a seconds
        /*
        if (frameCounter >= 60)
        {
            secondCounter++;
            Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "-----------------" + secondCounter + "-----------------");
            frameCounter = 0;
        }
        frameCounter++;
        //Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "---" + frameCounter + "---");
        */
    }

    
    void TeleportPlayer(Player self)
    {
        //Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "----------------------------------");
        //Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "started teleporting player");

        //Level is the current room (where the scene is), alternatives to "(Level)self.Scene" are "Scene as Level" and "SceneAs<Level>()"
        Level level = (Level)self.Scene;

        //finds the player
        Player player = level.Tracker.GetEntity<Player>();
        //sets playerPosition to the current position of the player
        playerPosition = player.Position;

        //Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "current player pos" + playerPosition);


        //sets currentlevelPos to the position of the current room
        currentLevelPos = level.LevelOffset;

        //finds difference between the player's position and the level's positions (note that -Y is up and +Y is down)
        playerPosRelativeToLevel = new Vector2(playerPosition.X - currentLevelPos.X, playerPosition.Y - currentLevelPos.Y);

        //Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "current relative player pos " + playerPosRelativeToLevel);

        //gets the name of the room the player is in
        currentLevelName = level.Session.Level;

        //checks the first character of the room's name and outputs 'the opposite'
        if (currentLevelName.StartsWith("a"))
        {
            tpToLevel = "b";
        }
        else if (currentLevelName.StartsWith("b"))
        {
            tpToLevel = "a";
        }

        //Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "current room " + currentLevelName);

        //gets the second and third characters of the current room's name
        string currentLevelNumberDigit1 = currentLevelName[1].ToString();
        string currentLevelNumberDigit2 = currentLevelName[2].ToString();

        //adds them to a single string
        currentLevelNumber = currentLevelNumberDigit1 + currentLevelNumberDigit2;

        //Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "room number " + currentLevelNumber);

        //MapData is a list of LevelData including all rooms in the map
        MapData currentMapData = level.Session.MapData;

        //finds the LevelData for the room that has the correct first character and contains the currentLevelNumber string (characters 2 & 3)
        nextPotentialLevel = currentMapData.Levels.Find(item => item.Name[0] == tpToLevel[0] && item.Name.Contains(currentLevelNumber));

        //safety if statement
        if (level != null)
        {
            //gets the name of the room to teleport to from the LevelData 
            nextLevelName = nextPotentialLevel.Name;

            //Logger.Log(LogLevel.Warn, "Waffles - TimeSwitch", "chosen next level " + nextLevelName);

            //gets the position of the rrom to teleport to (Bounds.Left is the X value of the left side of the room, Bounds.Top is the Y value of the top of the rooms)
            nextLevelPos = new Vector2(nextPotentialLevel.Bounds.Left, nextPotentialLevel.Bounds.Top);

            //Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "next level pos " + nextLevelPos);
        }

        //uses position of room to teleport to + the difference between the player and the position of the room the player is in to set the position the player should teleport to
        nextPlayerPosAbsolute = new Vector2(nextLevelPos.X + playerPosRelativeToLevel.X, nextLevelPos.Y + playerPosRelativeToLevel.Y);

        //Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "next player pos " + nextPlayerPosAbsolute);

        //relics of the past
        //Level.TeleportTo(self, nextLevelName, Player.IntroTypes.None, new Vector2(playerPosRelativeToRoom.X, playerPosRelativeToRoom.Y + 800));
        //Level.TeleportTo(self, nextLevelName, Player.IntroTypes.None, nextPlayerPosRelativeToRoom);

        //at the end of the frame it teleports the player to the new room at the new player position
        //note that the intro type must be Transition otherwise TeleportTo uses different code which spawns the player differently and skips the IL hooked code
        level.OnEndOfFrame += () => { level.TeleportTo(self, nextLevelName, Player.IntroTypes.Transition, nextPlayerPosAbsolute); };

        //Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "finished teleport");
        //Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "----------------------------------");

    }
}