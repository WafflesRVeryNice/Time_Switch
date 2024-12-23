﻿using System;
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



//WARNING I'm not very experienced at coding, I needed a lot of help to make this, so the comments may not be 100% accurate/correct



namespace Celeste.Mod.Time_Switch;

public static class Time_SwitchHooks {


    //---vars---

    //gets the 'scene' scene can be MainMenu, Level etc - it's kind of like the camera
    public static Scene Scene { get; private set; }

    //name of the room the player should teleport to
    private static string nextLevelName;

    //the position of the room the player should teleport to
    private static Vector2 nextLevelPos;

    //list of rooms that begin with the character in tpToLevel
    private static LevelData nextPotentialLevel;

    //the position in world space the player should teleport to (uses "Absolute" sufix due to the now remove variable "nextPlayerPosRelativeToRoom")
    private static Vector2 nextPlayerPosAbsolute;

    //frame counter used for debugging
    //private int frameCounter = 0;

    //second sounter used for debugging
    //private int secondCounter = 0;

    //used to activate/deactivate the IL hook
    private static bool modActive;

    //creates var for manual IL hook
    private static ILHook CancelDashHook;

    //---
    

    internal static void Load() 
    {
        //loads On hook to Player.Update - 'wraps mod's code around what it's hooking (Update)'
        On.Celeste.Player.Update += Player_Update;
        //loads IL hook to Level.TeleportTo - inserts this "IL_0000: call Microsoft.Xna.Framework.Vector2 Celeste.Mod.Time_Switch.Time_SwitchModule::GetPlayerPos(Microsoft.Xna.Framework.Vector2)" line under IL_008f
        IL.Celeste.Level.TeleportTo += Level_TeleportTo;
        //define manual IL hook to Player.orig_Added
        CancelDashHook = new(typeof(Player).GetMethod("orig_Added", BindingFlags.Public | BindingFlags.Instance), Time_SwitchHooks.CancelDashReset);
    }

    

    //unloads hooks
    internal static void Unload() 
    {
        On.Celeste.Player.Update -= Player_Update;
        IL.Celeste.Level.TeleportTo -= Level_TeleportTo;
        //manual IL hooks are unloaded a bit differently
        CancelDashHook.Dispose();
    }



    private static void CancelDashReset(ILContext il)
    {
        //creates IL cursor
        ILCursor cursor = new(il);

        //places cursor bellow where MatchCallvirt<Player>("get_MaxDashes") - Call virtual (type int32) Celeste.Player :: get_MaxDashes()
        cursor.GotoNext(MoveType.After, instr => instr.MatchCallvirt<Player>("get_MaxDashes"));

        //cursor writes "ldarg.0"
        cursor.EmitLdarg0();

        //cursor write delegate
        cursor.EmitDelegate(GetPlayerDashes);

        //Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "IL context CancelDashReset " + cursor.Context);

        //sets modActive false after exicution so it's not still true on the next frame
        //Time_SwitchModule.Instance. is added before modActive because this method is static so you have to specifiy the instance you want
        Time_SwitchHooks.modActive = false;
    }


    private static int GetPlayerDashes(int MaxDashes, Player player)
    {
        //if statement checks whether the modded code should trigger or it it should use the vanilla version
        if (Time_SwitchHooks.modActive)
        {
            //how many dash th eplayer currentlt has
            return player.Dashes;
        }
        else
        {
            //number of possible dashes (inventory)
            return MaxDashes;
        }
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

        //cursor writes the delegate
        cursor.EmitDelegate(GetPlayerPos);

        //moves the cursor one line down again so it can continue
        cursor.Index++;

        //Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "IL context Level_Teleport " + cursor.Context);

        //sets modActive false after exicution so it's not still true on the next frame
        //Time_SwitchModule.Instance. is added before modActive because this method is static so you have to specifiy the instance you want
    }

