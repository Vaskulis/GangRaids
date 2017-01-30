using System.Collections.Generic;
using LSPD_First_Response.Mod.API;
using System.Windows.Forms;
using Rage;
using RAGENativeUI;
using RAGENativeUI.Elements;
using GangsOfSouthLS.HelperClasses;
using GangsOfSouthLS.HelperClasses.DrugDealHelpers;
using GangsOfSouthLS.Callouts;
using GangsOfSouthLS.INIFile;
using GangsOfSouthLS.HelperClasses.CommonUtilities;

namespace GangsOfSouthLS.Menus
{
    class DrugDealMenu
    {
        private static UIMenu DrugDealPositionMenu;
        private static UIMenuListItem copCar1ListItem;
        private static UIMenuListItem copCar2ListItem;
        private static UIMenuListItem insertionPointListItem;
        private static List<dynamic> insertionPointDescriptionList;
        private static List<CopCarWayPoint> spawnPoints;
        private static bool IsDrugDealMenuActive = false;

        private static List<dynamic> CopCarBuildNameList;

        private static List<CopCarWayPoint> SelectedCopcarWaypointList;
        private static CopCarBuild SelectedCopCar1Build;
        private static CopCarBuild SelectedCopCar2Build;
        private static CopCarWayPoint SelectedPlayerWaypoint;

        private static MenuPool _menuPool;

        internal static void InitializeAndProcess()
        {
            GameFiber.StartNew(delegate 
            {
                Game.FrameRender += Process;

                _menuPool = new MenuPool();

                DrugDealPositionMenu = new UIMenu("GangsOfSouthLS", "~b~Drug Deal Menu");
                DrugDealPositionMenu.SetKey(Common.MenuControls.Up, Keys.W);
                DrugDealPositionMenu.SetKey(Common.MenuControls.Down, Keys.S);
                DrugDealPositionMenu.SetKey(Common.MenuControls.Left, Keys.A);
                DrugDealPositionMenu.SetKey(Common.MenuControls.Right, Keys.D);

                DrugDealPositionMenu.OnListChange += OnListChange;
                DrugDealPositionMenu.OnMenuClose += OnMenuClose;

                while (true)
                {
                    if (DrugDeal.IsCurrentlyRunning && (DrugDeal.DrugDealState == DrugDeal.EDrugDealState.InPreparation) && !IsDrugDealMenuActive)
                    {
                        CopCarBuildNameList = new List<dynamic> { };
                        foreach(var item in DrugDeal.Scenario.CopCarBuildList)
                        {
                            CopCarBuildNameList.Add(item.carName);
                        }

                        MakeInsertionPointListItem();
                        MakeCopCarListItems();
                        CreateBlips();

                        DrugDeal.Scenario.CopCar1 = DrugDeal.Scenario.MakeCopCarDictVehicleAndOccupy(SelectedCopcarWaypointList[0], SelectedCopCar1Build);
                        DrugDeal.Scenario.CopCar2 = DrugDeal.Scenario.MakeCopCarDictVehicleAndOccupy(SelectedCopcarWaypointList[1], SelectedCopCar2Build);
                        DrugDeal.Scenario.CopCarDict = DrugDeal.Scenario.MakeCopCarDict(SelectedCopcarWaypointList[0], SelectedCopcarWaypointList[1]);
                        DrugDeal.Scenario.CopList1 = DrugDeal.Scenario.MakeListOfOccupants(DrugDeal.Scenario.CopCar1);
                        DrugDeal.Scenario.CopList2 = DrugDeal.Scenario.MakeListOfOccupants(DrugDeal.Scenario.CopCar2);

                        DrugDealPositionMenu.RefreshIndex();

                        _menuPool.Add(DrugDealPositionMenu);

                        IsDrugDealMenuActive = !IsDrugDealMenuActive;
                    }
                    else if (DrugDeal.IsCurrentlyRunning && !(DrugDeal.DrugDealState == DrugDeal.EDrugDealState.InPreparation) && IsDrugDealMenuActive)
                    {
                        DrugDealPositionMenu.Visible = false;
                        DrugDealPositionMenu.MenuItems.Clear();
                        _menuPool.Remove(DrugDealPositionMenu);
                        IsDrugDealMenuActive = !IsDrugDealMenuActive;
                    }
                    GameFiber.Yield();
                }
            });           
        }

