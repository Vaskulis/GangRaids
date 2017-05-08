using GangsOfSouthLS.HelperClasses.CommonUtilities;
using GangsOfSouthLS.ScenarioCollections.DriveByShootingScenarios;
using Rage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GangsOfSouthLS.HelperClasses.DriveByShootingHelpers
{
    class CrimeSceneScenarioTemplate
    {
        internal string Name { get; private set; }
        internal List<Pos4> WitnessSpawnList { get; private set; }
        internal Pos4 AmbulanceSpawn { get; private set; }
        internal List<Pos4> VictimSpawnList { get; private set; }
        internal Vector3 Position { get; private set; }

        internal CrimeSceneScenarioTemplate(string name, List<Pos4> witnessSpawnList, Pos4 ambulanceSpawn, List<Pos4> victimSpawnList)
        {
            Name = name;
            WitnessSpawnList = witnessSpawnList;
            AmbulanceSpawn = ambulanceSpawn;
            VictimSpawnList = victimSpawnList;
            Position = VictimSpawnList[0].Position;
        }

        internal static bool ChooseScenario(out CrimeSceneScenarioTemplate scenarioTemplate)
        {
            //var playerPos = Game.LocalPlayer.Character.Position;
            //var TemplateList = ScenarioTemplateCollection.ScenarioTemplateList;
            //TemplateList.Shuffle();
            //scenarioTemplate = null;
            //var foundOne = false;
            //foreach (var item in TemplateList)
            //{
            //    if ((playerPos.DistanceTo(item.Position) < 750f) && (playerPos.DistanceTo(item.Position) > 200f))
            //    {
            //        scenarioTemplate = item;
            //        foundOne = true;
            //        Game.LogTrivial(string.Format("[GangsOfSouthLS] Chose Scenario: {0}", item.Name));
            //        break;
            //    }
            //}
            //return foundOne;
            scenarioTemplate = CrimeSceneScenarioTemplateCollection.Scenario1;
            return true;
        }
    }
}
