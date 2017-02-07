using GangsOfSouthLS.HelperClasses.ProtectionRacketeeringHelpers;
using GangsOfSouthLS.HelperClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;
using GangsOfSouthLS.HelperClasses.CommonUtilities;

namespace GangsOfSouthLS.Scenarios.ProtectionRacketeeringScenarios
{
    static class ProtectionRacketeeringScenarioSchemes
    {
        private static ProtectionRacketeeringScenarioScheme Scenario1 = new ProtectionRacketeeringScenarioScheme
        (
            "Strawberry 247 Supermarket",
            new Pos4(26.61937f, -1356.843f, 28.84396f, 90.90969f),
            new Pos4(42.9661f, -1359.115f, 28.87617f, 89.5684f),
            new Pos4(25.02721f, -1342.898f, 29.49704f, 221.721f),
            new Pos4(27.09624f, -1345.195f, 29.49704f, 40.14472f),
            new List<Pos4>
            {
                new Pos4(-43.08104f, -1251.596f, 35.45556f, 272.0149f),
            },
            new List<string> { "s_f_m_sweatshop_01", "mp_m_shopkeep_01" },
            new Vector3(29.0f, -1349.0f, 30.0f),
            new List<string> { "v_ilev_247door_r", "v_ilev_247door" }
        );

        private static ProtectionRacketeeringScenarioScheme Scenario2 = new ProtectionRacketeeringScenarioScheme
        (
            "Davis Herr Kutz Barber",
            new Pos4(129.3942f, -1716.341f, 29.00904f, 50.45683f),
            new Pos4(146.8948f, -1735.57f, 28.73005f, 50.13921f),
            new Pos4(137.8611f, -1706.771f, 29.29162f, 132.6466f),
            new Pos4(136.5526f, -1707.918f, 29.29162f, 311.1761f),
            new List<Pos4>
            {
                new Pos4(238.6752f, -1709.307f, 28.93003f, 140.1303f),
                new Pos4(122.0899f, -1838.084f, 25.91018f, 319.421f),
                new Pos4(251.5134f, -1821.831f, 26.6742f, 52.72974f)
            },
            new List<string> { "s_f_m_fembarber" },
            new Vector3(133.2188f, -1711.565f, 29.29166f),
            new List<string> { "V_ILEV_BS_DOOR" }
        );

        internal static List<ProtectionRacketeeringScenarioScheme> ScenarioSchemeList = new List<ProtectionRacketeeringScenarioScheme>
        {
            //Scenario1,
            Scenario2
        };
    }
}
