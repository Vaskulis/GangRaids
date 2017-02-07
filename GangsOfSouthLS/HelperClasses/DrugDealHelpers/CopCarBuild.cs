using System.Collections.Generic;

namespace GangsOfSouthLS.HelperClasses.DrugDealHelpers
{
    internal class CopCarBuild
    {
        private string CarName;
        private List<int> SeatIndicesToOccupy;
        private List<string> PedNameList;
        private Dictionary<string, List<string>> WeaponDict;

        internal CopCarBuild(string carName, List<string> pedNameList, Dictionary<string, List<string>> weaponDict, List<int> seatIndicesToOccupy)
        {
            this.CarName = carName;
            this.PedNameList = pedNameList;
            this.WeaponDict = weaponDict;
            this.SeatIndicesToOccupy = seatIndicesToOccupy;
        }

        internal string carName { get { return CarName; } }
        internal List<int> seatIndicesToOccupy { get { return SeatIndicesToOccupy; } }
        internal List<string> pedNameList { get { return PedNameList; } }
        internal Dictionary<string, List<string>> weaponDict { get { return WeaponDict; } }
    }
}