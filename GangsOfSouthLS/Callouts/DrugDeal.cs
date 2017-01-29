using System.Collections.Generic;
using System.Windows.Forms;
using Rage;
using Rage.Native;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using GangRaids.HelperClasses;
using GangRaids.HelperClasses.DrugDealHelpers;
using GangRaids.INIFile;
using GangRaids.HelperClasses.CommonUtilities;

namespace GangRaids.Callouts
{
    [CalloutInfo("Drug Deal", CalloutProbability.High)]
    class DrugDeal : Callout
    {
        internal static DrugDealScenario Scenario;
        internal static DrugDealScenarioScheme ScenarioScheme;
        internal static Vector3 PlayerStartPosition;
        internal static Vector3 PlayerEndPosition;
        internal static string PlayerDirection;
        private LHandle Pursuit;
        private bool DealersAndBuyersHateEachOther;
        private bool Buyer2IsFightingStraightAway;
        private bool HasAnyPedBeenAddedToPursuit = false;

        private Blip Dealer1Blip;
        private Blip Dealer2Blip;
        private Blip Dealer3Blip;
        private Blip Buyer1Blip;
        private Blip Buyer2Blip;
        private Blip copCar1Blip;
        private Blip copCar2Blip;
        private Dictionary<Ped, Blip> PedBlipDict;
        private List<Ped> FighterList;

        internal static Blip PlayerStartPointBlip;
        internal static Blip PlayerEndPointBlip;
        internal static Blip SuspectsAreaBlip;
        internal static EDrugDealState DrugDealState;
        internal static bool IsCurrentlyRunning = false;
        internal static bool PlayerIsInPosition = false;

        private bool IsWaitTimeOver;
        private bool firstloop = true;
        private bool endingregularly = false;
        private bool copCar1arrived = false;
        private bool copCar2arrived = false;
        private bool playerAddedToPursuit = false;


        public override bool OnBeforeCalloutDisplayed()
        {
            var scenarioFound = DrugDealScenarioScheme.ChooseScenario(out ScenarioScheme);
            if (!scenarioFound)
            {
                Game.LogTrivial("[GANG RAIDS] Could not find scenario in range.");
                return false;
            }
            Scenario = new DrugDealScenario(ScenarioScheme);
            CalloutMessage = "Drug deal in progress";
            CalloutPosition = Scenario.Position;
            Functions.PlayScannerAudioUsingPosition(string.Format("DISP_ATTENTION_UNIT_01 {0} ASSISTANCE_REQUIRED FOR CRIME_DRUGDEAL IN_OR_ON_POSITION UNITS_RESPOND_CODE_02", INIReader.UnitName), Scenario.Position);
            ShowCalloutAreaBlipBeforeAccepting(CalloutPosition, 70f);
            DealersAndBuyersHateEachOther = false;
            Buyer2IsFightingStraightAway = false;
            return base.OnBeforeCalloutDisplayed();
        }


        public override bool OnCalloutAccepted()
        {
            Scenario.Initialize();
            IsCurrentlyRunning = true;
            PedBlipDict = new Dictionary<Ped, Blip> { };
            FighterList = new List<Ped> { };
            DrugDealState = EDrugDealState.Accepted;
            SuspectsAreaBlip = new Blip(Scenario.Position, 50f);
            SuspectsAreaBlip.Alpha = 0.5f;
            SuspectsAreaBlip.Color = System.Drawing.Color.Yellow;
            SuspectsAreaBlip.IsRouteEnabled = true;
            if (Game.LocalPlayer.Character.Position.DistanceTo(Scenario.Position) < 201f)
            {
                Game.DisplayHelp("[GANG RAIDS] You are too close too the area, aborting callout.");
                Game.LogTrivial("[GANG RAIDS] Too close to callout area when accepting, aborting.");
                return false;
            }
            return base.OnCalloutAccepted();
        }


