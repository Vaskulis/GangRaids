using GangsOfSouthLS.HelperClasses.CommonUtilities;
using GangsOfSouthLS.Scenarios.ProtectionRacketeeringScenarios;
using Rage;
using System.Collections.Generic;

namespace GangsOfSouthLS.HelperClasses.ProtectionRacketeeringHelpers
{
    internal class ProtectionRacketeeringScenarioScheme
    {
        private string name;
        private Pos4 parkingPos4;
        private Pos4 carWaypointPos4;
        private Pos4 merchantSpawnPos4;
        private Pos4 racketeerShopPos4;
        private List<Pos4> carSpawnPos4List;
        private List<string> merchantPedStringList;
        private Vector3 doorLocation;
        private List<string> doorModelNames;
        private Vector3 position;

        internal string Name { get { return name; } }
        internal Pos4 ParkingPos4 { get { return parkingPos4; } }
        internal Pos4 MerchantSpawnPos4 { get { return merchantSpawnPos4; } }
        internal Pos4 RacketeerShopPos4 { get { return racketeerShopPos4; } }
        internal List<Pos4> CarSpawnPos4List { get { return carSpawnPos4List; } }
        internal List<string> MerchantPedStringList { get { return merchantPedStringList; } }
        internal Vector3 DoorLocation { get { return doorLocation; } }
        internal List<string> DoorModelNames { get { return doorModelNames; } }
        internal Vector3 Position { get { return position; } }
        internal Pos4 CarWaypointPos4 { get { return carWaypointPos4; } }

        internal ProtectionRacketeeringScenarioScheme
            (
            string Name,
            Pos4 ParkingPos4,
            Pos4 CarWaypointPos4,
            Pos4 MerchantSpawnPos4,
            Pos4 RacketeerShopPos4,
            List<Pos4> CarSpawnPos4List,
            List<string> MerchantPedStringList,
            Vector3 DoorLocation,
            List<string> DoorModelNames
            )
        {
            this.name = Name;
            this.parkingPos4 = ParkingPos4;
            this.merchantSpawnPos4 = MerchantSpawnPos4;
            this.racketeerShopPos4 = RacketeerShopPos4;
            this.carSpawnPos4List = CarSpawnPos4List;
            this.position = MerchantSpawnPos4.Position;
            this.merchantPedStringList = MerchantPedStringList;
            this.doorModelNames = DoorModelNames;
            this.doorLocation = DoorLocation;
            this.carWaypointPos4 = CarWaypointPos4;
        }

        internal static bool ChooseScenario(out ProtectionRacketeeringScenarioScheme scenarioScheme)
        {
            var playerPos = Game.LocalPlayer.Character.Position;
            var schemeList = ProtectionRacketeeringScenarioSchemes.ScenarioSchemeList;
            schemeList.Shuffle();
            scenarioScheme = null;
            var foundOne = false;
            foreach (var item in schemeList)
            {
                if ((playerPos.DistanceTo(item.Position) < 750f) && (playerPos.DistanceTo(item.Position) > 150f))
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