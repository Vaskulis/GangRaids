using System.Collections.Generic;

namespace GangsOfSouthLS.HelperClasses.DrugDealHelpers
{
    internal class CopCarBuild
    {
        internal CopCarBuild(string carName, List<string> pedNameList, Dictionary<string, List<string>> weaponDict, List<int> seatIndicesToOccupy)
        {
            CarName = carName;
            PedNameList = pedNameList;
            WeaponDict = weaponDict;
            SeatIndicesToOccupy = seatIndicesToOccupy;
        }

        internal string CarName { get; private set; }
        internal List<int> SeatIndicesToOccupy { get; private set; }
        internal List<string> PedNameList { get; private set; }
        internal Dictionary<string, List<string>> WeaponDict { get; private set; }
    }
}