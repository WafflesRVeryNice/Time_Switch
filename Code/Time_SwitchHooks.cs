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
using System.Reflection;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using static Celeste.Mod.Time_Switch.Time_SwitchModuleSettings;



namespace Celeste.Mod.Time_Switch;

public static class Time_SwitchHooks {


    //---Class-wide vars--- 

    //scene is what is currently being displayed (menu, level, etc)
    public static Scene Scene { get; private set; }

    private static string nextLevelName;

    private static Vector2 nextLevelPos;

    //the position in world space the player should teleport to
    private static Vector2 nextPlayerPosAbsolute;

    //used to activate/deactivate the IL hooks
    private static bool correctionILsActive;

    //creates var for manual IL hook
    private static ILHook CancelDashRefillHook;

    //---



    internal static void Load() 
    {
        On.Celeste.Player.Update += Player_Update;
        IL.Celeste.Level.TeleportTo += Level_TeleportTo;
        CancelDashRefillHook = new(typeof(Player).GetMethod("orig_Added", BindingFlags.Public | BindingFlags.Instance), Time_SwitchHooks.CancelDashReset);
    }

    

    internal static void Unload() 
    {
        On.Celeste.Player.Update -= Player_Update;
        IL.Celeste.Level.TeleportTo -= Level_TeleportTo;
        CancelDashRefillHook.Dispose();
    }



    //---Stops the player getting all their dashes back when time switching---
    private static void CancelDashReset(ILContext il)
    {
        ILCursor cursor = new(il);

        cursor.GotoNext(MoveType.After, instr => instr.MatchCallvirt<Player>("get_MaxDashes"));

        cursor.EmitLdarg0();

        cursor.EmitDelegate(GetPlayerDashes);

        //sets active false so it's not true next frame
        Time_SwitchHooks.correctionILsActive = false;
    }


    private static int GetPlayerDashes(int MaxDashes, Player player)
    {
        if (Time_SwitchHooks.correctionILsActive)
        {
            //doesn't change Dashes value
            return player.Dashes;
        }
        else
        {
            //vanilla
            return MaxDashes;
        }
    }

    //---



    //---Corrects position from nearest spawn to same position in the room as the player was before teleporting---
    private static void Level_TeleportTo(MonoMod.Cil.ILContext il)
    {
        ILCursor cursor = new (il);

        cursor.GotoNext(MoveType.After, instr => instr.MatchLdflda<Session>("RespawnPoint"));

        cursor.Index++;

        cursor.EmitDelegate(GetPlayerPos);

        cursor.Index++;

        //doesn't set active false so it's still true for other IL hook
    }


    public static Vector2 GetPlayerPos(Vector2 orig_playerPos)
    {
        if (Time_SwitchHooks.correctionILsActive)
        {
            return Time_SwitchHooks.nextPlayerPosAbsolute;
        }
        else
        {
            //causes vanilla behaviour
            return orig_playerPos;
        }
    }

    //---


    //hook that teleport the player, hooks Update instead of orig_Update because it's "safer"
    private static void Player_Update(On.Celeste.Player.orig_Update orig, Player self)
    {
        //calls Update
        orig(self);

        if (Time_SwitchModule.Settings.RoomNameFormat != Time_Switch.FormatMode.off && self.InControl && Time_SwitchModule.Settings.TimeSwitchBind.Pressed)
        {
            Logger.Log(LogLevel.Info, "Waffles - TimeSwitch", "Teleport triggered");

            //activates ILs
            correctionILsActive = true;

            TeleportPlayer(self);

            Time_SwitchModule.Settings.TimeSwitchBind.ConsumePress();
        }
        else
        {
            correctionILsActive = false; //TODO check if necessary
        }
    }


