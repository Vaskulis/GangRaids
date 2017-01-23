using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LSPD_First_Response.Mod.API;
using System.Windows.Forms;
using Rage;
using RAGENativeUI;
using RAGENativeUI.Elements;
using GangRaids.HelperClasses;
using GangRaids.Callouts;

namespace GangRaids.Menus
{
    class DrugDealMenu
    {
        private static UIMenu DrugDealMenuPositionMenu;
        private static UIMenuListItem copCar1ListItem;
        private static UIMenuListItem copCar2ListItem;
        private static UIMenuListItem insertionPointListItem;
        private static List<dynamic> insertionPointDescriptionList;
        private static List<CopCarWayPoint> spawnPoints;
        private static bool IsDrugDealMenuActive = false;

        private static UIMenuItem confirmItem;

        private static List<dynamic> CopCarBuildNameList;

        private static List<CopCarWayPoint> SelectedWaypointList;
        private static CopCarBuild SelectedCopCar1Build;
        private static CopCarBuild SelectedCopCar2Build;

        private static MenuPool _menuPool;

        public static void InitializeAndProcess()
        {
            GameFiber.StartNew(delegate 
            {
                Game.FrameRender += Process;

                _menuPool = new MenuPool();

                DrugDealMenuPositionMenu = new UIMenu("Gang Raids", "~b~Drug Deal Menu");
                DrugDealMenuPositionMenu.SetKey(Common.MenuControls.Up, Keys.W);
                DrugDealMenuPositionMenu.SetKey(Common.MenuControls.Down, Keys.S);
                DrugDealMenuPositionMenu.SetKey(Common.MenuControls.Left, Keys.A);
                DrugDealMenuPositionMenu.SetKey(Common.MenuControls.Right, Keys.D);

                DrugDealMenuPositionMenu.OnListChange += OnListChange;
                DrugDealMenuPositionMenu.OnItemSelect += OnItemSelect;

                while (true)
                {
                    if (DrugDeal.IsCurrentlyRunning && (DrugDeal.DrugDealState == DrugDeal.EDrugDealState.Accepted) && !IsDrugDealMenuActive)
                    {
                        CopCarBuildNameList = new List<dynamic> { };
                        foreach(var item in DrugDeal.Scenario.CopCarBuildList)
                        {
                            CopCarBuildNameList.Add(item.carName);
                        }

                        confirmItem = new UIMenuItem("Confirm", "Press this when you are ready!");
                        MakeInsertionPointListItem();
                        CreateInsertionPointBlip();
                        MakeCopCarListItems();

                        DrugDealMenuPositionMenu.AddItem(confirmItem);
                        DrugDealMenuPositionMenu.RefreshIndex();

                        _menuPool.Add(DrugDealMenuPositionMenu);

                        IsDrugDealMenuActive = !IsDrugDealMenuActive;
                    }
                    else if (DrugDeal.IsCurrentlyRunning && !(DrugDeal.DrugDealState == DrugDeal.EDrugDealState.Accepted) && IsDrugDealMenuActive)
                    {
                        DrugDealMenuPositionMenu.Visible = false;
                        DrugDealMenuPositionMenu.MenuItems.Clear();
                        _menuPool.Remove(DrugDealMenuPositionMenu);
                        IsDrugDealMenuActive = !IsDrugDealMenuActive;
                    }
                    GameFiber.Yield();
                }
            });           
        }

        private static void Process(object sender, GraphicsEventArgs e)
        {
            if (Game.IsKeyDown(Keys.L) && !_menuPool.IsAnyMenuOpen())
            {
                if (IsDrugDealMenuActive)
                {
                    DrugDealMenuPositionMenu.Visible = !DrugDealMenuPositionMenu.Visible;
                }
            }
            _menuPool.ProcessMenus();
        }

