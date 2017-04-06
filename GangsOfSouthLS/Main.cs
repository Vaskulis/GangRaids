using GangsOfSouthLS.INIFile;
using GangsOfSouthLS.Menus;
using LSPD_First_Response.Mod.API;
using Rage;
using System;
using System.Reflection;

[assembly: Rage.Attributes.Plugin("GangsOfSouthLS", Description = "Vaskulis' Gangs Of South Los Santos", Author = "Vaskulis")]

namespace GangsOfSouthLS
{
    internal class Main : Plugin
    {
        public override void Initialize()
        {
            Functions.OnOnDutyStateChanged += OnOnDutyStateChangedHandler;
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(LSPDFRResolveEventHandler);
            Game.LogTrivial("Initializing GangsOfSouthLS plugin.");
        }

        public override void Finally()
        {
            Game.LogTrivial("GangsOfSouthLS has been cleaned up.");
        }

        private static void OnOnDutyStateChangedHandler(bool OnDuty)
        {
            if (OnDuty)
            {
                if (INIReader.LoadINIFile())
                {
                    RegisterCallouts();
                    DrugDealMenu.InitializeAndProcess();
                }
            }
        }

        private static void RegisterCallouts()
        {
            Functions.RegisterCallout(typeof(Callouts.DrugDeal));
            Functions.RegisterCallout(typeof(Callouts.ProtectionRacketeering));
        }

        public static Assembly LSPDFRResolveEventHandler(object sender, ResolveEventArgs args)
        {
            foreach (Assembly assembly in Functions.GetAllUserPlugins())
            {
                if (args.Name.ToLower().Contains(assembly.GetName().Name.ToLower()))
                {
                    return assembly;
                }
            }
            return null;
        }
    }
}