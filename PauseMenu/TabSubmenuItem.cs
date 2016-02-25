﻿using System.Collections.Generic;
using System.Drawing;
using GTA;
using GTA.Native;

namespace NativeUI.PauseMenu
{
    public class TabSubmenuItem : TabItem
    {
        public TabSubmenuItem(string name, IEnumerable<TabItem> items) : base(name)
        {
            DrawBg = false;
            CanBeFocused = true;
            Items = new List<TabItem>(items);
            IsInList = true;
        }

        public void RefreshIndex()
        {
            foreach (var item in Items)
            {
                item.Focused = false;
                item.Active = false;
                item.Visible = false;
            }
            Index = (1000 - (1000 % Items.Count)) % Items.Count;
        }

        public List<TabItem> Items { get; set; }
        public int Index { get; set; }
        public bool IsInList { get; set; }

        public void ProcessControls()
        {
            if (JustOpened)
            {
                JustOpened = false;
                return;
            }

            if (!Focused) return;

            if (Game.IsControlJustPressed(0, Control.PhoneSelect) && Focused && Parent.FocusLevel == 1)
            {
                Function.Call(Hash.PLAY_SOUND_FRONTEND, -1, "SELECT", "HUD_FRONTEND_DEFAULT_SOUNDSET", 1);
                if (Items[Index].CanBeFocused && !Items[Index].Focused)
                {
                    Parent.FocusLevel++;
                    Items[Index].JustOpened = true;
                    Items[Index].Focused = true;
                }
                else
                {
                    Items[Index].OnActivated();
                }
            }

            if (Game.IsControlJustPressed(0, Control.PhoneCancel) && Focused && Parent.FocusLevel > 1)
            {
                Function.Call(Hash.PLAY_SOUND_FRONTEND, -1, "CANCEL", "HUD_FRONTEND_DEFAULT_SOUNDSET", 1);
                if (Items[Index].CanBeFocused && Items[Index].Focused)
                {
                    Parent.FocusLevel--;
                    Items[Index].Focused = false;
                }
            }

            if ((Game.IsControlJustPressed(0, Control.FrontendUp) || Game.IsControlJustPressed(0, Control.MoveUpOnly) || Game.IsControlJustPressed(0, Control.CursorScrollUp)) && Parent.FocusLevel == 1)
            {
                Index = (1000 - (1000 % Items.Count) + Index - 1) % Items.Count;
                Function.Call(Hash.PLAY_SOUND_FRONTEND, -1, "NAV_UP_DOWN", "HUD_FRONTEND_DEFAULT_SOUNDSET", 1);
            }
            else if ((Game.IsControlJustPressed(0, Control.FrontendDown) || Game.IsControlJustPressed(0, Control.MoveDownOnly) || Game.IsControlJustPressed(0, Control.CursorScrollDown)) && Parent.FocusLevel == 1)
            {
                Index = (1000 - (1000 % Items.Count) + Index + 1) % Items.Count;
                Function.Call(Hash.PLAY_SOUND_FRONTEND, -1, "NAV_UP_DOWN", "HUD_FRONTEND_DEFAULT_SOUNDSET", 1);
            }
        }

        public override void Draw()
        {
            if (!Visible) return;
            base.Draw();

            ProcessControls();

            var res = UIMenu.GetScreenResolutionMantainRatio();

            var alpha = Focused ? 120 : 30;
            var blackAlpha = Focused ? 200 : 100;
            var fullAlpha = Focused ? 255 : 150;

            var activeWidth = res.Width - SafeSize.X * 2;
            var submenuWidth = (int)(activeWidth * 0.6818f);
            var itemSize = new Size((int)activeWidth - (submenuWidth + 3), 40);

            for (int i = 0; i < Items.Count; i++)
            {
                var hovering = UIMenu.IsMouseInBounds(SafeSize.AddPoints(new Point(0, (itemSize.Height + 3)*i)),
                    itemSize);

                new UIResRectangle(SafeSize.AddPoints(new Point(0, (itemSize.Height + 3) * i)), itemSize, (Index == i && Focused) ? Color.FromArgb(fullAlpha, Color.White) : hovering && Focused ? Color.FromArgb(100, 50, 50, 50) : Color.FromArgb(blackAlpha, Color.Black)).Draw();
                new UIResText(Items[i].Title, SafeSize.AddPoints(new Point(6, 5 + (itemSize.Height + 3) * i)), 0.35f, Color.FromArgb(fullAlpha, (Index == i && Focused) ? Color.Black : Color.White)).Draw();

                if (Focused && hovering && Game.IsControlJustPressed(0, Control.CursorAccept))
                {
                    Items[Index].Focused = false;
                    Function.Call(Hash.PLAY_SOUND_FRONTEND, -1, "NAV_UP_DOWN", "HUD_FRONTEND_DEFAULT_SOUNDSET", 1);
                    bool open = Index == i;
                    Index = (1000 - (1000 % Items.Count) + i) % Items.Count;
                    if (open)
                    {
                        if (Items[Index].CanBeFocused && !Items[Index].Focused)
                        {
                            Parent.FocusLevel = 2;
                            Items[Index].JustOpened = true;
                            Items[Index].Focused = true;
                        }
                        else
                        {
                            Items[Index].OnActivated();
                        }
                    }
                    else
                    {
                        Parent.FocusLevel = 1;
                    }
                }
            }

            Items[Index].Visible = true;
            Items[Index].FadeInWhenFocused = true;
            //Items[Index].CanBeFocused = true;
            if (!Items[Index].CanBeFocused)
                Items[Index].Focused = true;
            Items[Index].UseDynamicPositionment = false;
            Items[Index].SafeSize = SafeSize.AddPoints(new Point((int)activeWidth - submenuWidth, 0));
            Items[Index].TopLeft = SafeSize.AddPoints(new Point((int)activeWidth - submenuWidth, 0));
            Items[Index].BottomRight = new Point((int)res.Width - SafeSize.X, (int)res.Height - SafeSize.Y);
            Items[Index].Draw();
        }
    }
}