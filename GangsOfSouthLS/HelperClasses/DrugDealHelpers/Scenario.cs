using GangsOfSouthLS.HelperClasses.CommonUtilities;
using Rage;
using Rage.Native;
using System.Collections.Generic;

namespace GangsOfSouthLS.HelperClasses.DrugDealHelpers
{
    internal class Scenario
    {
        private Pos4 buyerCarSpawnPos4;
        private Pos4 dealerCarSpawnPos4;
        private Pos4 dealerVanSpawnPos4;

        private Pos4 dealer1SpawnPos4;
        private Pos4 dealer2SpawnPos4;
        private Pos4 dealer3SpawnPos4;
        private Pos4 buyer1SpawnPos4;
        private Pos4 buyer2SpawnPos4;

        private List<string> buyerCarStringList = new List<string> { "Emperor", "Primo", "Stanier", "Seminole", "Landstalker", "Cavalcade", "Bison", "Oracle", "Asterope", "Fugitive", "Asea", "Ingot", "Premier", "Stratum", "Washington" };
        private List<string> dealerCarStringList = new List<string> { "Buccaneer", "Tornado", "Chino", "Dukes", "Stalion", "Phoenix", "Sabregt", "Vigero", "Peyote", "Ruiner", "Virgo" };
        private List<string> dealerVanStringList = new List<string> { "Speedo", "Burrito3", "Youga" };

        private Dictionary<List<string>, string> badBoyPedStringListDict = new Dictionary<List<string>, string>
        {
            { new List<string> { "g_m_y_ballaeast_01" , "g_m_y_ballaorig_01", "g_m_y_ballasout_01" }, "AFRICAN_AMERICAN_GANG" },
            { new List<string> { "g_m_y_famca_01", "g_m_y_famdnf_01", "g_m_y_famfor_01" }, "AFRICAN_AMERICAN_GANG" },
            { new List<string> { "g_m_y_lost_01", "g_m_y_lost_02", "g_m_y_lost_03" }, "BIKER_GANG" },
            { new List<string> { "g_m_y_mexgang_01", "g_m_y_mexgoon_01", "g_m_y_mexgoon_02", "g_m_y_mexgoon_03" } , "MEXICAN_GANG" }
        };

        private List<string> badBoyPistolList = new List<string> { "weapon_pistol", "weapon_snspistol", "weapon_combatpistol", "weapon_pistol50", "weapon_microsmg" };
        private List<string> badBoyBigGunList = new List<string> { "weapon_assaultrifle", "weapon_pumpshotgun", "weapon_sawnoffshotgun", "weapon_smg" };

        internal Scenario(ScenarioScheme scheme)
        {
            Name = scheme.Name;
            CopCarBuildList = new List<CopCarBuild>(scheme.CopCarBuildList);
            CopCarWayPointList = new List<CopCarWayPoint>(scheme.CopCarWayPointList);
            dealerVanSpawnPos4 = scheme.BadGuyCarSpawnPos4List[0].Copy();
            dealerCarSpawnPos4 = scheme.BadGuyCarSpawnPos4List[1].Copy();
            buyerCarSpawnPos4 = scheme.BadGuyCarSpawnPos4List[2].Copy();
            dealer1SpawnPos4 = scheme.BadGuyPedSpawnPos4List[0].Copy();
            dealer2SpawnPos4 = scheme.BadGuyPedSpawnPos4List[1].Copy();
            dealer3SpawnPos4 = scheme.BadGuyPedSpawnPos4List[2].Copy();
            buyer1SpawnPos4 = scheme.BadGuyPedSpawnPos4List[3].Copy();
            buyer2SpawnPos4 = scheme.BadGuyPedSpawnPos4List[4].Copy();
            Position = scheme.Position.Copy();
            BadBoyCarList = new List<Vehicle> { };
            CopCarDict = new Dictionary<Vehicle, CopCarWayPoint> { };
            Dealer3WasSpawned = false;
        }

