using GangsOfSouthLS.HelperClasses.CommonUtilities;
using Rage;
using System.Collections.Generic;

namespace GangsOfSouthLS.HelperClasses.ProtectionRacketeeringHelpers
{
    internal class Scenario
    {
        private Dictionary<List<string>, string> badBoyPedStringListDict = new Dictionary<List<string>, string>
        {
            { new List<string> { "g_m_y_ballaeast_01" , "g_m_y_ballaorig_01", "g_m_y_ballasout_01" }, "AFRICAN_AMERICAN_GANG" },
            { new List<string> { "g_m_y_famca_01", "g_m_y_famdnf_01", "g_m_y_famfor_01" }, "AFRICAN_AMERICAN_GANG" },
            { new List<string> { "g_m_y_lost_01", "g_m_y_lost_02", "g_m_y_lost_03" }, "BIKER_GANG" },
            { new List<string> { "g_m_y_mexgang_01", "g_m_y_mexgoon_01", "g_m_y_mexgoon_02", "g_m_y_mexgoon_03" } , "MEXICAN_GANG" }
        };

        private List<string> carList = new List<string> { "Emperor", "Tornado", "Buccaneer", "Stalion", "Sabregt", "Chino", "Virgo", "Tampa", "Blade", "Faction" };

        internal List<string> GunList { get; private set; }
        internal string Name { get; private set; }
        internal Pos4 ParkingPos4 { get; private set; }
        internal Pos4 MerchantSpawnPos4 { get; private set; }
        internal Pos4 RacketeerShopPos4 { get; private set; }
        internal Vector3 Position { get; private set; }
        internal Ped Merchant { get; private set; }
        internal Ped Driver { get; private set; }
        internal Ped Passenger { get; private set; }
        internal Vehicle GangsterCar { get; private set; }
        internal List<string> DoorModelNames { get; private set; }
        internal Vector3 DoorLocation { get; private set; }
        internal List<string> MerchantStringList { get; private set; }
        internal Pos4 CarWaypointPos4 { get; private set; }
        internal Pos4 CarSpawnPos4 { get; private set; }
        internal string GangNameString { get; private set; }
        internal string ShopNameString { get; private set; }
        private KeyValuePair<List<string>, string> PedKeyValuePair;

        internal Scenario(ScenarioScheme scheme)
        {
            Name = scheme.Name;
            CarSpawnPos4 = scheme.CarSpawnPos4;
            ParkingPos4 = scheme.ParkingPos4;
            MerchantSpawnPos4 = scheme.MerchantSpawnPos4;
            RacketeerShopPos4 = scheme.RacketeerShopPos4;
            Position = scheme.Position;
            MerchantStringList = scheme.MerchantPedStringList;
            DoorLocation = scheme.DoorLocation;
            DoorModelNames = scheme.DoorModelNames;
            CarWaypointPos4 = scheme.CarWaypointPos4;
            ShopNameString = scheme.AudioString;
            GunList = new List<string> { "weapon_pistol", "weapon_snspistol", "weapon_combatpistol", "weapon_pistol50", "weapon_microsmg" };
        }

        internal void Initialize()
        {
            PedKeyValuePair = badBoyPedStringListDict.RandomElement();
            GangNameString = PedKeyValuePair.Value;
            GameFiber.StartNew(delegate
            {
                Game.LogTrivial(string.Format("[GangsOfSouthLS] Initializing {0}", Name));
                foreach (var entity in World.GetEntities(ParkingPos4.Position, 5f, GetEntitiesFlags.ConsiderAllVehicles | GetEntitiesFlags.ExcludePlayerVehicle))
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
            Game.LogTrivial(string.Format("[GangsOfSouthLS] Spawning Gangster {4} at ({0}, {1}, {2}, {3}).", CarSpawnPos4.X, CarSpawnPos4.Y, CarSpawnPos4.Z, CarSpawnPos4.Heading, carnamestring.ToUpper()));
            GangsterCar = CarSpawnPos4.CreateVehicle(carnamestring);
            GangsterCar.IsPersistent = true;
            GangsterCar.RandomizePlate();
        }

        private void MakeMerchant()
        {
            var pedstring = MerchantStringList.RandomElement();
            Merchant = new Ped(pedstring, MerchantSpawnPos4.Position, MerchantSpawnPos4.Heading);
            Merchant.RelationshipGroup = "RACKET_MERCHANT";
            Merchant.RandomizeVariation();
            Merchant.BlockPermanentEvents = true;
            Merchant.IsPersistent = true;
        }

        private void MakeBadGuys()
        {
            Driver = new Ped(PedKeyValuePair.Key.RandomElement(), Vector3.Zero, 0f);
            Driver.RelationshipGroup = "RACKET_GANGSTER";
            Driver.WarpIntoVehicle(GangsterCar, -1);
            Driver.RandomizeVariation();
            Driver.BlockPermanentEvents = true;
            Driver.IsPersistent = true;
            MyNatives.SetPedCombatAbilityAndMovement(Driver, MyNatives.CombatAbilityFlag.Professional, MyNatives.CombatMovementFlag.Offensive);
            Driver.Inventory.GiveNewWeapon(GunList.RandomElement(), 999, false);
            Passenger = new Ped(PedKeyValuePair.Key.RandomElement(), Vector3.Zero, 0f);
            Passenger.RelationshipGroup = "RACKET_GANGSTER";
            Passenger.WarpIntoVehicle(GangsterCar, 0);
            Passenger.RandomizeVariation();
            Passenger.BlockPermanentEvents = true;
            Passenger.IsPersistent = true;
            MyNatives.SetPedCombatAbilityAndMovement(Passenger, MyNatives.CombatAbilityFlag.Professional, MyNatives.CombatMovementFlag.Offensive);
            MyNatives.MakePedAbleToShootOutOfCar(Passenger);
            Passenger.Inventory.GiveNewWeapon(GunList.RandomElement(), 999, false);
        }
    }
}