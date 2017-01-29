using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;
using GangRaids.HelperClasses.CommonUtilities;

namespace GangRaids.HelperClasses.ProtectionRacketeeringHelpers
{
    class ProtectionRacketeeringScenario
    {
        private string name;
        private Pos4 parkingPos4;
        private Pos4 merchantSpawnPos4;
        private Pos4 racketeerShopPos4;
        private Pos4 carSpawnPos4;
        private Vector3 position;

        private Ped merchant;
        private Ped driver;
        private Ped racketeer;
        private Dictionary<List<string>, string> badBoyPedStringListDict = new Dictionary<List<string>, string>
        {
            { new List<string> { "g_m_y_ballaeast_01" , "g_m_y_ballaorig_01", "g_m_y_ballasout_01" }, "AFRICAN_AMERICAN_GANG" },
            { new List<string> { "g_m_y_famca_01", "g_m_y_famdnf_01", "g_m_y_famfor_01" }, "AFRICAN_AMERICAN_GANG" },
            { new List<string> { "g_m_y_lost_01", "g_m_y_lost_02", "g_m_y_lost_03" }, "BIKER_GANG" },
            { new List<string> { "g_m_y_mexgang_01", "g_m_y_mexgoon_01", "g_m_y_mexgoon_02", "g_m_y_mexgoon_03" } , "MEXICAN_GANG" }
        };
        private List<string> merchantPedStringList;
        private List<string> carList = new List<string> { "Emperor", "Tornado", "Buccaneer", "Stalion", "Sabregt", "Chino", "Virgo", "Tampa" };
        private Vehicle gangsterCar;

        internal string Name { get { return name; } }
        internal Pos4 ParkingPos4 { get { return parkingPos4; } }
        internal Pos4 MerchantSpawnPos4 { get { return merchantSpawnPos4; } }
        internal Pos4 RacketeerShopPos4 { get { return racketeerShopPos4; } }
        internal Pos4 CarSpawnPos4 { get { return carSpawnPos4; } }
        internal Vector3 Position { get { return position; } }
        internal Ped Merchant { get { return merchant; } }
        internal Ped Driver { get { return driver; } }
        internal Ped Racketeer { get { return racketeer; } }
        internal Vehicle GangsterCar { get { return gangsterCar; } }

        internal ProtectionRacketeeringScenario(ProtectionRacketeeringScenarioScheme scheme)
        {
            this.name = scheme.Name;
            this.parkingPos4 = scheme.ParkingPos4;
            this.merchantSpawnPos4 = scheme.MerchantSpawnPos4;
            this.racketeerShopPos4 = scheme.RacketeerShopPos4;
            this.carSpawnPos4 = scheme.CarSpawnPos4;
            this.position = scheme.Position;
            this.merchantPedStringList = scheme.MerchantPedStringList;
        }

        internal void Initialize()
        {
            GameFiber.StartNew(delegate
            {
                Game.LogTrivial(string.Format("[GANG RAIDS] Initializing {0}", name));
                foreach (var entity in World.GetEntities(position, 40f, GetEntitiesFlags.ConsiderAllPeds | GetEntitiesFlags.ExcludePlayerPed))
                {
                    if (entity.Exists())
                    {
                        entity.Delete();
                    }
                }
                foreach (var entity in World.GetEntities(parkingPos4.Position, 10f, GetEntitiesFlags.ConsiderAllVehicles | GetEntitiesFlags.ExcludePlayerVehicle))
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
        }

        private void MakeGangsterCar()
        {
            var carnamestring = carList.RandomElement();
            gangsterCar = carSpawnPos4.CreateVehicle(carnamestring);
            gangsterCar.IsPersistent = true;
        }

        private void MakeMerchant()
        {
            var pedstring = merchantPedStringList.RandomElement();
            merchant = new Ped(pedstring, merchantSpawnPos4.Position, merchantSpawnPos4.Heading);
            merchant.RandomizeVariation();
            merchant.BlockPermanentEvents = true;
            merchant.IsPersistent = true;
        }

        private void MakeBadGuys()
        {
            var pedKeyValuePair = badBoyPedStringListDict.RandomElement();
            driver = new Ped(pedKeyValuePair.Key.RandomElement(), Vector3.Zero, 0f);
            driver.WarpIntoVehicle(gangsterCar, -1);
            driver.RandomizeVariation();
            driver.BlockPermanentEvents = true;
            driver.IsPersistent = true;
            racketeer = new Ped(pedKeyValuePair.Key.RandomElement(), Vector3.Zero, 0f);
            racketeer.WarpIntoVehicle(gangsterCar, 0);
            racketeer.RandomizeVariation();
            racketeer.BlockPermanentEvents = true;
            racketeer.IsPersistent = true;
        }
    }
}
