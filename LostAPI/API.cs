using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using BepInEx;
using HarmonyLib;
using UnityEngine;


namespace Lost
{
    [HarmonyPatch]
    public static class LostAPI
    {

        //private static List<Action<Event>> eventCallbacks;
        private static Dictionary<string, List<Action<Event>>> _eventCallbacks;
        private static readonly string[] _validEvents = { "match_start", "match_end" };

        //private static MatchStatistics _missionStats = null;

        private static Mission _activeMission = null;
        private static int _missionType = -1;

        static LostAPI()
        {
            FileLog.Log("Initializing Lost API");

            // Intitialize event callback dictionary.
            // Each event type gets a list where callback functions can be added.
            _eventCallbacks = new Dictionary<string, List<Action<Event>>>();
            foreach (string eventStr in _validEvents)
            {
                _eventCallbacks.Add(eventStr, new List<Action<Event>>());
            }
            //_activeMission = null;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Mission), "Start")]
        private static void MissionStarted(Mission __instance)
        {
            FileLog.Log("Mission started.");
            if (__instance is Deathmatch)
            {
                FileLog.Log("IS DM");
            }
            if (__instance is PracticeMission)
            {
                FileLog.Log("IS Practice");
            }
            if (__instance is Mission)
            {
                FileLog.Log("IS MISSION");
            }

            _activeMission = __instance;
            FileLog.Log($"\tMapId: {_activeMission.mapId}\n\tMapName: {_activeMission.mapName}\n\tObjectives: {_activeMission.mapObjectives}");
            DispatchEvent(new MatchStartEvent());
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Mission), "OnDisable")]
        private static void MissionEnded(Mission __instance)
        {
            FileLog.Log("Mission ended.");
            FileLog.Log($"\tEnded: {_activeMission.HadEnded} \n\tScore: {_activeMission.TeamScore(0)} ");
            _activeMission = null;

            DispatchEvent(new MatchEndEvent());
        }

        public static bool MatchRunning()
        {
            return _activeMission != null;
        }

        public static void AddEventListener(string eventStr, Action<Event> eventCallback)
        {
            if (!(Array.Exists(_validEvents, element => element == eventStr)))
            {
                throw new ArgumentException("Invalid event string");
            }

            _eventCallbacks[eventStr].Add(eventCallback);
        }

        private static void DispatchEvent(Event ev)
        {
            if (!(Array.Exists(_validEvents, element => element == ev.eventType)))
            {
                throw new ArgumentException("Invalid event string");
            }
            FileLog.Log($"Dispatching event: {ev.eventType}");
            foreach (Action<Event> callback in _eventCallbacks[ev.eventType])
            {
                callback(ev);
            }
        }

        private static void MatchStarted(Mission mission, int missionType)
        {
            _missionType = missionType;
            _activeMission = mission;
        }
        private static void MissionEnded()
        {
            _activeMission = null;
            _missionType = -1;
        }

    }
}
