namespace Swiss
{
    using Microsoft.Win32;
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows.Forms;

    public class MruStripMenu
    {
        private ClickedHandler clickedHandler;
        protected int maxEntries;
        protected int maxShortenPathLength;
        protected Mutex mruStripMutex;
        protected int numEntries;
        protected ToolStripMenuItem recentFileMenuItem;
        protected string registryKeyName;

        protected MruStripMenu()
        {
            this.maxEntries = 4;
            this.maxShortenPathLength = 0x60;
            this.numEntries = 0;
        }

        public MruStripMenu(ToolStripMenuItem recentFileMenuItem, ClickedHandler clickedHandler) : this(recentFileMenuItem, clickedHandler, null, false, 4)
        {
        }

        public MruStripMenu(ToolStripMenuItem recentFileMenuItem, ClickedHandler clickedHandler, int maxEntries) : this(recentFileMenuItem, clickedHandler, null, false, maxEntries)
        {
        }

        public MruStripMenu(ToolStripMenuItem recentFileMenuItem, ClickedHandler clickedHandler, string registryKeyName) : this(recentFileMenuItem, clickedHandler, registryKeyName, true, 4)
        {
        }

        public MruStripMenu(ToolStripMenuItem recentFileMenuItem, ClickedHandler clickedHandler, string registryKeyName, bool loadFromRegistry) : this(recentFileMenuItem, clickedHandler, registryKeyName, loadFromRegistry, 4)
        {
        }

        public MruStripMenu(ToolStripMenuItem recentFileMenuItem, ClickedHandler clickedHandler, string registryKeyName, int maxEntries) : this(recentFileMenuItem, clickedHandler, registryKeyName, true, maxEntries)
        {
        }

        public MruStripMenu(ToolStripMenuItem recentFileMenuItem, ClickedHandler clickedHandler, string registryKeyName, bool loadFromRegistry, int maxEntries)
        {
            this.maxEntries = 4;
            this.maxShortenPathLength = 0x60;
            this.numEntries = 0;
            this.Init(recentFileMenuItem, clickedHandler, registryKeyName, loadFromRegistry, maxEntries);
        }

        public void AddFile(string filename)
        {
            string fullPath = Path.GetFullPath(filename);
            this.AddFile(fullPath, ShortenPathname(fullPath, this.MaxShortenPathLength));
        }

        public void AddFile(string filename, string entryname)
        {
            MruMenuItem item;
            if (filename == null)
            {
                throw new ArgumentNullException("filename");
            }
            if (filename.Length == 0)
            {
                throw new ArgumentException("filename");
            }
            if (this.numEntries > 0)
            {
                int num = this.FindFilenameMenuIndex(filename);
                if (num >= 0)
                {
                    this.SetFirstFile((int) (num - this.StartIndex));
                    return;
                }
            }
            if (this.numEntries < this.maxEntries)
            {
                item = new MruMenuItem(filename, FixupEntryname(0, entryname), new EventHandler(this.OnClick));
                if (this.StartIndex == -1)
                {
                    this.MenuItems.Insert(0, item);
                }
                else
                {
                    this.MenuItems.Insert(this.StartIndex, item);
                }
                this.SetFirstFile(item);
                if (this.numEntries++ == 0)
                {
                    this.Enable();
                }
                else
                {
                    this.FixupPrefixes(1);
                }
            }
            else if (this.numEntries > 1)
            {
                item = (MruMenuItem) this.MenuItems[(this.StartIndex + this.numEntries) - 1];
                this.MenuItems.RemoveAt((this.StartIndex + this.numEntries) - 1);
                item.Text = FixupEntryname(0, entryname);
                item.Filename = filename;
                this.MenuItems.Insert(this.StartIndex, item);
                this.SetFirstFile(item);
                this.FixupPrefixes(1);
            }
        }

        public void AddFiles(string[] filenames)
        {
            for (int i = filenames.GetLength(0) - 1; i >= 0; i--)
            {
                this.AddFile(filenames[i]);
            }
        }

        protected virtual void Disable()
        {
            this.recentFileMenuItem.Enabled = false;
        }

        protected virtual void Enable()
        {
            this.recentFileMenuItem.Enabled = true;
        }

