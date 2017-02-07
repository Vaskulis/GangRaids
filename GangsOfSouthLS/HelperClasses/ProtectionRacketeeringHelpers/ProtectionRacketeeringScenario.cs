using GangsOfSouthLS.HelperClasses.CommonUtilities;
using Rage;
using System.Collections.Generic;

namespace GangsOfSouthLS.HelperClasses.ProtectionRacketeeringHelpers
{
    internal class ProtectionRacketeeringScenario
    {
        private string name;
        private Pos4 parkingPos4;
        private Pos4 carWaypointPos4;
        private Pos4 merchantSpawnPos4;
        private Pos4 racketeerShopPos4;
        private List<Pos4> carSpawnPos4List;
        private Vector3 position;
        private List<string> doorModelNames;
        private Vector3 doorLocation;

        private Ped merchant;
        private Ped driver;
        private Ped passenger;

        private Dictionary<List<string>, string> badBoyPedStringListDict = new Dictionary<List<string>, string>
        {
            { new List<string> { "g_m_y_ballaeast_01" , "g_m_y_ballaorig_01", "g_m_y_ballasout_01" }, "AFRICAN_AMERICAN_GANG" },
            { new List<string> { "g_m_y_famca_01", "g_m_y_famdnf_01", "g_m_y_famfor_01" }, "AFRICAN_AMERICAN_GANG" },
            { new List<string> { "g_m_y_lost_01", "g_m_y_lost_02", "g_m_y_lost_03" }, "BIKER_GANG" },
            { new List<string> { "g_m_y_mexgang_01", "g_m_y_mexgoon_01", "g_m_y_mexgoon_02", "g_m_y_mexgoon_03" } , "MEXICAN_GANG" }
        };

        private List<string> merchantPedStringList;
        private List<string> carList = new List<string> { "Emperor", "Tornado", "Buccaneer", "Stalion", "Sabregt", "Chino", "Virgo", "Tampa", "Blade", "Faction" };
        private List<string> gunList = new List<string> { "weapon_pistol", "weapon_snspistol", "weapon_combatpistol", "weapon_pistol50", "weapon_microsmg" };
        private Vehicle gangsterCar;

        internal string Name { get { return name; } }
        internal Pos4 ParkingPos4 { get { return parkingPos4; } }
        internal Pos4 MerchantSpawnPos4 { get { return merchantSpawnPos4; } }
        internal Pos4 RacketeerShopPos4 { get { return racketeerShopPos4; } }
        internal List<Pos4> CarSpawnPos4 { get { return carSpawnPos4List; } }
        internal Vector3 Position { get { return position; } }
        internal Ped Merchant { get { return merchant; } }
        internal Ped Driver { get { return driver; } }
        internal Ped Passenger { get { return passenger; } }
        internal Vehicle GangsterCar { get { return gangsterCar; } }
        internal List<string> DoorModelNames { get { return doorModelNames; } }
        internal Vector3 DoorLocation { get { return doorLocation; } }
        internal List<string> GunList { get { return gunList; } }
        internal List<string> MerchantStringList { get { return merchantPedStringList; } }
        internal Pos4 CarWaypointPos4 { get { return carWaypointPos4; } }

        internal ProtectionRacketeeringScenario(ProtectionRacketeeringScenarioScheme scheme)
        {
            this.name = scheme.Name;
            this.parkingPos4 = scheme.ParkingPos4;
            this.merchantSpawnPos4 = scheme.MerchantSpawnPos4;
            this.racketeerShopPos4 = scheme.RacketeerShopPos4;
            this.carSpawnPos4List = scheme.CarSpawnPos4List;
            this.position = scheme.Position;
            this.merchantPedStringList = scheme.MerchantPedStringList;
            this.doorLocation = scheme.DoorLocation;
            this.doorModelNames = scheme.DoorModelNames;
            this.carWaypointPos4 = scheme.CarWaypointPos4;
        }

        internal void Initialize()
        {
            GameFiber.StartNew(delegate
            {
                Game.LogTrivial(string.Format("[GangsOfSouthLS] Initializing {0}", name));
                foreach (var entity in World.GetEntities(parkingPos4.Position, 5f, GetEntitiesFlags.ConsiderAllVehicles | GetEntitiesFlags.ExcludePlayerVehicle))
                {
                    if (entity.Exists())
                    {
                        entity.Delete();
                    }
                }
                MakeMerchant();
                return;
            });
        }

        internal void SpawnCarAndBadGuys()
        {
            MakeGangsterCar();
            MakeBadGuys();
            Game.SetRelationshipBetweenRelationshipGroups("RACKET_GANGSTER", "COP", Relationship.Hate);
            Game.SetRelationshipBetweenRelationshipGroups("RACKET_GANGSTER", "PLAYER", Relationship.Hate);
        }

        private void MakeGangsterCar()
        {
            var carnamestring = carList.RandomElement();
            var carspawnpos4 = carSpawnPos4List.RandomElement();
            Game.LogTrivial(string.Format("[GangsOfSouthLS] Spawning Gangster {4} at ({0}, {1}, {2}, {3}).", carspawnpos4.X, carspawnpos4.Y, carspawnpos4.Z, carspawnpos4.Heading, carnamestring.ToUpper()));
            gangsterCar = carspawnpos4.CreateVehicle(carnamestring);
            gangsterCar.IsPersistent = true;
            gangsterCar.RandomizePlate();
        }

        private void MakeMerchant()
        {
            var pedstring = merchantPedStringList.RandomElement();
            merchant = new Ped(pedstring, merchantSpawnPos4.Position, merchantSpawnPos4.Heading);
            merchant.RelationshipGroup = "RACKET_MERCHANT";
            merchant.RandomizeVariation();
            merchant.BlockPermanentEvents = true;
            merchant.IsPersistent = true;
        }

        private void MakeBadGuys()
        {
            var pedKeyValuePair = badBoyPedStringListDict.RandomElement();
            driver = new Ped(pedKeyValuePair.Key.RandomElement(), Vector3.Zero, 0f);
            driver.RelationshipGroup = "RACKET_GANGSTER";
            driver.WarpIntoVehicle(gangsterCar, -1);
            driver.RandomizeVariation();
            driver.BlockPermanentEvents = true;
            driver.IsPersistent = true;
            MyNatives.SetPedCombatAbilityAndMovement(driver, MyNatives.CombatAbilityFlag.Professional, MyNatives.CombatMovementFlag.Offensive);
            driver.Inventory.GiveNewWeapon(gunList.RandomElement(), 999, false);
            passenger = new Ped(pedKeyValuePair.Key.RandomElement(), Vector3.Zero, 0f);
            passenger.RelationshipGroup = "RACKET_GANGSTER";
            passenger.WarpIntoVehicle(gangsterCar, 0);
            passenger.RandomizeVariation();
            passenger.BlockPermanentEvents = true;
            passenger.IsPersistent = true;
            MyNatives.SetPedCombatAbilityAndMovement(passenger, MyNatives.CombatAbilityFlag.Professional, MyNatives.CombatMovementFlag.Offensive);
            MyNatives.MakePedAbleToShootOutOfCar(passenger);
            passenger.Inventory.GiveNewWeapon(gunList.RandomElement(), 999, false);
        }
    }
}