namespace Swiss
{
    using System;
    using System.Windows.Forms;

    public class MruStripMenuInline : MruStripMenu
    {
        protected ToolStripMenuItem firstMenuItem;
        protected ToolStripMenuItem owningMenu;

        public MruStripMenuInline(ToolStripMenuItem owningMenu, ToolStripMenuItem recentFileMenuItem, MruStripMenu.ClickedHandler clickedHandler) : this(owningMenu, recentFileMenuItem, clickedHandler, null, false, 4)
        {
        }

        public MruStripMenuInline(ToolStripMenuItem owningMenu, ToolStripMenuItem recentFileMenuItem, MruStripMenu.ClickedHandler clickedHandler, int maxEntries) : this(owningMenu, recentFileMenuItem, clickedHandler, null, false, maxEntries)
        {
        }

        public MruStripMenuInline(ToolStripMenuItem owningMenu, ToolStripMenuItem recentFileMenuItem, MruStripMenu.ClickedHandler clickedHandler, string registryKeyName) : this(owningMenu, recentFileMenuItem, clickedHandler, registryKeyName, true, 4)
        {
        }

        public MruStripMenuInline(ToolStripMenuItem owningMenu, ToolStripMenuItem recentFileMenuItem, MruStripMenu.ClickedHandler clickedHandler, string registryKeyName, bool loadFromRegistry) : this(owningMenu, recentFileMenuItem, clickedHandler, registryKeyName, loadFromRegistry, 4)
        {
        }

        public MruStripMenuInline(ToolStripMenuItem owningMenu, ToolStripMenuItem recentFileMenuItem, MruStripMenu.ClickedHandler clickedHandler, string registryKeyName, int maxEntries) : this(owningMenu, recentFileMenuItem, clickedHandler, registryKeyName, true, maxEntries)
        {
        }

        public MruStripMenuInline(ToolStripMenuItem owningMenu, ToolStripMenuItem recentFileMenuItem, MruStripMenu.ClickedHandler clickedHandler, string registryKeyName, bool loadFromRegistry, int maxEntries)
        {
            base.maxShortenPathLength = 0x30;
            this.owningMenu = owningMenu;
            this.firstMenuItem = recentFileMenuItem;
            base.Init(recentFileMenuItem, clickedHandler, registryKeyName, loadFromRegistry, maxEntries);
        }

        protected override void Disable()
        {
            int index = this.MenuItems.IndexOf(this.firstMenuItem);
            this.MenuItems.RemoveAt(index);
            this.MenuItems.Insert(index, base.recentFileMenuItem);
            this.firstMenuItem = base.recentFileMenuItem;
        }

        protected override void Enable()
        {
            this.MenuItems.Remove(base.recentFileMenuItem);
        }

        public override void RemoveAll()
        {
            if (base.numEntries > 0)
            {
                for (int i = this.EndIndex - 1; i > this.StartIndex; i--)
                {
                    this.MenuItems.RemoveAt(i);
                }
                this.Disable();
                base.numEntries = 0;
            }
        }

        protected override void SetFirstFile(MruStripMenu.MruMenuItem menuItem)
        {
            this.firstMenuItem = menuItem;
        }

        public override int EndIndex =>
            (this.StartIndex + base.numEntries);

        public override bool IsInline =>
            true;

        public override ToolStripItemCollection MenuItems =>
            this.owningMenu.DropDownItems;

        public override int StartIndex =>
            this.MenuItems.IndexOf(this.firstMenuItem);
    }
}