        public int FindFilenameMenuIndex(string filename)
        {
            int num = this.FindFilenameNumber(filename);
            return ((num < 0) ? -1 : (this.StartIndex + num));
        }

        public int FindFilenameNumber(string filename)
        {
            if (filename == null)
            {
                throw new ArgumentNullException("filename");
            }
            if (filename.Length == 0)
            {
                throw new ArgumentException("filename");
            }
            if (this.numEntries > 0)
            {
                int num = 0;
                int startIndex = this.StartIndex;
                while (startIndex < this.EndIndex)
                {
                    if (string.Compare(((MruMenuItem) this.MenuItems[startIndex]).Filename, filename, true) == 0)
                    {
                        return num;
                    }
                    startIndex++;
                    num++;
                }
            }
            return -1;
        }

        public static string FixupEntryname(int number, string entryname)
        {
            if (number < 9)
            {
                return string.Concat(new object[] { "&", number + 1, "  ", entryname });
            }
            if (number == 9)
            {
                return ("1&0  " + entryname);
            }
            return ((number + 1) + "  " + entryname);
        }

        protected void FixupPrefixes(int startNumber)
        {
            if (startNumber < 0)
            {
                startNumber = 0;
            }
            if (startNumber < this.maxEntries)
            {
                int num = this.StartIndex + startNumber;
                while (num < this.EndIndex)
                {
                    int startIndex = (this.MenuItems[num].Text.Substring(0, 3) == "1&0") ? 5 : 4;
                    this.MenuItems[num].Text = FixupEntryname(startNumber, this.MenuItems[num].Text.Substring(startIndex));
                    num++;
                    startNumber++;
                }
            }
        }

        public string GetFileAt(int number)
        {
            if ((number < 0) || (number >= this.numEntries))
            {
                throw new ArgumentOutOfRangeException("number");
            }
            return ((MruMenuItem) this.MenuItems[this.StartIndex + number]).Filename;
        }

        public string[] GetFiles()
        {
            string[] strArray = new string[this.numEntries];
            int startIndex = this.StartIndex;
            int index = 0;
            while (index < strArray.GetLength(0))
            {
                strArray[index] = ((MruMenuItem) this.MenuItems[startIndex]).Filename;
                index++;
                startIndex++;
            }
            return strArray;
        }

        public string[] GetFilesFullEntrystring()
        {
            string[] strArray = new string[this.numEntries];
            int startIndex = this.StartIndex;
            int index = 0;
            while (index < strArray.GetLength(0))
            {
                strArray[index] = this.MenuItems[startIndex].Text;
                index++;
                startIndex++;
            }
            return strArray;
        }

        public int GetMenuIndex(int number)
        {
            if ((number < 0) || (number >= this.numEntries))
            {
                throw new ArgumentOutOfRangeException("number");
            }
            return (this.StartIndex + number);
        }

        protected void Init(ToolStripMenuItem recentFileMenuItem, ClickedHandler clickedHandler, string registryKeyName, bool loadFromRegistry, int maxEntries)
        {
            if (recentFileMenuItem == null)
            {
                throw new ArgumentNullException("recentFileMenuItem");
            }
            this.recentFileMenuItem = recentFileMenuItem;
            this.recentFileMenuItem.Checked = false;
            this.recentFileMenuItem.Enabled = false;
            this.MaxEntries = maxEntries;
            this.clickedHandler = clickedHandler;
            if (registryKeyName != null)
            {
                this.RegistryKeyName = registryKeyName;
                if (loadFromRegistry)
                {
                    this.LoadFromRegistry();
                }
            }
        }

        public void LoadFromRegistry()
        {
            if (this.registryKeyName != null)
            {
                this.mruStripMutex.WaitOne();
                this.RemoveAll();
                RegistryKey key = Registry.CurrentUser.OpenSubKey(this.registryKeyName);
                if (key != null)
                {
                    this.maxEntries = (int) key.GetValue("max", this.maxEntries);
                    for (int i = this.maxEntries; i > 0; i--)
                    {
                        string filename = (string) key.GetValue("File" + i.ToString());
                        if (filename != null)
                        {
                            this.AddFile(filename);
                        }
                    }
                    key.Close();
                }
                this.mruStripMutex.ReleaseMutex();
            }
        }

