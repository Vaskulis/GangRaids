using GangsOfSouthLS.HelperClasses.CommonUtilities;
using GangsOfSouthLS.HelperClasses.DriveByShootingHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GangsOfSouthLS.ScenarioCollections.DriveByShootingScenarios
{
    internal static class CrimeSceneScenarioTemplateCollection
    {
        internal static CrimeSceneScenarioTemplate Scenario1 = new CrimeSceneScenarioTemplate
        (
            "Scenario1",
            new List<Pos4>
            {
                new Pos4(490.7988f, -1688.277f, 29.3671f, 42.0048f),
                new Pos4(492.2702f, -1686.381f, 29.25876f, 64.76185f),
                new Pos4(488.2732f, -1687.397f, 29.1293f, 350.1546f),
                new Pos4(486.2477f, -1687.346f, 29.13265f, 323.7497f),
                new Pos4(485.3841f, -1684.973f, 29.274f, 284.3328f),
                new Pos4(485.2237f, -1682.553f, 29.30726f, 249.1525f),
                new Pos4(487.3123f, -1680.011f, 29.27447f, 202.2121f),
                new Pos4(493.5537f, -1683.636f, 29.27003f, 122.2111f),
            },
            new Pos4(502.0011f, -1685.072f, 29.00203f, 231.3387f),
            new List<Pos4>
            {
                new Pos4(489.9297f, -1685.689f, 29.21663f, 66.80321f),
                new Pos4(488.2348f, -1685.927f, 29.14826f, 108.2664f),
                new Pos4(488.0271f, -1683.535f, 29.28896f, 14.93433f),
                new Pos4(491.2874f, -1682.686f, 29.27125f, 32.4064f),
                new Pos4(490.7281f, -1681.037f, 29.27238f, 323.8563f),
            }
        );

        internal static List<CrimeSceneScenarioTemplate> ScenarioTemplateList = new List<CrimeSceneScenarioTemplate>
        {
            Scenario1,
        };
    }
}
