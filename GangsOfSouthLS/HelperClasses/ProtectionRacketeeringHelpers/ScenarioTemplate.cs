using GangsOfSouthLS.HelperClasses.CommonUtilities;
using GangsOfSouthLS.ScenarioCollections.ProtectionRacketeeringScenarios;
using Rage;
using System.Collections.Generic;

namespace GangsOfSouthLS.HelperClasses.ProtectionRacketeeringHelpers
{
    internal class ScenarioTemplate
    {
        internal string Name { get; private set; }
        internal Pos4 MerchantSpawnPos4 { get; private set; }
        internal Pos4 RacketeerShopPos4 { get; private set; }
        internal List<string> MerchantPedStringList { get; private set; }
        internal Vector3 DoorLocation { get; private set; }
        internal List<string> DoorModelNames { get; private set; }
        internal Vector3 Position { get; private set; }
        private CarWaypointCollection WaypointCollection;
        private List<CarWaypointCollection> WaypointCollectionList;
        internal Pos4 CarSpawnPos4 { get { return WaypointCollection.SpawnPos4; } }
        internal Pos4 CarWaypointPos4 { get { return WaypointCollection.WaypointPos4; } }
        internal Pos4 ParkingPos4 { get { return WaypointCollection.ParkingPos4; } }
        internal string AudioString { get; private set; }

        internal ScenarioTemplate
            (
            string name,
            Pos4 merchantSpawnPos4,
            Pos4 racketeerShopPos4,
            List<string> merchantPedStringList,
            Vector3 doorLocation,
            List<string> doorModelNames,
            List<CarWaypointCollection> waypointCollectionList,
            string audioString = "NONE"
            )
        {
            Name = name;
            MerchantSpawnPos4 = merchantSpawnPos4;
            RacketeerShopPos4 = racketeerShopPos4;
            Position = merchantSpawnPos4.Position;
            MerchantPedStringList = merchantPedStringList;
            DoorModelNames = doorModelNames;
            DoorLocation = doorLocation;
            WaypointCollectionList = waypointCollectionList;
            AudioString = audioString;
        }

        internal static bool ChooseScenario(out ScenarioTemplate scenarioTemplate)
        {
            var playerPos = Game.LocalPlayer.Character.Position;
            var TemplateList = ScenarioTemplateCollection.ScenarioTemplateList;
            TemplateList.Shuffle();
            scenarioTemplate = null;
            var foundOne = false;
            foreach (var item in TemplateList)
            {
                if ((playerPos.DistanceTo(item.Position) < 750f) && (playerPos.DistanceTo(item.Position) > 150f))
                {
                    scenarioTemplate = item;
                    scenarioTemplate.WaypointCollection = scenarioTemplate.WaypointCollectionList.RandomElement();
                    foundOne = true;
                    Game.LogTrivial(string.Format("[GangsOfSouthLS] Chose Scenario: {0}", item.Name));
                    break;
                }
            }
            return foundOne;
        }
    }

    internal class CarWaypointCollection
    {
        internal Pos4 SpawnPos4 { get; private set; }
        internal Pos4 WaypointPos4 { get; private set; }
        internal Pos4 ParkingPos4 { get; private set; }

        internal CarWaypointCollection(Pos4 spawnPos4, Pos4 waypointPos4, Pos4 parkingPos4)
        {
            SpawnPos4 = spawnPos4;
            WaypointPos4 = waypointPos4;
            ParkingPos4 = parkingPos4;
        }
    }
}