        public override void Process()
        {
            base.Process();

            if (DrugDealState == EDrugDealState.Accepted)
            {
                if (Game.LocalPlayer.Character.Position.DistanceTo(Scenario.Position) < 200f)
                {
                    SuspectsAreaBlip.DisableRoute();
                    Functions.PlayScannerAudio("DISP_ATTENTION_UNIT_02 " + INIReader.UnitName + " SUSPECTS_ARE_MEMBERS_OF " + Scenario.DealerGangNameString + " GangRaids_PROCEED_WITH_CAUTION");
                    DrugDealState = EDrugDealState.InPreparation;
                }
            }

            if (DrugDealState == EDrugDealState.InPreparation)
            {
                if (Game.LocalPlayer.Character.IsDead)
                {
                    Game.LogTrivial("[GANG RAIDS] Player died, ending callout.");
                    End();
                }
                if (Game.LocalPlayer.Character.Position.DistanceTo(PlayerStartPosition) > 7f)
                {
                    PlayerIsInPosition = false;
                    if (INIReader.MenuModifierKey != Keys.None)
                    {
                        Game.DisplayHelp(string.Format("Get into ~p~position ~w~or press ~b~{0} + {1} ~w~to open the menu. Be careful not to get to close to the ~y~suspects~w~!", INIReader.MenuModifierKey.ToString(), INIReader.MenuKey.ToString()));
                    }
                    else
                    {
                        Game.DisplayHelp(string.Format("Get into ~p~position ~w~or press ~b~{0} ~w~to open the menu. Be careful not to get to close to the ~y~suspects~w~!", INIReader.MenuKey.ToString()));
                    }
                }
                else
                {
                    Game.DisplayHelp("Press ~b~Y ~w~to engage suspects.");
                    PlayerIsInPosition = true;
                    if (Game.IsKeyDown(Keys.Y) && !Functions.IsPoliceComputerActive())
                    {
                        if (!(PlayerStartPointBlip == null) && PlayerStartPointBlip.Exists())
                        {
                            PlayerStartPointBlip.Delete();
                        }
                        PlayerIsInPosition = false;
                        Functions.PlayScannerAudio("APPROACH_FROM " + PlayerDirection);
                        DrugDealState = EDrugDealState.EngagingSuspects;
                    }
                }
                if (IsPlayerTooCloseToSuspects())
                {
                    Game.DisplayHelp("The ~r~suspects ~w~saw you, it's now or never!");
                    DrugDealState = EDrugDealState.Arrived;
                }
            }

            if (DrugDealState == EDrugDealState.EngagingSuspects)
            {
                if (Game.LocalPlayer.Character.IsDead)
                {
                    Game.LogTrivial("[GANG RAIDS] Player died, ending callout.");
                    End();
                }
                PlayerEndPointBlip = new Blip(PlayerEndPosition);
                PlayerEndPointBlip.Color = System.Drawing.Color.Purple;
                GameFiber.StartNew(delegate
                {
                    copCar1Blip = new Blip(Scenario.CopCar1);
                    copCar1Blip.Color = System.Drawing.Color.FromArgb(93, 182, 229);
                    Scenario.CopCar1.Driver.Tasks.DriveToPosition(Scenario.CopCarDict[Scenario.CopCar1].endPoint.Position, 10f, VehicleDrivingFlags.Emergency, 5f).WaitForCompletion(20000);
                    copCar1Blip.Delete();
                    copCar1arrived = true;
                    return;
                });
                GameFiber.StartNew(delegate
                {
                    copCar2Blip = new Blip(Scenario.CopCar2);
                    copCar2Blip.Color = System.Drawing.Color.FromArgb(93, 182, 229);
                    Scenario.CopCar2.Driver.Tasks.DriveToPosition(Scenario.CopCarDict[Scenario.CopCar2].endPoint.Position, 10f, VehicleDrivingFlags.Emergency, 5f).WaitForCompletion(20000);
                    copCar2Blip.Delete();
                    copCar2arrived = true;
                    return;
                });
                IsWaitTimeOver = false;
                GameFiber.StartNew(delegate
                {
                    GameFiber.Wait(20000);
                    if (DrugDealState == EDrugDealState.Waiting)
                    {
                        IsWaitTimeOver = true;
                        Game.LogTrivial("[GANG RAIDS] Gave Player and Cops 20 seconds to arrive, starting logic anyway.");
                    }

                    return;
                });
                DrugDealState = EDrugDealState.Waiting;
            }

            if (DrugDealState == EDrugDealState.Waiting)
            {
                if (Game.LocalPlayer.Character.IsDead)
                {
                    Game.LogTrivial("[GANG RAIDS] Player died, ending callout.");
                    End();
                }
                Game.DisplayHelp("Converge with the ~b~other units ~w~at the ~y~suspects' location~w~.");
                if (IsWaitTimeOver || IsPlayerTooCloseToSuspects() || HaveCopsArrived())
                {
                    foreach (var veh in Scenario.CopCarDict.Keys)
                    {
                        veh.IsSirenOn = true;
                    }
                    Game.LogTrivial("[GANG RAIDS] Engaging Suspects");
                    DrugDealState = EDrugDealState.Arrived;
                }
            }

            if (DrugDealState == EDrugDealState.Arrived)
            {
                if (Game.LocalPlayer.Character.IsDead)
                {
                    Game.LogTrivial("[GANG RAIDS] Player died, ending callout.");
                    End();
                }
                if (!(SuspectsAreaBlip == null) && SuspectsAreaBlip.Exists())
                {
                    SuspectsAreaBlip.Delete();
                }
                if (!(PlayerEndPointBlip == null) && PlayerEndPointBlip.Exists())
                {
                    PlayerEndPointBlip.Delete();
                }
                if (!(PlayerStartPointBlip == null) && PlayerStartPointBlip.Exists())
                {
                    PlayerStartPointBlip.Delete();
                }
                CreateBlip(Scenario.Buyer1, Buyer1Blip);
                CreateBlip(Scenario.Buyer2, Buyer2Blip);
                CreateBlip(Scenario.Dealer1, Dealer1Blip);
                CreateBlip(Scenario.Dealer2, Dealer2Blip);
                if (Scenario.Dealer3WasSpawned)
                {
                    CreateBlip(Scenario.Dealer3, Dealer3Blip);
                }

                Pursuit = Functions.CreatePursuit();
                Game.SetRelationshipBetweenRelationshipGroups("DRUGDEAL_DEALER", "COP", Relationship.Hate);
                Game.SetRelationshipBetweenRelationshipGroups("DRUGDEAL_BUYER", "COP", Relationship.Hate);
                Game.SetRelationshipBetweenRelationshipGroups("DRUGDEAL_DEALER", "PLAYER", Relationship.Hate);
                Game.SetRelationshipBetweenRelationshipGroups("DRUGDEAL_BUYER", "PLAYER", Relationship.Hate);
                Game.SetRelationshipBetweenRelationshipGroups("COP", "DRUGDEAL_BUYER", Relationship.Hate);
                Game.SetRelationshipBetweenRelationshipGroups("COP", "DRUGDEAL_DEALER", Relationship.Hate);
                if (UsefulExtensions.Decide(15))
                {
                    Game.SetRelationshipBetweenRelationshipGroups("DRUGDEAL_DEALER", "DRUGDEAL_BUYER", Relationship.Hate);
                    Game.SetRelationshipBetweenRelationshipGroups("DRUGDEAL_BUYER", "DRUGDEAL_DEALER", Relationship.Hate);
                    DealersAndBuyersHateEachOther = true;
                    Game.LogTrivial("[GANG RAIDS] Decided to make Dealers and Buyers hate each other.");
                }
                else
                {
                    Game.LogTrivial("[GANG RAIDS] Decided NOT to make Dealers and Buyers hate each other.");
                }
                MakeDealer2DoStuff();
                MakeDealer1DoStuff();
                MakeBuyer2DoStuff();
                MakeBuyer1DoStuff();
                if (Scenario.Dealer3WasSpawned)
                {
                    MakeDealer3DoStuff();
                }
                if (!(Functions.GetPursuitPeds(Pursuit) == null) && !(Functions.GetPursuitPeds(Pursuit).Length == 0) )
                {
                    Game.LogTrivial("[GANG RAIDS] Adding cops to pursuit.");
                    foreach (var cop in Scenario.CopList1)
                    {
                        GameFiber.StartNew(delegate
                        {
                            while (!copCar1arrived)
                            {
                                GameFiber.Yield();
                            }
                            if (cop.IsInAnyVehicle(false))
                            {
                                cop.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen).WaitForCompletion(5000);
                            }
                            Functions.AddCopToPursuit(Pursuit, cop);
                            return;
                        });
                    }
                    foreach (var cop in Scenario.CopList2)
                    {
                        GameFiber.StartNew(delegate
                        {
                            while (!copCar2arrived)
                            {
                                GameFiber.Yield();
                            }
                            if (cop.IsInAnyVehicle(false))
                            {
                                cop.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen).WaitForCompletion(5000);
                            }
                            Functions.AddCopToPursuit(Pursuit, cop);
                            return;
                        });
                    }
                }
                else
                {
                    Game.LogTrivial("[GANG RAIDS] Making cops fight.");
                    foreach (var cop in Scenario.CopList1)
                    {
                        GameFiber.StartNew(delegate
                        {
                            while (!copCar1arrived)
                            {
                                GameFiber.Yield();
                            }
                            cop.Tasks.FightAgainstClosestHatedTarget(150f);
                            return;
                        });
                    }
                    foreach (var cop in Scenario.CopList2)
                    {
                        GameFiber.StartNew(delegate
                        {
                            while (!copCar2arrived)
                            {
                                GameFiber.Yield();
                            }
                            cop.Tasks.FightAgainstClosestHatedTarget(150f);
                            return;
                        });
                    }
                }
                DrugDealState = EDrugDealState.PlayingLogic;
            }


