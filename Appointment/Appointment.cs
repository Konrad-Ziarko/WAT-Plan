using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Appointment
{
    [Serializable]
    public class Appointment
    {
        [XmlIgnore]
        private static int nextID;

        public int _AppointmentID { get; set; }
        public string _Subject { get; set; }
        public string _Short { get; set; }
        public string _Location { get; set; }
        public string _Details { get; set; }
        public DateTime _StartTime { get; set; }
        public DateTime _EndTime { get; set; }
        public DateTime _reccreatedDate { get; set; }
        public int no_Serialize { get { return nextID; } set { nextID = value; } }
        public int BGA { get; set; }
        public int BBA { get; set; }
        public int BGR { get; set; }
        public int BBR { get; set; }
        public int BGG { get; set; }
        public int BBG { get; set; }
        public int BGB { get; set; }
        public int BBB { get; set; }

        public Appointment() : base()
        {
            _AppointmentID = nextID++;
            _Subject = "";
            _Short = "";
            _Location = "";
            _Details = "";
            _StartTime = DateTime.MinValue;
            _EndTime = DateTime.MinValue;
            _reccreatedDate = DateTime.MinValue;
            BGA = Color.LightGreen.A;
            BGR = Color.LightGreen.R;
            BGG = Color.LightGreen.G;
            BGB = Color.LightGreen.B;
            BBA = Color.DarkGray.A;
            BBR = Color.DarkGray.R;
            BBG = Color.DarkGray.G;
            BBB = Color.DarkGray.B;
        }

        public Appointment(string _Subject, string _Short, string _Location, string _Details, DateTime _StartTime, DateTime _EndTime)
        {
            _AppointmentID = nextID++;
            this._Subject = _Subject;
            this._Short = _Short;
            this._Location = _Location;
            this._Details = _Details;
            this._StartTime = _StartTime;
            this._EndTime = _EndTime;
            this._reccreatedDate = DateTime.MinValue;
        }
        public bool IsEqualTo(Appointment compareTo)
        {
            return (_Subject == compareTo._Subject && _Location == compareTo._Location && _Details == compareTo._Details && _StartTime == compareTo._StartTime && _EndTime == compareTo._EndTime && _reccreatedDate == compareTo._reccreatedDate);
        }
        #region VB

        public int AppointmentID
        {
            get { return this._AppointmentID; }
            set
            {
                if (((this._AppointmentID == value) == false))
                {
                    this._AppointmentID = value;
                }
            }
        }

        public string Subject
        {
            get { return this._Subject; }
            set
            {
                if ((string.Equals(this._Subject, value) == false))
                {
                    this._Subject = value;
                }
            }
        }

        public string Location
        {
            get { return this._Location; }
            set
            {
                if ((string.Equals(this._Location, value) == false))
                {
                    this._Location = value;
                }
            }
        }

        public string Details
        {
            get { return this._Details; }
            set
            {
                if ((string.Equals(this._Details, value) == false))
                {
                    this._Details = value;
                }
            }
        }

        public System.Nullable<System.DateTime> StartTime
        {
            get { return this._StartTime; }
            set
            {
                if ((this._StartTime.Equals(value) == false))
                {
                    this._StartTime = (DateTime)value;
                }
            }
        }

        public System.Nullable<System.DateTime> EndTime
        {
            get { return this._EndTime; }
            set
            {
                if ((this._EndTime.Equals(value) == false))
                {
                    this._EndTime = (DateTime)value;
                }
            }
        }

        public System.DateTime reccreatedDate
        {
            get { return this._reccreatedDate; }
            set
            {
                if (((this._reccreatedDate == value) == false))
                {
                    this._reccreatedDate = value;
                }
            }
        }
        #endregion

    }
}