        public void LoadFromRegistry(string keyName)
        {
            this.RegistryKeyName = keyName;
            this.LoadFromRegistry();
        }

        protected void OnClick(object sender, EventArgs e)
        {
            MruMenuItem item = (MruMenuItem) sender;
            this.clickedHandler(this.MenuItems.IndexOf(item) - this.StartIndex, item.Filename);
        }

        public virtual void RemoveAll()
        {
            if (this.numEntries > 0)
            {
                this.MenuItems.Clear();
                this.Disable();
                this.numEntries = 0;
            }
        }

        public void RemoveFile(int number)
        {
            if ((number >= 0) && (number < this.numEntries))
            {
                if (--this.numEntries == 0)
                {
                    this.Disable();
                }
                else
                {
                    int startIndex = this.StartIndex;
                    if (number == 0)
                    {
                        this.SetFirstFile((MruMenuItem) this.MenuItems[startIndex + 1]);
                    }
                    this.MenuItems.RemoveAt(startIndex + number);
                    if (number < this.numEntries)
                    {
                        this.FixupPrefixes(number);
                    }
                }
            }
        }

        public void RemoveFile(string filename)
        {
            if (this.numEntries > 0)
            {
                this.RemoveFile(this.FindFilenameNumber(filename));
            }
        }

        public void RenameFile(string oldFilename, string newFilename)
        {
            string fullPath = Path.GetFullPath(newFilename);
            this.RenameFile(Path.GetFullPath(oldFilename), fullPath, ShortenPathname(fullPath, this.MaxShortenPathLength));
        }

        public void RenameFile(string oldFilename, string newFilename, string newEntryname)
        {
            if (newFilename == null)
            {
                throw new ArgumentNullException("newFilename");
            }
            if (newFilename.Length == 0)
            {
                throw new ArgumentException("newFilename");
            }
            if (this.numEntries > 0)
            {
                int num = this.FindFilenameMenuIndex(oldFilename);
                if (num >= 0)
                {
                    MruMenuItem item = (MruMenuItem) this.MenuItems[num];
                    item.Text = FixupEntryname(0, newEntryname);
                    item.Filename = newFilename;
                    return;
                }
            }
            this.AddFile(newFilename, newEntryname);
        }

        public void SaveToRegistry()
        {
            if (this.registryKeyName != null)
            {
                this.mruStripMutex.WaitOne();
                RegistryKey key = Registry.CurrentUser.CreateSubKey(this.registryKeyName);
                if (key != null)
                {
                    key.SetValue("max", this.maxEntries);
                    int num = 1;
                    int startIndex = this.StartIndex;
                    while (startIndex < this.EndIndex)
                    {
                        key.SetValue("File" + num.ToString(), ((MruMenuItem) this.MenuItems[startIndex]).Filename);
                        startIndex++;
                        num++;
                    }
                    while (num <= 0x10)
                    {
                        key.DeleteValue("File" + num.ToString(), false);
                        num++;
                    }
                    key.Close();
                }
                this.mruStripMutex.ReleaseMutex();
            }
        }

        public void SaveToRegistry(string keyName)
        {
            this.RegistryKeyName = keyName;
            this.SaveToRegistry();
        }

        public void SetFiles(string[] filenames)
        {
            this.RemoveAll();
            for (int i = filenames.GetLength(0) - 1; i >= 0; i--)
            {
                this.AddFile(filenames[i]);
            }
        }

        protected virtual void SetFirstFile(MruMenuItem menuItem)
        {
        }

        public void SetFirstFile(int number)
        {
            if (((number > 0) && (this.numEntries > 1)) && (number < this.numEntries))
            {
                MruMenuItem item = (MruMenuItem) this.MenuItems[this.StartIndex + number];
                this.MenuItems.RemoveAt(this.StartIndex + number);
                this.MenuItems.Insert(this.StartIndex, item);
                this.SetFirstFile(item);
                this.FixupPrefixes(0);
            }
        }

