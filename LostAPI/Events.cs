using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lost
{
    public abstract class Event
    {
        //public int time;
        public string eventType;
        public Event(string eventType)
        {
            this.eventType = eventType;
        }
    }
    public abstract class MatchEvent : Event
    {
        //public int teams;
        //public string map;
        //public int mapId;
        //public int[] scores;
        //public bool ended;
        public MatchEvent(string eventType = "match_event") : base(eventType)
        {

        }
    }
    public class MatchEndEvent : MatchEvent
    {
        //public int winner;
        public MatchEndEvent() : base("match_end")
        {

        }
    }
    public class MatchStartEvent : MatchEvent
    {
        public MatchStartEvent() : base("match_start")
        {

        }
    }
    //public class ProjectileEvent : Event
    //{
    //    bool hit;
    //}
    //public class RepairEvent : Event
    //{
    //    float delay;
    //    float healthRestored;
    //}
}
