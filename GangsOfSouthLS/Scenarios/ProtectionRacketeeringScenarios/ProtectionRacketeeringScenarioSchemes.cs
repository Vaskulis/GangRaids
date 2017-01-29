using GangsOfSouthLS.HelperClasses.ProtectionRacketeeringHelpers;
using GangsOfSouthLS.HelperClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GangsOfSouthLS.HelperClasses.CommonUtilities;

namespace GangsOfSouthLS.Scenarios.ProtectionRacketeeringScenarios
{
    static class ProtectionRacketeeringScenarioSchemes
    {
        internal static ProtectionRacketeeringScenarioScheme Scenario1 = new ProtectionRacketeeringScenarioScheme
        (
            "Strawberry 247 Supermarket",
            new Pos4(26.61937f, -1356.843f, 28.84396f, 90.90969f),
            new Pos4(25.02721f, -1342.898f, 29.49704f, 221.721f),
            new Pos4(27.09624f, -1345.195f, 29.49704f, 40.14472f),
            new Pos4(101.8241f, -1166.002f, 30.2289f, 95.3634f),
            new List<string> { "s_f_m_sweatshop_01", "mp_m_shopkeep_01" }
        );

        internal static List<ProtectionRacketeeringScenarioScheme> ScenarioSchemeList = new List<ProtectionRacketeeringScenarioScheme>
        {
            Scenario1
        };
    }
}