    public static Vector2 GetPlayerPos(Vector2 orig_playerPos)
    {
        //if statement checks whether the modded code should trigger or it it should use the vanilla version
        if (Time_SwitchHooks.modActive)
        {
            //spawns player at the excact position defined in Teleport_Player
            return Time_SwitchHooks.nextPlayerPosAbsolute;
        }
        else
        {
            //causes vanilla behaviour - player spawns at the nearest spawn point
            return orig_playerPos;
        }
    }



    private static void Player_Update(On.Celeste.Player.orig_Update orig, Player self)
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

    
    static void TeleportPlayer(Player self)
    {
        //Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "----------------------------------");
        //Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "started teleporting player");

        //Level is the current room (where the scene is), alternatives to "(Level)self.Scene" are "Scene as Level" and "SceneAs<Level>()"
        Level level = (Level)self.Scene;

        //finds the player
        Player player = level.Tracker.GetEntity<Player>();

        //player's position in world space measured in pixels
        Vector2 playerPosition = player.Position;

        //Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "current player pos" + playerPosition);


        //the position of the current room (position is at the top-left of the room) (room positions are also in pixels)
        Vector2 currentLevelPos = level.LevelOffset;

        //finds difference between the player's position and the level's positions (note that -Y is up and +Y is down)
        Vector2 playerPosRelativeToLevel = new Vector2(playerPosition.X - currentLevelPos.X, playerPosition.Y - currentLevelPos.Y);

        //Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "current relative player pos " + playerPosRelativeToLevel);

        //gets the name of the room the player is in
        string currentLevelName = level.Session.Level;

        //Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "current room " + currentLevelName);

        //new comment
        string nextLevelTimeline = TimelinePicker(currentLevelName);

        //new comment
        string currentLevelNumber = FetchCurrentLevelIdentifier(currentLevelName);

        //Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "room number " + currentLevelNumber);

        //new comment
        FindNextLevel(level, nextLevelTimeline, currentLevelNumber);

        //uses position of room to teleport to + the difference between the player and the position of the room the player is in to set the position the player should teleport to
        nextPlayerPosAbsolute = new Vector2(nextLevelPos.X + playerPosRelativeToLevel.X, nextLevelPos.Y + playerPosRelativeToLevel.Y);

        //Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "next player pos " + nextPlayerPosAbsolute);

        //at the end of the frame it teleports the player to the new room at the new player position
        //note that the intro type must be Transition otherwise TeleportTo uses different code which spawns the player differently and skips the IL hooked code
        level.OnEndOfFrame += () => { level.TeleportTo(self, nextLevelName, Player.IntroTypes.Transition, nextPlayerPosAbsolute); };

        //Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "finished teleport");
        //Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "----------------------------------");

    }



    static string TimelinePicker(string currentLevelName)
    {
        //return the character used for the other timeline
        if (currentLevelName.StartsWith("a"))
        {
            return "b";
        }
        else if (currentLevelName.StartsWith("b"))
        {
            return "a";
        }
        else
        {
            return null;
        }
    }



    static string FetchCurrentLevelIdentifier(string currentLevelName)
    {
        //gets the second and third characters of the current room's name
        string currentLevelNumberDigit1 = currentLevelName[1].ToString();
        string currentLevelNumberDigit2 = currentLevelName[2].ToString();

        //adds them to a single string
        return currentLevelNumberDigit1 + currentLevelNumberDigit2;
    }



    static void FindNextLevel(Level level, string nextLevelTimeline, string currentLevelNumber)
    {
        //MapData is a list of LevelData including all rooms in the map
        MapData currentMapData = level.Session.MapData;

        //finds the LevelData for the room that has the correct first character and contains the currentLevelNumber string (characters 2 & 3)
        nextPotentialLevel = currentMapData.Levels.Find(item => item.Name[0] == nextLevelTimeline[0] && item.Name.Contains(currentLevelNumber));

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
    }
}