using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;
using LSPD_First_Response.Mod.Callouts;
using LSPD_First_Response.Mod.API;
using GangsOfSouthLS.HelperClasses.DriveByShootingHelpers;
using System.Windows.Forms;
using GangsOfSouthLS.HelperClasses.CommonUtilities;
using System.Drawing;
using GangsOfSouthLS.Menus;
using GangsOfSouthLS.INIFile;
using GangsOfSouthLS.ScenarioCollections.DriveByShootingScenarios;

namespace GangsOfSouthLS.Callouts
{
    [CalloutInfo("Drive-by shooting", CalloutProbability.High)]
    class DriveByShooting : Callout
    {
        internal static CrimeSceneScenarioTemplate ScenarioTemplate;
        internal static CrimeSceneScenario Scenario;
        internal static EDriveByState DriveByState;
        internal SuspectCarTemplate CarTemplate;
        internal static Statements Statements;

        internal static Blip CrimeSceneBlip;

        internal static CorrectOwnerHouseScenario COHS;

        internal static bool IsCurrentlyRunning = false;
        internal static Statements.EStatementsQuality StatementsQuality;


        public override bool OnBeforeCalloutDisplayed()
        {
            var scenarioFound = CrimeSceneScenarioTemplate.ChooseScenario(out ScenarioTemplate);
            if (!scenarioFound)
            {
                Game.LogTrivial("[GangsOfSouthLS] Could not find scenario in range.");
                return false;
            }
            Scenario = new CrimeSceneScenario(ScenarioTemplate);
            CalloutMessage = "Drive-by shooting";
            CalloutPosition = Scenario.Position;
            ShowCalloutAreaBlipBeforeAccepting(CalloutPosition, 70f);
            return base.OnBeforeCalloutDisplayed();
        }


        public override bool OnCalloutAccepted()
        {
            Scenario.Initialize();
            CarTemplate = new SuspectCarTemplate();
            CrimeSceneBlip = new Blip(CalloutPosition, 50f);
            CrimeSceneBlip.Alpha = 0.5f;
            CrimeSceneBlip.Color = Color.Yellow;
            CrimeSceneBlip.EnableRoute(Color.Yellow);
            DriveByState = EDriveByState.Accepted;
            IsCurrentlyRunning = true;
            return base.OnCalloutAccepted();
        }