        private static void Process(object sender, GraphicsEventArgs e)
        {
            if (IsDrugDealMenuActive)                
            {
                if (!DrugDeal.PlayerIsInPosition && Game.IsKeyDown(INIReader.MenuKey) && !_menuPool.IsAnyMenuOpen())
                {
                    if (Game.IsKeyDownRightNow(INIReader.MenuModifierKey) && !Functions.IsPoliceComputerActive())
                    {
                        DrugDealPositionMenu.Visible = !DrugDealPositionMenu.Visible;
                    }
                }
            }
            _menuPool.ProcessMenus();
        }

        private static void OnListChange(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if (sender == DrugDealPositionMenu)
            {
                if (selectedItem == insertionPointListItem)
                {
                    SelectedPlayerWaypoint = spawnPoints.Find(delegate (CopCarWayPoint ccwp)
                    {
                        return ccwp.description == insertionPointListItem.IndexToItem(insertionPointListItem.Index);
                    });

                    DrugDeal.PlayerStartPointBlip.IsRouteEnabled = false;
                    DrugDeal.PlayerStartPointBlip.Position = SelectedPlayerWaypoint.startPoint.Position;
                    DrugDeal.PlayerStartPointBlip.IsRouteEnabled = true;

                    SelectedCopcarWaypointList = spawnPoints.FindAll(delegate (CopCarWayPoint ccwp) 
                    {
                        return ccwp.description != insertionPointListItem.IndexToItem(insertionPointListItem.Index);
                    });

                    copCar1ListItem.Text = string.Format("Car 1: {0}", SelectedCopcarWaypointList[0].description);
                    copCar2ListItem.Text = string.Format("Car 2: {0}", SelectedCopcarWaypointList[1].description);
                }

                if (selectedItem == copCar1ListItem)
                {
                    SelectedCopCar1Build = DrugDeal.Scenario.CopCarBuildList.Find(delegate (CopCarBuild ccb)
                    {
                        return ccb.carName == copCar1ListItem.IndexToItem(copCar1ListItem.Index);
                    });
                }

                if (selectedItem == copCar2ListItem)
                {
                    SelectedCopCar2Build = DrugDeal.Scenario.CopCarBuildList.Find(delegate (CopCarBuild ccb)
                    {
                        return ccb.carName == copCar2ListItem.IndexToItem(copCar2ListItem.Index);
                    });
                }
            }

            else
            {
                return;
            };
        }

        private static void OnMenuClose(UIMenu sender)
        {
            if (sender == DrugDealPositionMenu)
            {
                GameFiber.StartNew(delegate
                {
                    foreach (var cop in DrugDeal.Scenario.CopList1)
                    {
                        if (!(cop == null) && cop.Exists())
                        {
                            cop.Delete();
                        }
                    }
                    foreach (var cop in DrugDeal.Scenario.CopList2)
                    {
                        if (!(cop == null) && cop.Exists())
                        {
                            cop.Delete();
                        }
                    }
                    foreach (var car in DrugDeal.Scenario.CopCarDict.Keys)
                    {
                        if (!(car == null) && car.Exists())
                        {
                            car.Delete();
                        }
                    }
                    DrugDeal.Scenario.CopCarDict.Clear();
                    DrugDeal.Scenario.CopList1.Clear();
                    DrugDeal.Scenario.CopList2.Clear();

                    DrugDeal.Scenario.CopCar1 = DrugDeal.Scenario.MakeCopCarDictVehicleAndOccupy(SelectedCopcarWaypointList[0], SelectedCopCar1Build);
                    DrugDeal.Scenario.CopCar2 = DrugDeal.Scenario.MakeCopCarDictVehicleAndOccupy(SelectedCopcarWaypointList[1], SelectedCopCar2Build);
                    DrugDeal.Scenario.CopCarDict = DrugDeal.Scenario.MakeCopCarDict(SelectedCopcarWaypointList[0], SelectedCopcarWaypointList[1]);
                    DrugDeal.Scenario.CopList1 = DrugDeal.Scenario.MakeListOfOccupants(DrugDeal.Scenario.CopCar1);
                    DrugDeal.Scenario.CopList2 = DrugDeal.Scenario.MakeListOfOccupants(DrugDeal.Scenario.CopCar2);

                    DrugDeal.PlayerStartPosition = SelectedPlayerWaypoint.startPoint.Position;
                    DrugDeal.PlayerEndPosition = SelectedPlayerWaypoint.endPoint.Position;
                    DrugDeal.PlayerDirection = SelectedPlayerWaypoint.direction;
                    return;
                });
            }
            else
            {
                return;
            }
        }


