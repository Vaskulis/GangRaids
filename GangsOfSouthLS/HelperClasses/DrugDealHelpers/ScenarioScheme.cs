using GangsOfSouthLS.HelperClasses.CommonUtilities;
using GangsOfSouthLS.Scenarios.DrugDealScenarios;
using Rage;
using System.Collections.Generic;

namespace GangsOfSouthLS.HelperClasses.DrugDealHelpers
{
    internal class ScenarioScheme
    {
        internal ScenarioScheme
            (
            string name,
            List<CopCarWayPoint> copCarWayPointList,
            List<CopCarBuild> copCarBuildList,
            List<Pos4> badGuyCarSpawnPos4List,
            List<Pos4> badGuyPedSpawnPos4List,
            Vector3 position
            )
        {
            Name = name;
            CopCarBuildList = copCarBuildList;
            CopCarWayPointList = copCarWayPointList;
            BadGuyCarSpawnPos4List = badGuyCarSpawnPos4List;
            BadGuyPedSpawnPos4List = badGuyPedSpawnPos4List;
            Position = position;
        }

        internal List<CopCarBuild> CopCarBuildList { get; private set; }
        internal List<CopCarWayPoint> CopCarWayPointList { get; private set; }
        internal List<Pos4> BadGuyCarSpawnPos4List { get; private set; }
        internal List<Pos4> BadGuyPedSpawnPos4List { get; private set; }
        internal string Name { get; private set; }
        internal Vector3 Position { get; private set; }

        internal static bool ChooseScenario(out ScenarioScheme scenarioScheme)
        {
            var playerPos = Game.LocalPlayer.Character.Position;
            var schemeList = ScenarioSchemeCollection.ScenarioSchemeList;
            schemeList.Shuffle();
            scenarioScheme = null;
            var foundOne = false;
            foreach (var item in schemeList)
            {
                if ((playerPos.DistanceTo(item.Position) < 750f) && (playerPos.DistanceTo(item.Position) > 250f))
                {
                    scenarioScheme = item;
                    foundOne = true;
                    Game.LogTrivial(string.Format("[GangsOfSouthLS] Chose Scenario: {0}", item.Name));
                    break;
                }
            }
            return foundOne;
        }
    }
}