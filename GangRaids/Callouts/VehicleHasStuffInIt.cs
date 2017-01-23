using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;
using PoliceSearch.API;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using LSPD_First_Response.Engine.Scripting.Entities;

namespace GangRaids.Callouts
{
    [CalloutInfo("Vehicle has stuff in it!!1!1", CalloutProbability.VeryHigh)]
    class VehicleHasStuffInIt : Callout
    {
        private Vehicle suspectVehicle;
        private Vector3 spawnPoint;
        private string[] stuffToFind;
        private Blip vehBlip;

        public override bool OnBeforeCalloutDisplayed()
        {
            spawnPoint = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(250f));
            ShowCalloutAreaBlipBeforeAccepting(spawnPoint, 20f);
            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            suspectVehicle = new Vehicle("stanier", spawnPoint);
            suspectVehicle.IsPersistent = true;
            vehBlip = new Blip(suspectVehicle);
            vehBlip.Color = System.Drawing.Color.Yellow;
            stuffToFind = new string[] { "Stuff", "Even more stuff" };
            VehicleSearch.AddItems(suspectVehicle, 1, -1, stuffToFind);

            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();
            if(VehicleSearch.Searched(suspectVehicle, 1))
            {
                Game.DisplayNotification("You have successfully found ~r~STUFF");
                End();
            }
        }

        public override void End()
        {
            base.End();
            if (suspectVehicle.Exists()) { suspectVehicle.Dismiss(); }

        }
    }
}