        public override void Process()
        {
            base.Process();

            switch (DriveByState)
            {
                case EDriveByState.Accepted:
                    if (Game.LocalPlayer.Character.DistanceTo(Scenario.Position) < 100f)
                    {
                        CrimeSceneBlip.SafelyDelete();
                        Game.LogTrivial("[GangsOfSouthLS] Arrived on scene.");
                        DriveByState = EDriveByState.ArrivedOnScene;
                    }
                    break;

                case EDriveByState.ArrivedOnScene:
                    Statements = new Statements(Scenario.WitnessList, CarTemplate);
                    DriveByMenu.Initialize();
                    DriveByState = EDriveByState.TakingStatements;
                    break;

                case EDriveByState.TakingStatements:
                    if (Statements.IsFinished)
                    {
                        StatementsQuality = Statements.Quality;
                        Game.LogTrivial("[GangsOfSouthLS] Finished taking statements. Quality: " + StatementsQuality.ToString());
                        DriveByMenu.AddActionToMenu("Report status to dispatch", InformDispatchAction);
                        DriveByState = EDriveByState.FinishedTakingStatements;
                    }
                    break;

                case EDriveByState.FinishedTakingStatements:
                    Game.DisplayHelp("Open your menu with ~b~" + INIReader.MenuKeyString + "~w~ and use the Actions tab to report your findings to dispatch");
                    break;

                case EDriveByState.Waiting:
                    GameFiber.Yield();
                    break;

                case EDriveByState.ReadyToEnd:
                    Game.DisplayHelp("Press End to end.");
                    if (Game.IsKeyDown(Keys.End))
                    {
                        End();
                    }
                    break;

                case EDriveByState.Evaluating:
                    DriveByState = EDriveByState.Waiting;

                    var OHSTList = new List<OwnerHouseScenarioTemplate>(OwnerHouseScenarioTemplateCollection.OwnerHouseScenarioTemplateList);

                    GameFiber.StartNew(delegate
                    {
                        switch (StatementsQuality)
                        {
                            case Statements.EStatementsQuality.Definite:
                                var COHST = OHSTList.RandomElement();

                                var carStolen = false;
                                //if (UsefulFunctions.Decide(20))
                                //{
                                //    carStolen = true;
                                //}
                                if (carStolen)
                                {
                                    Game.DisplaySubtitle("~g~Dispatch~w~: The car was reported stolen by its owner a few days ago.", 4500);
                                    GameFiber.WaitUntil(() => Game.IsKeyDown(Keys.Y), 4700);
                                    Game.DisplaySubtitle("~g~Dispatch~w~: I'm putting out an APB on it in your area.", 4500);
                                    GameFiber.WaitUntil(() => Game.IsKeyDown(Keys.Y), 4700);
                                }
                                else
                                {
                                    COHS = new CorrectOwnerHouseScenario(COHST, CarTemplate, Scenario);
                                    var Owner = COHS.Owner;

                                    Game.DisplaySubtitle("~g~Dispatch~w~: Alright, I've found it.", 4500);
                                    GameFiber.WaitUntil(() => Game.IsKeyDown(Keys.Y), 4700);
                                    Game.DisplaySubtitle("~g~Dispatch~w~: The owner is " + Owner.Name + ", a " + Owner.Age + " year old " + Owner.Gender.ToLower() + ",", 4500);
                                    GameFiber.WaitUntil(() => Game.IsKeyDown(Keys.Y), 4700);
                                    Game.DisplaySubtitle("~g~Dispatch~w~: living at " + COHS.Address + ".", 4500);
                                    GameFiber.WaitUntil(() => Game.IsKeyDown(Keys.Y), 4700);
                                    Game.DisplaySubtitle("~g~Dispatch~w~: Be advised, suspect is a member of the " + Scenario.SuspectGang.ToString() + " and possibly armed.", 4500);
                                    GameFiber.WaitUntil(() => Game.IsKeyDown(Keys.Y), 4700);
                                    Game.DisplaySubtitle("~b~You~w~: 10-4. I'm going to check out the residence.", 4500);
                                    GameFiber.WaitUntil(() => Game.IsKeyDown(Keys.Y), 4700);

                                    Statements.CompleteVehicleInformation();
                                    DriveByMenu.InformationDict.Add("Owner", Owner.Name);

                                    DriveByState = EDriveByState.CorrectResidenceAction;
                                }
                                break;

                            case Statements.EStatementsQuality.Ambiguous:
                                Game.DisplaySubtitle("");

                                DriveByState = EDriveByState.IncorrectResidenceAction;
                                break;
                        }
                    });
                    break;

                case EDriveByState.CorrectResidenceAction:
                    COHS.PlayAction();
                    if (COHS.State == CorrectOwnerHouseScenario.EHouseState.Ending)
                    {
                        DriveByState = EDriveByState.ReadyToEnd;
                    }
                    break;

                case EDriveByState.IncorrectResidenceAction:

                    break;
            }
        }


        public override void End()
        {
            IsCurrentlyRunning = false;
            DriveByMenu.Terminate();
            CrimeSceneBlip.SafelyDelete();
            if (Scenario != null)
            {
                if (Scenario.VictimList != null)
                {
                    foreach (var vic in Scenario.VictimList)
                    {
                        vic.SafelyDelete();
                    }
                }
                if (Scenario.WitnessList != null)
                {
                    foreach (var wit in Scenario.WitnessList)
                    {
                        wit.SafelyDelete();
                    }
                }
                if (Scenario.MedicList != null)
                {
                    foreach (var med in Scenario.MedicList)
                    {
                        med.SafelyDelete();
                    }
                }
                Scenario.Ambulance.SafelyDelete();
            }
            if (COHS != null)
            {
                COHS.End();
            }
            base.End();
        }


        internal enum EDriveByState
        {
            Accepted,
            ArrivedOnScene,
            TakingStatements,
            FinishedTakingStatements,
            Waiting,
            Evaluating,
            CorrectResidenceAction,
            IncorrectResidenceAction,
            ReadyToEnd
        }


