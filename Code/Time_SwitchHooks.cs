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
using System.Collections.Generic;
using System.Collections;
using System.Linq;



namespace Celeste.Mod.Time_Switch;

public static class Time_SwitchHooks {


    //---Class-wide vars--- 

    //scene is what is currently being displayed (menu, level, etc)
    public static Scene Scene { get; private set; }

    //vars for using any 2 characters for timeline
    private static string timelineStartA;
    private static string timelineStartB;
    private static string timelineEndA;
    private static string timelineEndB;

    private static string nextLevelName;

    private static Vector2? nextLevelPos;

    //the position in world space the player should teleport to
    private static Vector2 nextPlayerPosAbsolute;

    //used to activate/deactivate the IL hooks
    private static bool correctionILsActive = false;

    //creates var for manual IL hook
    private static ILHook CancelDashRefillHook;

    private static bool popupShown = false;

    private static bool TimelineDetectionFormat1Fail = false;
    private static bool TimelineDetectionFormat2Fail = false;

    //---



    internal static void Load() 
    {
        On.Celeste.LevelLoader.StartLevel += LevelLoader_StartLevel;
        On.Celeste.Player.Update += Player_Update;
        IL.Celeste.Level.TeleportTo += Level_TeleportTo;
        CancelDashRefillHook = new(typeof(Player).GetMethod("orig_Added", BindingFlags.Public | BindingFlags.Instance), Time_SwitchHooks.CancelDashReset);
        On.Celeste.LevelExit.Begin += LevelExit_Begin;
    }



    internal static void Unload() 
    {
        On.Celeste.LevelLoader.StartLevel -= LevelLoader_StartLevel;
        On.Celeste.Player.Update -= Player_Update;
        IL.Celeste.Level.TeleportTo -= Level_TeleportTo;
        CancelDashRefillHook.Dispose();
        On.Celeste.LevelExit.Begin -= LevelExit_Begin;
    }



    //---changing settings on map load---
    private static void LevelLoader_StartLevel(On.Celeste.LevelLoader.orig_StartLevel orig, LevelLoader self)
    {
        orig(self);

        //save user settings - redundant unless user accessiblility for settings or trigger code is changed
        Time_SwitchModule.Settings.userRoomNameFormat = Time_SwitchModule.Settings.RoomNameFormat;
        Time_SwitchModule.Settings.userLegacyTimelines = Time_SwitchModule.Settings.LegacyTimelines;


        //applies room name format from save file
        if (!Time_SwitchModule.SaveData.firstLoad)
        {
            Time_SwitchModule.Settings.RoomNameFormat = Time_SwitchModule.SaveData.RoomNameFormat;

            Logger.Log(LogLevel.Info, "Time Switch", "Room name format was applied from save file");
        }
        else
        {
            Time_SwitchModule.Settings.RoomNameFormat = Time_SwitchModule.Settings.userRoomNameFormat;

            if (Time_SwitchModule.Settings.RoomNameFormat != Time_Switch.FormatMode.off)
            {
                Logger.Log(LogLevel.Info, "Time Switch", "Using user set room name format (unless changed by map)");
            }
        }

        //applies Timeline type from save file or session
        if (Time_SwitchModule.SaveData.LegacyTimelines == Time_Switch.TimelineTypes.legacySaveFile)
        {
            Time_SwitchModule.Settings.LegacyTimelines = Time_SwitchModule.SaveData.LegacyTimelines;
        }
        if (Time_SwitchModule.Session.LegacyTimelines == Time_Switch.TimelineTypes.legacySaveSession)
        {
            Time_SwitchModule.Settings.LegacyTimelines = Time_SwitchModule.Session.LegacyTimelines;
        }

        //runs everytime to be nicer to in-development maps
        DetectTimelines(self);

        if (Time_SwitchModule.Session.disableKeybind != default)
        {
            Time_SwitchModule.Settings.DisableKeyBind = (bool)Time_SwitchModule.Session.disableKeybind;
        }
    }



