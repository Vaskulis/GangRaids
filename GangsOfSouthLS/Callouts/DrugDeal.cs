using GangsOfSouthLS.APIWrappers;
using GangsOfSouthLS.HelperClasses.CommonUtilities;
using GangsOfSouthLS.HelperClasses.DrugDealHelpers;
using GangsOfSouthLS.INIFile;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace GangsOfSouthLS.Callouts
{
    [CalloutInfo("Drug Deal", CalloutProbability.High)]
    internal class DrugDeal : Callout
    {
        internal static Scenario Scenario;
        internal static ScenarioScheme ScenarioScheme;
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
        private List<MyPed> FighterList;
        private List<MyPed> SuspectList;

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

        private bool isLSPDFRPlusRunning;
        private bool isComputerPlusRunning;
        private Guid callID;

        public override bool OnBeforeCalloutDisplayed()
        {
            var scenarioFound = ScenarioScheme.ChooseScenario(out ScenarioScheme);
            if (!scenarioFound)
            {
                Game.LogTrivial("[GangsOfSouthLS] Could not find scenario in range.");
                return false;
            }
            Scenario = new Scenario(ScenarioScheme);
            CalloutMessage = "Drug deal in progress";
            CalloutPosition = Scenario.Position;
            Functions.PlayScannerAudioUsingPosition(string.Format("DISP_ATTENTION_UNIT_01 {0} ASSISTANCE_REQUIRED FOR CRIME_DRUGDEAL IN_OR_ON_POSITION UNITS_RESPOND_CODE_02_02", INIReader.UnitName), Scenario.Position);
            ShowCalloutAreaBlipBeforeAccepting(CalloutPosition, 70f);
            DealersAndBuyersHateEachOther = false;
            Buyer2IsFightingStraightAway = false;
            isComputerPlusRunning = DependencyPluginCheck.IsLSPDFRPluginRunning("ComputerPlus", new Version("1.3.0.0"));
            isLSPDFRPlusRunning = DependencyPluginCheck.IsLSPDFRPluginRunning("LSPDFR+", new Version("1.4.1.0"));
            if (isComputerPlusRunning)
            {
                callID = ComputerPlusWrapperClass.CreateCallout("Drug Deal", "DRUG DEAL", Scenario.Position, ComputerPlus.EResponseType.Code_2, "Reports of a major drug deal between local gangs need to be acted upon. The plan is for three cars to converge on the suspects' location at the same time to try to make it impossible to flee. The suspects could be heavily armed.");
            }
            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            Scenario.Initialize();
            IsCurrentlyRunning = true;
            PedBlipDict = new Dictionary<Ped, Blip> { };
            FighterList = new List<MyPed> { };
            SuspectList = new List<MyPed> { };
            DrugDealState = EDrugDealState.Accepted;
            SuspectsAreaBlip = new Blip(Scenario.Position, 50f);
            SuspectsAreaBlip.Alpha = 0.5f;
            SuspectsAreaBlip.Color = System.Drawing.Color.Yellow;
            SuspectsAreaBlip.IsRouteEnabled = true;
            if (Game.LocalPlayer.Character.Position.DistanceTo(Scenario.Position) < 201f)
            {
                Game.DisplayHelp("[GangsOfSouthLS] You are too close too the area, aborting callout.");
                Game.LogTrivial("[GangsOfSouthLS] Too close to callout area when accepting, aborting.");
                return false;
            }
            if (isComputerPlusRunning)
            {
                ComputerPlusWrapperClass.SetCalloutStatusToUnitResponding(callID);
            }
            return base.OnCalloutAccepted();
        }

        public override void OnCalloutNotAccepted()
        {
            if (isComputerPlusRunning)
            {
                ComputerPlusWrapperClass.AssignCallToAIUnit(callID);
            }
            base.OnCalloutNotAccepted();
        }

        public override void Process()
        {
            base.Process();

            if (DrugDealState == EDrugDealState.Accepted)
            {
                if (Game.LocalPlayer.Character.Position.DistanceTo(Scenario.Position) < 200f)
                {
                    SuspectsAreaBlip.DisableRoute();
                    Functions.PlayScannerAudio("DISP_ATTENTION_UNIT_02 " + INIReader.UnitName + " SUSPECTS_ARE_MEMBERS_OF " + Scenario.DealerGangNameString + " GangsOfSouthLS_PROCEED_WITH_CAUTION");
                    DrugDealState = EDrugDealState.InPreparation;
                    foreach (var suspect in Scenario.DealerList)
                    {
                        SuspectList.Add(suspect);
                        suspect.AddCrimeToList("Conspiracy to distribute drugs", 120);
                    }
                    foreach (var suspect in Scenario.BuyerList)
                    {
                        SuspectList.Add(suspect);
                        suspect.AddCrimeToList("Conspiracy to distribute drugs", 120);
                    }
                    if (isComputerPlusRunning)
                    {
                        ComputerPlusWrapperClass.SetCalloutStatusToAtScene(callID);
                        ComputerPlusWrapperClass.AddUpdateToCallout(callID, "Officer arrived at the scene and starting preparation of the raid. Suspect vehicles identified.");
                        foreach (var veh in Scenario.BadBoyCarList)
                        {
                            ComputerPlusWrapperClass.AddVehicleToCallout(callID, veh);
                        }
                    }
                }
            }

            if (DrugDealState == EDrugDealState.InPreparation)
            {
                if (Game.LocalPlayer.Character.IsDead)
                {
                    Game.LogTrivial("[GangsOfSouthLS] Player died, ending callout.");
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
                    if (isComputerPlusRunning)
                    {
                        ComputerPlusWrapperClass.AddUpdateToCallout(callID, "Officer was seen by the suspects. Starting raid.");
                    }
                    Game.DisplayHelp("The ~r~suspects ~w~saw you, it's now or never!");
                    DrugDealState = EDrugDealState.Arrived;
                }
            }

            if (DrugDealState == EDrugDealState.EngagingSuspects)
            {
                if (Game.LocalPlayer.Character.IsDead)
                {
                    Game.LogTrivial("[GangsOfSouthLS] Player died, ending callout.");
                    End();
                }
                if (isComputerPlusRunning)
                {
                    ComputerPlusWrapperClass.AddUpdateToCallout(callID, "Starting raid. Officer is approaching from the " + PlayerDirection.ToLower() + ".");
                }
                PlayerEndPointBlip = new Blip(PlayerEndPosition);
                PlayerEndPointBlip.Color = System.Drawing.Color.Purple;
                GameFiber.StartNew(delegate
                {
                    copCar1Blip = new Blip(Scenario.CopCar1);
                    copCar1Blip.Color = System.Drawing.Color.FromArgb(93, 182, 229);
                    Scenario.CopCar1.Driver.Tasks.DriveToPosition(Scenario.CopCarDict[Scenario.CopCar1].EndPoint.Position, 10f, VehicleDrivingFlags.Emergency, 5f).WaitForCompletion(20000);
                    copCar1Blip.Delete();
                    copCar1arrived = true;
                    return;
                });
                GameFiber.StartNew(delegate
                {
                    copCar2Blip = new Blip(Scenario.CopCar2);
                    copCar2Blip.Color = System.Drawing.Color.FromArgb(93, 182, 229);
                    Scenario.CopCar2.Driver.Tasks.DriveToPosition(Scenario.CopCarDict[Scenario.CopCar2].EndPoint.Position, 10f, VehicleDrivingFlags.Emergency, 5f).WaitForCompletion(20000);
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
                        Game.LogTrivial("[GangsOfSouthLS] Gave Player and Cops 20 seconds to arrive, starting logic anyway.");
                    }

                    return;
                });
                DrugDealState = EDrugDealState.Waiting;
            }

            if (DrugDealState == EDrugDealState.Waiting)
            {
                if (Game.LocalPlayer.Character.IsDead)
                {
                    Game.LogTrivial("[GangsOfSouthLS] Player died, ending callout.");
                    End();
                }
                Game.DisplayHelp("Converge with the ~b~other units ~w~at the ~y~suspects' location~w~.");
                if (IsWaitTimeOver || IsPlayerTooCloseToSuspects() || HaveCopsArrived())
                {
                    foreach (var veh in Scenario.CopCarDict.Keys)
                    {
                        veh.IsSirenOn = true;
                    }
                    Game.LogTrivial("[GangsOfSouthLS] Engaging Suspects");
                    DrugDealState = EDrugDealState.Arrived;
                }
            }

            if (DrugDealState == EDrugDealState.Arrived)
            {
                if (Game.LocalPlayer.Character.IsDead)
                {
                    Game.LogTrivial("[GangsOfSouthLS] Player died, ending callout.");
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
                CreateSuspectBlip(Scenario.Buyer1, Buyer1Blip);
                CreateSuspectBlip(Scenario.Buyer2, Buyer2Blip);
                CreateSuspectBlip(Scenario.Dealer1, Dealer1Blip);
                CreateSuspectBlip(Scenario.Dealer2, Dealer2Blip);
                if (Scenario.Dealer3WasSpawned)
                {
                    CreateSuspectBlip(Scenario.Dealer3, Dealer3Blip);
                }

                Pursuit = Functions.CreatePursuit();
                Game.SetRelationshipBetweenRelationshipGroups("DRUGDEAL_DEALER", "COP", Relationship.Hate);
                Game.SetRelationshipBetweenRelationshipGroups("DRUGDEAL_BUYER", "COP", Relationship.Hate);
                Game.SetRelationshipBetweenRelationshipGroups("DRUGDEAL_DEALER", "PLAYER", Relationship.Hate);
                Game.SetRelationshipBetweenRelationshipGroups("DRUGDEAL_BUYER", "PLAYER", Relationship.Hate);
                Game.SetRelationshipBetweenRelationshipGroups("COP", "DRUGDEAL_BUYER", Relationship.Hate);
                Game.SetRelationshipBetweenRelationshipGroups("COP", "DRUGDEAL_DEALER", Relationship.Hate);
                if (UsefulFunctions.Decide(15))
                {
                    Game.SetRelationshipBetweenRelationshipGroups("DRUGDEAL_DEALER", "DRUGDEAL_BUYER", Relationship.Hate);
                    Game.SetRelationshipBetweenRelationshipGroups("DRUGDEAL_BUYER", "DRUGDEAL_DEALER", Relationship.Hate);
                    DealersAndBuyersHateEachOther = true;
                    Game.LogTrivial("[GangsOfSouthLS] Decided to make Dealers and Buyers hate each other.");
                }
                else
                {
                    Game.LogTrivial("[GangsOfSouthLS] Decided NOT to make Dealers and Buyers hate each other.");
                }
                MakeDealer2DoStuff();
                MakeDealer1DoStuff();
                MakeBuyer2DoStuff();
                MakeBuyer1DoStuff();
                if (Scenario.Dealer3WasSpawned)
                {
                    MakeDealer3DoStuff();
                }
                if (!(Functions.GetPursuitPeds(Pursuit) == null) && !(Functions.GetPursuitPeds(Pursuit).Length == 0))
                {
                    Game.LogTrivial("[GangsOfSouthLS] Adding cops to pursuit.");
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
                    Game.LogTrivial("[GangsOfSouthLS] Making cops fight.");
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
                    Game.LogTrivial("[GangsOfSouthLS] Player died, ending callout.");
                    End();
                }
                if (firstloop)
                {
                    firstloop = false;
                    GameFiber.StartNew(delegate
                    {
                        Game.LogTrivial("[GangsOfSouthLS] Waiting 10 s to allow the callout logic to play out.");
                        GameFiber.Wait(10000);
                        Game.LogTrivial("[GangsOfSouthLS] Waited 10 s, checking if player left scene.");
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
                    Game.LogTrivial("[GangsOfSouthLS] Player died, ending callout.");
                    End();
                }
                if (HasPlayerLeftScene())
                {
                    Game.LogTrivial("[GangsOfSouthLS] Player left scene, allowing callout to be ended");
                    DrugDealState = EDrugDealState.CanBeEnded;
                }
            }

            if (DrugDealState == EDrugDealState.CanBeEnded)
            {
                CleanUp();
                if (Game.LocalPlayer.Character.IsDead)
                {
                    Game.LogTrivial("[GangsOfSouthLS] Player died, ending callout.");
                    End();
                }
                if (!HasPlayerLeftScene())
                {
                    Game.LogTrivial("[GangsOfSouthLS] Player reentered the scene, NOT allowing callout to be ended");
                    DrugDealState = EDrugDealState.WaitingToLeaveScene;
                }
                if (HasAnyPedBeenAddedToPursuit && !Functions.IsPursuitStillRunning(Pursuit) && (FighterList.Count == 0))
                {
                    endingregularly = true;
                    Game.LogTrivial("[GangsOfSouthLS] Pursuit is not running any more and nobody's still fighting, ending callout.");
                    End();
                }
                else if (HasAnyPedBeenAddedToPursuit && (Functions.GetActivePursuit() == null) && (FighterList.Count == 0))
                {
                    endingregularly = true;
                    Game.LogTrivial("[GangsOfSouthLS] Player left pursuit and nobody's still fighting, ending callout.");
                    End();
                }
                else if (!HasAnyPedBeenAddedToPursuit && !IsAnySuspectStillAliveAndNotArrested())
                {
                    endingregularly = true;
                    Game.LogTrivial("[GangsOfSouthLS] Every suspect is either dead or arrested, ending callout.");
                    End();
                }
            }
        }

        public override void End()
        {
            IsCurrentlyRunning = false;
            SuspectsAreaBlip.SafelyDelete();
            PlayerStartPointBlip.SafelyDelete();
            PlayerEndPointBlip.SafelyDelete();
            copCar1Blip.SafelyDelete();
            copCar2Blip.SafelyDelete();

            if (!(PedBlipDict == null))
            {
                foreach (var blip in PedBlipDict.Values)
                {
                    blip.SafelyDelete();
                }
            }

            if (!(Scenario == null))
            {
                if (!(Scenario.BadBoyCarList == null))
                {
                    foreach (var veh in Scenario.BadBoyCarList)
                    {
                        veh.SafelyDismiss();
                    }
                }
                if (!(Scenario.CopCarDict == null))
                {
                    foreach (var veh in Scenario.CopCarDict.Keys)
                    {
                        veh.SafelyDismiss();
                    }
                }
            }

            if (!endingregularly)
            {
                if (isComputerPlusRunning)
                {
                    ComputerPlusWrapperClass.CancelCallout(callID);
                }
                Game.LogTrivial("[GangsOfSouthLS] NOT ending callout regularly.");
                if (!(Scenario == null))
                {
                    if (!(Scenario.DealerList == null))
                    {
                        foreach (var Dealer in Scenario.DealerList)
                        {
                            Dealer.SafelyDismiss();
                        }
                    }

                    if (!(Scenario.BuyerList == null))
                    {
                        foreach (var Buyer in Scenario.BuyerList)
                        {
                            Buyer.SafelyDismiss();
                        }
                    }

                    if (!(Scenario.CopList1 == null))
                    {
                        foreach (var Cop in Scenario.CopList1)
                        {
                            Cop.SafelyDismiss();
                        }
                    }

                    if (!(Scenario.CopList2 == null))
                    {
                        foreach (var Cop in Scenario.CopList2)
                        {
                            Cop.SafelyDismiss();
                        }
                    }
                }
            }
            else
            {
                if (isComputerPlusRunning)
                {
                    ComputerPlusWrapperClass.ConcludeCallout(callID);
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
            if (UsefulFunctions.Decide(50))
            {
                Game.LogTrivial("[GangsOfSouthLS] Decided to make Dealer2 try to enter van.");
                GameFiber.StartNew(delegate
                {
                    Scenario.Dealer2.Tasks.EnterVehicle(Scenario.DealerVan, -1, 3f);
                    var counter = 0;
                    while (!Scenario.Dealer2.IsInAnyVehicle(false) && counter <= 6000)
                    {
                        counter += 500;
                        GameFiber.Wait(500);
                        Game.LogTrivial(string.Format("[GangsOfSouthLS] Waited {0} ms for Dealer2 to get into van.", counter));
                    }
                    if (Scenario.Dealer2.IsInAnyVehicle(false))
                    {
                        Game.LogTrivial("[GangsOfSouthLS] Dealer2 made it into van, adding to pursuit.");
                        AddToPursuitAndDeleteBlip(Scenario.Dealer2);
                    }
                    else
                    {
                        Game.LogTrivial("[GangsOfSouthLS] Dealer2 did NOT make it into van, making him fight.");
                        MakeFight(Scenario.Dealer2);
                    }
                    return;
                });
            }
            else
            {
                if (UsefulFunctions.Decide(80))
                {
                    Game.LogTrivial("[GangsOfSouthLS] Decided to make Dealer2 fight straight away.");
                    MakeFight(Scenario.Dealer2);
                }
                else
                {
                    Game.LogTrivial("[GangsOfSouthLS] Decided to make Dealer2 run away, adding to pursuit.");
                    AddToPursuitAndDeleteBlip(Scenario.Dealer2);
                }
            }
        }

        private void MakeDealer1DoStuff()
        {
            if (Scenario.Dealer1.IsInAnyVehicle(false))
            {
                Game.LogTrivial("[GangsOfSouthLS] Dealer1 is in car.");
                if (UsefulFunctions.Decide(70))
                {
                    Game.LogTrivial("[GangsOfSouthLS] Decided to make Dealer1 flee in car, adding to pursuit");
                    AddToPursuitAndDeleteBlip(Scenario.Dealer1);
                }
                else
                {
                    Game.LogTrivial("[GangsOfSouthLS] Decided to make Dealer1 exit car and fight");
                    MakeFight(Scenario.Dealer1);
                }
            }
            else
            {
                Game.LogTrivial("[GangsOfSouthLS] Dealer1 is NOT in car.");
                if (UsefulFunctions.Decide(60))
                {
                    Game.LogTrivial("[GangsOfSouthLS] Decided to make Dealer1 try to enter car.");
                    GameFiber.StartNew(delegate
                    {
                        Scenario.Dealer1.Tasks.EnterVehicle(Scenario.DealerCar, -1, 3f);
                        var counter = 0;
                        while (!Scenario.Dealer1.IsInAnyVehicle(false) && counter <= 6000)
                        {
                            counter += 500;
                            GameFiber.Wait(500);
                            Game.LogTrivial(string.Format("[GangsOfSouthLS] Waited {0} ms for Dealer1 to get into car.", counter));
                        }
                        if (Scenario.Dealer1.IsInAnyVehicle(false))
                        {
                            Game.LogTrivial("[GangsOfSouthLS] Dealer1 made it into car, adding to pursuit.");
                            AddToPursuitAndDeleteBlip(Scenario.Dealer1);
                        }
                        else
                        {
                            Game.LogTrivial("[GangsOfSouthLS] Dealer1 did NOT make it into car, making him fight.");
                            MakeFight(Scenario.Dealer1);
                        }
                        return;
                    });
                }
                else
                {
                    Game.LogTrivial("[GangsOfSouthLS] Decided to make Dealer1 fight straight away.");
                    MakeFight(Scenario.Dealer1);
                }
            }
        }

        private void MakeDealer3DoStuff()
        {
            Game.LogTrivial("[GangsOfSouthLS] Dealer3 always fights straight away.");
            MakeFight(Scenario.Dealer3);
        }

        private void MakeBuyer1DoStuff()
        {
            if (Buyer2IsFightingStraightAway)
            {
                if (UsefulFunctions.Decide(70))
                {
                    Game.LogTrivial("[GangsOfSouthLS] Decided to make Buyer1 join Buyer2 in combat");
                    MakeFight(Scenario.Buyer1);
                }
                else
                {
                    Game.LogTrivial("[GangsOfSouthLS] Decided to make Buyer1 try to enter car.");
                    GameFiber.StartNew(delegate
                    {
                        Scenario.Buyer1.Tasks.EnterVehicle(Scenario.BuyerCar, -1, 3f);
                        var counter = 0;
                        while (!Scenario.Buyer1.IsInAnyVehicle(false) && counter <= 6000)
                        {
                            counter += 500;
                            GameFiber.Wait(500);
                            Game.LogTrivial(string.Format("[GangsOfSouthLS] Waited {0} ms for Buyer1 to get into car.", counter));
                        }
                        if (Scenario.Buyer1.IsInAnyVehicle(false))
                        {
                            Game.LogTrivial("[GangsOfSouthLS] Buyer1 made it into car, adding to pursuit.");
                            AddToPursuitAndDeleteBlip(Scenario.Buyer1);
                        }
                        else
                        {
                            Game.LogTrivial("[GangsOfSouthLS] Buyer1 did NOT make it into car, making him fight.");
                            MakeFight(Scenario.Buyer1);
                        }
                        return;
                    });
                }
            }
            else
            {
                Game.LogTrivial("[GangsOfSouthLS] Making Buyer1 enter car and wait for Buyer2");
                GameFiber.StartNew(delegate
                {
                    Scenario.Buyer1.Tasks.EnterVehicle(Scenario.BuyerCar, 0, 3f);
                    var counter = 0;
                    while (!Scenario.Buyer1.IsInAnyVehicle(false) && counter <= 4000)
                    {
                        counter += 500;
                        GameFiber.Wait(500);
                        Game.LogTrivial(string.Format("[GangsOfSouthLS] Waited {0} ms for Buyer1 to get into car.", counter));
                    }
                    if (Scenario.Buyer1.IsInAnyVehicle(false))
                    {
                        Game.LogTrivial("[GangsOfSouthLS] Buyer1 made it into car, waiting for Buyer2.");
                        while (!Scenario.Buyer2.IsInAnyVehicle(false) && counter <= 6000)
                        {
                            counter += 500;
                            GameFiber.Wait(500);
                            Game.LogTrivial(string.Format("[GangsOfSouthLS] Buyer1 Waited {0} ms for Buyer2 to get in car.", counter));
                        }
                        if (Scenario.Buyer2.IsInAnyVehicle(false))
                        {
                            Game.LogTrivial("[GangsOfSouthLS] Both Buyers are in car, making Buyer1 shoot out of the vehicle.");
                            MyNatives.MakePedAbleToShootOutOfCar(Scenario.Buyer1);
                            MakeFight(Scenario.Buyer1);
                            GameFiber.StartNew(delegate
                            {
                                while (Scenario.Buyer1.IsInAnyVehicle(false) && !Scenario.Buyer1.CurrentVehicle.IsSeatFree(-1) && !Scenario.Buyer1.CurrentVehicle.Driver.IsDead)
                                {
                                    GameFiber.Yield();
                                }
                                if (Scenario.Buyer1.Exists() && Scenario.Buyer1.IsAlive)
                                {
                                    Game.LogTrivial("[GangsOfSouthLS] Buyer1's driver died or disappeared.");
                                    GameFiber.Wait(200); //Allowing time for driver to despawn (if "suspect escaped")
                                    if (!Scenario.Buyer2.Exists())
                                    {
                                        Game.LogTrivial("[GangsOfSouthLS] Buyer 2 doesn't exist any more (suspect escaped). Despawning Buyer1, too.");
                                        Buyer1Blip.SafelyDelete();
                                        Scenario.Buyer1.SafelyDelete();
                                    }
                                    if (UsefulFunctions.Decide(30))
                                    {
                                        Game.LogTrivial("[GangsOfSouthLS] Decided to make Buyer1 flee.");
                                        if (HasAnyPedBeenAddedToPursuit && Functions.IsPursuitStillRunning(Pursuit))
                                        {
                                            Game.LogTrivial("[GangsOfSouthLS] Pursuit is still active, adding Buyer1.");
                                            AddToPursuitAndDeleteBlip(Scenario.Buyer1);
                                        }
                                        else
                                        {
                                            Game.LogTrivial("[GangsOfSouthLS] Pursuit is not active anymore, making Buyer1 fight instead");
                                        }
                                    }
                                    else
                                    {
                                        Game.LogTrivial("[GangsOfSouthLS] Decided to make Buyer1 fight.");
                                    }
                                }
                                return;
                            });
                        }
                        else
                        {
                            Game.LogTrivial("[GangsOfSouthLS] Buyer1 is alone in car.");
                            if (Functions.IsPursuitStillRunning(Pursuit))
                            {
                                Game.LogTrivial("[GangsOfSouthLS] Pursuit is active, making Buyer1 shuffle to drivers seat and adding him to pursuit.");
                                Scenario.Buyer1.Tasks.ShuffleToAdjacentSeat().WaitForCompletion(3000);
                                AddToPursuitAndDeleteBlip(Scenario.Buyer1);
                            }
                            else
                            {
                                Game.LogTrivial("[GangsOfSouthLS] Pursuit isn't active, making Buyer1 fight.");
                                MakeFight(Scenario.Buyer1);
                            }
                        }
                    }
                    else
                    {
                        Game.LogTrivial("[GangsOfSouthLS] Buyer1 did NOT make it into car, making him fight.");
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
                Game.LogTrivial("[GangsOfSouthLS] Dealers and Buyers hate each other, so made Buyer2 fight straight away.");
                MakeFight(Scenario.Buyer2);
                Buyer2IsFightingStraightAway = true;
            }
            else
            {
                if (UsefulFunctions.Decide(70))
                {
                    Game.LogTrivial("[GangsOfSouthLS] Decided to make Buyer2 try to enter car.");
                    GameFiber.StartNew(delegate
                    {
                        Scenario.Buyer2.Tasks.EnterVehicle(Scenario.BuyerCar, -1, 3f);
                        var counter = 0;
                        while (!Scenario.Buyer2.IsInAnyVehicle(false) && counter <= 4000)
                        {
                            counter += 500;
                            GameFiber.Wait(500);
                            Game.LogTrivial(string.Format("[GangsOfSouthLS] Waited {0} ms for Buyer2 to get into car.", counter));
                        }
                        if (Scenario.Buyer2.IsInAnyVehicle(false))
                        {
                            Game.LogTrivial("[GangsOfSouthLS] Buyer2 made it to car, adding to pursuit.");
                            AddToPursuitAndDeleteBlip(Scenario.Buyer2);
                        }
                        else
                        {
                            Game.LogTrivial("[GangsOfSouthLS] Buyer2 did NOT make it into van, making him fight.");
                            MakeFight(Scenario.Buyer2);
                        }
                        return;
                    });
                }
                else
                {
                    Game.LogTrivial("[GangsOfSouthLS] Decided not to make Buyer2 try to enter the car and fight straight away instead.");
                    MakeFight(Scenario.Buyer2);
                    Buyer2IsFightingStraightAway = true;
                }
            }
        }

        private void CleanUp()
        {
            var newPedBlipDict = new Dictionary<Ped, Blip> { };
            foreach (var pedblip in PedBlipDict)
            {
                if (pedblip.Key.Exists() && !pedblip.Key.IsDead)
                {
                    newPedBlipDict.Add(pedblip.Key, pedblip.Value);
                }
                else
                {
                    Game.LogTrivial("[GangsOfSouthLS] Suspect was killed, dismissing ped and deleting blip.");
                    pedblip.Value.SafelyDelete();
                    pedblip.Key.SafelyDismiss();
                }
            }
            PedBlipDict = newPedBlipDict;

            var newFighterList = new List<MyPed> { };
            foreach (var fighter in FighterList)
            {
                if (fighter.Exists() && !fighter.IsDead && !Functions.IsPedArrested(fighter))
                {
                    newFighterList.Add(fighter);
                }
            }
            FighterList = newFighterList;

            var newSuspectList = new List<MyPed> { };
            foreach (var suspect in SuspectList)
            {
                if (!suspect.Exists())
                {
                    continue;
                }
                else if (suspect.IsDead)
                {
                    if (isComputerPlusRunning)
                    {
                        ComputerPlusWrapperClass.AddPedToCallout(callID, suspect);
                        ComputerPlusWrapperClass.AddUpdateToCallout(callID, string.Format("Suspect {0} was killed.", Functions.GetPersonaForPed(suspect).FullName));
                    }
                }
                else if (Functions.IsPedArrested(suspect))
                {
                    if (isComputerPlusRunning)
                    {
                        ComputerPlusWrapperClass.AddPedToCallout(callID, suspect);
                        ComputerPlusWrapperClass.AddUpdateToCallout(callID, string.Format("Suspect {0} was arrested.", Functions.GetPersonaForPed(suspect).FullName));
                    }
                    suspect.CreateCourtCase(isLSPDFRPlusRunning);
                }
                else
                {
                    newSuspectList.Add(suspect);
                }
            }
            SuspectList = newSuspectList;
        }

        private void MakeFight(MyPed ped)
        {
            ped.Tasks.FightAgainstClosestHatedTarget(100f);
            ped.AddCrimeToList("Aggravated Assault", 36);
            ped.AddCrimeToList("Resisting Arrest", 12);
            FighterList.Add(ped);
        }

        private void AddToPursuitAndDeleteBlip(MyPed ped)
        {
            if (PedBlipDict.ContainsKey(ped))
            {
                PedBlipDict[ped].SafelyDelete();
            }
            Functions.AddPedToPursuit(Pursuit, ped);
            ped.AddCrimeToList("Resisting Arrest", 24);
            if (!playerAddedToPursuit)
            {
                playerAddedToPursuit = true;
                if (isComputerPlusRunning)
                {
                    ComputerPlusWrapperClass.AddUpdateToCallout(callID, "Suspects are trying to flee, officer in pursuit");
                }
                Game.LogTrivial("[GangsOfSouthLS] Setting Pursuit as active for player");
                Functions.SetPursuitIsActiveForPlayer(Pursuit, true);
                if (INIReader.RequestAirSupport)
                {
                    Game.LogTrivial("[GangsOfSouthLS] Requesting air support unit.");
                    Functions.RequestBackup(Game.LocalPlayer.Character.Position, LSPD_First_Response.EBackupResponseType.Pursuit, LSPD_First_Response.EBackupUnitType.AirUnit);
                }
            }
            HasAnyPedBeenAddedToPursuit = true;
        }

        private bool IsPlayerTooCloseToSuspects()
        {
            foreach (var waypoint in Scenario.CopCarWayPointList)
            {
                if (Game.LocalPlayer.Character.Position.DistanceTo(waypoint.EndPoint.Position) < 5f)
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
            foreach (var waypoint in Scenario.CopCarWayPointList)
            {
                if (Scenario.CopCar1.DistanceTo(waypoint.EndPoint.Position) < 5f)
                {
                    return true;
                }
                if (Scenario.CopCar2.DistanceTo(waypoint.EndPoint.Position) < 5f)
                {
                    return true;
                }
            }
            return false;
        }

        private void CreateSuspectBlip(Ped ped, Blip blip)
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