            if (DrugDealState == EDrugDealState.PlayingLogic)
            {
                if (Game.LocalPlayer.Character.IsDead)
                {
                    Game.LogTrivial("[GANG RAIDS] Player died, ending callout.");
                    End();
                }
                if (firstloop)
                {
                    firstloop = false;
                    GameFiber.StartNew(delegate
                    {
                        Game.LogTrivial("[GANG RAIDS] Waiting 10 s to allow the callout logic to play out.");
                        GameFiber.Wait(10000);
                        Game.LogTrivial("[GANG RAIDS] Waited 10 s, checking if player left scene.");
                        DrugDealState = EDrugDealState.WaitingToLeaveScene;
                        firstloop = true;
                        return;
                    });
                }
                CleanUp();
            }


            if (DrugDealState == EDrugDealState.WaitingToLeaveScene)
            {
                CleanUp();
                if (Game.LocalPlayer.Character.IsDead)
                {
                    Game.LogTrivial("[GANG RAIDS] Player died, ending callout.");
                    End();
                }
                if (HasPlayerLeftScene())
                {
                    Game.LogTrivial("[GANG RAIDS] Player left scene, allowing callout to be ended");
                    DrugDealState = EDrugDealState.CanBeEnded;
                }
            }


            if (DrugDealState == EDrugDealState.CanBeEnded)
            {
                CleanUp();
                if (Game.LocalPlayer.Character.IsDead)
                {
                    Game.LogTrivial("[GANG RAIDS] Player died, ending callout.");
                    End();
                }
                if (!HasPlayerLeftScene())
                {
                    Game.LogTrivial("[GANG RAIDS] Player reentered the scene, NOT allowing callout to be ended");
                    DrugDealState = EDrugDealState.WaitingToLeaveScene;
                }
                if (HasAnyPedBeenAddedToPursuit && !Functions.IsPursuitStillRunning(Pursuit) && (FighterList.Count == 0))
                {
                    endingregularly = true;
                    Game.LogTrivial("[GANG RAIDS] Pursuit is not running any more and nobody's still fighting, ending callout.");
                    End();
                }
                else if (HasAnyPedBeenAddedToPursuit && (Functions.GetActivePursuit() == null) && (FighterList.Count == 0))
                {
                    endingregularly = true;
                    Game.LogTrivial("[GANG RAIDS] Player left pursuit and nobody's still fighting, ending callout.");
                    End();
                }
                else if (!HasAnyPedBeenAddedToPursuit && !IsAnySuspectStillAliveAndNotArrested())
                {
                    endingregularly = true;
                    Game.LogTrivial("[GANG RAIDS] Every suspect is either dead or arrested, ending callout.");
                    End();
                }
            }
        }


        public override void End()
        {
                IsCurrentlyRunning = false;

                if (!(SuspectsAreaBlip == null) && SuspectsAreaBlip.Exists())
                {
                    SuspectsAreaBlip.Delete();
                }
                if (!(PlayerStartPointBlip == null) && PlayerStartPointBlip.Exists())
                {
                    PlayerStartPointBlip.Delete();
                }
                if (!(PlayerEndPointBlip == null) && PlayerEndPointBlip.Exists())
                {
                    PlayerEndPointBlip.Delete();
                }
                if (!(copCar1Blip == null) && copCar1Blip.Exists())
                {
                    copCar1Blip.Delete();
                }
                if (!(copCar2Blip == null) && copCar2Blip.Exists())
                {
                    copCar2Blip.Delete();
                }
                if (!(PedBlipDict == null))
                {
                    foreach (var blip in PedBlipDict.Values)
                    {
                        if (blip.Exists())
                        {
                            blip.Delete();
                        }
                    }
                }
                if (!(Scenario == null))
                {
                    if (!(Scenario.BadBoyCarList == null))
                    {
                        foreach (var veh in Scenario.BadBoyCarList)
                        {
                            if (veh.Exists())
                            {
                                veh.Dismiss();
                            }
                        }
                    }
                    if (!(Scenario.CopCarDict == null))
                    {
                        foreach (var veh in Scenario.CopCarDict.Keys)
                        {
                            if (veh.Exists())
                            {
                                veh.Dismiss();
                            }
                        }
                    }
                }


                if (!endingregularly)
                {
                    Game.LogTrivial("[GANG RAIDS] NOT ending callout regularly.");
                    if (!(Scenario == null))
                    {
                        if (!(Scenario.DealerList == null))
                        {
                            foreach (var dealer in Scenario.DealerList)
                            {
                                if (dealer.Exists())
                                {
                                    dealer.Dismiss();
                                }
                            }
                        }

                        if (!(Scenario.BuyerList == null))
                        {
                            foreach (var Buyer in Scenario.BuyerList)
                            {
                                if (Buyer.Exists())
                                {
                                    Buyer.Dismiss();
                                }
                            }
                        }

                        if (!(Scenario.CopList1 == null))
                        {
                            foreach (var Cop in Scenario.CopList1)
                            {
                                if (Cop.Exists())
                                {
                                    Cop.Dismiss();
                                }
                            }
                        }

                        if (!(Scenario.CopList2 == null))
                        {
                            foreach (var Cop in Scenario.CopList2)
                            {
                                if (Cop.Exists())
                                {
                                    Cop.Dismiss();
                                }
                            }
                        }
                    }
                }
                base.End();
        }


        internal enum EDrugDealState
        {
            Accepted, InPreparation, EngagingSuspects, CanBeEnded, Arrived, Waiting, PlayingLogic, WaitingToLeaveScene
        }


        private void MakeDealer2DoStuff()
        {
            if (UsefulExtensions.Decide(50))
            {
                Game.LogTrivial("[GANG RAIDS] Decided to make Dealer2 try to enter van.");
                GameFiber.StartNew(delegate
                {
                    Scenario.Dealer2.Tasks.EnterVehicle(Scenario.DealerVan, -1, 3f);
                    var counter = 0;
                    while (!Scenario.Dealer2.IsInAnyVehicle(false) && counter <= 6000)
                    {
                        counter += 500;
                        GameFiber.Wait(500);
                        Game.LogTrivial(string.Format("[GANG RAIDS] Waited {0} ms for Dealer2 to get into van.", counter));
                    }
                    if (Scenario.Dealer2.IsInAnyVehicle(false))
                    {
                        Game.LogTrivial("[GANG RAIDS] Dealer2 made it into van, adding to pursuit.");
                        AddToPursuitAndDeleteBlip(Scenario.Dealer2);
                    }
                    else
                    {
                        Game.LogTrivial("[GANG RAIDS] Dealer2 did NOT make it into van, making him fight.");
                        MakeFight(Scenario.Dealer2);
                    }
                    return;
                });
            }
            else
            {
                if (UsefulExtensions.Decide(80))
                {
                    Game.LogTrivial("[GANG RAIDS] Decided to make Dealer2 fight straight away.");
                    MakeFight(Scenario.Dealer2);
                }
                else
                {
                    Game.LogTrivial("[GANG RAIDS] Decided to make Dealer2 run away, adding to pursuit.");
                    AddToPursuitAndDeleteBlip(Scenario.Dealer2);
                }
            }
        }


        private void MakeDealer1DoStuff()
        {
            if (Scenario.Dealer1.IsInAnyVehicle(false))
            {
                Game.LogTrivial("[GANG RAIDS] Dealer1 is in car.");
                if (UsefulExtensions.Decide(70))
                {
                    Game.LogTrivial("[GANG RAIDS] Decided to make Dealer1 flee in car, adding to pursuit");
                    AddToPursuitAndDeleteBlip(Scenario.Dealer1);
                }
                else
                {
                    Game.LogTrivial("[GANG RAIDS] Decided to make Dealer1 exit car and fight");
                    MakeFight(Scenario.Dealer1);
                }
            }
            else
            {
                Game.LogTrivial("[GANG RAIDS] Dealer1 is NOT in car.");
                if (UsefulExtensions.Decide(60))
                {
                    Game.LogTrivial("[GANG RAIDS] Decided to make Dealer1 try to enter car.");
                    GameFiber.StartNew(delegate
                    {
                        Scenario.Dealer1.Tasks.EnterVehicle(Scenario.DealerCar, -1, 3f);
                        var counter = 0;
                        while(!Scenario.Dealer1.IsInAnyVehicle(false) && counter <= 6000)
                        {
                            counter += 500;
                            GameFiber.Wait(500);
                            Game.LogTrivial(string.Format("[GANG RAIDS] Waited {0} ms for Dealer1 to get into car.", counter));
                        }
                        if (Scenario.Dealer1.IsInAnyVehicle(false))
                        {
                            Game.LogTrivial("[GANG RAIDS] Dealer1 made it into car, adding to pursuit.");
                            AddToPursuitAndDeleteBlip(Scenario.Dealer1);
                        }
                        else
                        {
                            Game.LogTrivial("[GANG RAIDS] Dealer1 did NOT make it into car, making him fight.");
                            MakeFight(Scenario.Dealer1);
                        }
                        return;
                    });
                }
                else
                {
                    Game.LogTrivial("[GANG RAIDS] Decided to make Dealer1 fight straight away.");
                    MakeFight(Scenario.Dealer1);
                }
            }
        }


        private void MakeDealer3DoStuff()
        {
            Game.LogTrivial("[GANG RAIDS] Dealer3 always fights straight away.");
            MakeFight(Scenario.Dealer3);
        }


        private void MakeBuyer1DoStuff()
        {
            if (Buyer2IsFightingStraightAway)
            {
                if (UsefulExtensions.Decide(70))
                {
                    Game.LogTrivial("[GANG RAIDS] Decided to make Buyer1 join Buyer2 in combat");
                    MakeFight(Scenario.Buyer1);
                }
                else
                {
                    Game.LogTrivial("[GANG RAIDS] Decided to make Buyer1 try to enter car.");
                    GameFiber.StartNew(delegate
                    {
                        Scenario.Buyer1.Tasks.EnterVehicle(Scenario.BuyerCar, -1, 3f);
                        var counter = 0;
                        while (!Scenario.Buyer1.IsInAnyVehicle(false) && counter <= 6000)
                        {
                            counter += 500;
                            GameFiber.Wait(500);
                            Game.LogTrivial(string.Format("[GANG RAIDS] Waited {0} ms for Buyer1 to get into car.", counter));
                        }
                        if (Scenario.Buyer1.IsInAnyVehicle(false))
                        {
                            Game.LogTrivial("[GANG RAIDS] Buyer1 made it into car, adding to pursuit.");
                            AddToPursuitAndDeleteBlip(Scenario.Buyer1);
                        }
                        else
                        {
                            Game.LogTrivial("[GANG RAIDS] Buyer1 did NOT make it into car, making him fight.");
                            MakeFight(Scenario.Buyer1);
                        }
                        return;
                    });
                }
            }
            else
            {
                Game.LogTrivial("[GANG RAIDS] Making Buyer1 enter car and wait for Buyer2");
                GameFiber.StartNew(delegate 
                {
                    Scenario.Buyer1.Tasks.EnterVehicle(Scenario.BuyerCar, 0, 3f);
                    var counter = 0;
                    while(!Scenario.Buyer1.IsInAnyVehicle(false) && counter <= 4000)
                    {
                        counter += 500;
                        GameFiber.Wait(500);
                        Game.LogTrivial(string.Format("[GANG RAIDS] Waited {0} ms for Buyer1 to get into car.", counter));
                    }
                    if (Scenario.Buyer1.IsInAnyVehicle(false))
                    {
                        Game.LogTrivial("[GANG RAIDS] Buyer1 made it into car, waiting for Buyer2.");
                        while(!Scenario.Buyer2.IsInAnyVehicle(false) && counter <= 6000)
                        {
                            counter += 500;
                            GameFiber.Wait(500);
                            Game.LogTrivial(string.Format("[GANG RAIDS] Buyer1 Waited {0} ms for Buyer2 to get in car.", counter));
                        }
                        if (Scenario.Buyer2.IsInAnyVehicle(false))
                        {
                            Game.LogTrivial("[GANG RAIDS] Both Buyers are in car, making Buyer1 shoot out of the vehicle.");
                            NativeFunction.Natives.SetPedCombatAttributes(Scenario.Buyer1, 1, true);
                            MakeFight(Scenario.Buyer1);
                            GameFiber.StartNew(delegate 
                            {
                                while(Scenario.Buyer1.IsInAnyVehicle(false) && !Scenario.Buyer1.CurrentVehicle.IsSeatFree(-1) && !Scenario.Buyer1.CurrentVehicle.Driver.IsDead)
                                {
                                    GameFiber.Yield();
                                }
                                if (Scenario.Buyer1.Exists() && Scenario.Buyer1.IsAlive)
                                {
                                    Game.LogTrivial("[GANG RAIDS] Buyer1's driver died or disappeared.");
                                    GameFiber.Wait(200); //Allowing time for driver to despawn (if "suspect escaped")
                                    if (!Scenario.Buyer2.Exists())
                                    {
                                        Game.LogTrivial("[GANG RAIDS] Buyer 2 doesn't exist any more (suspect escaped). Despawning Buyer1, too.");
                                        if (Buyer1Blip.Exists())
                                        {
                                            Buyer1Blip.Delete();
                                        }
                                        if (Scenario.Buyer1.Exists())
                                        {
                                            Scenario.Buyer1.Delete();
                                        }
                                    }
                                    if (UsefulExtensions.Decide(30))
                                    {
                                        Game.LogTrivial("[GANG RAIDS] Decided to make Buyer1 flee.");
                                        if (HasAnyPedBeenAddedToPursuit && Functions.IsPursuitStillRunning(Pursuit))
                                        {
                                            Game.LogTrivial("[GANG RAIDS] Pursuit is still active, adding Buyer1.");
                                            AddToPursuitAndDeleteBlip(Scenario.Buyer1);
                                        }
                                        else
                                        {
                                            Game.LogTrivial("[GANG RAIDS] Pursuit is not active anymore, making Buyer1 fight instead");
                                        }
                                    }
                                    else
                                    {
                                        Game.LogTrivial("[GANG RAIDS] Decided to make Buyer1 fight.");
                                    }
                                }
                                return;
                            });
                        }
                        else
                        {
                            Game.LogTrivial("[GANG RAIDS] Buyer1 is alone in car.");
                            if (Functions.IsPursuitStillRunning(Pursuit))
                            {
                                Game.LogTrivial("[GANG RAIDS] Pursuit is active, making Buyer1 shuffle to drivers seat and adding him to pursuit.");
                                Scenario.Buyer1.Tasks.ShuffleToAdjacentSeat().WaitForCompletion(3000);
                                AddToPursuitAndDeleteBlip(Scenario.Buyer1);
                            }
                            else
                            {
                                Game.LogTrivial("[GANG RAIDS] Pursuit isn't active, making Buyer1 fight.");
                                MakeFight(Scenario.Buyer1);
                            }
                        }
                    }
                    else
                    {
                        Game.LogTrivial("[GANG RAIDS] Buyer1 did NOT make it into car, making him fight.");
                        MakeFight(Scenario.Buyer1);
                    }
                    return;
                });
            }
        }


        private void MakeBuyer2DoStuff()
        {
            if (DealersAndBuyersHateEachOther)
            {
                Game.LogTrivial("[GANG RAIDS] Dealers and Buyers hate each other, so made Buyer2 fight straight away.");
                MakeFight(Scenario.Buyer2);
                Buyer2IsFightingStraightAway = true;
            }
            else
            {
                if (UsefulExtensions.Decide(70))
                {
                    Game.LogTrivial("[GANG RAIDS] Decided to make Buyer2 try to enter car.");
                    GameFiber.StartNew(delegate
                    {
                        Scenario.Buyer2.Tasks.EnterVehicle(Scenario.BuyerCar, -1, 3f);
                        var counter = 0;
                        while (!Scenario.Buyer2.IsInAnyVehicle(false) && counter <= 4000)
                        {
                            counter += 500;
                            GameFiber.Wait(500);
                            Game.LogTrivial(string.Format("[GANG RAIDS] Waited {0} ms for Buyer2 to get into car.", counter));
                        }
                        if (Scenario.Buyer2.IsInAnyVehicle(false))
                        {
                            Game.LogTrivial("[GANG RAIDS] Buyer2 made it to car, adding to pursuit.");
                            AddToPursuitAndDeleteBlip(Scenario.Buyer2);
                        }
                        else
                        {
                            Game.LogTrivial("[GANG RAIDS] Buyer2 did NOT make it into van, making him fight.");
                            MakeFight(Scenario.Buyer2);
                        }
                        return;
                    });
                }
                else
                {
                    Game.LogTrivial("[GANG RAIDS] Decided not to make Buyer2 try to enter the car and fight straight away instead.");
                    MakeFight(Scenario.Buyer2);
                    Buyer2IsFightingStraightAway = true;
                }
            }
        }


        private void CleanUp()
        {

            foreach(var pedblip in PedBlipDict)
            {
                if (pedblip.Key.Exists() && pedblip.Key.IsDead)
                {
                    pedblip.Key.Dismiss();
                }
                if (pedblip.Value.Exists() && (!pedblip.Key.Exists() || pedblip.Key.IsDead))
                {
                    Game.LogTrivial("[GANG RAIDS] Suspect was killed, deleting blip.");
                    pedblip.Value.Delete();
                }
            }
            var newList = new List<Ped> { };
            foreach(var fighter in FighterList)
            {
                if (fighter.Exists() && !fighter.IsDead && !Functions.IsPedArrested(fighter))
                {
                    newList.Add(fighter);
                }
            }
            FighterList = newList;
        }


        private void MakeFight(Ped ped)
        {
            ped.Tasks.FightAgainstClosestHatedTarget(100f);
            FighterList.Add(ped);
        }


        private void AddToPursuitAndDeleteBlip(Ped ped)
        {
            if (!(PedBlipDict[ped] == null) && PedBlipDict[ped].Exists())
            {
                PedBlipDict[ped].Delete();
            }
            Functions.AddPedToPursuit(Pursuit, ped);
            if(!playerAddedToPursuit)
            {
                playerAddedToPursuit = true;
                Game.LogTrivial("[GANG RAIDS] Setting Pursuit as active for player");
                Functions.SetPursuitIsActiveForPlayer(Pursuit, true);
                if (INIReader.RequestAirSupport)
                {
                    Game.LogTrivial("[GANG RAIDS] Requesting air support unit.");
                    Functions.RequestBackup(Game.LocalPlayer.Character.Position, LSPD_First_Response.EBackupResponseType.Pursuit, LSPD_First_Response.EBackupUnitType.AirUnit);
                }
            }
            HasAnyPedBeenAddedToPursuit = true;
        }


        private bool IsPlayerTooCloseToSuspects()
        {
            foreach (var waypoint in Scenario.CopCarWayPointList)
            {
                if (Game.LocalPlayer.Character.Position.DistanceTo(waypoint.endPoint.Position) < 5f)
                {
                    return true;
                }
            }
            if (Game.LocalPlayer.Character.Position.DistanceTo(Scenario.Position) < 30f)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool IsAnySuspectStillAliveAndNotArrested()
        {
            var SuspectsAreStillAliveAndFree = false;
            foreach (var dealer in Scenario.DealerList)
            {
                if (dealer.Exists() && dealer.IsAlive && !Functions.IsPedArrested(dealer))
                {
                    SuspectsAreStillAliveAndFree = true;
                }
            }
            foreach (var buyer in Scenario.BuyerList)
            {
                if (buyer.Exists() && buyer.IsAlive && !Functions.IsPedArrested(buyer))
                {
                    SuspectsAreStillAliveAndFree = true;
                }
            }
            return SuspectsAreStillAliveAndFree;
        }

        private bool HaveCopsArrived()
        {
            foreach(var waypoint in Scenario.CopCarWayPointList)
            {
                if (Scenario.CopCar1.DistanceTo(waypoint.endPoint.Position) < 5f)
                {
                    return true;
                }
                if (Scenario.CopCar2.DistanceTo(waypoint.endPoint.Position) < 5f)
                {
                    return true;
                }
            }
            return false;
        }

        private void CreateBlip(Ped ped, Blip blip)
        {
            blip = new Blip(ped);
            blip.Color = System.Drawing.Color.FromArgb(224, 50, 50);
            blip.Scale = 0.75f;
            blip.Order = 0;
            PedBlipDict.Add(ped, blip);
        }

        private bool HasPlayerLeftScene()
        {
            if (Game.LocalPlayer.Character.Position.DistanceTo(Scenario.Position) < 70f)
            {
                return false;
            }
            return true;
        }
    }
}