    private static void DetectTimelines(LevelLoader self)
    {
        List<char> firstCharList = [];
        List<char> lastCharList = [];

        foreach (LevelData level in self.session.MapData.Levels)
        {
            if (level.Name[0] != '_' && level.Name[0] != '-')
            {
                firstCharList.Add(level.Name[0]);
            }
            if (level.Name[^1] != '_' && level.Name[^1] != '-')
            {
                lastCharList.Add(level.Name[^1]);
            }
        }

        List<char> mostCommonFirstChars = firstCharList.FindModes().ToList();
        List<char> mostCommonLastChars = lastCharList.FindModes().ToList();

        if (mostCommonFirstChars.Count == 2)
        {
            timelineStartA = mostCommonFirstChars[0].ToString();
            timelineStartB = mostCommonFirstChars[1].ToString();
        }
        else
        {
            TimelineDetectionFormat1Fail = true;
        }
        if (mostCommonLastChars.Count == 2)
        {
            timelineEndA = mostCommonLastChars[0].ToString();
            timelineEndB = mostCommonLastChars[1].ToString();
        }
        else
        {
            TimelineDetectionFormat2Fail = true;
        }

        if (TimelineDetectionFormat1Fail && TimelineDetectionFormat2Fail)
        {
            Logger.Log(LogLevel.Warn, "Time Switch", "Timeline detection failed for both room name formats");
        }

        if (mostCommonFirstChars.Count != 2 && Time_SwitchModule.Settings.RoomNameFormat == Time_Switch.FormatMode.Format1
            || mostCommonLastChars.Count != 2 && Time_SwitchModule.Settings.RoomNameFormat == Time_Switch.FormatMode.Format2)
        {
            Logger.Log(LogLevel.Warn, "Time Switch", $"The room name format setting is set to {Time_SwitchModule.Settings.RoomNameFormat} - "
                + Dialog.Clean($"Time_Switch_Setting_RoomNameFormat_Option_{Enum.GetName(Time_SwitchModule.Settings.RoomNameFormat)}")
                + " which doesn't match the timeline position in the map's room names or number of timelines");
        }
    }



    //+++returns most common characters+++ stolen from https://stackoverflow.com/a/16850071
    internal static IEnumerable<T> FindModes<T>(this IEnumerable<T> input)
    {
        var list = input.ToLookup(x => x);
        if (list.Count == 0)
            return Enumerable.Empty<T>();
        var maxCount = list.Max(x => x.Count());
        return list.Where(x => x.Count() == maxCount).Select(x => x.Key);
    }
    //+++

    //---



    //hook that teleports the player, hooks Update instead of orig_Update because it's "safer"
    private static void Player_Update(On.Celeste.Player.orig_Update orig, Player self)
    {
        //calls Update
        orig(self);

        popupShown = false;

        bool inputValid = (Time_SwitchModule.Settings.RoomNameFormat != Time_Switch.FormatMode.off && self.InControl && Time_SwitchModule.Settings.TimeSwitchBind.Pressed) ? true : false;

        if (inputValid && !Time_SwitchModule.Settings.DisableKeyBind || Time_SwitchModule.Session.forceTimeSwitch == true)
        {
            Logger.Log(LogLevel.Verbose, "Time Switch", "Teleport triggered");

            Time_SwitchModule.Session.forceTimeSwitch = false;

            //activates ILs
            correctionILsActive = true;

            TeleportPlayer(self);

            Time_SwitchModule.Settings.TimeSwitchBind.ConsumePress();
        }
        else if (Time_SwitchModule.Settings.RoomNameFormat == Time_Switch.FormatMode.off && self.InControl && Time_SwitchModule.Settings.TimeSwitchBind.Pressed)
        {
            Popup.Show("You can't time switch");
            popupShown = true;
        }
        else if (inputValid && Time_SwitchModule.Settings.DisableKeyBind)
        {
            Popup.Show("You don't have control of time switch");
            popupShown = true;
        }
    }