        private static void OnListChange(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if (sender == DrugDealMenuPositionMenu)
            {
                if (selectedItem == insertionPointListItem)
                {
                    var waypoint = spawnPoints.Find(delegate (CopCarWayPoint ccwp)
                    {
                        return ccwp.description == insertionPointListItem.IndexToItem(insertionPointListItem.Index); //Finds the CopCarWayPoint with the description that is currently displayed 
                    });

                    DrugDeal.SpawnPointBlip.IsRouteEnabled = false;
                    DrugDeal.SpawnPointBlip.Position = waypoint.startPoint.Position;
                    DrugDeal.SpawnPointBlip.IsRouteEnabled = true;

                    SelectedWaypointList = spawnPoints.FindAll(delegate (CopCarWayPoint ccwp) 
                    {
                        return ccwp.description != insertionPointListItem.IndexToItem(insertionPointListItem.Index);
                    });

                    copCar1ListItem.Text = string.Format("Car 1: {0}", SelectedWaypointList[0].description);
                    copCar2ListItem.Text = string.Format("Car 2: {0}", SelectedWaypointList[1].description);
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


        private static void OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if (sender == DrugDealMenuPositionMenu)
            {
                if (selectedItem == confirmItem)
                {
                    GameFiber.StartNew(delegate
                    {
                        DrugDeal.Scenario.CopCar1 = DrugDeal.Scenario.MakeCopCarDictVehicleAndOccupy(SelectedWaypointList[0], SelectedCopCar1Build);
                        DrugDeal.Scenario.CopCar2 = DrugDeal.Scenario.MakeCopCarDictVehicleAndOccupy(SelectedWaypointList[1], SelectedCopCar2Build);
                        DrugDeal.Scenario.CopCarDict = DrugDeal.Scenario.MakeCopCarDict(SelectedWaypointList[0], SelectedWaypointList[1]);
                        DrugDeal.Scenario.CopList1 = DrugDeal.Scenario.MakeListOfOccupants(DrugDeal.Scenario.CopCar1);
                        DrugDeal.Scenario.CopList2 = DrugDeal.Scenario.MakeListOfOccupants(DrugDeal.Scenario.CopCar2);

                        DrugDeal.PlayerStartPosition = DrugDeal.SpawnPointBlip.Position;
                        DrugDeal.DrugDealState = DrugDeal.EDrugDealState.GettingInPosition;
                        return;
                    });
                }
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
            insertionPointListItem = new UIMenuListItem("Your insertion point:", insertionPointDescriptionList, 0);
            DrugDealMenuPositionMenu.AddItem(insertionPointListItem);
        }


        private static void CreateInsertionPointBlip()
        {
            var waypoint = spawnPoints.Find(delegate (CopCarWayPoint ccwp) 
            {
                return ccwp.description == insertionPointListItem.IndexToItem(insertionPointListItem.Index);
            });

            DrugDeal.SpawnPointBlip = new Blip(waypoint.startPoint.Position);
            DrugDeal.SpawnPointBlip.Color = System.Drawing.Color.Purple;
            DrugDeal.SpawnPointBlip.RouteColor = System.Drawing.Color.Yellow;
            DrugDeal.SpawnPointBlip.IsRouteEnabled = true;
        }


        private static void MakeCopCarListItems()
        {
            SelectedWaypointList = spawnPoints.FindAll(delegate (CopCarWayPoint ccwp) { return ccwp.description != insertionPointListItem.IndexToItem(insertionPointListItem.Index); });
            copCar1ListItem = new UIMenuListItem(string.Format("Car 1: {0}", SelectedWaypointList[0].description), CopCarBuildNameList, 0);
            copCar2ListItem = new UIMenuListItem(string.Format("Car 2: {0}", SelectedWaypointList[1].description), CopCarBuildNameList, 0);
            SelectedCopCar1Build = DrugDeal.Scenario.CopCarBuildList.Find(delegate (CopCarBuild ccb)
            {
                return ccb.carName == copCar1ListItem.IndexToItem(copCar1ListItem.Index);
            });
            SelectedCopCar2Build = DrugDeal.Scenario.CopCarBuildList.Find(delegate (CopCarBuild ccb)
            {
                return ccb.carName == copCar2ListItem.IndexToItem(copCar2ListItem.Index);
            });
            DrugDealMenuPositionMenu.AddItem(copCar1ListItem);
            DrugDealMenuPositionMenu.AddItem(copCar2ListItem);
        }       
    }
}