    //---Teleporting the player---
    static void TeleportPlayer(Player self)
    {
        //positions use pixels for units

        Level level = (Level)self.Scene;

        Player player = level.Tracker.GetEntity<Player>();

        //position in world space
        Vector2 playerPosition = player.Position;

        //position is at the top-left of the room
        Vector2 currentLevelPos = level.LevelOffset;

        //finds difference between the player's position and the room's position (-Y is up)
        Vector2 playerPosRelativeToLevel = new Vector2(playerPosition.X - currentLevelPos.X, playerPosition.Y - currentLevelPos.Y);

        string currentLevelName = level.Session.Level;

        string nextLevelTimeline = TimelinePicker(currentLevelName);

        string currentLevelIdentifier = FetchCurrentLevelIdentifier(currentLevelName);

        //finds room on the other timeline with the same identifier
        FindNextLevel(level, nextLevelTimeline, currentLevelIdentifier);

        //gets player position to teleport to in world space
        nextPlayerPosAbsolute = new Vector2(nextLevelPos.X + playerPosRelativeToLevel.X, nextLevelPos.Y + playerPosRelativeToLevel.Y);

        //at the end of the frame it teleports the player to the new room with the new player position
        //note: the intro type must be Transition otherwise TeleportTo uses different code which spawns the player differently and skips the IL hooked code
        level.OnEndOfFrame += () => { level.TeleportTo(self, nextLevelName, Player.IntroTypes.Transition, nextPlayerPosAbsolute); };
    }


    //returns the character used for the other timeline
    static string TimelinePicker(string currentLevelName)
    {
        Time_Switch.FormatMode format = Time_SwitchModule.Settings.RoomNameFormat;

        //TODO add support for any unicode characters

        if (format == Time_Switch.FormatMode.Format1)
        {
            if (currentLevelName.StartsWith("a"))
            {
                return "b";
            }
            else if (currentLevelName.StartsWith("b"))
            {
                return "a";
            }
            return null;
        }

        if (format == Time_Switch.FormatMode.Format2)
        {
            if (currentLevelName.EndsWith("a"))
            {
                return "b";
            }
            else if (currentLevelName.EndsWith("b"))
            {
                return "a";
            }
            return null;
        }

        return null;
    }


    static string FetchCurrentLevelIdentifier(string currentLevelName)
    {
        //if the "clean" format is used it reads the first character as well
        string currentLevelNumberDigit0 = string.Empty;

        if (Time_SwitchModule.Settings.RoomNameFormat == Time_Switch.FormatMode.Format2)
        {
            currentLevelNumberDigit0 = currentLevelName[0].ToString();
        }

        string currentLevelNumberDigit1 = currentLevelName[1].ToString();
        string currentLevelNumberDigit2 = currentLevelName[2].ToString();

        //adds characters to a single string
        return currentLevelNumberDigit0 + currentLevelNumberDigit1 + currentLevelNumberDigit2;
    }


    static void FindNextLevel(Level level, string nextLevelTimeline, string currentLevelIdentifier)
    {
        //MapData is a list of LevelData of all rooms in the map
        MapData currentMapData = level.Session.MapData;

        LevelData nextPotentialLevel = null;

        if (Time_SwitchModule.Settings.RoomNameFormat == Time_Switch.FormatMode.Format1)
        {
            //finds the LevelData for the room that has the correct first character and same identifier
            nextPotentialLevel = currentMapData.Levels.Find(item => item.Name[0] == nextLevelTimeline[0] && item.Name[1..3] == currentLevelIdentifier);
        }

        if (Time_SwitchModule.Settings.RoomNameFormat == Time_Switch.FormatMode.Format2)
        {
            //finds the LevelData for the room that has the correct last character and same identifier
            nextPotentialLevel = currentMapData.Levels.Find(item => item.Name[^1] == nextLevelTimeline[0] && item.Name[0..3] == currentLevelIdentifier);
        }

        if (nextPotentialLevel != null)
        {
            nextLevelName = nextPotentialLevel.Name;

            //Bounds.Left is the X value of the left side of the room, Bounds.Top is the Y value of the top of the room
            nextLevelPos = new Vector2(nextPotentialLevel.Bounds.Left, nextPotentialLevel.Bounds.Top);
        }
    }

    //---

}