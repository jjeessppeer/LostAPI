using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using BepInEx;
using HarmonyLib;
using UnityEngine;
//using Muse;
using Muse.Networking;


namespace LostAPI
{
    [HarmonyPatch]
    public static class LostAPI
    {

        //private static List<Action<Event>> eventCallbacks;
        private static Dictionary<string, List<Action<Event>>> _eventCallbacks;
        private static readonly string[] _validEvents = { 
            "match_start", "match_end", "match_update",
            "bullet_fired", "bullet_update", "bullet_hit"};

        //private static MatchStatistics _missionStats = null;

        private static Mission _activeMission = null;
        private static int _missionType = -1;

        //private static readonly EventSubscriber<UserAvatarEntity, UserEventOnWeaponFire> eventSubscriberFire;


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

        // Called on mission start.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Mission), "Start")]
        private static void MissionStarted(Mission __instance)
        {
            //FileLog.Log("Mission started.");
            //if (__instance is Deathmatch)
            //{
            //    FileLog.Log("IS DM");
            //}
            //if (__instance is PracticeMission)
            //{
            //    FileLog.Log("IS Practice");
            //}
            //if (__instance is Mission)
            //{
            //    FileLog.Log("IS MISSION");
            //}

            _activeMission = __instance;
            DispatchEvent(MissionEvent.LoadEventFromMission(__instance, "match_start"));
        }

        // Called upon change in mission status.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Mission), "OnRemoteUpdate")]
        private static void MissionUpdated(Mission __instance)
        {
            DispatchEvent(MissionEvent.LoadEventFromMission(__instance, "match_update"));
        }

        // Called upon mission end.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Mission), "OnDisable")]
        private static void MissionEnded(Mission __instance)
        {
            DispatchEvent(MissionEvent.LoadEventFromMission(__instance, "match_end"));
            _activeMission = null;
        }

        // Called on turret fire.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Turret), "Fire")]
        [HarmonyPatch(typeof(Turret), "ContinuousFireTriggered")]
        private static void BulletFired(Turret __instance)
        {
            TurretFireEvent fireEvt = TurretFireEvent.LoadEvent(__instance, "bullet_fired");
            DispatchEvent(fireEvt);
        }

        // Called on projectile hit.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Turret), "OnCustomEvent")]
        private static void ProjectileHit(int senderId, MuseEvent evt, Turret __instance)
        {
            if (evt.Action == 1)
            {
                HitEvent hitEvt = HitEvent.LoadEvent(evt, __instance, "bullet_hit");
                DispatchEvent(hitEvt);
            }
        }

        //public static bool MatchRunning()
        //{
        //    return _activeMission != null;
        //}

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
            FileLog.Log($"{ev.ToString()}");
            if (!(Array.Exists(_validEvents, element => element == ev.eventType)))
            {
                throw new ArgumentException("Invalid event type. Cannot dispatch.");
            }
            foreach (Action<Event> callback in _eventCallbacks[ev.eventType])
            {
                callback(ev);
            }
        }

    }
}
