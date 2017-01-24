using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;
using Rage.Native;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using GangRaids.Scenarios;

namespace GangRaids.HelperClasses
{
    class DrugDealScenarioScheme
    {
        private string name;
        private Vector3 position;
        private List<CopCarBuild> copCarBuildList;
        private List<CopCarWayPoint> copCarWayPointList;
        private List<Pos4> badGuyCarSpawnPos4List;
        private List<Pos4> badGuyPedSpawnPos4List;
        private int waitTime;

        public DrugDealScenarioScheme
            (
            string Name,
            List<CopCarWayPoint> CopCarWayPointList,
            List<CopCarBuild> CopCarBuildList,
            List<Pos4> BadGuyCarSpawnPos4List,
            List<Pos4> BadGuyPedSpawnPos4List,
            Vector3 Position,
            int WaitTime
            )
        {
            this.name = Name;
            this.copCarBuildList = CopCarBuildList;
            this.copCarWayPointList = CopCarWayPointList;
            this.badGuyCarSpawnPos4List = BadGuyCarSpawnPos4List;
            this.badGuyPedSpawnPos4List = BadGuyPedSpawnPos4List;
            this.position = Position;
            this.waitTime = WaitTime;

        }

        public List<CopCarBuild> CopCarBuildList { get { return copCarBuildList; } }
        public List<CopCarWayPoint> CopCarWayPointList { get { return copCarWayPointList; } }
        public List<Pos4> BadGuyCarSpawnPos4List { get { return badGuyCarSpawnPos4List; } }
        public List<Pos4> BadGuyPedSpawnPos4List { get { return badGuyPedSpawnPos4List; } }
        public string Name { get { return name; } }
        public Vector3 Position { get { return position; } }
        public int WaitTime { get { return waitTime; } }


        public static bool ChooseScenario(out DrugDealScenarioScheme scenarioScheme )
        {
            var playerPos = Game.LocalPlayer.Character.Position;
            var schemeList = DrugDealScenarioSchemes.ScenarioSchemeList;
            schemeList.Shuffle();
            scenarioScheme = null;
            var foundOne = false;
            foreach (var item in schemeList)
            {
                if ((playerPos.DistanceTo(item.Position) < 1500f) && (playerPos.DistanceTo(item.Position) > 300f))
                {
                    scenarioScheme = item;
                    foundOne = true;
                    Game.LogTrivial(string.Format("Chose Scenario: {0}", item.Name));
                    break;
                }
            }
            return foundOne;

        }
    }
}
