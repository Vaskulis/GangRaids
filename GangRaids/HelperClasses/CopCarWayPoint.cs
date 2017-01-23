using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;

namespace GangRaids.HelperClasses
{
    class CopCarWayPoint
    {
        private string Description;
        private Pos4 StartPoint;
        private Pos4 EndPoint;

        public CopCarWayPoint(string description, Pos4 startPoint, Pos4 endPoint)
        {
            this.Description = description;
            this.StartPoint = startPoint;
            this.EndPoint = endPoint;
        }

        public string description { get { return Description; } }
        public Pos4 startPoint { get { return StartPoint; } }
        public Pos4 endPoint { get { return EndPoint; } }
    }
}
