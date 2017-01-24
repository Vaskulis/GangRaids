using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;
using Rage.Native;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;

namespace GangRaids.HelperClasses
{
    class DrugDealScenario
    {
        private string name;
        private Vector3 position;
        private int waitTime;

        private Vehicle buyerCar;
        private Pos4 buyerCarSpawnPos4;
        private Vehicle dealerCar;
        private Pos4 dealerCarSpawnPos4;
        private Vehicle dealerVan;
        private Pos4 dealerVanSpawnPos4;

        private Vehicle copCar1;
        private Vehicle copCar2;

        private List<string> buyerCarStringList = new List<string> { "Emperor", "Primo", "Stanier", "Seminole", "Landstalker", "Cavalcade", "Granger", "Patriot", "Bison" };
        private List<string> dealerCarStringList = new List<string> { "Buccaneer", "Tornado", "Manana", "Chino", "Dukes", "Stalion", "Phoenix", "Sabregt" };
        private List<string> dealerVanStringList = new List<string> { "Speedo", "Burrito3", "Youga" };
        private Dictionary<List<string>, string> badBoyPedStringListDict = new Dictionary<List<string>, string>
        {
            { new List<string> { "g_m_y_ballaeast_01" , "g_m_y_ballaorig_01", "g_m_y_ballasout_01" }, "AFRICAN_AMERICAN_GANG" },
            { new List<string> { "g_m_y_famca_01", "g_m_y_famdnf_01", "g_m_y_famfor_01" }, "AFRICAN_AMERICAN_GANG" },
            { new List<string> { "g_m_y_lost_01", "g_m_y_lost_02", "g_m_y_lost_03" }, "BIKER_GANG" },
            { new List<string> { "g_m_y_mexgang_01", "g_m_y_mexgoon_01", "g_m_y_mexgoon_02", "g_m_y_mexgoon_03" } , "MEXICAN_GANG" }
        };
        private List<string> badBoyPistolList = new List<string> { "weapon_pistol", "weapon_snspistol", "weapon_combatpistol", "weapon_pistol50" };
        private List<string> badBoyBigGunList = new List<string> { "weapon_assaultrifle", "weapon_pumpshotgun", "weapon_sawnoffshotgun", "weapon_smg", "weapon_microsmg" };

        private List<Ped> dealerList;
        private List<Ped> buyerList;
        private List<Vehicle> badBoyCarList;
        private Dictionary<Vehicle, CopCarWayPoint> copCarDict;
        private List<Ped> copList1;
        private List<Ped> copList2;
        
        private List<CopCarBuild> copCarBuildList;
        private List<CopCarWayPoint> copCarWayPointList;

        private Ped dealer1;
        private Pos4 dealer1SpawnPos4;
        private Ped dealer2;
        private Pos4 dealer2SpawnPos4;
        private Ped dealer3;
        private Pos4 dealer3SpawnPos4;
        private Ped buyer1;
        private Pos4 buyer1SpawnPos4;
        private Ped buyer2;
        private Pos4 buyer2SpawnPos4;
        private bool dealer3WasSpawned;
        private bool dealer3WasGivenBiggerGun;

        private string dealerGangNameString;

        public DrugDealScenario(DrugDealScenarioScheme scheme)
        {
            this.name = Name;
            this.copCarBuildList = new List<CopCarBuild>(scheme.CopCarBuildList);
            this.copCarWayPointList = new List<CopCarWayPoint>(scheme.CopCarWayPointList);
            this.dealerVanSpawnPos4 = scheme.BadGuyCarSpawnPos4List[0].Copy();
            this.dealerCarSpawnPos4 = scheme.BadGuyCarSpawnPos4List[1].Copy();
            this.buyerCarSpawnPos4 = scheme.BadGuyCarSpawnPos4List[2].Copy();
            this.dealer1SpawnPos4 = scheme.BadGuyPedSpawnPos4List[0].Copy();
            this.dealer2SpawnPos4 = scheme.BadGuyPedSpawnPos4List[1].Copy();
            this.dealer3SpawnPos4 = scheme.BadGuyPedSpawnPos4List[2].Copy();
            this.buyer1SpawnPos4 = scheme.BadGuyPedSpawnPos4List[3].Copy();
            this.buyer2SpawnPos4 = scheme.BadGuyPedSpawnPos4List[4].Copy();
            this.position = scheme.Position.Copy();
            this.badBoyCarList = new List<Vehicle> { };
            this.copCarDict = new Dictionary<Vehicle, CopCarWayPoint> { };
            this.waitTime = scheme.WaitTime;
            dealer3WasSpawned = false;
            dealer3WasGivenBiggerGun = false;
        }

