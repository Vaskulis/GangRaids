using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using LSPD_First_Response.Engine.Scripting.Entities;

namespace GangRaids.Callouts
{
    [CalloutInfo("Simple pursuit", CalloutProbability.VeryHigh)]
    class PursuitCalllout : Callout
    {
        private Vector3 spawnPoint;
        private Ped suspect;
        private Ped cop;
        private EPursuitCalloutEnum CalloutState;
        private LHandle pursuit;

        public override bool OnBeforeCalloutDisplayed()
        {
            spawnPoint = Game.LocalPlayer.Character.GetOffsetPositionFront(3f);
            ShowCalloutAreaBlipBeforeAccepting(spawnPoint, 20f);
            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            suspect = new Ped(Game.LocalPlayer.Character.GetOffsetPositionFront(3f));
            suspect.IsPersistent = true;
            suspect.BlockPermanentEvents = true;
            cop = new Ped("s_f_y_cop_01", Game.LocalPlayer.Character.GetOffsetPositionFront(2f), 0f);
            cop.BlockPermanentEvents = true;
            CalloutState = EPursuitCalloutEnum.Accepted;

            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();
            if (CalloutState == EPursuitCalloutEnum.Accepted)
            {
                pursuit = Functions.CreatePursuit();
                Functions.AddPedToPursuit(pursuit, suspect);
                Functions.AddCopToPursuit(pursuit, cop);
                Functions.SetPursuitIsActiveForPlayer(pursuit, true);
                CalloutState = EPursuitCalloutEnum.InPursuit;
            }
            if (CalloutState == EPursuitCalloutEnum.InPursuit)
            {
                if (!Functions.IsPursuitStillRunning(pursuit))
                {
                    End();
                }
            }

        }

        public override void End()
        {
            suspect.Delete();
            cop.Delete();

            base.End();
        }
        public enum EPursuitCalloutEnum
        {
            Accepted, InPursuit
        }
    }
}
