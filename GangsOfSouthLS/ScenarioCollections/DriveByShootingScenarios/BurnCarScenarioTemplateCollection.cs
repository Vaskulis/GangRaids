using GangsOfSouthLS.HelperClasses.CommonUtilities;
using GangsOfSouthLS.HelperClasses.DriveByShootingHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GangsOfSouthLS.ScenarioCollections.DriveByShootingScenarios
{
    internal static class BurnCarScenarioTemplateCollection
    {
        internal static BurnCarScenarioTemplate Burn1 = new BurnCarScenarioTemplate
        (
            new Pos4(468.9062f, -1874.292f, 26.47411f, 286.5867f),
            new List<Pos4>
            {
                new Pos4(472.2838f, -1868.781f, 26.96667f, 156.893f),
                new Pos4(469.1867f, -1869.124f, 26.97666f, 169.3779f),
                new Pos4(461.6102f, -1875.362f, 26.86746f, 110.1229f),
                new Pos4(468.7542f, -1878.831f, 26.86749f, 16.75464f),
                new Pos4(480.2662f, -1869.003f, 26.69138f, 95.30272f),
                new Pos4(474.6379f, -1867.499f, 27.03574f, 142.4467f),
            },
            new List<Pos4>
            {
                new Pos4(454.9879f, -1883.37f, 25.90527f, 206.0402f),
                new Pos4(450.3333f, -1884.716f, 26.26466f, 283.398f),
            }
        );
        internal static BurnCarScenarioTemplate Burn2 = new BurnCarScenarioTemplate
        (
            new Pos4(219.7405f, -2240.094f, 5.772473f, 268.823f),
            new List<Pos4>
            {
                new Pos4(223.8153f, -2233.863f, 6.468091f, 145.5934f),
                new Pos4(220.0159f, -2234.458f, 6.465251f, 170.913f),
                new Pos4(214.1797f, -2235.719f, 6.418705f, 232.9058f),
                new Pos4(213.3008f, -2237.698f, 6.294624f, 89.71424f),
                new Pos4(225.1098f, -2246.623f, 5.859924f, 293.9897f),
                new Pos4(219.7298f, -2247.75f, 5.777448f, 354.3471f),
            },
            new List<Pos4>
            {
                new Pos4(203.7126f, -2238.139f, 5.684254f, 271.1045f),
                new Pos4(233.7336f, -2244.324f, 5.599869f, 274.2128f),
            }
        );

        internal static List<BurnCarScenarioTemplate> ScenarioList = new List<BurnCarScenarioTemplate>
        {
            Burn1, Burn2
        };
    }
}
