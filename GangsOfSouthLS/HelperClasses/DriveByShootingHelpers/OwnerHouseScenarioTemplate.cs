using GangsOfSouthLS.HelperClasses.CommonUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GangsOfSouthLS.HelperClasses.DriveByShootingHelpers
{
    internal class OwnerHouseScenarioTemplate
    {
        internal string Address;
        internal List<Pos4> PossibleCarSpawnList;
        internal List<Pos4> PossibleSuspectSpawnList;

        internal OwnerHouseScenarioTemplate(string address, List<Pos4> possibleCarSpawnList, List<Pos4> possibleSuspectSpawnList)
        {
            Address = address;
            PossibleSuspectSpawnList = possibleSuspectSpawnList;
            PossibleCarSpawnList = possibleCarSpawnList;
        }
    }
}