        internal static void InformDispatchAction(object sender, EventArgs e)
        {
            DriveByMenu.RemoveActionFromMenu(InformDispatchAction);

            DriveByState = EDriveByState.Waiting;
            PlayRadioAnimationWhileState(EDriveByState.Waiting);

            GameFiber.StartNew(delegate
            {
                Game.DisplaySubtitle("~b~You~w~: Unit " + INIReader.UnitNameReadable + " to dispatch. I'm on scene at the drive-by attack.", 4500);
                GameFiber.WaitUntil(() => Game.IsKeyDown(Keys.Y), 4700);

                switch (StatementsQuality)
                {
                    case Statements.EStatementsQuality.Definite:
                        Game.DisplaySubtitle("~b~You~w~: A witness saw the vehicle's license plate. It's " + Statements.CarTemplate.LicensePlate + ".", 4500);
                        GameFiber.WaitUntil(() => Game.IsKeyDown(Keys.Y), 4700);
                        Game.DisplaySubtitle("~b~You~w~: Could you please run it for me?", 4500);
                        GameFiber.WaitUntil(() => Game.IsKeyDown(Keys.Y), 4700);
                        Game.DisplaySubtitle("~g~Dispatch~w~: Checking it now.", 4500);
                        GameFiber.WaitUntil(() => Game.IsKeyDown(Keys.Y), 4700);
                        DriveByState = EDriveByState.Evaluating;
                        break;

                    case Statements.EStatementsQuality.Useless:
                        Game.DisplaySubtitle("~b~You~w~: Witnesses here aren't very helpful, I'm afraid.", 4500);
                        GameFiber.WaitUntil(() => Game.IsKeyDown(Keys.Y), 4700);
                        Game.DisplaySubtitle("~b~You~w~: Some of them saw a " + Statements.GetDescription() + ", but that's about all.", 4500);
                        GameFiber.WaitUntil(() => Game.IsKeyDown(Keys.Y), 4700);
                        Game.DisplaySubtitle("~b~You~w~: Also, at least one of the victims appears to be a member of the " + Scenario.VictimGang.ToString() + ".", 4500);
                        GameFiber.WaitUntil(() => Game.IsKeyDown(Keys.Y), 4700);
                        Game.DisplaySubtitle("~g~Dispatch~w~: Thank you, officer. Please clear the scene and continue your patrol. We will be expecting a full report.", 4500);
                        GameFiber.WaitUntil(() => Game.IsKeyDown(Keys.Y), 4700);
                        DriveByState = EDriveByState.ReadyToEnd;
                        break;

                    case Statements.EStatementsQuality.Ambiguous:
                        Game.DisplaySubtitle("~b~You~w~: Witnesses saw a " + Statements.GetDescription() + ". No plates, unfortunately.", 4500);
                        GameFiber.WaitUntil(() => Game.IsKeyDown(Keys.Y), 4700);
                        Game.DisplaySubtitle("~b~You~w~: Also, at least one of the victims appears to be a member of the " + Scenario.VictimGang.ToString() + ".", 4500);
                        GameFiber.WaitUntil(() => Game.IsKeyDown(Keys.Y), 4700);
                        Game.DisplaySubtitle("~g~Dispatch~w~: Thank you officer. I'm checking for possible suspects, hang on.", 4500);
                        GameFiber.WaitUntil(() => Game.IsKeyDown(Keys.Y), 4700);
                        DriveByState = EDriveByState.ReadyToEnd;
                        break;
                }
            });
        }


        internal static void EndCalloutAction(object sender, EventArgs e)
        {
            DriveByState = EDriveByState.ReadyToEnd;
        }


        internal static void PlayRadioAnimationWhileState(EDriveByState state)
        {
            GameFiber.StartNew(delegate
            {
                Game.LocalPlayer.Character.Tasks.PlayAnimation("random@arrests", "generic_radio_enter", 6f, AnimationFlags.UpperBodyOnly | AnimationFlags.SecondaryTask).WaitForCompletion(2000);
                Game.LocalPlayer.Character.Tasks.PlayAnimation("random@arrests", "generic_radio_idle", 6f, AnimationFlags.UpperBodyOnly | AnimationFlags.SecondaryTask | AnimationFlags.Loop);
                while (DriveByState == state)
                {
                    GameFiber.Yield();
                }
                Game.LocalPlayer.Character.Tasks.PlayAnimation("random@arrests", "generic_radio_exit", 6f, AnimationFlags.UpperBodyOnly | AnimationFlags.SecondaryTask).WaitForCompletion(2000);
            });
        }
    }
}