        private static void MakeInsertionPointListItem()
        {
            insertionPointDescriptionList = new List<dynamic> { };
            spawnPoints = new List<CopCarWayPoint>(DrugDeal.Scenario.CopCarWayPointList);
            foreach (var spawnPoint in spawnPoints)
            {
                insertionPointDescriptionList.Add(spawnPoint.description);
            }
            insertionPointListItem = new UIMenuListItem("Your insertion point:", insertionPointDescriptionList, UsefulFunctions.rng.Next(insertionPointDescriptionList.Count));
            DrugDealPositionMenu.AddItem(insertionPointListItem);
        }


        private static void CreateBlips()
        {
            SelectedPlayerWaypoint = spawnPoints.Find(delegate (CopCarWayPoint ccwp)
            {
                return ccwp.description == insertionPointListItem.IndexToItem(insertionPointListItem.Index);
            });

            DrugDeal.PlayerStartPointBlip = new Blip(SelectedPlayerWaypoint.startPoint.Position);
            DrugDeal.PlayerStartPointBlip.Color = System.Drawing.Color.Purple;
            DrugDeal.PlayerStartPointBlip.RouteColor = System.Drawing.Color.Yellow;
            DrugDeal.PlayerStartPointBlip.IsRouteEnabled = true;

            DrugDeal.PlayerStartPosition = SelectedPlayerWaypoint.startPoint.Position;
            DrugDeal.PlayerEndPosition = SelectedPlayerWaypoint.endPoint.Position;
            DrugDeal.PlayerDirection = SelectedPlayerWaypoint.direction;
        }


        private static void MakeCopCarListItems()
        {
            SelectedCopcarWaypointList = spawnPoints.FindAll(delegate (CopCarWayPoint ccwp) { return ccwp.description != insertionPointListItem.IndexToItem(insertionPointListItem.Index); });
            copCar1ListItem = new UIMenuListItem(string.Format("Car 1: {0}", SelectedCopcarWaypointList[0].description), CopCarBuildNameList, UsefulFunctions.rng.Next(DrugDeal.Scenario.CopCarBuildList.Count));
            copCar2ListItem = new UIMenuListItem(string.Format("Car 2: {0}", SelectedCopcarWaypointList[1].description), CopCarBuildNameList, UsefulFunctions.rng.Next(DrugDeal.Scenario.CopCarBuildList.Count));
            SelectedCopCar1Build = DrugDeal.Scenario.CopCarBuildList.Find(delegate (CopCarBuild ccb)
            {
                return ccb.carName == copCar1ListItem.IndexToItem(copCar1ListItem.Index);
            });
            SelectedCopCar2Build = DrugDeal.Scenario.CopCarBuildList.Find(delegate (CopCarBuild ccb)
            {
                return ccb.carName == copCar2ListItem.IndexToItem(copCar2ListItem.Index);
            });
            DrugDealPositionMenu.AddItem(copCar1ListItem);
            DrugDealPositionMenu.AddItem(copCar2ListItem);
        }       
    }
}
