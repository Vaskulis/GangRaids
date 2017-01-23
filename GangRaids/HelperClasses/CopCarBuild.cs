using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GangRaids.HelperClasses
{
    class CopCarBuild
    {
        private string CarName;
        private List<int> SeatIndicesToOccupy;
        private List<string> PedNameList;
        private Dictionary<string, List<string>> WeaponDict;

        public CopCarBuild(string carName, List<string> pedNameList, Dictionary<string, List<string>> weaponDict, List<int> seatIndicesToOccupy)
        {
            this.CarName = carName;
            this.PedNameList = pedNameList;
            this.WeaponDict = weaponDict;
            this.SeatIndicesToOccupy = seatIndicesToOccupy;
        }
        
        public string carName { get { return CarName; } }
        public List<int> seatIndicesToOccupy { get { return SeatIndicesToOccupy; } }
        public List<string> pedNameList { get { return PedNameList; } }
        public Dictionary<string, List<string>> weaponDict { get { return WeaponDict; } }
    }
}
