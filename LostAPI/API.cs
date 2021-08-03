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


namespace Lost
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

            //eventSubscriberFire = new EventSubscriber<UserAvatarEntity, UserEventOnWeaponFire>();
            //eventSubscriberFire.OnEvent = delegate(UserAvatarEntity user, UserEventOnWeaponFire evt)
            //{
            //    FileLog.Log($"USER EVENT ON WEAPON FIRE");
            //    //if (evt.IsInMatch && evt.IsActor(user))
            //    //{
            //    //    UserStatsWrapper stats = user.Stats;
            //    //    int inc = 1;
            //    //    stats.Add(evt.Match.CreatedGameType, GameStatType.Shots, user.CurrentClass, 0, inc);
            //    //}
            //};
            //EventSubscriber<UserAvatarEntity, UserEventOnWeaponFire> eventSubscriber = new EventSubscriber<UserAvatarEntity, UserEventOnWeaponFire>();

        }

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
            


            FileLog.Log($"MISSION STARTED");

            _activeMission = __instance;
            DispatchEvent(MissionEvent.LoadEventFromMission(__instance, "match_start"));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Mission), "OnRemoteUpdate")]
        private static void MissionUpdated(Mission __instance)
        {
            FileLog.Log($"MISSION UPDATED");
            DispatchEvent(MissionEvent.LoadEventFromMission(__instance, "match_update"));
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Mission), "OnDisable")]
        private static void MissionEnded(Mission __instance)
        {
            FileLog.Log($"MISSION ENDED");
            DispatchEvent(MissionEvent.LoadEventFromMission(__instance, "match_end"));
            _activeMission = null;
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(Turret), "Fire")]
        [HarmonyPatch(typeof(Turret), "ContinuousFireTriggered")]
        private static void BulletFired()
        {

            FileLog.Log($"TURRET FIRED");
            //NetworkedPlayer shooter = ___turretLaunchedFrom.UsingPlayer;
            //int shooterId = shooter.UserId;
            //int turretId = ___turretLaunchedFrom.ItemId;
            //FileLog.Log($"Shell Laynched\n\tshooter: {shooterId}\n\tturret: {turretId}");

        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Turret), "OnWeaponHit")]
        private static void TurretHit()
        {

            FileLog.Log($"__HIT__");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Turret), "OnCustomEvent")]
        private static void TurretEvt(int senderId, MuseEvent evt, Turret __instance)
        {
            FileLog.Log($"Turret event {evt.Action}\n{evt.ToString()}");
            if (evt.Action == 1)
            {
                HitEvent hitEvt = HitEvent.LoadEvent(evt, __instance);
                FileLog.Log(hitEvt.ToString());
            }
        }

        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(Turret), "ContinuousFireTriggered")]
        //private static void BulletDryFired()
        //{

        //    FileLog.Log($"TURRET CONT FIRE TRIGGERED");


        //}

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
            FileLog.Log($"0Dispatching event: {ev.eventType}");
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

    }
}
