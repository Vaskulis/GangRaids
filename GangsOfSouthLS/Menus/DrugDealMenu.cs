using GangsOfSouthLS.Callouts;
using GangsOfSouthLS.HelperClasses.CommonUtilities;
using GangsOfSouthLS.HelperClasses.DrugDealHelpers;
using GangsOfSouthLS.INIFile;
using LSPD_First_Response.Mod.API;
using Rage;
using RAGENativeUI;
using RAGENativeUI.Elements;
using System.Collections.Generic;
using System.Windows.Forms;

namespace GangsOfSouthLS.Menus
{
    internal static class DrugDealMenu
    {
        private static UIMenu drugDealPositionMenu;
        private static UIMenuListItem copCar1ListItem;
        private static UIMenuListItem copCar2ListItem;
        private static UIMenuListItem insertionPointListItem;
        private static List<dynamic> insertionPointDescriptionList;
        private static List<CopCarWayPoint> spawnPoints;
        private static bool isDrugDealMenuActive = false;

        private static List<dynamic> copCarBuildNameList;

        private static List<CopCarWayPoint> selectedCopcarWaypointList;
        private static CopCarBuild selectedCopCar1Build;
        private static CopCarBuild selectedCopCar2Build;
        private static CopCarWayPoint selectedPlayerWaypoint;

        private static MenuPool _menuPool;

        internal static void InitializeAndProcess()
        {
            GameFiber.StartNew(delegate
            {
                Game.FrameRender += Process;

                _menuPool = new MenuPool();

                drugDealPositionMenu = new UIMenu("GangsOfSouthLS", "~b~Drug Deal Menu");
                drugDealPositionMenu.SetKey(Common.MenuControls.Up, Keys.W);
                drugDealPositionMenu.SetKey(Common.MenuControls.Down, Keys.S);
                drugDealPositionMenu.SetKey(Common.MenuControls.Left, Keys.A);
                drugDealPositionMenu.SetKey(Common.MenuControls.Right, Keys.D);

                drugDealPositionMenu.OnListChange += OnListChange;
                drugDealPositionMenu.OnMenuClose += OnMenuClose;

                while (true)
                {
                    if (DrugDeal.IsCurrentlyRunning && (DrugDeal.DrugDealState == DrugDeal.EDrugDealState.InPreparation) && !isDrugDealMenuActive)
                    {
                        copCarBuildNameList = new List<dynamic> { };
                        foreach (var item in DrugDeal.Scenario.CopCarBuildList)
                        {
                            copCarBuildNameList.Add(item.CarName);
                        }

                        MakeInsertionPointListItem();
                        MakeCopCarListItems();
                        CreateBlips();

                        DrugDeal.Scenario.CopCar1 = DrugDeal.Scenario.MakeCopCarDictVehicleAndOccupy(selectedCopcarWaypointList[0], selectedCopCar1Build);
                        DrugDeal.Scenario.CopCar2 = DrugDeal.Scenario.MakeCopCarDictVehicleAndOccupy(selectedCopcarWaypointList[1], selectedCopCar2Build);
                        DrugDeal.Scenario.CopCarDict = DrugDeal.Scenario.MakeCopCarDict(selectedCopcarWaypointList[0], selectedCopcarWaypointList[1]);
                        DrugDeal.Scenario.CopList1 = DrugDeal.Scenario.MakeListOfOccupants(DrugDeal.Scenario.CopCar1);
                        DrugDeal.Scenario.CopList2 = DrugDeal.Scenario.MakeListOfOccupants(DrugDeal.Scenario.CopCar2);

                        drugDealPositionMenu.RefreshIndex();

                        _menuPool.Add(drugDealPositionMenu);

                        isDrugDealMenuActive = !isDrugDealMenuActive;
                    }
                    else if (DrugDeal.IsCurrentlyRunning && !(DrugDeal.DrugDealState == DrugDeal.EDrugDealState.InPreparation) && isDrugDealMenuActive)
                    {
                        drugDealPositionMenu.Visible = false;
                        drugDealPositionMenu.MenuItems.Clear();
                        _menuPool.Remove(drugDealPositionMenu);
                        isDrugDealMenuActive = !isDrugDealMenuActive;
                    }
                    GameFiber.Yield();
                }
            });
        }

        private static void Process(object sender, GraphicsEventArgs e)
        {
            if (isDrugDealMenuActive)
            {
                if (!DrugDeal.PlayerIsInPosition && Game.IsKeyDown(INIReader.MenuKey) && !_menuPool.IsAnyMenuOpen())
                {
                    if (Game.IsKeyDownRightNow(INIReader.MenuModifierKey) && !Functions.IsPoliceComputerActive())
                    {
                        drugDealPositionMenu.Visible = !drugDealPositionMenu.Visible;
                    }
                }
            }
            _menuPool.ProcessMenus();
        }