        public Vehicle BuyerCar { get { return buyerCar; } }
        public Vehicle DealerCar { get { return dealerCar; } }
        public Vehicle DealerVan { get { return dealerVan; } }
        public List<Ped> DealerList { get { return dealerList; } }
        public List<Ped> BuyerList { get { return buyerList; } }
        public Vehicle CopCar1 { get { return copCar1; } set { copCar1 = value; } }
        public Vehicle CopCar2 { get { return copCar2; } set { copCar2 = value; } }
        public Dictionary<Vehicle, CopCarWayPoint> CopCarDict { get { return copCarDict; } set { copCarDict = value; } }
        public List<Vehicle> BadBoyCarList { get { return badBoyCarList; } }
        public string Name { get { return name; } }
        public Vector3 Position { get { return position; } }
        public Ped Dealer1 { get { return dealer1; } }
        public Ped Dealer2 { get { return dealer2; } }
        public Ped Dealer3 { get { return dealer3; } }
        public Ped Buyer1 { get { return buyer1; } }
        public Ped Buyer2 { get { return buyer2; } }
        public List<Ped> CopList1 { get { return copList1; } set { copList1 = value; } }
        public List<Ped> CopList2 { get { return copList2; } set { copList2 = value; } }
        public int WaitTime { get { return waitTime; } }
        public bool Dealer3WasSpawned { get { return dealer3WasSpawned; } }
        public List<CopCarWayPoint> CopCarWayPointList { get { return copCarWayPointList; } }
        public List<CopCarBuild> CopCarBuildList { get { return copCarBuildList; } }
        public string DealerGangNameString { get { return dealerGangNameString; } }


        public void Initialize()
        {
            GameFiber.StartNew(delegate 
            {
                Game.LogTrivial(string.Format("Initializing {0}", name));
                foreach (var entity in World.GetEntities(position, 100f, GetEntitiesFlags.ConsiderAllPeds | GetEntitiesFlags.ConsiderAllVehicles | GetEntitiesFlags.ExcludePlayerPed | GetEntitiesFlags.ExcludePlayerVehicle))
                {
                    if (entity.Exists())
                    {
                        entity.Delete();
                    }
                }
                badBoyCarList = MakeBadBoyCarList();
                dealerList = MakeDealerList();
                buyerList = MakeBuyerList();
                GameFiber.Wait(300);
                if (!dealer1.IsInAnyVehicle(true))
                {
                    dealer1.Tasks.PlayAnimation("amb@world_human_drug_dealer_hard@male@idle_a", "idle_b", 3f, AnimationFlags.Loop);
                }
                dealer2.Tasks.PlayAnimation("amb@world_human_stand_impatient@male@no_sign@idle_a", "idle_c", 1f, AnimationFlags.Loop);
                buyer1.Tasks.PlayAnimation("amb@world_human_drug_dealer_hard@male@idle_a", "idle_c", 4f, AnimationFlags.Loop);
                buyer2.Tasks.PlayAnimation("amb@world_human_drug_dealer_hard@male@idle_a", "idle_b", 2f, AnimationFlags.Loop);
                return;
            });            
        }


        private List<Vehicle> MakeBadBoyCarList()
        {
            var list = new List<Vehicle> { };
            dealerVan = dealerVanSpawnPos4.CreateVehicle(dealerVanStringList.RandomElement());
            list.Add(dealerVan);
            dealerVan.Doors[2].Open(true);
            dealerVan.Doors[3].Open(true);
            dealerCar = dealerCarSpawnPos4.CreateVehicle(dealerCarStringList.RandomElement());
            list.Add(dealerCar);
            buyerCar = buyerCarSpawnPos4.CreateVehicle(buyerCarStringList.RandomElement());
            list.Add(buyerCar);
            if(World.DateTime.Hour >= 18 || World.DateTime.Hour < 6)
            {
                foreach(var veh in list)
                {
                    NativeFunction.Natives.SetVehicleLights(veh, 3);
                    veh.IsPersistent = true;
                }
            }
            else
            {
                foreach(var veh in list)
                {
                    veh.IsEngineOn = true;
                    veh.IsPersistent = true;
                }
            }

            return list;
        }


        public Dictionary<Vehicle, CopCarWayPoint> MakeCopCarDict(CopCarWayPoint WayPoint1, CopCarWayPoint WayPoint2)
        {
            var Dict = new Dictionary<Vehicle, CopCarWayPoint> { };
            Dict.Add(copCar1, WayPoint1);
            Dict.Add(copCar2, WayPoint2);
            foreach(var veh in Dict.Keys)
            {
                veh.IsPersistent = true;
            }
            return Dict;
        }