        internal Vehicle BuyerCar { get; private set; }
        internal Vehicle DealerCar { get; private set; }
        internal Vehicle DealerVan { get; private set; }
        internal List<MyPed> DealerList { get; private set; }
        internal List<MyPed> BuyerList { get; private set; }
        internal Vehicle CopCar1 { get; set; }
        internal Vehicle CopCar2 { get; set; }
        internal Dictionary<Vehicle, CopCarWayPoint> CopCarDict { get; set; }
        internal List<Vehicle> BadBoyCarList { get; private set; }
        internal string Name { get; private set; }
        internal Vector3 Position { get; private set; }
        internal MyPed Dealer1 { get; private set; }
        internal MyPed Dealer2 { get; private set; }
        internal MyPed Dealer3 { get; private set; }
        internal MyPed Buyer1 { get; private set; }
        internal MyPed Buyer2 { get; private set; }
        internal List<Ped> CopList1 { get; set; }
        internal List<Ped> CopList2 { get; set; }
        internal bool Dealer3WasSpawned { get; private set; }
        internal List<CopCarWayPoint> CopCarWayPointList { get; private set; }
        internal List<CopCarBuild> CopCarBuildList { get; private set; }
        internal string DealerGangNameString { get; private set; }
        internal string BuyerGangNameString { get; private set; }

        internal void Initialize()
        {
            GameFiber.StartNew(delegate
            {
                Game.LogTrivial(string.Format("[GangsOfSouthLS] Initializing {0}", Name));
                foreach (var entity in World.GetEntities(Position, 100f, GetEntitiesFlags.ConsiderAllPeds | GetEntitiesFlags.ConsiderAllVehicles | GetEntitiesFlags.ExcludePlayerPed | GetEntitiesFlags.ExcludePlayerVehicle))
                {
                    if (entity.Exists())
                    {
                        entity.Delete();
                    }
                }
                BadBoyCarList = MakeBadBoyCarList();
                DealerList = MakeDealerList();
                BuyerList = MakeBuyerList();
                GameFiber.Wait(300);
                if (!Dealer1.IsInAnyVehicle(true))
                {
                    Dealer1.Tasks.PlayAnimation("amb@world_human_drug_dealer_hard@male@idle_a", "idle_b", 3f, AnimationFlags.Loop);
                }
                Dealer2.Tasks.PlayAnimation("amb@world_human_stand_impatient@male@no_sign@idle_a", "idle_c", 1f, AnimationFlags.Loop);
                Buyer1.Tasks.PlayAnimation("amb@world_human_drug_dealer_hard@male@idle_a", "idle_c", 4f, AnimationFlags.Loop);
                Buyer2.Tasks.PlayAnimation("amb@world_human_drug_dealer_hard@male@idle_a", "idle_b", 2f, AnimationFlags.Loop);
                return;
            });
        }

        private List<Vehicle> MakeBadBoyCarList()
        {
            var list = new List<Vehicle> { };
            DealerVan = dealerVanSpawnPos4.CreateVehicle(dealerVanStringList.RandomElement());
            list.Add(DealerVan);
            DealerVan.Doors[2].Open(true);
            DealerVan.Doors[3].Open(true);
            DealerCar = dealerCarSpawnPos4.CreateVehicle(dealerCarStringList.RandomElement());
            list.Add(DealerCar);
            BuyerCar = buyerCarSpawnPos4.CreateVehicle(buyerCarStringList.RandomElement());
            list.Add(BuyerCar);
            if (World.DateTime.Hour >= 18 || World.DateTime.Hour < 6)
            {
                foreach (var veh in list)
                {
                    NativeFunction.Natives.SetVehicleLights(veh, 3);
                    veh.IsPersistent = true;
                }
            }
            else
            {
                foreach (var veh in list)
                {
                    veh.IsEngineOn = true;
                    veh.IsPersistent = true;
                }
            }

            return list;
        }

        internal Dictionary<Vehicle, CopCarWayPoint> MakeCopCarDict(CopCarWayPoint WayPoint1, CopCarWayPoint WayPoint2)
        {
            var Dict = new Dictionary<Vehicle, CopCarWayPoint> { };
            Dict.Add(CopCar1, WayPoint1);
            Dict.Add(CopCar2, WayPoint2);
            foreach (var veh in Dict.Keys)
            {
                veh.IsPersistent = true;
            }
            return Dict;
        }

        internal Vehicle MakeCopCarDictVehicleAndOccupy(CopCarWayPoint waypoint, CopCarBuild build)
        {
            Game.LogTrivial(string.Format("[GangsOfSouthLS] Spawned {0} at {1}.", build.CarName, waypoint.Description));
            var veh = waypoint.StartPoint.CreateVehicle(build.CarName);
            foreach (var seat in build.SeatIndicesToOccupy)
            {
                var cop = new Ped(build.PedNameList.RandomElement(), Vector3.Zero, 0f);
                cop.RandomizeVariation();
                var weaponDictEntry = build.WeaponDict.RandomElement();
                cop.Inventory.GiveNewWeapon(weaponDictEntry.Key, 999, true);
                foreach (var component in weaponDictEntry.Value)
                {
                    cop.Inventory.AddComponentToWeapon(weaponDictEntry.Key, component);
                }
                MyNatives.SetPedCombatAbilityAndMovement(cop, MyNatives.CombatAbilityFlag.Professional, MyNatives.CombatMovementFlag.Offensive);
                cop.WarpIntoVehicle(veh, seat);
            }
            veh.IsPersistent = true;
            return veh;
        }

