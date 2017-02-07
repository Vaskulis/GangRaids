using GangsOfSouthLS.HelperClasses.CommonUtilities;
using GangsOfSouthLS.Scenarios.DrugDealScenarios;
using Rage;
using System.Collections.Generic;

namespace GangsOfSouthLS.HelperClasses.DrugDealHelpers
{
    internal class DrugDealScenarioScheme
    {
        private string name;
        private Vector3 position;
        private List<CopCarBuild> copCarBuildList;
        private List<CopCarWayPoint> copCarWayPointList;
        private List<Pos4> badGuyCarSpawnPos4List;
        private List<Pos4> badGuyPedSpawnPos4List;

        internal DrugDealScenarioScheme
            (
            string Name,
            List<CopCarWayPoint> CopCarWayPointList,
            List<CopCarBuild> CopCarBuildList,
            List<Pos4> BadGuyCarSpawnPos4List,
            List<Pos4> BadGuyPedSpawnPos4List,
            Vector3 Position
            )
        {
            this.name = Name;
            this.copCarBuildList = CopCarBuildList;
            this.copCarWayPointList = CopCarWayPointList;
            this.badGuyCarSpawnPos4List = BadGuyCarSpawnPos4List;
            this.badGuyPedSpawnPos4List = BadGuyPedSpawnPos4List;
            this.position = Position;
        }

        internal List<CopCarBuild> CopCarBuildList { get { return copCarBuildList; } }
        internal List<CopCarWayPoint> CopCarWayPointList { get { return copCarWayPointList; } }
        internal List<Pos4> BadGuyCarSpawnPos4List { get { return badGuyCarSpawnPos4List; } }
        internal List<Pos4> BadGuyPedSpawnPos4List { get { return badGuyPedSpawnPos4List; } }
        internal string Name { get { return name; } }
        internal Vector3 Position { get { return position; } }

        internal static bool ChooseScenario(out DrugDealScenarioScheme scenarioScheme)
        {
            var playerPos = Game.LocalPlayer.Character.Position;
            var schemeList = DrugDealScenarioSchemes.ScenarioSchemeList;
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