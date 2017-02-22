using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace WAT
{
    [Serializable]
    public class SingleEvent
    {
        [XmlAttribute]
        public string eventName { get; set; }
        [XmlAttribute]
        public string eventPlace { get; set; }
        [XmlAttribute]
        public DateTime eventStart { get; set; }
        [XmlAttribute]
        public DateTime eventStop { get; set; }
        public SingleEvent()
        {
            this.eventName = "";
            this.eventPlace = "";
            this.eventStart = DateTime.MinValue;
            this.eventStop = DateTime.MinValue;
        }

        public SingleEvent(string eventName, string eventPlace, DateTime eventStart, DateTime eventStop)
        {
            this.eventName = eventName;
            this.eventPlace = eventPlace;
            this.eventStart = eventStart;
            this.eventStop = eventStop;
        }

        public SingleEvent(string eventName, string eventPlace, string sEventStart, string sEventStop)
        {
            this.eventName = eventName;
            this.eventPlace = eventPlace;
            this.eventStart = DateTime.ParseExact(sEventStart, "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture);
            this.eventStop = DateTime.ParseExact(sEventStop, "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture);
        }
        public bool IsEqualTo(SingleEvent compareTo)
        {
            return (eventName == compareTo.eventName && eventPlace == compareTo.eventPlace && eventStart == compareTo.eventStart && eventStop == compareTo.eventStop);
        }
    }
}
