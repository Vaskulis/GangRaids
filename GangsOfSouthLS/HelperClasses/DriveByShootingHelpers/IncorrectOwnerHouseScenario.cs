using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GangsOfSouthLS.HelperClasses.DriveByShootingHelpers
{
    class IncorrectOwnerHouseScenario : HouseScenario
    {
        internal IncorrectOwnerHouseScenario(OwnerHouseScenarioTemplate scenarioTemplate, CrimeSceneScenario CSScenario) : base(scenarioTemplate, CSScenario)
        {

        }

        internal override void SetEnRoute()
        {
            base.SetEnRoute();
        }

        internal override void PlayAction()
        {

        }

        internal override void End()
        {
            base.End();
        }
    }
}
