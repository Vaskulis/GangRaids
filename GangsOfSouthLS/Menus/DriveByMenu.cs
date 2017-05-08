﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;
using RAGENativeUI;
using RAGENativeUI.PauseMenu;
using RAGENativeUI.Elements;
using System.Windows.Forms;
using GangsOfSouthLS.Callouts;
using GangsOfSouthLS.INIFile;
using LSPD_First_Response.Mod.API;
using GangsOfSouthLS.HelperClasses.DriveByShootingHelpers;

namespace GangsOfSouthLS.Menus
{
    internal static class DriveByMenu
    {
        private static bool Initialized = false;
        internal static bool Visible
        {
            get { return MainTabView.Visible; }
            set { MainTabView.Visible = value; }
        }

        private static TabView MainTabView;

        private static TabItemSimpleList InformationListTab;
        internal static Dictionary<string, string> InformationDict;

        private static TabSubmenuItem ActionTab;
        internal static List<TabItem> ActionList;

        internal static void Initialize()
        {
            Game.FrameRender += Process;
            MainTabView = new TabView("Drive-By Shooting Menu");

            InformationDict = new Dictionary<string, string> { };
            ActionList = new List<TabItem> { };

            InformationListTab = new TabItemSimpleList("Vehicle Information", InformationDict);
            ActionTab = new TabSubmenuItem("Actions", ActionList);

            MainTabView.Tabs.Add(InformationListTab);
            MainTabView.RefreshIndex();
            Initialized = true;
        }

        internal static void Terminate()
        {
            if (Initialized)
            {
                MainTabView.Visible = false;
                Game.FrameRender -= Process;
                Initialized = false;
            }
        }

        private static void Process(object sender, GraphicsEventArgs e)
        {
            if (Game.IsKeyDown(INIReader.MenuKey))
            {
                if (Game.IsKeyDownRightNow(INIReader.MenuModifierKey) && !Functions.IsPoliceComputerActive())
                {
                    InformationListTab.Dictionary = InformationDict;
                    MainTabView.Visible = !MainTabView.Visible;
                }
            }
            MainTabView.Update();
        }

        internal static void AddActionToMenu(string title, EventHandler action)
        {
            MainTabView.Visible = false;
            var actionTabItem = new TabItem(title);
            actionTabItem.Activated += action;
            ActionList.Add(actionTabItem);
            ActionTab.Items = ActionList;
            if (!MainTabView.Tabs.Contains(ActionTab))
            {
                MainTabView.Tabs.Add(ActionTab);
                MainTabView.RefreshIndex();
            }
        }

        internal static void RemoveActionFromMenu(string title)
        {
            MainTabView.Visible = false;
            ActionList.RemoveAll(actionTab => actionTab.Title == title);
            if (ActionList.Count == 0)
            {
                MainTabView.Tabs.Remove(ActionTab);
                MainTabView.RefreshIndex();
            }
        }
    }
}