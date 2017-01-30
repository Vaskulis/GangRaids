using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;
using Rage.Native;

namespace GangsOfSouthLS.HelperClasses.CommonUtilities
{
    static class MyNatives
    {
        internal static void OpenDoor(Model doormodel, Vector3 doorlocation)
        {
            NativeFunction.Natives.SetStateOfClosestDoorOfType(doormodel.Hash, doorlocation.X, doorlocation.Y, doorlocation.Z, false, 0f, 0);
        }

        internal static void MakePedAbleToShootOutOfCar(Ped ped)
        {
            NativeFunction.Natives.SetPedCombatAttributes(ped, 1, true);
        }

        internal enum CombatAbilityFlag { Poor = 0, Average = 1, Professional = 2 }
        internal enum CombatMovementFlag { Stationary = 0, Defensive = 1, Offensive = 2, Suicidal = 3}
        internal static void SetPedCombatAbilityAndMovement(Ped ped, CombatAbilityFlag caFlag, CombatMovementFlag cmFlag)
        {
            NativeFunction.Natives.SetPedCombatAbility(ped, (int)caFlag);
            NativeFunction.Natives.SetPedCombatMovement(ped, (int)cmFlag);
        }
    }
}
