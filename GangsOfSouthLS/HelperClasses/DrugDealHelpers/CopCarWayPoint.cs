using GangsOfSouthLS.HelperClasses.CommonUtilities;

namespace GangsOfSouthLS.HelperClasses.DrugDealHelpers
{
    internal class CopCarWayPoint
    {
        internal CopCarWayPoint(string description, Pos4 startPoint, Pos4 endPoint, string direction)
        {
            Description = description;
            StartPoint = startPoint;
            EndPoint = endPoint;
            Direction = direction;
        }

        internal string Description { get; private set; }
        internal Pos4 StartPoint { get; private set; }
        internal Pos4 EndPoint { get; private set; }
        internal string Direction { get; private set; }
    }
}