        private List<MyPed> MakeDealerList()
        {
            var list = new List<MyPed> { };
            var dealerPedStringKeyValuePair = badBoyPedStringListDict.RandomElement();
            DealerGangNameString = dealerPedStringKeyValuePair.Value;
            Dealer1 = dealer1SpawnPos4.CreateMyPed(dealerPedStringKeyValuePair.Key.RandomElement());
            Dealer1.RandomizeVariation();
            Dealer1.IsPersistent = true;
            Dealer1.BlockPermanentEvents = true;
            list.Add(Dealer1);
            Dealer2 = dealer2SpawnPos4.CreateMyPed(dealerPedStringKeyValuePair.Key.RandomElement());
            Dealer2.RandomizeVariation();
            Dealer2.IsPersistent = true;
            Dealer2.BlockPermanentEvents = true;
            list.Add(Dealer2);
            if (UsefulFunctions.Decide(50))
            {
                Dealer3WasSpawned = true;
                Dealer3 = dealer3SpawnPos4.CreateMyPed(dealerPedStringKeyValuePair.Key.RandomElement());
                Dealer3.RandomizeVariation();
                Dealer3.IsPersistent = true;
                Dealer3.BlockPermanentEvents = true;
                Game.LogTrivial("[GangsOfSouthLS] Decided to spawn dealer3.");
                if (UsefulFunctions.Decide(50))
                {
                    Dealer1.WarpIntoVehicle(DealerCar, -1);
                    Game.LogTrivial("[GangsOfSouthLS] Decided to warp dealer1 into dealerCar.");
                }
                else { Game.LogTrivial("[GangsOfSouthLS] Decided NOT to warp dealer1 into dealerCar."); }
                if (UsefulFunctions.Decide(70))
                {
                    Dealer3.Inventory.GiveNewWeapon(badBoyBigGunList.RandomElement(), 999, true);
                    Game.LogTrivial("[GangsOfSouthLS] Decided to give dealer3 bigger gun.");
                }
                else { Game.LogTrivial("[GangsOfSouthLS] Decided NOT to give dealer3 bigger gun."); }
                list.Add(Dealer3);
            }
            else { Game.LogTrivial("[GangsOfSouthLS] Decided NOT to spawn dealer3."); }
            foreach (var dealer in list)
            {
                MyNatives.SetPedCombatAbilityAndMovement(dealer, MyNatives.CombatAbilityFlag.Professional, MyNatives.CombatMovementFlag.Defensive);
                dealer.Inventory.GiveNewWeapon(badBoyPistolList.RandomElement(), 999, false);
                dealer.RelationshipGroup = "DRUGDEAL_DEALER";
            }
            return list;
        }

        private List<MyPed> MakeBuyerList()
        {
            var list = new List<MyPed> { };
            var buyerPedStringKeyValuePair = badBoyPedStringListDict.RandomElement();
            BuyerGangNameString = buyerPedStringKeyValuePair.Value;
            Buyer1 = buyer1SpawnPos4.CreateMyPed(buyerPedStringKeyValuePair.Key.RandomElement());
            Buyer1.RandomizeVariation();
            Buyer1.IsPersistent = true;
            Buyer1.BlockPermanentEvents = true;
            list.Add(Buyer1);
            Buyer2 = buyer2SpawnPos4.CreateMyPed(buyerPedStringKeyValuePair.Key.RandomElement());
            Buyer2.RandomizeVariation();
            Buyer2.IsPersistent = true;
            Buyer2.BlockPermanentEvents = true;
            list.Add(Buyer2);
            foreach (var buyer in list)
            {
                buyer.Inventory.GiveNewWeapon(badBoyPistolList.RandomElement(), 999, false);
                MyNatives.SetPedCombatAbilityAndMovement(buyer, MyNatives.CombatAbilityFlag.Professional, MyNatives.CombatMovementFlag.Defensive);
                buyer.RelationshipGroup = "DRUGDEAL_BUYER";
            }
            return list;
        }

        internal List<Ped> MakeListOfOccupants(Vehicle vehicle)
        {
            return new List<Ped>(vehicle.Occupants);
        }
    }
}