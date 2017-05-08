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

namespace GangsOfSouthLS.Callouts
{
    [CalloutInfo("Drive-by shooting", CalloutProbability.High)]
    class DriveByShooting : Callout
    {
        internal static CrimeSceneScenarioTemplate ScenarioTemplate;
        internal static Scenario Scenario;
        internal static EDriveByState DriveByState;
        internal SuspectCarTemplate CarTemplate;
        internal static Questioning Questioning;

        internal static bool IsCurrentlyRunning = false;

        public override bool OnBeforeCalloutDisplayed()
        {
            var scenarioFound = CrimeSceneScenarioTemplate.ChooseScenario(out ScenarioTemplate);
            if (!scenarioFound)
            {
                Game.LogTrivial("[GangsOfSouthLS] Could not find scenario in range.");
                return false;
            }
            Scenario = new Scenario(ScenarioTemplate);
            CalloutMessage = "Drive-by shooting";
            CalloutPosition = Scenario.Position;
            ShowCalloutAreaBlipBeforeAccepting(CalloutPosition, 70f);
            return base.OnBeforeCalloutDisplayed();
        }


        public override bool OnCalloutAccepted()
        {
            Scenario.Initialize();
            CarTemplate = new SuspectCarTemplate();
            DriveByState = EDriveByState.Accepted;
            IsCurrentlyRunning = true;
            return base.OnCalloutAccepted();
        }


        public override void Process()
        {
            base.Process();

            if (DriveByState == EDriveByState.Accepted)
            {
                if (Game.LocalPlayer.Character.DistanceTo(Scenario.Position) < 100f)
                {
                    DriveByState = EDriveByState.ArrivedOnScene;
                }
            }

            if (DriveByState == EDriveByState.ArrivedOnScene)
            {
                Questioning = new Questioning(Scenario.WitnessList, CarTemplate);
                DriveByMenu.Initialize();
                DriveByState = EDriveByState.InQuestioning;
            }

            if (DriveByState == EDriveByState.InQuestioning)
            {
                if (Questioning.IsFinished)
                {
                    DriveByMenu.AddActionToMenu("Get further information from dispatch", InformDispatchAction);
                    DriveByState = EDriveByState.FinishedQuestioning;
                }
            }

            if (DriveByState == EDriveByState.FinishedQuestioning)
            {
                GameFiber.Yield();
            }

            if (DriveByState == EDriveByState.ReadyToEnd)
            {
                End();
            }
        }


        public override void End()
        {
            IsCurrentlyRunning = false;
            DriveByMenu.Terminate();
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
            base.End();
        }

        internal enum EDriveByState
        {
            Accepted, ArrivedOnScene, InQuestioning, FinishedQuestioning, ReadyToEnd
        }

        internal static void InformDispatchAction(object sender, EventArgs e)
        {
            var radioSequence = new TaskSequence(Game.LocalPlayer.Character);
            radioSequence.Tasks.PlayAnimation("random@arrests", "generic_radio_enter", 6f, AnimationFlags.UpperBodyOnly | AnimationFlags.SecondaryTask);
            radioSequence.Tasks.PlayAnimation("random@arrests", "generic_radio_exit", 6f, AnimationFlags.UpperBodyOnly | AnimationFlags.SecondaryTask);
            GameFiber.StartNew(delegate { radioSequence.Execute(); });
            GameFiber.StartNew(delegate
            {
                var quality = Questioning.Quality;
                Game.DisplaySubtitle("~b~You~w~: Unit " + INIReader.UnitName + " to dispatch. I'm on scene at the drive-by attack.", 4500);
                GameFiber.Wait(4700);
                if (quality == Questioning.EQuestioningQuality.Definite)
                {
                    Game.DisplaySubtitle("~b~You~w~: A witness saw the vehicle's license plate. It's " + Questioning.CarTemplate.LicensePlate, 4500);
                    GameFiber.Wait(4700);
                    Game.DisplaySubtitle("~b~You~w~: Could you please run them for me?", 4500);
                    GameFiber.Wait(4700);
                }
                else if (quality == Questioning.EQuestioningQuality.Useless)
                {
                    Game.DisplaySubtitle("~b~You~w~: Witnesses here aren't very helpful, I'm afraid.", 4500);
                    GameFiber.Wait(4700);
                    Game.DisplaySubtitle("~b~You~w~: Some of them saw a " + Questioning.GetDescription() + ". But that's about all.", 4500);
                    GameFiber.Wait(4700);
                    Game.DisplaySubtitle("~b~You~w~: Also, at least one of the victims appears to be a member of " + Scenario.VictimGang.ToString() + ".", 4500);
                    GameFiber.Wait(4700);
                    Game.DisplaySubtitle("~g~Dispatch~w~: Thank you, officer. Please clear the scene and continue your patrol. We will be expecting a full report.", 4500);
                    GameFiber.Wait(4700);
                }
                else if (quality == Questioning.EQuestioningQuality.Ambiguous)
                {
                    Game.DisplaySubtitle("~b~You~w~: Witnesses saw a " + Questioning.GetDescription() +". No plates, unfortunately.", 4500);
                    GameFiber.Wait(4700);
                    Game.DisplaySubtitle("~b~You~w~: Also, at least one of the victims appears to be a member of " + Scenario.VictimGang.ToString() + ".", 4500);
                    GameFiber.Wait(4700);
                    Game.DisplaySubtitle("~g~Dispatch~w~: Thank you officer. I'm checking for possible suspects, hang on.", 4500);
                    GameFiber.Wait(4700);
                }
                DriveByMenu.AddActionToMenu("End Callout", EndCalloutAction);
            });
        }

        internal static void EndCalloutAction(object sender, EventArgs e)
        {
            DriveByState = EDriveByState.ReadyToEnd;
        }
    }
}