        private static void OnListChange(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if (sender == drugDealPositionMenu)
            {
                if (selectedItem == insertionPointListItem)
                {
                    selectedPlayerWaypoint = spawnPoints.Find(delegate (CopCarWayPoint ccwp)
                    {
                        return ccwp.Description == insertionPointListItem.IndexToItem(insertionPointListItem.Index);
                    });

                    DrugDeal.PlayerStartPointBlip.IsRouteEnabled = false;
                    DrugDeal.PlayerStartPointBlip.Position = selectedPlayerWaypoint.StartPoint.Position;
                    DrugDeal.PlayerStartPointBlip.IsRouteEnabled = true;

                    selectedCopcarWaypointList = spawnPoints.FindAll(delegate (CopCarWayPoint ccwp)
                    {
                        return ccwp.Description != insertionPointListItem.IndexToItem(insertionPointListItem.Index);
                    });

                    copCar1ListItem.Text = string.Format("Car 1: {0}", selectedCopcarWaypointList[0].Description);
                    copCar2ListItem.Text = string.Format("Car 2: {0}", selectedCopcarWaypointList[1].Description);
                }

                if (selectedItem == copCar1ListItem)
                {
                    selectedCopCar1Build = DrugDeal.Scenario.CopCarBuildList.Find(delegate (CopCarBuild ccb)
                    {
                        return ccb.CarName == copCar1ListItem.IndexToItem(copCar1ListItem.Index);
                    });
                }

                if (selectedItem == copCar2ListItem)
                {
                    selectedCopCar2Build = DrugDeal.Scenario.CopCarBuildList.Find(delegate (CopCarBuild ccb)
                    {
                        return ccb.CarName == copCar2ListItem.IndexToItem(copCar2ListItem.Index);
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
            if (sender == drugDealPositionMenu)
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

                    DrugDeal.Scenario.CopCar1 = DrugDeal.Scenario.MakeCopCarDictVehicleAndOccupy(selectedCopcarWaypointList[0], selectedCopCar1Build);
                    DrugDeal.Scenario.CopCar2 = DrugDeal.Scenario.MakeCopCarDictVehicleAndOccupy(selectedCopcarWaypointList[1], selectedCopCar2Build);
                    DrugDeal.Scenario.CopCarDict = DrugDeal.Scenario.MakeCopCarDict(selectedCopcarWaypointList[0], selectedCopcarWaypointList[1]);
                    DrugDeal.Scenario.CopList1 = DrugDeal.Scenario.MakeListOfOccupants(DrugDeal.Scenario.CopCar1);
                    DrugDeal.Scenario.CopList2 = DrugDeal.Scenario.MakeListOfOccupants(DrugDeal.Scenario.CopCar2);

                    DrugDeal.PlayerStartPosition = selectedPlayerWaypoint.StartPoint.Position;
                    DrugDeal.PlayerEndPosition = selectedPlayerWaypoint.EndPoint.Position;
                    DrugDeal.PlayerDirection = selectedPlayerWaypoint.Direction;
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
                insertionPointDescriptionList.Add(spawnPoint.Description);
            }
            insertionPointListItem = new UIMenuListItem("Your insertion point:", insertionPointDescriptionList, UsefulFunctions.rng.Next(insertionPointDescriptionList.Count));
            drugDealPositionMenu.AddItem(insertionPointListItem);
        }

        private static void CreateBlips()
        {
            selectedPlayerWaypoint = spawnPoints.Find(delegate (CopCarWayPoint ccwp)
            {
                return ccwp.Description == insertionPointListItem.IndexToItem(insertionPointListItem.Index);
            });

            DrugDeal.PlayerStartPointBlip = new Blip(selectedPlayerWaypoint.StartPoint.Position);
            DrugDeal.PlayerStartPointBlip.Color = System.Drawing.Color.Purple;
            DrugDeal.PlayerStartPointBlip.RouteColor = System.Drawing.Color.Yellow;
            DrugDeal.PlayerStartPointBlip.IsRouteEnabled = true;

            DrugDeal.PlayerStartPosition = selectedPlayerWaypoint.StartPoint.Position;
            DrugDeal.PlayerEndPosition = selectedPlayerWaypoint.EndPoint.Position;
            DrugDeal.PlayerDirection = selectedPlayerWaypoint.Direction;
        }

        private static void MakeCopCarListItems()
        {
            selectedCopcarWaypointList = spawnPoints.FindAll(delegate (CopCarWayPoint ccwp) { return ccwp.Description != insertionPointListItem.IndexToItem(insertionPointListItem.Index); });
            copCar1ListItem = new UIMenuListItem(string.Format("Car 1: {0}", selectedCopcarWaypointList[0].Description), copCarBuildNameList, UsefulFunctions.rng.Next(DrugDeal.Scenario.CopCarBuildList.Count));
            copCar2ListItem = new UIMenuListItem(string.Format("Car 2: {0}", selectedCopcarWaypointList[1].Description), copCarBuildNameList, UsefulFunctions.rng.Next(DrugDeal.Scenario.CopCarBuildList.Count));
            selectedCopCar1Build = DrugDeal.Scenario.CopCarBuildList.Find(delegate (CopCarBuild ccb)
            {
                return ccb.CarName == copCar1ListItem.IndexToItem(copCar1ListItem.Index);
            });
            selectedCopCar2Build = DrugDeal.Scenario.CopCarBuildList.Find(delegate (CopCarBuild ccb)
            {
                return ccb.CarName == copCar2ListItem.IndexToItem(copCar2ListItem.Index);
            });
            drugDealPositionMenu.AddItem(copCar1ListItem);
            drugDealPositionMenu.AddItem(copCar2ListItem);
        }
    }
}