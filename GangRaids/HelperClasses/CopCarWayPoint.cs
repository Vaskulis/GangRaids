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
        private string Direction;

        internal CopCarWayPoint(string description, Pos4 startPoint, Pos4 endPoint, string direction)
        {
            this.Description = description;
            this.StartPoint = startPoint;
            this.EndPoint = endPoint;
            this.Direction = direction;
        }

        internal string description { get { return Description; } }
        internal Pos4 startPoint { get { return StartPoint; } }
        internal Pos4 endPoint { get { return EndPoint; } }
        internal string direction { get { return Direction; } }
    }
}