    //---Teleporting the player---
    private static void TeleportPlayer(Player self)
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

        if (nextLevelTimeline != null && currentLevelIdentifier != null)
        {
            //finds room on the other timeline with the same identifier
            FindNextLevel(level, nextLevelTimeline, currentLevelIdentifier);
        }
        else
        {
            nextLevelName = null;
            nextLevelPos = null;
        }

        if (nextLevelName != null && nextLevelPos != null)
        {
            Vector2 pos = (Vector2)nextLevelPos;
            //gets player position to teleport to in world space
            nextPlayerPosAbsolute = new Vector2(pos.X + playerPosRelativeToLevel.X, pos.Y + playerPosRelativeToLevel.Y);

            //at the end of the frame it teleports the player to the new room with the new player position
            //note: the intro type must be Transition otherwise TeleportTo uses different code which spawns the player differently and skips the IL hooked code
            level.OnEndOfFrame += () => { level.TeleportTo(self, nextLevelName, Player.IntroTypes.Transition, nextPlayerPosAbsolute); };
        }
        else
        {
            if (!popupShown)
            {
                Popup.Show("Time Switch Failed");
            }

            Logger.Log(LogLevel.Error, "Time Switch", "Time Switch failed - Did not find a room to teleport to");

            correctionILsActive = false;
        }
    }



    //returns the character used for the other timeline
    private static string TimelinePicker(string currentLevelName)
    {
        bool legacyActive = false;
        if (Time_SwitchModule.Settings.LegacyTimelines != Time_Switch.TimelineTypes.auto)
        {
            timelineStartA = timelineEndA = "a";
            timelineStartB = timelineEndB = "b";

            legacyActive = true;
        }


        if (Time_SwitchModule.Settings.RoomNameFormat == Time_Switch.FormatMode.Format1 && legacyActive
            || Time_SwitchModule.Settings.RoomNameFormat == Time_Switch.FormatMode.Format1 && !TimelineDetectionFormat1Fail)
        {
            if (RoomDisabled(currentLevelName))
            {
                return null;
            }
            else if (currentLevelName.StartsWith(timelineStartA))
            {
                return timelineStartB;
            }
            else if (currentLevelName.StartsWith(timelineStartB))
            {
                return timelineStartA;
            }
            return null;
        }
        

        if (Time_SwitchModule.Settings.RoomNameFormat == Time_Switch.FormatMode.Format2 && legacyActive
            || Time_SwitchModule.Settings.RoomNameFormat == Time_Switch.FormatMode.Format2 && !TimelineDetectionFormat2Fail)
        {
            if (RoomDisabled(currentLevelName))
            {
                return null;
            }
            if (currentLevelName.EndsWith(timelineEndA))
            {
                return timelineEndB;
            }
            else if (currentLevelName.EndsWith(timelineEndB))
            {
                return timelineEndA;
            }
            return null;
        }


        if (TimelineDetectionFormat1Fail || TimelineDetectionFormat2Fail)
        {
            Logger.Log(LogLevel.Error, "Time Switch", $"Timeline check failed - automatic detection could not assign the timeline values for {Time_SwitchModule.Settings.RoomNameFormat} - "
                + Dialog.Clean($"Time_Switch_Setting_RoomNameFormat_Option_{Enum.GetName(Time_SwitchModule.Settings.RoomNameFormat)}"));
        }

        return null;
    }



    private static bool RoomDisabled(string currentLevelName)
    {
        if (currentLevelName.StartsWith("_") || currentLevelName.StartsWith("-"))
        {
            Popup.Show("You can't time switch in this room");
            popupShown = true;
            return true;
        }
        else
        {
            return false;
        }
    }



    private static string FetchCurrentLevelIdentifier(string currentLevelName)
    {
        if (currentLevelName.Length >= 3)
        {
            //if the "clean" format is used it reads the first character as well
            string currentLevelNumberDigit0 = string.Empty;

            if (Time_SwitchModule.Settings.RoomNameFormat == Time_Switch.FormatMode.Format2 && currentLevelName.Length >= 4)
            {
                currentLevelNumberDigit0 = currentLevelName[0].ToString();
            }
            else if (Time_SwitchModule.Settings.RoomNameFormat == Time_Switch.FormatMode.Format2 && !(currentLevelName.Length >= 4))
            {
                Logger.Log(LogLevel.Error, "Time Switch", "room name is too short for format 2 - clean, attempting to use format 1 - simple identifier");
            }

            string currentLevelNumberDigit1 = currentLevelName[1].ToString();
            string currentLevelNumberDigit2 = currentLevelName[2].ToString();

            //adds characters to a single string
            return currentLevelNumberDigit0 + currentLevelNumberDigit1 + currentLevelNumberDigit2;
        }
        else
        {
            Logger.Log(LogLevel.Error, "Time Switch", "Room name is too short");

            return null;
        }
    }


    private static void FindNextLevel(Level level, string nextLevelTimeline, string currentLevelIdentifier)
    {
        //MapData.levels is a list of LevelData of all rooms in the map
        MapData currentMapData = level.Session.MapData;

        LevelData nextPotentialLevel = null;

        if (Time_SwitchModule.Settings.RoomNameFormat == Time_Switch.FormatMode.Format1 && nextLevelTimeline != null && currentLevelIdentifier != null)
        {
            //finds the LevelData for the room that has the correct first character and same identifier
            nextPotentialLevel = currentMapData.Levels.Find(item => item.Name[0] == nextLevelTimeline[0] && item.Name[1..3] == currentLevelIdentifier);
        }

        if (Time_SwitchModule.Settings.RoomNameFormat == Time_Switch.FormatMode.Format2 && nextLevelTimeline != null && currentLevelIdentifier != null)
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
        else
        {
            nextLevelName = null;
            nextLevelPos = null;
        }
    }

    //---

    


    //---Corrects position from nearest spawn to same position in the room as the player was before teleporting---
    private static void Level_TeleportTo(MonoMod.Cil.ILContext il)
    {
        ILCursor cursor = new(il);

        cursor.GotoNext(MoveType.After, instr => instr.MatchLdflda<Session>("RespawnPoint"));

        cursor.Index++;

        cursor.EmitDelegate(GetPlayerPos);

        cursor.Index++;

        //doesn't set active false so it's still true for other IL hook
    }


    private static Vector2 GetPlayerPos(Vector2 orig_playerPos)
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


    //---turns off mod when exiting map---
    private static void LevelExit_Begin(On.Celeste.LevelExit.orig_Begin orig, LevelExit self)
    {
        orig(self);

        Time_SwitchModule.SaveData.firstLoad = false;

        //reapply user settings
        if (!Time_SwitchModule.SaveData.defaultRoomNameFormat)
        {
            Time_SwitchModule.Settings.RoomNameFormat = Time_SwitchModule.Settings.userRoomNameFormat;
        }
        else
        {
            Time_SwitchModule.Settings.userRoomNameFormat = Time_SwitchModule.Settings.RoomNameFormat;
        }

        if (!Time_SwitchModule.SaveData.defaultLegacyTimelines && !Time_SwitchModule.Session.defaultLegacyTimelines)
        {
            Time_SwitchModule.Settings.LegacyTimelines = Time_SwitchModule.Settings.userLegacyTimelines;
        }
        else if (Time_SwitchModule.SaveData.defaultLegacyTimelines && Time_SwitchModule.Session.defaultLegacyTimelines)
        {
            Time_SwitchModule.Settings.userLegacyTimelines = Time_SwitchModule.Settings.LegacyTimelines; //redundant unless user accessiblility for setting is changed
        }

        if (Time_SwitchModule.Session.disableKeybind != default)
        {
            Time_SwitchModule.Settings.DisableKeyBind = default;
        }
    }

    //---

}