using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WAT
{
    public class Appointment
    {

        private int _AppointmentID;
        private string _Subject;
        private string _Location;
        private string _Details;
        private System.Nullable<System.DateTime> _StartTime;
        private System.Nullable<System.DateTime> _EndTime;

        private System.DateTime _reccreatedDate;

        public Appointment()
            : base()
        {
        }

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
                    this._StartTime = value;
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
                    this._EndTime = value;
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
    }
}
