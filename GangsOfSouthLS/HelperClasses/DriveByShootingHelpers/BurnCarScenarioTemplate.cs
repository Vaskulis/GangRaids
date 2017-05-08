using GangsOfSouthLS.HelperClasses.CommonUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GangsOfSouthLS.HelperClasses.DriveByShootingHelpers
{
    internal class BurnCarScenarioTemplate
    {
        Pos4 CarSpawn;
        List<Pos4> SuspectSpawnList;
        List<Pos4> SuspectCarSpawnList;

        internal BurnCarScenarioTemplate(Pos4 carSpawn, List<Pos4> suspectSpawnList, List<Pos4> suspectCarSpawnList)
        {
            CarSpawn = carSpawn;
            SuspectSpawnList = suspectSpawnList;
            SuspectCarSpawnList = suspectCarSpawnList;
        }
    }
}