        public Vehicle MakeCopCarDictVehicleAndOccupy(CopCarWayPoint waypoint, CopCarBuild build)
        {
            Game.LogTrivial(string.Format("Player decided to spawn {0} at {1}.", build.carName, waypoint.description));
            var veh = waypoint.startPoint.CreateVehicle(build.carName);
            foreach(var seat in build.seatIndicesToOccupy)
            {
                var cop = new Ped(build.pedNameList.RandomElement(), Vector3.Zero, 0f);
                var weaponDictEntry = build.weaponDict.RandomElement();
                cop.Inventory.GiveNewWeapon(weaponDictEntry.Key, 999, true);
                foreach(var component in weaponDictEntry.Value)
                {
                    cop.Inventory.AddComponentToWeapon(weaponDictEntry.Key, component);
                }
                NativeFunction.Natives.SetPedCombatAbility(cop, 2);
                NativeFunction.Natives.SetPedCombatMovement(cop, 2);
                cop.WarpIntoVehicle(veh, seat);
            }
            veh.IsPersistent = true;
            return veh;
        }

        private List<Ped> MakeDealerList()
        {
            var list = new List<Ped> { };
            var dealerPedStringKeyValuePair = badBoyPedStringListDict.RandomElement();
            dealerGangNameString = dealerPedStringKeyValuePair.Value;
            dealer1 = dealer1SpawnPos4.CreatePed(dealerPedStringKeyValuePair.Key.RandomElement());
            dealer1.IsPersistent = true;
            dealer1.BlockPermanentEvents = true;
            list.Add(dealer1);
            dealer2 = dealer2SpawnPos4.CreatePed(dealerPedStringKeyValuePair.Key.RandomElement());
            dealer2.IsPersistent = true;
            dealer2.BlockPermanentEvents = true;
            list.Add(dealer2);
            if (UsefulExtensions.Decide(50))
            {
                dealer3WasSpawned = true;
                dealer3 = dealer3SpawnPos4.CreatePed(dealerPedStringKeyValuePair.Key.RandomElement());
                dealer3.IsPersistent = true;
                dealer3.BlockPermanentEvents = true;
                Game.LogTrivial("Decided to spawn dealer3.");
                if (UsefulExtensions.Decide(50))
                {
                    dealer1.WarpIntoVehicle(dealerCar, -1);
                    Game.LogTrivial("Decided to warp dealer1 into dealerCar.");
                }
                else { Game.LogTrivial("Decided NOT to warp dealer1 into dealerCar."); }
                if (UsefulExtensions.Decide(70))
                {
                    dealer3.Inventory.GiveNewWeapon(badBoyBigGunList.RandomElement(), 999, true);
                    Game.LogTrivial("Decided to give dealer3 bigger gun.");
                    dealer3WasGivenBiggerGun = true;
                }
                else { Game.LogTrivial("Decided NOT to give dealer3 bigger gun."); }
                list.Add(dealer3);
            }
            else { Game.LogTrivial("Decided NOT to spawn dealer3."); }
            foreach (var dealer in list)
            {
                NativeFunction.Natives.SetPedCombatAbility(dealer, 2);
                NativeFunction.Natives.SetPedCombatMovement(dealer, 1);
                dealer.Inventory.GiveNewWeapon(badBoyPistolList.RandomElement(), 999, false);
                dealer.RelationshipGroup = "DRUGDEAL_DEALER";
            }
            return list;
        }

        private List<Ped> MakeBuyerList()
        {
            var list = new List<Ped> { };
            var buyerPedStringKeyValuePair = badBoyPedStringListDict.RandomElement();
            buyer1 = buyer1SpawnPos4.CreatePed(buyerPedStringKeyValuePair.Key.RandomElement());
            buyer1.IsPersistent = true;
            buyer1.BlockPermanentEvents = true;
            list.Add(buyer1);
            buyer2 = buyer2SpawnPos4.CreatePed(buyerPedStringKeyValuePair.Key.RandomElement());
            buyer2.IsPersistent = true;
            buyer2.BlockPermanentEvents = true;
            list.Add(buyer2);
            foreach (var buyer in list)
            {
                buyer.Inventory.GiveNewWeapon(badBoyPistolList.RandomElement(), 999, false);
                NativeFunction.Natives.SetPedCombatAbility(buyer, 2);
                NativeFunction.Natives.SetPedCombatMovement(buyer, 1);
                buyer.RelationshipGroup = "DRUGDEAL_BUYER";
            }
            return list;
        }

        public List<Ped> MakeListOfOccupants(Vehicle vehicle)
        {
            return new List<Ped>(vehicle.Occupants);
        }
    }
}
