using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;
using LSPD_First_Response.Mod.Callouts;
using LSPD_First_Response.Mod.API;

namespace GangRaids.Callouts
{
    [CalloutInfo("Drug Deal", CalloutProbability.High)]
    class Extortion : Callout
    {
        public override bool OnBeforeCalloutDisplayed()
        {

            return base.OnBeforeCalloutDisplayed();
        }
    }
}
