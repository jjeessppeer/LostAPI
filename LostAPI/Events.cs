using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Serialization;
using System.IO;

using Muse.Networking;
using HarmonyLib;
using UnityEngine;

namespace Lost
{
    public abstract class Event
    {
        private static float s_timerStart = Time.time;
        public static void ResetTimer() { s_timerStart = Time.time; }

        public readonly float EventTime;
        public readonly string eventType;
        public Event() { }
        public Event(string eventType)
        {
            this.eventType = eventType;
            EventTime = Time.time - s_timerStart;
        }
    }
    public class MissionEvent : Event
    {
        //public int teams;
        //public string map;
        //public int mapId;
        //public int[] scores;
        //public bool ended;
        //public readonly string missionType;
        //public readonly bool ended;
        public readonly int teams;
        public readonly int[] scores;
        public readonly bool ended;
        public MissionEvent() { }
        public MissionEvent(int teams, int[] scores, bool ended, string eventType) : base(eventType)
        {
            this.teams = teams;
            this.scores = scores;
            this.ended = ended;
        }

        public string ToString()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(this.GetType());
            using (StringWriter textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, this);
                return textWriter.ToString();
            }
        }

        public static MissionEvent LoadEventFromMission(Mission mission, string eventType)
        {
            int teams = mission.numberOfTeams;
            int[] scores = new int[teams];
            bool ended = mission.HadEnded;
            for (int i = 0; i < teams; ++i) scores[i] = mission.TeamScore(i);
            return new MissionEvent(teams, scores, ended, eventType);
        }
    }

    /* 
     */
    public class TurretFireEvent : Event
    {
        public readonly Turret SoruceTurret;

    }


    /* Event used for recording data on projectile hit.
     */
    public class HitEvent : Event
    {
        //public readonly int TurretId;
        //public readonly int ShooterId;
        //public readonly int TargetShipId;
        //public readonly int TargetComponentId;

        public readonly Turret SourceTurret;
        public readonly NetworkedPlayer Shooter;
        public readonly Repairable[] DestroyedComponents;

        public readonly HitData[] Hits;

        public readonly float Distance;
        public readonly int DamageDealt;

        public struct HitData
        {
            //public readonly int TargetShipId;
            //public readonly int TargetComponentId;
            public readonly int DamageDealt;
            public readonly float Distance;
            public readonly bool ComponentDestroyed;

            public readonly Repairable TargetComponent;
            public readonly Ship TargetShip;

            public HitData(int damageDealt, float distance, Repairable targetComponent, Ship targetShip)
            {
                DamageDealt = damageDealt;
                Distance = distance;
                ComponentDestroyed = targetComponent.NoHealth;
                TargetComponent = targetComponent;
                TargetShip = targetShip;
            }
            public string ToString()
            {
                return $"Target: {TargetShip.ShipId} {TargetComponent.ItemId} {TargetComponent.Type} Dist: {Distance} Dam: {DamageDealt} Destoyed: {ComponentDestroyed}";
            }
        }
        

        public HitEvent() { }
        public HitEvent(Turret sourceTurret, NetworkedPlayer shooter, HitData[] hits)
        {
            SourceTurret = sourceTurret;
            Shooter = shooter;
            Hits = hits;

            // Find avreage distance. Add destroyed components.
            Distance = 0;
            int nDestoyedComponents = 0;
            foreach (HitData hit in hits)
            {
                Distance += hit.Distance;
                DamageDealt += hit.DamageDealt;
                if (hit.ComponentDestroyed) nDestoyedComponents += 1;
            }
            Distance /= hits.Length;

            // Add the destroyed compoenents to array.
            DestroyedComponents = new Repairable[nDestoyedComponents];
            int i = 0;
            foreach (HitData hit in hits)
            {
                if (hit.ComponentDestroyed) DestroyedComponents[i++] = hit.TargetComponent;
            }
        }

        public string ToString()
        {
            string hitsString = "";
            foreach (HitData hit in Hits)
            {
                hitsString += $"\n\t{hit.ToString()}";
            }
            return $"Shooter: {Shooter.UserId} Hits: {Hits.Length} Damage: {DamageDealt} Breaks: {DestroyedComponents.Length} {hitsString}";
        }

        // Create and return a new HitEvent based on a valid MuseEvent and Turret.
        public static HitEvent LoadEvent(MuseEvent evt, Turret sourceTurret)
        {
            if (evt.Action != 1) throw new Exception("Invalid muse event for hit");

            NetworkedPlayer shooter = null;

            int nHits = (int)evt.GetInteger(0);
            HitData[] hits = new HitData[nHits]; 
            for (int i = 0; i < nHits; i++)
            {
                int damage = (int)evt.GetInteger(i * 10 + 2);

                // Find the GameObject hit based on identifier.
                int targetId = (int)evt.GetInteger(i * 10 + 1);
                Transform targetTransform = MuseWorldObject.FindByNetworkId(targetId);
                GameObject targetGO = targetTransform.gameObject;

                // Find the Repairable object that was hit.
                Component[] components = targetGO.GetComponents<MonoBehaviour>();
                Repairable hitComponent = null;
                foreach (Component c in components)
                {
                    // TODO: does this detect broken armor? Is that even a repairable?
                    if (c is Engine || c is Hull || c is Turret || c is Balloon)
                    {
                        hitComponent = (Repairable)c;
                        break;
                    }
                }

                // Make sure valid target component was found.
                if (hitComponent == null) throw new Exception("Invalid component hit!");

                Ship hitShip = hitComponent.Ship;

                long shooterId = evt.GetInteger(i * 10 + 6);
                //NetworkedPlayer shooterPlayer = NetworkedPlayer.ByUserId[(int)shooterId];
                if (shooter == null) shooter = NetworkedPlayer.ByUserId[(int)shooterId];

                float distance = (sourceTurret.transform.position - hitComponent.transform.position).magnitude;

                hits[i] = new HitData(damage, distance, hitComponent, hitShip);

                //Muse.Vector3 fixedVector = evt.GetFixedVector(i * 10 + 3, 2);
                //int num2 = (int)evt.GetInteger(i * 10 + 4);
                //if (transform == null && evt.IsField(i * 10 + 5))
                //{
                //    fixedVector = evt.GetFixedVector(i * 10 + 5, 2);
                //}
                //bool hitCore = (num2 & 1) > 0;
                //bool hitWeakness = (num2 & 2) > 0;
                //bool hitProtection = (num2 & 4) > 0;
                //bool ownedByLocalUser = NetworkedPlayer.Local != null && NetworkedPlayer.Local.UserId != -1 && (long)NetworkedPlayer.Local.UserId == evt.GetInteger(i * 10 + 6);

                //this.OnWeaponHit(transform, new UnityEngine.Vector3(fixedVector.x, fixedVector.y, fixedVector.z), damage, hitCore, hitWeakness, hitProtection, ownedByLocalUser);
            }
            return new HitEvent(sourceTurret, shooter, hits);
        }
    }
    //public class RepairEvent : Event
    //{
    //    float delay;
    //    float healthRestored;
    //}
}
