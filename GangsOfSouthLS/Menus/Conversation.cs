using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RAGENativeUI;
using RAGENativeUI.Elements;
using Rage;
using System.Windows.Forms;
using GangsOfSouthLS.HelperClasses.CommonUtilities;

namespace GangsOfSouthLS.Menus
{
    public class Conversation
    {
        private MenuPool MenuPool;
        private UIMenu ConversationMenu;
        private List<UIMenuItem> ScheduledToRemove;

        private bool IsActive;
        private MyPed Ped;



        public Conversation(MyPed ped)
        {
            IsActive = true;

            MenuPool = new MenuPool();
            ScheduledToRemove = new List<UIMenuItem> { };

            Ped = ped;

            Game.FrameRender += Process;

            ConversationMenu = new UIMenu("GangsOfSouthLS", "Conversation Menu");
            MenuPool.Add(ConversationMenu);
        }


        private void Process(object sender, GraphicsEventArgs e)
        {
            if (IsActive && (MyPed)Ped)
            {
                if (Game.LocalPlayer.Character.DistanceTo(Ped.Position) < 2.5f && Game.IsKeyDown(Keys.Y))
                {
                    ConversationMenu.Visible = true;
                }
                else if (Game.LocalPlayer.Character.DistanceTo(Ped.Position) > 2.5f)
                {
                    ConversationMenu.Visible = false;
                }

                MenuPool.ProcessMenus();
                foreach (var item in ScheduledToRemove)
                {
                    if (ConversationMenu.MenuItems.Contains(item))
                    {
                        ConversationMenu.MenuItems.Remove(item);
                    }
                }
            }
        }


        internal void Terminate()
        {
            IsActive = false;
            Game.FrameRender -= Process;
        }


        internal void AddLine(string text, string reply, Action action, bool removeWhenFinished = true)
        {
            var Line = new UIMenuItem(text);
            Action<UIMenu, UIMenuItem> func = (sender, selectedItem) => 
            {
                GameFiber.StartNew(delegate
                {
                    action();
                });
                Game.DisplaySubtitle(reply, 4500);
                if (removeWhenFinished)
                {
                    ScheduledToRemove.Add(Line);
                }
            };
            Line.Activated += new ItemActivatedEvent(func);
            ConversationMenu.AddItem(Line);
        }

        internal void AddLine(string text, List<string> reply, Action action, bool removeWhenFinished = true)
        {
            var Line = new UIMenuItem(text);
            Action<UIMenu, UIMenuItem> func = (sender, selectedItem) =>
            {
                GameFiber.StartNew(delegate
                {
                    action();
                });
                GameFiber.StartNew(delegate
                {
                    foreach (var rep in reply)
                    {
                        Game.DisplaySubtitle(rep, 4500);
                        GameFiber.Wait(4700);
                    }
                });
                if (removeWhenFinished)
                {
                    ScheduledToRemove.Add(Line);
                }
            };
            Line.Activated += new ItemActivatedEvent(func);
            ConversationMenu.AddItem(Line);
        }

        internal void AddLine(string text, string reply, bool removeWhenFinished = true)
        {
            var Line = new UIMenuItem(text);
            Action<UIMenu, UIMenuItem> func = (sender, selectedItem) =>
            {
                Game.DisplaySubtitle(reply, 4500);
                if (removeWhenFinished)
                {
                    ScheduledToRemove.Add(Line);
                }
            };
            Line.Activated += new ItemActivatedEvent(func);
            ConversationMenu.AddItem(Line);
        }

        internal void AddLine(string text, List<string> reply, bool removeWhenFinished = true)
        {
            var Line = new UIMenuItem(text);
            Action<UIMenu, UIMenuItem> func = (sender, selectedItem) =>
            {
                GameFiber.StartNew(delegate
                {
                    foreach (var rep in reply)
                    {
                        Game.DisplaySubtitle(rep, 4500);
                        GameFiber.Wait(4700);
                    }
                });
                if (removeWhenFinished)
                {
                    ScheduledToRemove.Add(Line);
                }
            };
            Line.Activated += new ItemActivatedEvent(func);
            ConversationMenu.AddItem(Line);
        }


        internal void AddAskForIDItem(int probability)
        {
            var Item = new UIMenuItem("Let me see some ID, please.");
            if (UsefulFunctions.Decide(probability))
            {
                Action<UIMenu, UIMenuItem> func = (sender, selectedItem) =>
                {
                    Game.DisplayNotification("~b~" + Ped.Name + "\n~y~" + Ped.Gender + "~w~, Born: ~y~" + Ped.Birthday.ToString());
                };
                Item.Activated += new ItemActivatedEvent(func);
            }
            else
            {
                var ExcuseList = new List<string>
                {
                    "Sorry, officer. I don't have it with me right now.",
                    "Oh, damn, I think I left it in my other jacket.",
                    "I don't have any."
                };
                Action<UIMenu, UIMenuItem> func = (sender, selectedItem) =>
                {
                    Game.DisplaySubtitle(ExcuseList.RandomElement(), 4500);
                };
                Item.Activated += new ItemActivatedEvent(func);
            }
            ConversationMenu.AddItem(Item);
        }

    }
}