        public static string ShortenPathname(string pathname, int maxLength)
        {
            int length;
            int num4;
            if (pathname.Length <= maxLength)
            {
                return pathname;
            }
            string pathRoot = Path.GetPathRoot(pathname);
            if (pathRoot.Length > 3)
            {
                pathRoot = pathRoot + Path.DirectorySeparatorChar;
            }
            string[] strArray = pathname.Substring(pathRoot.Length).Split(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });
            int index = strArray.GetLength(0) - 1;
            if (strArray.GetLength(0) == 1)
            {
                if (strArray[0].Length > 5)
                {
                    if ((pathRoot.Length + 6) >= maxLength)
                    {
                        return (pathRoot + strArray[0].Substring(0, 3) + "...");
                    }
                    return (pathname.Substring(0, maxLength - 3) + "...");
                }
                return pathname;
            }
            if (((pathRoot.Length + 4) + strArray[index].Length) > maxLength)
            {
                pathRoot = pathRoot + @"...\";
                if (strArray[index].Length < 6)
                {
                    return (pathRoot + strArray[index]);
                }
                if ((pathRoot.Length + 6) >= maxLength)
                {
                    length = 3;
                }
                else
                {
                    length = (maxLength - pathRoot.Length) - 3;
                }
                return (pathRoot + strArray[index].Substring(0, length) + "...");
            }
            if (strArray.GetLength(0) == 2)
            {
                return (pathRoot + @"...\" + strArray[1]);
            }
            length = 0;
            int num3 = 0;
            for (num4 = 0; num4 < index; num4++)
            {
                if (strArray[num4].Length > length)
                {
                    num3 = num4;
                    length = strArray[num4].Length;
                }
            }
            int num5 = (pathname.Length - length) + 3;
            int num6 = num3 + 1;
            while (num5 > maxLength)
            {
                if (num3 > 0)
                {
                    num5 -= strArray[--num3].Length - 1;
                }
                if (num5 <= maxLength)
                {
                    break;
                }
                if (num6 < index)
                {
                    num5 -= strArray[++num6].Length - 1;
                }
                if ((num3 == 0) && (num6 == index))
                {
                    break;
                }
            }
            num4 = 0;
            while (num4 < num3)
            {
                pathRoot = pathRoot + strArray[num4] + '\\';
                num4++;
            }
            pathRoot = pathRoot + @"...\";
            for (num4 = num6; num4 < index; num4++)
            {
                pathRoot = pathRoot + strArray[num4] + '\\';
            }
            return (pathRoot + strArray[index]);
        }

        public virtual int EndIndex =>
            this.numEntries;

        public virtual bool IsInline =>
            false;

        public int MaxEntries
        {
            get => 
                this.maxEntries;
            set
            {
                if (value > 0x10)
                {
                    this.maxEntries = 0x10;
                }
                else
                {
                    this.maxEntries = (value < 4) ? 4 : value;
                    int index = this.StartIndex + this.maxEntries;
                    while (this.numEntries > this.maxEntries)
                    {
                        this.MenuItems.RemoveAt(index);
                        this.numEntries--;
                    }
                }
            }
        }

        public int MaxShortenPathLength
        {
            get => 
                this.maxShortenPathLength;
            set
            {
                this.maxShortenPathLength = (value < 0x10) ? 0x10 : value;
            }
        }

        public virtual ToolStripItemCollection MenuItems =>
            this.recentFileMenuItem.DropDownItems;

        public int NumEntries =>
            this.numEntries;

        public string RegistryKeyName
        {
            get => 
                this.registryKeyName;
            set
            {
                if (this.mruStripMutex != null)
                {
                    this.mruStripMutex.Close();
                }
                this.registryKeyName = value.Trim();
                if (this.registryKeyName.Length == 0)
                {
                    this.registryKeyName = null;
                    this.mruStripMutex = null;
                }
                else
                {
                    string name = this.registryKeyName.Replace('\\', '_').Replace('/', '_') + "Mutex";
                    this.mruStripMutex = new Mutex(false, name);
                }
            }
        }

        public virtual int StartIndex =>
            0;

        public delegate void ClickedHandler(int number, string filename);

        public class MruMenuItem : ToolStripMenuItem
        {
            public MruMenuItem()
            {
                base.Tag = "";
            }

            public MruMenuItem(string filename, string entryname, EventHandler eventHandler)
            {
                base.Tag = filename;
                this.Text = entryname;
                base.Click += eventHandler;
            }

            public string Filename
            {
                get => 
                    ((string) base.Tag);
                set
                {
                    base.Tag = value;
                }
            }
        }
    }
}

