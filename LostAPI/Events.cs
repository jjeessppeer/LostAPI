using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Serialization;
using System.IO;


namespace Lost
{
    public abstract class Event
    {
        //public int time;
        public readonly string eventType;
        public Event() { }
        public Event(string eventType)
        {
            this.eventType = eventType;
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

    public class BulletEvent : Event
    {
        float x, y, z;
        public BulletEvent() { }
        public BulletEvent(float x, float y, float z)
        {

        }
    }
    //public class RepairEvent : Event
    //{
    //    float delay;
    //    float healthRestored;
    //}
}
