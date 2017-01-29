using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;
using GangRaids.Scenarios.ProtectionRacketeeringScenarios;
using GangRaids.HelperClasses.CommonUtilities;

namespace GangRaids.HelperClasses.ProtectionRacketeeringHelpers
{
    class ProtectionRacketeeringScenarioScheme
    {
        private string name;
        private Pos4 parkingPos4;
        private Pos4 merchantSpawnPos4;
        private Pos4 racketeerShopPos4;
        private Pos4 carSpawnPos4;
        private List<string> merchantPedStringList;
        private Vector3 position;

        internal string Name { get { return name; } }
        internal Pos4 ParkingPos4 { get { return parkingPos4; } }
        internal Pos4 MerchantSpawnPos4 { get { return merchantSpawnPos4; } }
        internal Pos4 RacketeerShopPos4 { get { return racketeerShopPos4; } }
        internal Pos4 CarSpawnPos4 { get { return carSpawnPos4; } }
        internal List<string> MerchantPedStringList { get { return merchantPedStringList; } }
        internal Vector3 Position { get { return position; } }

        internal ProtectionRacketeeringScenarioScheme(string Name, Pos4 ParkingPos4, Pos4 MerchantSpawnPos4, Pos4 RacketeerShopPos4, Pos4 CarSpawnPos4, List<string> MerchantPedStringList)
        {
            this.name = Name;
            this.parkingPos4 = ParkingPos4;
            this.merchantSpawnPos4 = MerchantSpawnPos4;
            this.racketeerShopPos4 = RacketeerShopPos4;
            this.carSpawnPos4 = CarSpawnPos4;
            this.position = MerchantSpawnPos4.Position;
            this.merchantPedStringList = MerchantPedStringList;
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
                    Game.LogTrivial(string.Format("[GANG RAIDS] Chose Scenario: {0}", item.Name));
                    break;
                }
            }
            return foundOne;
        }
    }
}
