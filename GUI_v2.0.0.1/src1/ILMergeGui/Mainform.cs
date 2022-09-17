namespace ILMergeGui
{
    using ILMergeGui.Properties;
    using Microsoft.Win32;
    using Swiss;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows.Forms;
    using System.Xml.Linq;

    public class Mainform : Form
    {
        private ToolStripMenuItem aboutToolStripMenuItem;
        private List<string> arrAssemblies;
        private bool AutoClose;
        internal GroupBox BoxOptions;
        internal GroupBox BoxOutput;
        internal Button btnAddFile;
        internal Button btnKeyFile;
        internal Button btnLogFile;
        internal Button btnMerge;
        internal Button btnOutputPath;
        internal ComboBox CboDebug;
        internal ComboBox CboTargetFramework;
        private ToolStripMenuItem checkForUpdatesToolStripMenuItem;
        internal CheckBox ChkCopyAttributes;
        internal CheckBox ChkDelayedSign;
        internal CheckBox ChkGenCmdLine;
        internal CheckBox ChkGenerateLog;
        internal CheckBox ChkInternalize;
        internal CheckBox ChkMergeXml;
        internal CheckBox ChkSignKeyFile;
        internal CheckBox ChkUnionDuplicates;
        private const uint CLR_NONE = uint.MaxValue;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
        private IContainer components;
        private int ExitCode;
        private string ExitMsg;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem fileToolStripMenuItem1;
        private ToolStripMenuItem fileToolStripMenuItem2;
        private ToolStripMenuItem fileToolStripMenuItem3;
        private string frameversion;
        private DotNet framework;
        private List<DotNet> frameworks;
        private GroupBox groupBox1;
        private Label label1;
        private Label label2;
        internal Label LblDebug;
        internal Label LblOutputPath;
        internal Label LblPrimaryAssembly;
        internal Label LblPrimaryAssemblyInfo;
        internal Label LblTargetFramework;
        internal LinkLabel LinkILMerge;
        internal LinkLabel linkLabel1;
        private ListView ListAssembly;
        private ToolStripMenuItem menuRecentFile;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem mnuFileExit;
        private ToolStripMenuItem mnuFileNew;
        private ToolStripMenuItem mnuFileOpen;
        private ToolStripMenuItem mnuFileSave;
        private MruStripMenu mruMenu;
        private string mruRegKey = (@"SOFTWARE\" + Application.CompanyName + @"\ " + AppTitle);
        private const string MyAppName = "ILMergeGui";
        private const string MyExtension = ".ilproj";
        private const string MyWildcard = "*.ilproj";
        private ToolStripMenuItem newToolStripMenuItem;
        private ToolStripMenuItem newToolStripMenuItem1;
        private ToolStripMenuItem newToolStripMenuItem2;
        internal OpenFileDialog openFile1;
        private OpenFileDialog openFileDialog1;
        private ToolStripMenuItem openToolStripMenuItem;
        private ToolStripMenuItem openToolStripMenuItem1;
        private ToolStripMenuItem openToolStripMenuItem2;
        private RadioButton radioButton1;
        private RadioButton radioButton2;
        private SaveFileDialog saveFileDialog1;
        private ToolStripMenuItem saveToolStripMenuItem;
        private ToolStripMenuItem saveToolStripMenuItem1;
        private ToolStripMenuItem toolStripMenuItem1;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripSeparator toolStripSeparator6;
        private ToolStripSeparator toolStripSeparator7;
        private ToolStripSeparator toolStripSeparator8;
        private ToolStripSeparator toolStripSeparator9;
        internal ToolTip ToolTips;
        internal TextBox TxtKeyFile;
        internal TextBox TxtLogFile;
        internal TextBox TxtOutputAssembly;
        private ToolStripMenuItem visitWebsiteToolStripMenuItem;
        internal BackgroundWorker WorkerILMerge;
        private ToolStripMenuItem xToolStripMenuItem;

        public Mainform()
        {
            this.InitializeComponent();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AboutDialog().ShowDialog();
        }

        private void btnAddFile_Click(object sender, EventArgs e)
        {
            this.openFile1.CheckFileExists = true;
            this.openFile1.Multiselect = true;
            this.openFile1.FileName = string.Empty;
            this.openFile1.Filter = ".NET Assembly|*.dll;*.exe";
            if (this.openFile1.ShowDialog() == DialogResult.OK)
            {
                this.ProcessFiles(this.openFile1.FileNames);
            }
        }

        private void btnKeyFile_Click(object sender, EventArgs e)
        {
            this.SelectKeyFile();
        }

        private void btnLogFile_Click(object sender, EventArgs e)
        {
            this.SelectLogFile();
        }

        private void btnMerge_Click(object sender, EventArgs e)
        {
            if ((this.Engine != Merger.None) && DynaInvoke.PreLoadAssembly(this.iLMergePath, this.ilMerge, null))
            {
                this.arrAssemblies = new List<string>();
                this.PreMerge();
                if (!string.IsNullOrWhiteSpace(this.TxtOutputAssembly.Text) && !Directory.Exists(Path.GetDirectoryName(this.TxtOutputAssembly.Text.Trim())))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(this.TxtOutputAssembly.Text.Trim()));
                }
                if (string.IsNullOrWhiteSpace(this.TxtOutputAssembly.Text) || !Directory.Exists(Path.GetDirectoryName(this.TxtOutputAssembly.Text.Trim())))
                {
                    this.ExitMsg = Resources.Error_NoOutputPath;
                    this.ExitCode = 8;
                    if (!this.AutoClose)
                    {
                        MessageBox.Show(this.ExitMsg, Resources.Error_Term, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    }
                    else
                    {
                        this.ExitILMergeGUI();
                    }
                    this.TxtOutputAssembly.Focus();
                }
                else
                {
                    this.TxtOutputAssembly.Text = this.TxtOutputAssembly.Text.Trim();
                    if (File.Exists(this.TxtKeyFile.Text) && !File.Exists(this.TxtKeyFile.Text))
                    {
                        this.ExitMsg = Resources.Error_KeyFileNotExists;
                        this.ExitCode = 7;
                        if (!this.AutoClose)
                        {
                            MessageBox.Show(this.ExitMsg, Resources.Error_Term, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                        }
                        else
                        {
                            this.ExitILMergeGUI();
                        }
                    }
                    else
                    {
                        for (int i = 0; i < this.ListAssembly.Items.Count; i++)
                        {
                            if (((string) this.ListAssembly.Items[i].Tag).ToLower().Equals(this.TxtOutputAssembly.Text.ToLower()))
                            {
                                this.ExitMsg = Resources.Error_OutputConflict;
                                this.ExitCode = 6;
                                if (!this.AutoClose)
                                {
                                    MessageBox.Show(this.ExitMsg, Resources.Error_Term, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                                }
                                else
                                {
                                    this.ExitILMergeGUI();
                                }
                                this.TxtOutputAssembly.Focus();
                                return;
                            }
                        }
                        if (File.Exists(this.TxtOutputAssembly.Text))
                        {
                            try
                            {
                                new FileInfo(this.TxtOutputAssembly.Text) { Attributes = FileAttributes.Normal }.Delete();
                            }
                            catch (Exception)
                            {
                                this.ExitMsg = Resources.Error_OutputPathInUse;
                                this.ExitCode = 5;
                                if (!this.AutoClose)
                                {
                                    MessageBox.Show(this.ExitMsg, Resources.Error_Term, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                                }
                                else
                                {
                                    this.ExitILMergeGUI();
                                }
                                return;
                            }
                        }
                        if (this.ListAssembly.SelectedItems.Count == 0)
                        {
                            for (int k = 0; k < this.ListAssembly.Items.Count; k++)
                            {
                                if (Path.GetExtension((string) this.ListAssembly.Items[k].Tag).ToLower() == ".exe")
                                {
                                    this.ListAssembly.Items[k].Selected = true;
                                    break;
                                }
                            }
                        }
                        if (this.ListAssembly.SelectedItems.Count == 0)
                        {
                            this.ListAssembly.Items[0].Selected = true;
                        }
                        this.arrAssemblies.Add(this.Primary);
                        for (int j = 0; j < this.ListAssembly.Items.Count; j++)
                        {
                            if (!this.arrAssemblies.Contains((string) this.ListAssembly.Items[j].Tag))
                            {
                                this.arrAssemblies.Add((string) this.ListAssembly.Items[j].Tag);
                            }
                        }
                        Console.WriteLine("{0}.{1}={1}", this.ilMerge, "CopyAttributes", DynaInvoke.SetProperty<bool>(this.iLMergePath, this.ilMerge, "CopyAttributes", this.ChkCopyAttributes.Checked, null));
                        Console.WriteLine("{0}.{1}={1}", this.ilMerge, "UnionMerge", DynaInvoke.SetProperty<bool>(this.iLMergePath, this.ilMerge, "UnionMerge", this.ChkUnionDuplicates.Checked, null));
                        Console.WriteLine("{0}.{1}={1}", this.ilMerge, "Internalize", DynaInvoke.SetProperty<bool>(this.iLMergePath, this.ilMerge, "Internalize", this.ChkInternalize.Checked, null));
                        Console.WriteLine("{0}.{1}={1}", this.ilMerge, "XmlDocumentation", DynaInvoke.SetProperty<bool>(this.iLMergePath, this.ilMerge, "XmlDocumentation", this.ChkMergeXml.Checked, null));
                        if (this.ChkSignKeyFile.Checked)
                        {
                            Console.WriteLine("{0}.{1}={2}", this.ilMerge, "KeyFile", DynaInvoke.SetProperty<string>(this.iLMergePath, this.ilMerge, "KeyFile", this.TxtKeyFile.Text, null));
                            Console.WriteLine("{0}.{1}={2}", this.ilMerge, "DelaySign", DynaInvoke.SetProperty<bool>(this.iLMergePath, this.ilMerge, "DelaySign", this.ChkDelayedSign.Checked, null));
                        }
                        if (this.ChkGenerateLog.Checked)
                        {
                            Console.WriteLine("{0}.{1}={2}", this.ilMerge, "Log", DynaInvoke.SetProperty<bool>(this.iLMergePath, this.ilMerge, "Log", this.ChkGenerateLog.Checked, null).ToString());
                            Console.WriteLine("{0}.{1}={2}", this.ilMerge, "LogFile", DynaInvoke.SetProperty<string>(this.iLMergePath, this.ilMerge, "LogFile", this.TxtLogFile.Text, null));
                        }
                        Console.WriteLine("{0}.{1}={2}", this.ilMerge, "DebugInfo", DynaInvoke.SetProperty<bool>(this.iLMergePath, this.ilMerge, "DebugInfo", this.CboDebug.SelectedIndex == 0, null));
                        this.framework = (DotNet) this.CboTargetFramework.SelectedItem;
                        this.frameversion = $"{this.framework.version.Major}.{this.framework.version.Minor}";
                        if (((this.Engine == Merger.ILMerge) && (this.framework.version.Major == 4)) && (this.framework.version.Minor >= 5))
                        {
                            this.frameversion = "4.0";
                        }
                        string frameversion = this.frameversion;
                        if (this.Engine == Merger.ILRepack)
                        {
                            this.frameversion = null;
                        }
                        try
                        {
                            object[] objArray;
                            if (Environment.Is64BitOperatingSystem)
                            {
                                object[] arg = new object[] { this.ilMerge, "SetTargetPlatform", this.frameversion, this.framework.x64WindowsPath };
                                Console.WriteLine("{0}.{1}('{2}', '{3}')", arg);
                                string[] textArray1 = new string[] { this.frameversion, this.framework.x64WindowsPath };
                                objArray = textArray1;
                                DynaInvoke.CallMethod<object>(this.iLMergePath, this.ilMerge, "SetTargetPlatform", objArray, null);
                            }
                            else
                            {
                                object[] objArray2 = new object[] { this.ilMerge, "SetTargetPlatform", this.frameversion, this.framework.x86WindowsPath };
                                Console.WriteLine("{0}.{1}('{2}', '{3}')", objArray2);
                                string[] textArray2 = new string[] { this.frameversion, this.framework.x86WindowsPath };
                                objArray = textArray2;
                                DynaInvoke.CallMethod<object>(this.iLMergePath, this.ilMerge, "SetTargetPlatform", objArray, null);
                            }
                        }
                        catch (TargetInvocationException)
                        {
                            this.ExitMsg = string.Format(Resources.Error_Framework, frameversion, this.ilMerge);
                            this.ExitCode = 9;
                            if (!this.AutoClose)
                            {
                                MessageBox.Show(this.ExitMsg, Resources.Error_Term, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                            }
                            else
                            {
                                this.ExitILMergeGUI();
                            }
                            return;
                        }
                        switch (this.Engine)
                        {
                            case Merger.ILMerge:
                                Console.WriteLine("{0}.{1}={2}", this.ilMerge, "TargetKind", DynaInvoke.SetProperty<int>(this.iLMergePath, this.ilMerge, "TargetKind", 3, null));
                                break;

                            case Merger.ILRepack:
                                Console.WriteLine("{0}.{1}={2}", this.ilMerge, "TargetKind", DynaInvoke.SetProperty<int?>(this.iLMergePath, this.ilMerge, "TargetKind", 3, null));
                                break;
                        }
                        Console.WriteLine("{0}.{1}={2}", this.ilMerge, "OutputFile", DynaInvoke.SetProperty<string>(this.iLMergePath, this.ilMerge, "OutputFile", this.TxtOutputAssembly.Text, null));
                        Console.WriteLine("{0}.{1}(", this.ilMerge, "SetInputAssemblies");
                        foreach (string str2 in this.arrAssemblies)
                        {
                            Console.WriteLine("                           '{0}'", str2);
                        }
                        Console.WriteLine("                          )");
                        object[] mArgs = new object[] { this.arrAssemblies.ToArray() };
                        DynaInvoke.CallMethod<object>(this.iLMergePath, this.ilMerge, "SetInputAssemblies", mArgs, null);
                        if (this.ChkGenCmdLine.Checked)
                        {
                            string path = Path.Combine(Path.GetDirectoryName(this.TxtOutputAssembly.Text.Trim()), Path.ChangeExtension(Path.GetFileName(this.TxtOutputAssembly.Text.Trim()), ".txt"));
                            File.WriteAllText(path, this.DoGenerateCmdLine());
                            Process.Start(new ProcessStartInfo(path));
                        }
                        this.EnableForm(false);
                        this.WorkerILMerge.RunWorkerAsync();
                    }
                }
            }
        }

        private void btnOutputPath_Click(object sender, EventArgs e)
        {
            this.SelectOutputFile();
        }

        private void CboDebug_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClickOnceUpdater.InstallUpdateSyncWithInfo("http://ilmergegui.codeplex.com/releases/view/latest");
        }

        private void ChkGenerateLog_CheckedChanged(object sender, EventArgs e)
        {
            this.TxtLogFile.Enabled = this.ChkGenerateLog.Checked;
            this.btnLogFile.Enabled = this.ChkGenerateLog.Checked;
        }

        private void ChkSignKeyFile_CheckedChanged(object sender, EventArgs e)
        {
            this.TxtKeyFile.Enabled = this.ChkSignKeyFile.Checked;
            this.ChkDelayedSign.Enabled = this.ChkSignKeyFile.Checked;
            this.btnKeyFile.Enabled = this.ChkSignKeyFile.Checked;
        }

        public static bool DetectIlProj()
        {
            using (RegistryKey key = Registry.ClassesRoot.OpenSubKey(".ilproj", false))
            {
                if (key == null)
                {
                    return false;
                }
                if ((key.GetValue(null) == null) || !key.GetValue(null).Equals("ILMergeGui_file"))
                {
                    return false;
                }
                if ((key.GetValue("Content Type") == null) || !key.GetValue("Content Type").Equals($"application/{"ILMergeGui".ToLower()}"))
                {
                    return false;
                }
                using (RegistryKey key2 = Registry.ClassesRoot.OpenSubKey(@"\ILMergeGui_file", false))
                {
                    if (key2 == null)
                    {
                        return false;
                    }
                    if ((key2.GetValue(null) == null) || !key2.GetValue(null).Equals("ILMergeGui File"))
                    {
                        return false;
                    }
                    if (key2.GetValue("EditFlags") == null)
                    {
                        return false;
                    }
                    if (key2.GetValue("AlwaysShowExt") == null)
                    {
                        return true;
                    }
                    using (RegistryKey key3 = key2.OpenSubKey("DefaultIcon", false))
                    {
                        if (((key3 == null) || (key3.GetValue(null) == null)) || !key3.GetValue(null).ToString().Equals(Application.ExecutablePath + ",0", StringComparison.OrdinalIgnoreCase))
                        {
                            return false;
                        }
                    }
                    using (RegistryKey key4 = key2.OpenSubKey(@"shell\open\command", false))
                    {
                        if (((key4 == null) || (key4.GetValue(null) == null)) || !key4.GetValue(null).ToString().Equals("\"" + Application.ExecutablePath + "\" \"%1\"", StringComparison.OrdinalIgnoreCase))
                        {
                            return false;
                        }
                    }
                    using (RegistryKey key5 = key2.OpenSubKey(@"shell\merge\command", false))
                    {
                        if (((key5 == null) || (key5.GetValue(null) == null)) || !key5.GetValue(null).ToString().Equals("\"" + Application.ExecutablePath + "\" \"%1\" /Merge", StringComparison.OrdinalIgnoreCase))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private string DoGenerateCmdLine()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($""{this.iLMergePath}"");
            if (DynaInvoke.GetProperty<bool>(this.iLMergePath, this.ilMerge, "Log", null))
            {
                if (DynaInvoke.GetProperty<bool>(this.iLMergePath, this.ilMerge, "LogFile", null))
                {
                    builder.AppendLine($"/log:"{DynaInvoke.GetProperty<bool>(this.iLMergePath, this.ilMerge, "LogFile", null)}"");
                }
                else
                {
                    builder.AppendLine("/log");
                }
            }
            if (this.ChkSignKeyFile.Checked)
            {
                builder.AppendLine($"/keyfile:"{DynaInvoke.GetProperty<string>(this.iLMergePath, this.ilMerge, "KeyFile", null)}"");
                if (DynaInvoke.GetProperty<bool>(this.iLMergePath, this.ilMerge, "DelaySign", null))
                {
                    builder.AppendLine("/delaysign");
                }
            }
            if (DynaInvoke.GetProperty<bool>(this.iLMergePath, this.ilMerge, "Internalize", null))
            {
                builder.AppendLine("/internalize");
            }
            switch (3)
            {
                case 0:
                    builder.AppendLine("/target:library");
                    break;

                case 1:
                    builder.AppendLine("/target:exe");
                    break;

                case 2:
                    builder.AppendLine("/target:winexe");
                    break;
            }
            if (!DynaInvoke.GetProperty<bool>(this.iLMergePath, this.ilMerge, "DebugInfo", null))
            {
                builder.AppendLine("/ndebug");
            }
            if (DynaInvoke.GetProperty<bool>(this.iLMergePath, this.ilMerge, "CopyAttributes", null))
            {
                builder.AppendLine("/copyattrs");
            }
            if (DynaInvoke.GetProperty<bool>(this.iLMergePath, this.ilMerge, "XmlDocumentation", null))
            {
                builder.AppendLine("/xmldocs");
            }
            builder.AppendLine($"/targetplatform:{this.frameversion},"{Environment.Is64BitOperatingSystem ? this.framework.x64WindowsPath : this.framework.x86WindowsPath}"");
            if (DynaInvoke.GetProperty<bool>(this.iLMergePath, this.ilMerge, "UnionMerge", null))
            {
                builder.AppendLine("/union");
            }
            builder.AppendLine($"/out:"{DynaInvoke.GetProperty<string>(this.iLMergePath, this.ilMerge, "OutputFile", null)}"");
            builder.AppendLine();
            builder.AppendLine($"     "{this.arrAssemblies.First<string>()}"");
            builder.AppendLine();
            foreach (string str in this.arrAssemblies.Skip<string>(1))
            {
                builder.AppendLine($"     "{str}"");
            }
            return builder.ToString();
        }

        private void EnableForm(bool Enable)
        {
            this.ListAssembly.Enabled = Enable;
            this.btnAddFile.Enabled = Enable;
            this.BoxOptions.Enabled = Enable;
            this.BoxOutput.Enabled = Enable;
            this.btnMerge.Enabled = Enable;
            Application.DoEvents();
        }

        private void ExitILMergeGUI()
        {
            Console.WriteLine(this.ExitMsg);
            Console.WriteLine("ExitCode: {0}", this.ExitCode);
            Environment.Exit(this.ExitCode);
        }

        private void FormatItems()
        {
            foreach (ListViewItem item in this.ListAssembly.Items)
            {
                if (!string.IsNullOrEmpty(this.Primary))
                {
                    item.Text = MakeRelativePath(this.Primary, (string) item.Tag);
                }
                else
                {
                    item.Text = Path.GetFileName((string) item.Tag);
                }
            }
            this.ListAssembly.Columns[0].Width = -1;
        }

        private static void GetDotNetVersion(RegistryKey parentKey, string versionKey, List<DotNet> versions)
        {
            if ((parentKey != null) && (Convert.ToString(parentKey.GetValue("Install")) == "1"))
            {
                DotNet net;
                string str = Convert.ToString(parentKey.GetValue("Version"));
                if (string.IsNullOrEmpty(str))
                {
                    if (versionKey.StartsWith("v"))
                    {
                        str = versionKey.Substring(1);
                    }
                    else
                    {
                        str = versionKey;
                    }
                }
                Version ver = new Version(str);
                string frameworkpath = string.Empty;
                string str3 = string.Empty;
                string str4 = string.Empty;
                string str5 = string.Empty;
                if (Environment.Is64BitOperatingSystem)
                {
                    frameworkpath = Convert.ToString(parentKey.GetValue("InstallPath"));
                }
                else
                {
                    str3 = Convert.ToString(parentKey.GetValue("InstallPath"));
                }
                if (Environment.Is64BitOperatingSystem)
                {
                    frameworkpath = TestDotnetPath(ver, frameworkpath, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), @"Microsoft.NET\Framework64\"));
                    str4 = TestDotnetPath(ver, str4, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Reference Assemblies\Microsoft\Framework\"));
                }
                str3 = TestDotnetPath(ver, str3, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), @"Microsoft.NET\Framework\"));
                str5 = TestDotnetPath(ver, str5, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Reference Assemblies\Microsoft\Framework\"));
                string format = ".NET Framework {0}";
                if (parentKey.GetValue("SP") != null)
                {
                    format = format + " Service Pack " + Convert.ToString(parentKey.GetValue("SP"));
                }
                string str7 = parentKey.Name.Substring(parentKey.Name.LastIndexOf('\\') + 1);
                if (str7.Equals(versionKey))
                {
                    net = new DotNet {
                        key = versionKey,
                        name = string.Format(format, versionKey),
                        version = ver
                    };
                    char[] trimChars = new char[] { Path.DirectorySeparatorChar };
                    net.x86WindowsPath = str3.TrimEnd(trimChars);
                    char[] chArray2 = new char[] { Path.DirectorySeparatorChar };
                    net.x86ProgramFilesPath = str5.TrimEnd(chArray2);
                    char[] chArray3 = new char[] { Path.DirectorySeparatorChar };
                    net.x64WindowsPath = frameworkpath.TrimEnd(chArray3);
                    char[] chArray4 = new char[] { Path.DirectorySeparatorChar };
                    net.x64ProgramFilesPath = str4.TrimEnd(chArray4);
                    versions.Add(net);
                }
                else if ((ver.Major == 4) && (ver.Minor >= 5))
                {
                    versionKey = $"4.{ver.Minor}";
                    net = new DotNet {
                        key = "v4",
                        name = string.Format(format, versionKey + " " + str7),
                        version = ver
                    };
                    char[] chArray5 = new char[] { Path.DirectorySeparatorChar };
                    net.x86WindowsPath = str3.TrimEnd(chArray5);
                    char[] chArray6 = new char[] { Path.DirectorySeparatorChar };
                    net.x86ProgramFilesPath = str5.TrimEnd(chArray6);
                    char[] chArray7 = new char[] { Path.DirectorySeparatorChar };
                    net.x64WindowsPath = frameworkpath.TrimEnd(chArray7);
                    char[] chArray8 = new char[] { Path.DirectorySeparatorChar };
                    net.x64ProgramFilesPath = str4.TrimEnd(chArray8);
                    versions.Add(net);
                }
                else
                {
                    net = new DotNet {
                        key = versionKey,
                        name = string.Format(format, versionKey + " " + str7),
                        version = ver
                    };
                    char[] chArray9 = new char[] { Path.DirectorySeparatorChar };
                    net.x86WindowsPath = str3.TrimEnd(chArray9);
                    char[] chArray10 = new char[] { Path.DirectorySeparatorChar };
                    net.x86ProgramFilesPath = str5.TrimEnd(chArray10);
                    char[] chArray11 = new char[] { Path.DirectorySeparatorChar };
                    net.x64WindowsPath = frameworkpath.TrimEnd(chArray11);
                    char[] chArray12 = new char[] { Path.DirectorySeparatorChar };
                    net.x64ProgramFilesPath = str4.TrimEnd(chArray12);
                    versions.Add(net);
                }
            }
        }

        private void InitializeComponent()
        {
            this.components = new Container();
            this.ToolTips = new ToolTip(this.components);
            this.LinkILMerge = new LinkLabel();
            this.ChkSignKeyFile = new CheckBox();
            this.btnAddFile = new Button();
            this.ChkGenerateLog = new CheckBox();
            this.ChkDelayedSign = new CheckBox();
            this.btnLogFile = new Button();
            this.ChkUnionDuplicates = new CheckBox();
            this.TxtLogFile = new TextBox();
            this.btnKeyFile = new Button();
            this.ChkCopyAttributes = new CheckBox();
            this.TxtKeyFile = new TextBox();
            this.TxtOutputAssembly = new TextBox();
            this.btnOutputPath = new Button();
            this.btnMerge = new Button();
            this.CboDebug = new ComboBox();
            this.CboTargetFramework = new ComboBox();
            this.ListAssembly = new ListView();
            this.columnHeader1 = new ColumnHeader();
            this.columnHeader2 = new ColumnHeader();
            this.ChkInternalize = new CheckBox();
            this.ChkMergeXml = new CheckBox();
            this.WorkerILMerge = new BackgroundWorker();
            this.openFile1 = new OpenFileDialog();
            this.LblPrimaryAssembly = new Label();
            this.LblPrimaryAssemblyInfo = new Label();
            this.BoxOutput = new GroupBox();
            this.LblOutputPath = new Label();
            this.LblDebug = new Label();
            this.LblTargetFramework = new Label();
            this.BoxOptions = new GroupBox();
            this.radioButton2 = new RadioButton();
            this.radioButton1 = new RadioButton();
            this.linkLabel1 = new LinkLabel();
            this.menuStrip1 = new MenuStrip();
            this.fileToolStripMenuItem3 = new ToolStripMenuItem();
            this.mnuFileNew = new ToolStripMenuItem();
            this.toolStripSeparator6 = new ToolStripSeparator();
            this.mnuFileOpen = new ToolStripMenuItem();
            this.mnuFileSave = new ToolStripMenuItem();
            this.toolStripSeparator7 = new ToolStripSeparator();
            this.mnuFileExit = new ToolStripMenuItem();
            this.toolStripSeparator8 = new ToolStripSeparator();
            this.menuRecentFile = new ToolStripMenuItem();
            this.xToolStripMenuItem = new ToolStripMenuItem();
            this.toolStripMenuItem1 = new ToolStripMenuItem();
            this.checkForUpdatesToolStripMenuItem = new ToolStripMenuItem();
            this.visitWebsiteToolStripMenuItem = new ToolStripMenuItem();
            this.toolStripSeparator9 = new ToolStripSeparator();
            this.aboutToolStripMenuItem = new ToolStripMenuItem();
            this.fileToolStripMenuItem = new ToolStripMenuItem();
            this.newToolStripMenuItem = new ToolStripMenuItem();
            this.openToolStripMenuItem = new ToolStripMenuItem();
            this.saveToolStripMenuItem = new ToolStripMenuItem();
            this.exitToolStripMenuItem = new ToolStripMenuItem();
            this.toolStripSeparator1 = new ToolStripSeparator();
            this.toolStripSeparator2 = new ToolStripSeparator();
            this.fileToolStripMenuItem1 = new ToolStripMenuItem();
            this.newToolStripMenuItem1 = new ToolStripMenuItem();
            this.toolStripSeparator3 = new ToolStripSeparator();
            this.openToolStripMenuItem1 = new ToolStripMenuItem();
            this.saveToolStripMenuItem1 = new ToolStripMenuItem();
            this.toolStripSeparator4 = new ToolStripSeparator();
            this.exitToolStripMenuItem1 = new ToolStripMenuItem();
            this.fileToolStripMenuItem2 = new ToolStripMenuItem();
            this.newToolStripMenuItem2 = new ToolStripMenuItem();
            this.toolStripSeparator5 = new ToolStripSeparator();
            this.openToolStripMenuItem2 = new ToolStripMenuItem();
            this.groupBox1 = new GroupBox();
            this.label1 = new Label();
            this.openFileDialog1 = new OpenFileDialog();
            this.saveFileDialog1 = new SaveFileDialog();
            this.label2 = new Label();
            this.ChkGenCmdLine = new CheckBox();
            this.BoxOutput.SuspendLayout();
            this.BoxOptions.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            base.SuspendLayout();
            this.ToolTips.AutomaticDelay = 800;
            this.ToolTips.IsBalloon = true;
            this.LinkILMerge.ActiveLinkColor = SystemColors.HotTrack;
            this.LinkILMerge.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this.LinkILMerge.AutoSize = true;
            this.LinkILMerge.ImeMode = ImeMode.NoControl;
            this.LinkILMerge.LinkBehavior = LinkBehavior.HoverUnderline;
            this.LinkILMerge.LinkColor = SystemColors.HotTrack;
            this.LinkILMerge.Location = new Point(370, 0x217);
            this.LinkILMerge.Name = "LinkILMerge";
            this.LinkILMerge.Size = new Size(0x107, 13);
            this.LinkILMerge.TabIndex = 0x25;
            this.LinkILMerge.TabStop = true;
            this.LinkILMerge.Text = "http://research.microsoft.com/~mbarnett/ilmerge.aspx";
            this.ToolTips.SetToolTip(this.LinkILMerge, "ILMerge homepage");
            this.LinkILMerge.VisitedLinkColor = SystemColors.HotTrack;
            this.LinkILMerge.LinkClicked += new LinkLabelLinkClickedEventHandler(this.LinkILMerge_LinkClicked);
            this.ChkSignKeyFile.AutoSize = true;
            this.ChkSignKeyFile.Location = new Point(6, 0x31);
            this.ChkSignKeyFile.Name = "ChkSignKeyFile";
            this.ChkSignKeyFile.Size = new Size(0x69, 0x11);
            this.ChkSignKeyFile.TabIndex = 5;
            this.ChkSignKeyFile.Text = "Sign with key file";
            this.ToolTips.SetToolTip(this.ChkSignKeyFile, "Sign the output assembly with a key file.");
            this.ChkSignKeyFile.UseVisualStyleBackColor = true;
            this.ChkSignKeyFile.CheckedChanged += new EventHandler(this.ChkSignKeyFile_CheckedChanged);
            this.btnAddFile.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this.btnAddFile.Cursor = Cursors.Hand;
            this.btnAddFile.Image = Resources.IconAdd;
            this.btnAddFile.ImageAlign = ContentAlignment.MiddleLeft;
            this.btnAddFile.ImeMode = ImeMode.NoControl;
            this.btnAddFile.Location = new Point(0x1f7, 0xca);
            this.btnAddFile.Name = "btnAddFile";
            this.btnAddFile.Size = new Size(0x70, 0x17);
            this.btnAddFile.TabIndex = 0x21;
            this.btnAddFile.Text = "Add assemblies";
            this.btnAddFile.TextAlign = ContentAlignment.MiddleRight;
            this.ToolTips.SetToolTip(this.btnAddFile, "Click to select and add assemblies to the list.");
            this.btnAddFile.UseVisualStyleBackColor = true;
            this.btnAddFile.Click += new EventHandler(this.btnAddFile_Click);
            this.ChkGenerateLog.AutoSize = true;
            this.ChkGenerateLog.Location = new Point(6, 0x63);
            this.ChkGenerateLog.Name = "ChkGenerateLog";
            this.ChkGenerateLog.Size = new Size(0x67, 0x11);
            this.ChkGenerateLog.TabIndex = 13;
            this.ChkGenerateLog.Text = "Generate log file";
            this.ToolTips.SetToolTip(this.ChkGenerateLog, "Write results to a log file.");
            this.ChkGenerateLog.UseVisualStyleBackColor = true;
            this.ChkGenerateLog.CheckedChanged += new EventHandler(this.ChkGenerateLog_CheckedChanged);
            this.ChkDelayedSign.AutoSize = true;
            this.ChkDelayedSign.Enabled = false;
            this.ChkDelayedSign.Location = new Point(0x7e, 0x31);
            this.ChkDelayedSign.Name = "ChkDelayedSign";
            this.ChkDelayedSign.Size = new Size(0x57, 0x11);
            this.ChkDelayedSign.TabIndex = 7;
            this.ChkDelayedSign.Text = "Delayed sign";
            this.ToolTips.SetToolTip(this.ChkDelayedSign, "Use delayed sign.");
            this.ChkDelayedSign.UseVisualStyleBackColor = true;
            this.btnLogFile.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            this.btnLogFile.Cursor = Cursors.Hand;
            this.btnLogFile.Enabled = false;
            this.btnLogFile.Image = Resources.IconFolder;
            this.btnLogFile.ImeMode = ImeMode.NoControl;
            this.btnLogFile.Location = new Point(590, 0x74);
            this.btnLogFile.Name = "btnLogFile";
            this.btnLogFile.Size = new Size(0x19, 0x17);
            this.btnLogFile.TabIndex = 0x11;
            this.ToolTips.SetToolTip(this.btnLogFile, "Click to select a log path");
            this.btnLogFile.UseVisualStyleBackColor = true;
            this.btnLogFile.Click += new EventHandler(this.btnLogFile_Click);
            this.ChkUnionDuplicates.AutoSize = true;
            this.ChkUnionDuplicates.Location = new Point(0x7e, 0x15);
            this.ChkUnionDuplicates.Name = "ChkUnionDuplicates";
            this.ChkUnionDuplicates.Size = new Size(0x69, 0x11);
            this.ChkUnionDuplicates.TabIndex = 3;
            this.ChkUnionDuplicates.Text = "Union duplicates";
            this.ToolTips.SetToolTip(this.ChkUnionDuplicates, "Union duplicate classes and references.");
            this.ChkUnionDuplicates.UseVisualStyleBackColor = true;
            this.TxtLogFile.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.TxtLogFile.AutoCompleteMode = AutoCompleteMode.Suggest;
            this.TxtLogFile.AutoCompleteSource = AutoCompleteSource.FileSystem;
            this.TxtLogFile.Enabled = false;
            this.TxtLogFile.Location = new Point(6, 0x76);
            this.TxtLogFile.MaxLength = 0xff;
            this.TxtLogFile.Name = "TxtLogFile";
            this.TxtLogFile.Size = new Size(0x242, 20);
            this.TxtLogFile.TabIndex = 15;
            this.ToolTips.SetToolTip(this.TxtLogFile, "Path to the log file.");
            this.btnKeyFile.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            this.btnKeyFile.Cursor = Cursors.Hand;
            this.btnKeyFile.Enabled = false;
            this.btnKeyFile.Image = Resources.IconFolder;
            this.btnKeyFile.ImeMode = ImeMode.NoControl;
            this.btnKeyFile.Location = new Point(590, 0x42);
            this.btnKeyFile.Name = "btnKeyFile";
            this.btnKeyFile.Size = new Size(0x19, 0x17);
            this.btnKeyFile.TabIndex = 11;
            this.ToolTips.SetToolTip(this.btnKeyFile, "Click to select a key file");
            this.btnKeyFile.UseVisualStyleBackColor = true;
            this.btnKeyFile.Click += new EventHandler(this.btnKeyFile_Click);
            this.ChkCopyAttributes.AutoSize = true;
            this.ChkCopyAttributes.Checked = true;
            this.ChkCopyAttributes.CheckState = CheckState.Checked;
            this.ChkCopyAttributes.Location = new Point(6, 0x15);
            this.ChkCopyAttributes.Name = "ChkCopyAttributes";
            this.ChkCopyAttributes.Size = new Size(0x60, 0x11);
            this.ChkCopyAttributes.TabIndex = 1;
            this.ChkCopyAttributes.Text = "Copy attributes";
            this.ToolTips.SetToolTip(this.ChkCopyAttributes, "Copy assembly attributes.");
            this.ChkCopyAttributes.UseVisualStyleBackColor = true;
            this.TxtKeyFile.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.TxtKeyFile.AutoCompleteMode = AutoCompleteMode.Suggest;
            this.TxtKeyFile.AutoCompleteSource = AutoCompleteSource.FileSystem;
            this.TxtKeyFile.Enabled = false;
            this.TxtKeyFile.Location = new Point(6, 0x44);
            this.TxtKeyFile.MaxLength = 0xff;
            this.TxtKeyFile.Name = "TxtKeyFile";
            this.TxtKeyFile.Size = new Size(0x242, 20);
            this.TxtKeyFile.TabIndex = 9;
            this.ToolTips.SetToolTip(this.TxtKeyFile, "Path to the key file");
            this.TxtOutputAssembly.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.TxtOutputAssembly.AutoCompleteMode = AutoCompleteMode.Suggest;
            this.TxtOutputAssembly.AutoCompleteSource = AutoCompleteSource.FileSystem;
            this.TxtOutputAssembly.Location = new Point(6, 0x25);
            this.TxtOutputAssembly.MaxLength = 0xff;
            this.TxtOutputAssembly.Name = "TxtOutputAssembly";
            this.TxtOutputAssembly.Size = new Size(0x242, 20);
            this.TxtOutputAssembly.TabIndex = 2;
            this.ToolTips.SetToolTip(this.TxtOutputAssembly, "Path to the output generated assembly.");
            this.btnOutputPath.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            this.btnOutputPath.Cursor = Cursors.Hand;
            this.btnOutputPath.Image = Resources.IconFolder;
            this.btnOutputPath.ImeMode = ImeMode.NoControl;
            this.btnOutputPath.Location = new Point(590, 0x23);
            this.btnOutputPath.Name = "btnOutputPath";
            this.btnOutputPath.Size = new Size(0x19, 0x17);
            this.btnOutputPath.TabIndex = 4;
            this.ToolTips.SetToolTip(this.btnOutputPath, "Click to select the path to the output assembly");
            this.btnOutputPath.UseVisualStyleBackColor = true;
            this.btnOutputPath.Click += new EventHandler(this.btnOutputPath_Click);
            this.btnMerge.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            this.btnMerge.Cursor = Cursors.Hand;
            this.btnMerge.Enabled = false;
            this.btnMerge.Font = new Font("Segoe UI", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.btnMerge.ImageAlign = ContentAlignment.MiddleLeft;
            this.btnMerge.ImeMode = ImeMode.NoControl;
            this.btnMerge.Location = new Point(0x1fd, 0x48);
            this.btnMerge.Name = "btnMerge";
            this.btnMerge.Size = new Size(0x6a, 0x17);
            this.btnMerge.TabIndex = 10;
            this.btnMerge.Text = "Merge!";
            this.btnMerge.TextAlign = ContentAlignment.MiddleRight;
            this.ToolTips.SetToolTip(this.btnMerge, "Click to start merging.");
            this.btnMerge.UseVisualStyleBackColor = true;
            this.btnMerge.Click += new EventHandler(this.btnMerge_Click);
            this.CboDebug.DropDownStyle = ComboBoxStyle.DropDownList;
            this.CboDebug.FormattingEnabled = true;
            object[] items = new object[] { "True", "False" };
            this.CboDebug.Items.AddRange(items);
            this.CboDebug.Location = new Point(0x36, 0x48);
            this.CboDebug.Name = "CboDebug";
            this.CboDebug.Size = new Size(0x40, 0x15);
            this.CboDebug.TabIndex = 6;
            this.ToolTips.SetToolTip(this.CboDebug, "Set the debug parameter.");
            this.CboTargetFramework.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.CboTargetFramework.DropDownStyle = ComboBoxStyle.DropDownList;
            this.CboTargetFramework.FormattingEnabled = true;
            object[] objArray2 = new object[] { ".NET 2.0", ".NET 3.0", ".NET 3.5", ".NET 4.0" };
            this.CboTargetFramework.Items.AddRange(objArray2);
            this.CboTargetFramework.Location = new Point(0xc0, 0x48);
            this.CboTargetFramework.Name = "CboTargetFramework";
            this.CboTargetFramework.Size = new Size(0x120, 0x15);
            this.CboTargetFramework.TabIndex = 8;
            this.ToolTips.SetToolTip(this.CboTargetFramework, "Set the target framework.");
            this.ListAssembly.Activation = ItemActivation.OneClick;
            this.ListAssembly.AllowDrop = true;
            this.ListAssembly.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            this.ListAssembly.CheckBoxes = true;
            ColumnHeader[] values = new ColumnHeader[] { this.columnHeader1, this.columnHeader2 };
            this.ListAssembly.Columns.AddRange(values);
            this.ListAssembly.FullRowSelect = true;
            this.ListAssembly.GridLines = true;
            this.ListAssembly.HeaderStyle = ColumnHeaderStyle.None;
            this.ListAssembly.HideSelection = false;
            this.ListAssembly.Location = new Point(3, 0x10);
            this.ListAssembly.Name = "ListAssembly";
            this.ListAssembly.Size = new Size(0x267, 180);
            this.ListAssembly.TabIndex = 0x22;
            this.ToolTips.SetToolTip(this.ListAssembly, "Assemblies to be merged.");
            this.ListAssembly.UseCompatibleStateImageBehavior = false;
            this.ListAssembly.View = View.Details;
            this.ListAssembly.ItemCheck += new ItemCheckEventHandler(this.ListAssembly_ItemCheck);
            this.ListAssembly.ItemChecked += new ItemCheckedEventHandler(this.ListAssembly_ItemChecked);
            this.ListAssembly.DragDrop += new DragEventHandler(this.ListAssembly_DragDrop);
            this.ListAssembly.DragEnter += new DragEventHandler(this.ListAssembly_DragEnter);
            this.ListAssembly.KeyDown += new KeyEventHandler(this.ListAssembly_KeyDown);
            this.columnHeader1.Text = "Assembly";
            this.ChkInternalize.AutoSize = true;
            this.ChkInternalize.Location = new Point(0xfd, 0x15);
            this.ChkInternalize.Name = "ChkInternalize";
            this.ChkInternalize.Size = new Size(0x4a, 0x11);
            this.ChkInternalize.TabIndex = 0x12;
            this.ChkInternalize.Text = "Internalize";
            this.ToolTips.SetToolTip(this.ChkInternalize, "Change all public identifiers into internal ones.");
            this.ChkInternalize.UseVisualStyleBackColor = true;
            this.ChkMergeXml.AutoSize = true;
            this.ChkMergeXml.Location = new Point(0xfd, 0x31);
            this.ChkMergeXml.Name = "ChkMergeXml";
            this.ChkMergeXml.Size = new Size(0x93, 0x11);
            this.ChkMergeXml.TabIndex = 0x13;
            this.ChkMergeXml.Text = "Merge xml documentation";
            this.ToolTips.SetToolTip(this.ChkMergeXml, "Merge xml documentation into a single file.");
            this.ChkMergeXml.UseVisualStyleBackColor = true;
            this.WorkerILMerge.DoWork += new DoWorkEventHandler(this.WorkerILMerge_DoWork);
            this.WorkerILMerge.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.WorkerILMerge_RunWorkerCompleted);
            this.LblPrimaryAssembly.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            this.LblPrimaryAssembly.AutoSize = true;
            this.LblPrimaryAssembly.ImeMode = ImeMode.NoControl;
            this.LblPrimaryAssembly.Location = new Point(0x6c, 0xcf);
            this.LblPrimaryAssembly.Name = "LblPrimaryAssembly";
            this.LblPrimaryAssembly.Size = new Size(0x10, 13);
            this.LblPrimaryAssembly.TabIndex = 30;
            this.LblPrimaryAssembly.Text = "\x00b7\x00b7\x00b7";
            this.LblPrimaryAssembly.TextChanged += new EventHandler(this.LblPrimaryAssembly_TextChanged);
            this.LblPrimaryAssemblyInfo.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            this.LblPrimaryAssemblyInfo.AutoSize = true;
            this.LblPrimaryAssemblyInfo.ImeMode = ImeMode.NoControl;
            this.LblPrimaryAssemblyInfo.Location = new Point(6, 0xcf);
            this.LblPrimaryAssemblyInfo.Name = "LblPrimaryAssemblyInfo";
            this.LblPrimaryAssemblyInfo.Size = new Size(90, 13);
            this.LblPrimaryAssemblyInfo.TabIndex = 0x1f;
            this.LblPrimaryAssemblyInfo.Text = "Primary assembly:";
            this.BoxOutput.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            this.BoxOutput.Controls.Add(this.LblOutputPath);
            this.BoxOutput.Controls.Add(this.TxtOutputAssembly);
            this.BoxOutput.Controls.Add(this.btnOutputPath);
            this.BoxOutput.Controls.Add(this.btnMerge);
            this.BoxOutput.Controls.Add(this.CboDebug);
            this.BoxOutput.Controls.Add(this.LblDebug);
            this.BoxOutput.Controls.Add(this.LblTargetFramework);
            this.BoxOutput.Controls.Add(this.CboTargetFramework);
            this.BoxOutput.Location = new Point(12, 0x1a6);
            this.BoxOutput.Name = "BoxOutput";
            this.BoxOutput.Size = new Size(0x26d, 0x65);
            this.BoxOutput.TabIndex = 0x23;
            this.BoxOutput.TabStop = false;
            this.BoxOutput.Text = "Output";
            this.LblOutputPath.AutoSize = true;
            this.LblOutputPath.ImeMode = ImeMode.NoControl;
            this.LblOutputPath.Location = new Point(3, 0x15);
            this.LblOutputPath.Name = "LblOutputPath";
            this.LblOutputPath.Size = new Size(0x58, 13);
            this.LblOutputPath.TabIndex = 0;
            this.LblOutputPath.Text = "Output assembly:";
            this.LblDebug.AutoSize = true;
            this.LblDebug.ImeMode = ImeMode.NoControl;
            this.LblDebug.Location = new Point(3, 0x4b);
            this.LblDebug.Name = "LblDebug";
            this.LblDebug.Size = new Size(0x2a, 13);
            this.LblDebug.TabIndex = 0;
            this.LblDebug.Text = "Debug:";
            this.LblTargetFramework.AutoSize = true;
            this.LblTargetFramework.ImeMode = ImeMode.NoControl;
            this.LblTargetFramework.Location = new Point(0x7c, 0x4b);
            this.LblTargetFramework.Name = "LblTargetFramework";
            this.LblTargetFramework.Size = new Size(0x3e, 13);
            this.LblTargetFramework.TabIndex = 0;
            this.LblTargetFramework.Text = "Framework:";
            this.BoxOptions.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            this.BoxOptions.Controls.Add(this.ChkGenCmdLine);
            this.BoxOptions.Controls.Add(this.radioButton2);
            this.BoxOptions.Controls.Add(this.radioButton1);
            this.BoxOptions.Controls.Add(this.ChkMergeXml);
            this.BoxOptions.Controls.Add(this.ChkInternalize);
            this.BoxOptions.Controls.Add(this.ChkSignKeyFile);
            this.BoxOptions.Controls.Add(this.ChkGenerateLog);
            this.BoxOptions.Controls.Add(this.ChkDelayedSign);
            this.BoxOptions.Controls.Add(this.btnLogFile);
            this.BoxOptions.Controls.Add(this.TxtKeyFile);
            this.BoxOptions.Controls.Add(this.ChkUnionDuplicates);
            this.BoxOptions.Controls.Add(this.TxtLogFile);
            this.BoxOptions.Controls.Add(this.btnKeyFile);
            this.BoxOptions.Controls.Add(this.ChkCopyAttributes);
            this.BoxOptions.Location = new Point(12, 0x108);
            this.BoxOptions.Name = "BoxOptions";
            this.BoxOptions.Size = new Size(0x26d, 0x98);
            this.BoxOptions.TabIndex = 0x22;
            this.BoxOptions.TabStop = false;
            this.BoxOptions.Text = "Options";
            this.radioButton2.AutoSize = true;
            this.radioButton2.Enabled = false;
            this.radioButton2.Location = new Point(0x1b6, 0x2d);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new Size(0x48, 0x11);
            this.radioButton2.TabIndex = 0x15;
            this.radioButton2.TabStop = true;
            this.radioButton2.Text = "ILRepack";
            this.radioButton2.UseVisualStyleBackColor = true;
            this.radioButton2.CheckedChanged += new EventHandler(this.radioButton2_CheckedChanged);
            this.radioButton1.AutoSize = true;
            this.radioButton1.Enabled = false;
            this.radioButton1.Location = new Point(0x1b6, 0x13);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new Size(0x40, 0x11);
            this.radioButton1.TabIndex = 20;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "ILMerge";
            this.radioButton1.UseVisualStyleBackColor = true;
            this.radioButton1.CheckedChanged += new EventHandler(this.radioButton1_CheckedChanged);
            this.linkLabel1.ActiveLinkColor = SystemColors.HotTrack;
            this.linkLabel1.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.ImeMode = ImeMode.NoControl;
            this.linkLabel1.LinkBehavior = LinkBehavior.HoverUnderline;
            this.linkLabel1.LinkColor = SystemColors.HotTrack;
            this.linkLabel1.Location = new Point(0x1da, 0x229);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new Size(0x9f, 13);
            this.linkLabel1.TabIndex = 0x24;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "http://ilmergegui.codeplex.com/";
            this.linkLabel1.VisitedLinkColor = SystemColors.HotTrack;
            ToolStripItem[] toolStripItems = new ToolStripItem[] { this.fileToolStripMenuItem3, this.toolStripMenuItem1 };
            this.menuStrip1.Items.AddRange(toolStripItems);
            this.menuStrip1.Location = new Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new Size(0x285, 0x18);
            this.menuStrip1.TabIndex = 0x26;
            this.menuStrip1.Text = "menuStrip1";
            ToolStripItem[] itemArray2 = new ToolStripItem[] { this.mnuFileNew, this.toolStripSeparator6, this.mnuFileOpen, this.mnuFileSave, this.toolStripSeparator7, this.mnuFileExit, this.toolStripSeparator8, this.menuRecentFile };
            this.fileToolStripMenuItem3.DropDownItems.AddRange(itemArray2);
            this.fileToolStripMenuItem3.Name = "fileToolStripMenuItem3";
            this.fileToolStripMenuItem3.Size = new Size(0x25, 20);
            this.fileToolStripMenuItem3.Text = "File";
            this.mnuFileNew.Name = "mnuFileNew";
            this.mnuFileNew.Size = new Size(0x67, 0x16);
            this.mnuFileNew.Text = "New";
            this.mnuFileNew.Click += new EventHandler(this.mnuFileNew_Click);
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new Size(100, 6);
            this.mnuFileOpen.Name = "mnuFileOpen";
            this.mnuFileOpen.Size = new Size(0x67, 0x16);
            this.mnuFileOpen.Text = "Open";
            this.mnuFileOpen.Click += new EventHandler(this.mnuFileOpen_Click);
            this.mnuFileSave.Name = "mnuFileSave";
            this.mnuFileSave.Size = new Size(0x67, 0x16);
            this.mnuFileSave.Text = "Save";
            this.mnuFileSave.Click += new EventHandler(this.mnuFileSave_Click);
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new Size(100, 6);
            this.mnuFileExit.Name = "mnuFileExit";
            this.mnuFileExit.Size = new Size(0x67, 0x16);
            this.mnuFileExit.Text = "Exit";
            this.mnuFileExit.Click += new EventHandler(this.mnuFileExit_Click);
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new Size(100, 6);
            ToolStripItem[] itemArray3 = new ToolStripItem[] { this.xToolStripMenuItem };
            this.menuRecentFile.DropDownItems.AddRange(itemArray3);
            this.menuRecentFile.Name = "menuRecentFile";
            this.menuRecentFile.Size = new Size(0x67, 0x16);
            this.menuRecentFile.Text = "MRU";
            this.xToolStripMenuItem.Name = "xToolStripMenuItem";
            this.xToolStripMenuItem.Size = new Size(0x4f, 0x16);
            this.xToolStripMenuItem.Text = "x";
            ToolStripItem[] itemArray4 = new ToolStripItem[] { this.checkForUpdatesToolStripMenuItem, this.visitWebsiteToolStripMenuItem, this.toolStripSeparator9, this.aboutToolStripMenuItem };
            this.toolStripMenuItem1.DropDownItems.AddRange(itemArray4);
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new Size(0x2c, 20);
            this.toolStripMenuItem1.Text = "Help";
            this.checkForUpdatesToolStripMenuItem.Name = "checkForUpdatesToolStripMenuItem";
            this.checkForUpdatesToolStripMenuItem.Size = new Size(0xab, 0x16);
            this.checkForUpdatesToolStripMenuItem.Text = "Check for Updates";
            this.checkForUpdatesToolStripMenuItem.Click += new EventHandler(this.checkForUpdatesToolStripMenuItem_Click);
            this.visitWebsiteToolStripMenuItem.Name = "visitWebsiteToolStripMenuItem";
            this.visitWebsiteToolStripMenuItem.Size = new Size(0xab, 0x16);
            this.visitWebsiteToolStripMenuItem.Text = "Visit Website";
            this.visitWebsiteToolStripMenuItem.Click += new EventHandler(this.visitWebsiteToolStripMenuItem_Click);
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new Size(0xa8, 6);
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new Size(0xab, 0x16);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new EventHandler(this.aboutToolStripMenuItem_Click);
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new Size(0x25, 20);
            this.fileToolStripMenuItem.Text = "File";
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.Size = new Size(0x98, 0x16);
            this.newToolStripMenuItem.Text = "New";
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new Size(0x98, 0x16);
            this.openToolStripMenuItem.Text = "Open";
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new Size(0x98, 0x16);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new EventHandler(this.mnuFileSave_Click);
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new Size(0x98, 0x16);
            this.exitToolStripMenuItem.Text = "Exit";
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new Size(0x95, 6);
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new Size(0x95, 6);
            this.fileToolStripMenuItem1.Name = "fileToolStripMenuItem1";
            this.fileToolStripMenuItem1.Size = new Size(0x25, 20);
            this.fileToolStripMenuItem1.Text = "File";
            this.newToolStripMenuItem1.Name = "newToolStripMenuItem1";
            this.newToolStripMenuItem1.Size = new Size(0x98, 0x16);
            this.newToolStripMenuItem1.Text = "New";
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new Size(0x95, 6);
            this.openToolStripMenuItem1.Name = "openToolStripMenuItem1";
            this.openToolStripMenuItem1.Size = new Size(0x98, 0x16);
            this.openToolStripMenuItem1.Text = "Open";
            this.saveToolStripMenuItem1.Name = "saveToolStripMenuItem1";
            this.saveToolStripMenuItem1.Size = new Size(0x98, 0x16);
            this.saveToolStripMenuItem1.Text = "Save";
            this.saveToolStripMenuItem1.Click += new EventHandler(this.mnuFileSave_Click);
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new Size(0x95, 6);
            this.exitToolStripMenuItem1.Name = "exitToolStripMenuItem1";
            this.exitToolStripMenuItem1.Size = new Size(0x98, 0x16);
            this.exitToolStripMenuItem1.Text = "Exit";
            this.fileToolStripMenuItem2.Name = "fileToolStripMenuItem2";
            this.fileToolStripMenuItem2.Size = new Size(0x25, 20);
            this.fileToolStripMenuItem2.Text = "File";
            this.newToolStripMenuItem2.Name = "newToolStripMenuItem2";
            this.newToolStripMenuItem2.Size = new Size(0x98, 0x16);
            this.newToolStripMenuItem2.Text = "New";
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new Size(0x95, 6);
            this.openToolStripMenuItem2.Name = "openToolStripMenuItem2";
            this.openToolStripMenuItem2.Size = new Size(0x98, 0x16);
            this.openToolStripMenuItem2.Text = "Open";
            this.groupBox1.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            this.groupBox1.Controls.Add(this.ListAssembly);
            this.groupBox1.Controls.Add(this.btnAddFile);
            this.groupBox1.Controls.Add(this.LblPrimaryAssemblyInfo);
            this.groupBox1.Controls.Add(this.LblPrimaryAssembly);
            this.groupBox1.Location = new Point(12, 0x1b);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new Size(0x26d, 0xe7);
            this.groupBox1.TabIndex = 0x27;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Assemblies to merge:";
            this.label1.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            this.label1.AutoSize = true;
            this.label1.Location = new Point(9, 0x217);
            this.label1.Name = "label1";
            this.label1.Size = new Size(0x23, 13);
            this.label1.TabIndex = 40;
            this.label1.Text = "label1";
            this.openFileDialog1.DefaultExt = "*.ilproj";
            this.openFileDialog1.FileName = "*.ilproj";
            this.openFileDialog1.Filter = "IlMerge Project|*.ilproj|All Files|*.*";
            this.openFileDialog1.Title = "Select an IlMergeGui Project";
            this.saveFileDialog1.DefaultExt = "*.ilproj";
            this.saveFileDialog1.FileName = "*.ilproj";
            this.saveFileDialog1.Filter = "IlMerge Project|*.ilproj|All Files|*.*";
            this.saveFileDialog1.Title = "Save as IlMergeGui Project";
            this.label2.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            this.label2.AutoSize = true;
            this.label2.Location = new Point(9, 0x229);
            this.label2.Name = "label2";
            this.label2.Size = new Size(0x23, 13);
            this.label2.TabIndex = 0x29;
            this.label2.Text = "label2";
            this.ChkGenCmdLine.AutoSize = true;
            this.ChkGenCmdLine.Location = new Point(0x7e, 0x63);
            this.ChkGenCmdLine.Name = "ChkGenCmdLine";
            this.ChkGenCmdLine.Size = new Size(0x80, 0x11);
            this.ChkGenCmdLine.TabIndex = 0x16;
            this.ChkGenCmdLine.Text = "Generate cmd line file";
            this.ToolTips.SetToolTip(this.ChkGenCmdLine, "Write results to a log file.");
            this.ChkGenCmdLine.UseVisualStyleBackColor = true;
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(0x285, 0x23f);
            base.Controls.Add(this.label2);
            base.Controls.Add(this.label1);
            base.Controls.Add(this.groupBox1);
            base.Controls.Add(this.LinkILMerge);
            base.Controls.Add(this.linkLabel1);
            base.Controls.Add(this.BoxOutput);
            base.Controls.Add(this.BoxOptions);
            base.Controls.Add(this.menuStrip1);
            this.DoubleBuffered = true;
            base.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new Size(440, 520);
            base.Name = "Mainform";
            base.SizeGripStyle = SizeGripStyle.Show;
            this.Text = "ILMerge-GUI";
            base.Load += new EventHandler(this.Mainform_Load);
            base.Shown += new EventHandler(this.Mainform_Shown);
            this.BoxOutput.ResumeLayout(false);
            this.BoxOutput.PerformLayout();
            this.BoxOptions.ResumeLayout(false);
            this.BoxOptions.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        public static List<DotNet> InstalledDotNetVersions()
        {
            List<DotNet> versions = new List<DotNet>();
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Active Setup\Installed Components");
            if (key != null)
            {
                GetDotNetVersion(key.OpenSubKey("{78705f0d-e8db-4b2d-8193-982bdda15ecd}"), "{78705f0d-e8db-4b2d-8193-982bdda15ecd}", versions);
                GetDotNetVersion(key.OpenSubKey("{FDC11A6F-17D1-48f9-9EA3-9051954BAA24}"), "{FDC11A6F-17D1-48f9-9EA3-9051954BAA24}", versions);
            }
            RegistryKey key2 = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP");
            if (key2 != null)
            {
                foreach (string str in key2.GetSubKeyNames())
                {
                    if ((key2.OpenSubKey(str).GetValue("") == null) || !key2.OpenSubKey(str).GetValue("").Equals("deprecated"))
                    {
                        GetDotNetVersion(key2.OpenSubKey(str), str, versions);
                        GetDotNetVersion(key2.OpenSubKey(str).OpenSubKey("Client"), str, versions);
                        GetDotNetVersion(key2.OpenSubKey(str).OpenSubKey("Full"), str, versions);
                    }
                }
            }
            return versions;
        }

        private void LblPrimaryAssembly_TextChanged(object sender, EventArgs e)
        {
            this.FormatItems();
        }

        private void LinkILMerge_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(this.LinkILMerge.Text);
        }

        private void ListAssembly_DragDrop(object sender, DragEventArgs e)
        {
            this.ProcessFiles((string[]) e.Data.GetData(DataFormats.FileDrop));
        }

        private void ListAssembly_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
            {
                e.Effect = DragDropEffects.Move | DragDropEffects.Copy | DragDropEffects.Scroll;
            }
        }

        private void ListAssembly_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.NewValue == CheckState.Checked)
            {
                this.ListAssembly.BeginUpdate();
                foreach (ListViewItem item in this.ListAssembly.CheckedItems)
                {
                    if (item.Index != e.Index)
                    {
                        item.Checked = false;
                        item.Selected = false;
                    }
                }
                foreach (ListViewItem item1 in this.ListAssembly.Items)
                {
                    item1.Selected = item1.Index == e.Index;
                }
                this.ListAssembly.EndUpdate();
            }
        }

        private void ListAssembly_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            this.UpdatePrimary();
        }

        private void ListAssembly_KeyDown(object sender, KeyEventArgs e)
        {
            int index = 0;
            if ((e.KeyCode == Keys.Delete) && (this.ListAssembly.SelectedItems != null))
            {
                while (this.ListAssembly.SelectedItems.Count > 0)
                {
                    index = this.ListAssembly.SelectedIndices[0];
                    this.ListAssembly.Items[index].Selected = false;
                    this.ListAssembly.Items.RemoveAt(index);
                }
                if ((index > 0) || (this.ListAssembly.Items.Count > 0))
                {
                    this.ListAssembly.Items[Math.Max(0, index - 1)].Selected = true;
                }
                this.btnMerge.Enabled = this.ListAssembly.Items.Count > 1;
                this.FormatItems();
            }
            this.SetWaterMark(this.ListAssembly.Items.Count == 0);
        }

        private void LocateEngine(Merger merger)
        {
            if (merger != Merger.ILMerge)
            {
                if (merger == Merger.ILRepack)
                {
                    this.LocateIlRepack();
                }
            }
            else
            {
                this.LocateIlMerge();
            }
            if (File.Exists(this.iLMergePath))
            {
                this.label1.Text = $"{Path.GetFileNameWithoutExtension(this.iLMergePath)}: v{AssemblyName.GetAssemblyName(this.iLMergePath).Version.ToString()}";
            }
            else
            {
                this.label1.Text = $"{merger.ToString()}: {"not found."}";
            }
        }

        private void LocateEngines()
        {
            this.Engine = Merger.None;
            this.LocateEngine(Merger.ILRepack);
            this.LocateEngine(Merger.ILMerge);
            switch (this.Engine)
            {
                case Merger.ILMerge:
                    this.radioButton1.Checked = true;
                    break;

                case Merger.ILRepack:
                    this.radioButton2.Checked = true;
                    break;
            }
            if (this.Engine != Merger.None)
            {
                this.LocateEngine(this.Engine);
            }
            if (string.IsNullOrEmpty(this.iLMergePath) || !File.Exists(this.iLMergePath))
            {
                MessageBox.Show("IlMerge/Repack could not be located, please reinstall ILMerge/Repack!", "ILMergeGui", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void LocateIlMerge()
        {
            this.iLMergePath = string.Empty;
            if (Environment.Is64BitOperatingSystem)
            {
                this.iLMergePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Microsoft\ILMerge\ILMerge.exe");
            }
            else
            {
                this.iLMergePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Microsoft\ILMerge\ILMerge.exe");
            }
            if (string.IsNullOrEmpty(this.ilMerge) || !File.Exists(this.iLMergePath))
            {
                char[] separator = new char[] { ';' };
                foreach (string str in Environment.GetEnvironmentVariable("Path").Split(separator))
                {
                    if (Directory.Exists(str))
                    {
                        string[] files = Directory.GetFiles(str, "ILMerge.exe");
                        int index = 0;
                        while (index < files.Length)
                        {
                            string str2 = files[index];
                            this.iLMergePath = Path.Combine(str, str2);
                            break;
                        }
                    }
                }
            }
            if (string.IsNullOrEmpty(this.iLMergePath) || !File.Exists(this.iLMergePath))
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Installer\Assemblies", false))
                {
                    if (key != null)
                    {
                        foreach (string str3 in key.GetSubKeyNames())
                        {
                            if (str3.EndsWith("ILMerge.exe", StringComparison.OrdinalIgnoreCase) && File.Exists(str3.Replace('|', '\\')))
                            {
                                this.iLMergePath = str3.Replace('|', '\\');
                                goto Label_018B;
                            }
                        }
                    }
                }
            }
        Label_018B:
            if (File.Exists("ILMerge.exe"))
            {
                this.iLMergePath = Path.GetFullPath("ILMerge.exe");
            }
            if (!string.IsNullOrEmpty(this.iLMergePath) && File.Exists(this.iLMergePath))
            {
                this.Engine = Merger.ILMerge;
                this.radioButton1.Enabled = true;
            }
        }

        private void LocateIlRepack()
        {
            this.iLMergePath = string.Empty;
            if (File.Exists(@".\ILRepack.exe"))
            {
                this.iLMergePath = Path.GetFullPath(@".\ILRepack.exe");
            }
            else
            {
                char[] separator = new char[] { ';' };
                foreach (string str in Environment.GetEnvironmentVariable("Path").Split(separator))
                {
                    if (Directory.Exists(str))
                    {
                        string[] files = Directory.GetFiles(str, "ILRepack.exe");
                        int index = 0;
                        while (index < files.Length)
                        {
                            string str2 = files[index];
                            this.iLMergePath = Path.Combine(str, str2);
                            break;
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(this.iLMergePath))
            {
                this.Engine = Merger.ILRepack;
                this.radioButton2.Enabled = true;
            }
        }

        private void Mainform_Load(object sender, EventArgs e)
        {
            bool flag = RegisterIlProj();
            this.Extensions = new Dictionary<string, string>();
            this.Extensions.Add("exe", "Executable(s)");
            this.Extensions.Add("dll", "Assemblies or dll(s)");
            this.openFileDialog1.DefaultExt = "*.ilproj";
            this.openFileDialog1.FileName = "*.ilproj";
            this.openFileDialog1.Filter = $"IlMerge Project|{"*.ilproj"}|All Files|*.*";
            this.saveFileDialog1.DefaultExt = "*.ilproj";
            this.saveFileDialog1.FileName = "*.ilproj";
            this.saveFileDialog1.Filter = $"IlMerge Project|{"*.ilproj"}|All Files|*.*";
            this.ListAssembly.Columns[0].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
            foreach (KeyValuePair<string, string> pair in this.Extensions)
            {
                this.ListAssembly.Groups.Add(pair.Key, pair.Value);
            }
            SendMessage(this.ListAssembly.Handle, 0x1026, IntPtr.Zero, uint.MaxValue);
            SendMessage(this.ListAssembly.Handle, 0x1036, (uint) 0x10000, (uint) 0x10000);
            this.SetWaterMark(true);
            this.LocateEngines();
            this.label2.Text = $"IlMergeGui: v{Assembly.GetExecutingAssembly().GetName().Version} {string.Format(flag ? "({0} extension registered)" : "({0} extension not registered, run elevated once)", ".ilproj")}";
            this.RestoreDefaults();
            RegistryKey key = Registry.CurrentUser.OpenSubKey(this.mruRegKey);
            if (key != null)
            {
                int num1 = (int) key.GetValue("delSubkey", 0);
                key.Close();
            }
            this.mruMenu = new MruStripMenuInline(this.fileToolStripMenuItem3, this.menuRecentFile, new MruStripMenu.ClickedHandler(this.OnMruFile), this.mruRegKey + @"\MRU", 0x10);
            this.mruMenu.LoadFromRegistry();
            this.menuStrip1.Update();
            this.menuStrip1.Refresh();
            this.AutoClose = false;
            this.ExitCode = 0;
            string[] commandLineArgs = Environment.GetCommandLineArgs();
            for (int i = 0; i < commandLineArgs.Length; i++)
            {
                if (commandLineArgs[i].Equals("/close", StringComparison.OrdinalIgnoreCase))
                {
                    this.AutoClose = true;
                }
            }
            foreach (string str in Environment.GetCommandLineArgs())
            {
                if (!str.StartsWith("/") && str.EndsWith(".ilproj", StringComparison.OrdinalIgnoreCase))
                {
                    if (File.Exists(str))
                    {
                        this.RestoreSettings(str);
                        string[] strArray2 = Environment.GetCommandLineArgs();
                        for (int j = 0; j < strArray2.Length; j++)
                        {
                            if (strArray2[j].Equals("/merge", StringComparison.OrdinalIgnoreCase))
                            {
                                this.btnMerge.PerformClick();
                            }
                        }
                        break;
                    }
                    this.ExitCode = 4;
                    this.ExitMsg = $"Project File not Found:

{Path.GetFullPath(str)}";
                    if (!this.AutoClose)
                    {
                        MessageBox.Show(this.ExitMsg, Application.ProductName);
                    }
                    else
                    {
                        this.ExitILMergeGUI();
                    }
                }
            }
            foreach (string str2 in Environment.GetCommandLineArgs())
            {
                if ((str2.Equals("/?", StringComparison.OrdinalIgnoreCase) || str2.Equals("/h?", StringComparison.OrdinalIgnoreCase)) || str2.Equals("/help", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Commandline syntax is:\r\n\r\n" + $"ILMergeGui <{"*.ilproj"}> [/Merge] [/Close] [/?]

" + " /Merge will automaticaly merge the supplied ilproject\r\n\r\n /Close will automaticaly close ILMergeGUI if /Merge is present\r\n\r\n /? will display this help\r\n", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    Environment.Exit(this.ExitCode);
                }
            }
        }

        private void Mainform_Shown(object sender, EventArgs e)
        {
            this.ListAssembly.Focus();
        }

        public static string MakeRelativePath(string fromPath, string toPath)
        {
            if (string.IsNullOrEmpty(fromPath))
            {
                throw new ArgumentNullException("fromPath");
            }
            if (string.IsNullOrEmpty(toPath))
            {
                throw new ArgumentNullException("toPath");
            }
            Uri uri = new Uri(toPath);
            string str = Uri.UnescapeDataString(new Uri(fromPath).MakeRelativeUri(uri).ToString());
            if (string.IsNullOrEmpty(str))
            {
                return Path.GetFileName(toPath);
            }
            return str.Replace('/', Path.DirectorySeparatorChar);
        }

        private void mnuFileExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void mnuFileNew_Click(object sender, EventArgs e)
        {
            this.Text = Application.ProductName;
            this.RestoreDefaults();
        }

        private void mnuFileOpen_Click(object sender, EventArgs e)
        {
            if (this.openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                this.RestoreSettings(this.openFileDialog1.FileName);
                this.mruMenu.AddFile(this.openFileDialog1.FileName);
                this.mruMenu.SetFirstFile(this.mruMenu.FindFilenameNumber(this.openFileDialog1.FileName));
                this.mruMenu.SaveToRegistry();
                this.saveFileDialog1.InitialDirectory = Path.GetDirectoryName(this.openFileDialog1.FileName);
                this.saveFileDialog1.FileName = Path.GetFileName(this.openFileDialog1.FileName);
            }
        }

        private void mnuFileSave_Click(object sender, EventArgs e)
        {
            if (this.saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                this.SaveSettings(this.saveFileDialog1.FileName);
            }
        }

        private void OnMruFile(int number, string filename)
        {
            if (File.Exists(filename))
            {
                this.RestoreSettings(filename);
                this.saveFileDialog1.InitialDirectory = Path.GetDirectoryName(filename);
                this.saveFileDialog1.FileName = Path.GetFileName(filename);
                this.mruMenu.SetFirstFile(number);
            }
            else if (MessageBox.Show("The file:\n\n'" + filename + "'\n\ncannot be opened.\n\nRemove this file from the MRU list?", AppTitle, MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                this.mruMenu.RemoveFile(number);
            }
            this.mruMenu.SaveToRegistry();
        }

        private void PreMerge()
        {
            if (string.IsNullOrEmpty(this.TxtKeyFile.Text))
            {
                this.ChkSignKeyFile.Checked = false;
            }
            if (string.IsNullOrEmpty(this.TxtLogFile.Text))
            {
                this.ChkGenerateLog.Checked = false;
            }
            if (this.TxtOutputAssembly.Text.Length < 5)
            {
                this.SelectOutputFile();
            }
        }

        private void ProcessFiles(string[] filenames)
        {
            base.UseWaitCursor = true;
            this.ListAssembly.BeginUpdate();
            bool flag = false;
            for (int i = 0; i < filenames.Length; i++)
            {
                if (File.Exists(filenames[i]))
                {
                    char[] trimChars = new char[] { '.' };
                    string key = Path.GetExtension(filenames[i]).ToLower().TrimStart(trimChars);
                    if (this.Extensions.ContainsKey(key))
                    {
                        flag = false;
                        for (int j = 0; j < this.ListAssembly.Items.Count; j++)
                        {
                            if (filenames[i] == ((string) this.ListAssembly.Items[j].Tag))
                            {
                                flag = true;
                            }
                        }
                        if (!flag)
                        {
                            ListViewItem item1 = this.ListAssembly.Items.Add(filenames[i]);
                            item1.SubItems.Add(AssemblyName.GetAssemblyName(filenames[i]).Version.ToString());
                            item1.Tag = filenames[i];
                            item1.Group = this.ListAssembly.Groups[key];
                        }
                    }
                }
                else if (Directory.Exists(filenames[i]))
                {
                    string[] files = Directory.GetFiles(filenames[i]);
                    this.ProcessFiles(files);
                    string[] directories = Directory.GetDirectories(filenames[i]);
                    this.ProcessFiles(directories);
                }
            }
            this.ListAssembly.Columns[0].Width = -1;
            this.ListAssembly.Columns[1].Width = -1;
            this.btnMerge.Enabled = this.ListAssembly.Items.Count > 1;
            if (string.IsNullOrEmpty(this.Primary) && (this.ListAssembly.Groups[0].Items.Count > 0))
            {
                this.ListAssembly.Groups[0].Items[0].Checked = true;
            }
            this.UpdatePrimary();
            this.FormatItems();
            this.SetWaterMark(this.ListAssembly.Items.Count == 0);
            this.ListAssembly.EndUpdate();
            base.UseWaitCursor = false;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton) sender).Checked)
            {
                this.LocateEngine(Merger.ILMerge);
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton) sender).Checked)
            {
                this.LocateEngine(Merger.ILRepack);
            }
        }

        public static bool RegisterIlProj()
        {
            if (DetectIlProj())
            {
                return true;
            }
            try
            {
                using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(".ilproj"))
                {
                    if (key == null)
                    {
                        return false;
                    }
                    if ((key.GetValue(null) == null) || !key.GetValue(null).Equals("ILMergeGui_file"))
                    {
                        key.SetValue(null, "ILMergeGui_file");
                    }
                    if ((key.GetValue("Content Type") == null) || !key.GetValue("Content Type").Equals($"application/{"ILMergeGui".ToLower()}"))
                    {
                        key.SetValue("Content Type", $"application/{"ILMergeGui".ToLower()}");
                    }
                    using (RegistryKey key2 = Registry.ClassesRoot.CreateSubKey("ILMergeGui_file"))
                    {
                        if (key2 != null)
                        {
                            if ((key2.GetValue(null) == null) || !key2.GetValue(null).Equals("ILMergeGui File"))
                            {
                                key2.SetValue(null, "ILMergeGui File");
                            }
                            if (key2.GetValue("EditFlags") == null)
                            {
                                key2.SetValue("EditFlags", new byte[4], RegistryValueKind.Binary);
                            }
                            if (key2.GetValue("AlwaysShowExt") == null)
                            {
                                key2.SetValue("AlwaysShowExt", string.Empty);
                            }
                            using (RegistryKey key3 = key2.CreateSubKey("DefaultIcon"))
                            {
                                if (key3 != null)
                                {
                                    if ((key3.GetValue(null) == null) || !key3.GetValue(null).Equals(Application.ExecutablePath + ",0"))
                                    {
                                        key3.SetValue(null, Application.ExecutablePath + ",0");
                                    }
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            using (RegistryKey key4 = key2.CreateSubKey(@"shell\open\command"))
                            {
                                if (key4 != null)
                                {
                                    if ((key4.GetValue(null) == null) || !key4.GetValue(null).Equals("\"" + Application.ExecutablePath + "\" \"%1\""))
                                    {
                                        key4.SetValue(null, "\"" + Application.ExecutablePath + "\" \"%1\"");
                                    }
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            using (RegistryKey key5 = key2.CreateSubKey(@"shell\merge\command"))
                            {
                                if (key5 != null)
                                {
                                    if ((key5.GetValue(null) == null) || !key5.GetValue(null).Equals("\"" + Application.ExecutablePath + "\" \"%1\" /Merge"))
                                    {
                                        key5.SetValue(null, "\"" + Application.ExecutablePath + "\" \"%1\" /Merge");
                                    }
                                    goto Label_02EB;
                                }
                                return false;
                            }
                        }
                        return false;
                    }
                }
            }
            catch
            {
                return false;
            }
        Label_02EB:
            return true;
        }

        private void RestoreDefaults()
        {
            this.ListAssembly.Items.Clear();
            this.Primary = string.Empty;
            this.LblPrimaryAssembly.Text = string.Empty;
            this.frameworks = InstalledDotNetVersions();
            using (List<DotNet>.Enumerator enumerator = this.frameworks.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    DotNet current = enumerator.Current;
                    bool flag1 = Environment.Is64BitOperatingSystem;
                }
            }
            this.CboTargetFramework.DataSource = this.frameworks;
            this.CboTargetFramework.SelectedIndex = this.frameworks.Count - 1;
            this.CboDebug.SelectedIndex = 1;
            this.ChkCopyAttributes.Checked = true;
            this.ChkUnionDuplicates.Checked = false;
            this.ChkSignKeyFile.Checked = false;
            this.ChkDelayedSign.Checked = false;
            this.TxtKeyFile.Text = string.Empty;
            this.TxtLogFile.Text = string.Empty;
            this.TxtOutputAssembly.Text = string.Empty;
        }

        private void RestoreSettings(string filename)
        {
            XDocument document = XDocument.Load(filename);
            this.Text = $"{Application.ProductName} - [{Path.GetFileName(filename)}]";
            this.ChkCopyAttributes.Checked = bool.Parse(document.Root.Element("CopyAttributes").Attribute("Enabled").Value);
            this.ChkUnionDuplicates.Checked = bool.Parse(document.Root.Element("UnionDuplicates").Attribute("Enabled").Value);
            this.CboDebug.SelectedIndex = int.Parse(document.Root.Element("Debug").Attribute("Enabled").Value);
            if (document.Root.Element("Internalize") != null)
            {
                this.ChkInternalize.Checked = bool.Parse(document.Root.Element("Internalize").Attribute("Enabled").Value);
            }
            if (document.Root.Element("MergeXml") != null)
            {
                this.ChkMergeXml.Checked = bool.Parse(document.Root.Element("MergeXml").Attribute("Enabled").Value);
            }
            this.ChkSignKeyFile.Checked = bool.Parse(document.Root.Element("Sign").Attribute("Enabled").Value);
            this.ChkDelayedSign.Checked = bool.Parse(document.Root.Element("Sign").Attribute("Delay").Value);
            this.TxtKeyFile.Text = document.Root.Element("Sign").Value;
            this.ChkGenerateLog.Checked = bool.Parse(document.Root.Element("Log").Attribute("Enabled").Value);
            this.TxtLogFile.Text = document.Root.Element("Log").Value;
            this.ListAssembly.Items.Clear();
            List<string> list = new List<string>();
            foreach (XElement element in document.Root.Element("Assemblies").Elements("Assembly"))
            {
                list.Add(element.Value);
            }
            this.ProcessFiles(list.ToArray());
            this.Primary = document.Root.Element("Assemblies").Element("Primary").Value;
            this.LblPrimaryAssembly.Text = Path.GetFileName(this.Primary);
            this.TxtOutputAssembly.Text = document.Root.Element("OutputAssembly").Value;
            string str = document.Root.Element("Framework").Value;
            foreach (object obj2 in this.CboTargetFramework.Items)
            {
                if (((DotNet) obj2).name.Equals(str))
                {
                    this.CboTargetFramework.SelectedItem = obj2;
                    break;
                }
            }
            if (document.Root.Element("Engine") != null)
            {
                this.Engine = (Merger) Enum.Parse(typeof(Merger), document.Root.Element("Engine").Attribute("Name").Value);
                this.LocateEngine(this.Engine);
                this.radioButton1.Checked = this.Engine == Merger.ILMerge;
                this.radioButton2.Checked = this.Engine == Merger.ILRepack;
            }
        }

        private void SaveSettings(string filename)
        {
            object[] content = new object[] { new XElement("Settings") };
            XDocument document = new XDocument(content);
            object[] objArray2 = new object[] { new XComment("Switches"), new XElement("CopyAttributes", new XAttribute("Enabled", this.ChkCopyAttributes.Checked)), new XElement("UnionDuplicates", new XAttribute("Enabled", this.ChkUnionDuplicates.Checked)), new XElement("Debug", new XAttribute("Enabled", this.CboDebug.SelectedIndex)), new XElement("Internalize", new XAttribute("Enabled", this.ChkInternalize.Checked)), new XElement("MergeXml", new XAttribute("Enabled", this.ChkMergeXml.Checked)) };
            document.Root.Add(objArray2);
            object[] objArray3 = new object[2];
            objArray3[0] = new XComment("Signing");
            object[] objArray4 = new object[] { new XAttribute("Enabled", this.ChkSignKeyFile.Checked), new XAttribute("Delay", this.ChkDelayedSign.Checked), new XText(this.TxtKeyFile.Text) };
            objArray3[1] = new XElement("Sign", objArray4);
            document.Root.Add(objArray3);
            object[] objArray5 = new object[2];
            objArray5[0] = new XComment("Logging");
            object[] objArray6 = new object[] { new XAttribute("Enabled", this.ChkGenerateLog.Checked), new XText(this.TxtLogFile.Text) };
            objArray5[1] = new XElement("Log", objArray6);
            document.Root.Add(objArray5);
            XElement element = new XElement("Assemblies");
            foreach (ListViewItem item in this.ListAssembly.Items)
            {
                element.Add(new XElement("Assembly", (string) item.Tag));
            }
            element.Add(new XElement("Primary", this.Primary));
            object[] objArray7 = new object[] { new XComment("Assemblies"), element };
            document.Root.Add(objArray7);
            object[] objArray8 = new object[] { new XComment("Output"), new XElement("OutputAssembly", this.TxtOutputAssembly.Text) };
            document.Root.Add(objArray8);
            if (this.CboTargetFramework.SelectedIndex != -1)
            {
                DotNet selectedItem = (DotNet) this.CboTargetFramework.SelectedItem;
                object[] objArray9 = new object[] { new XComment(".NET Framework"), new XElement("Framework", selectedItem.name) };
                document.Root.Add(objArray9);
            }
            object[] objArray10 = new object[] { new XComment("Merge Engine"), new XElement("Engine", new XAttribute("Name", this.Engine.ToString())) };
            document.Root.Add(objArray10);
            document.Save(filename);
            this.Text = $"{Application.ProductName} - [{Path.GetFileName(filename)}]";
        }

        private void SelectKeyFile()
        {
            this.SetOpenFileDefaults("Strong Name Key|*.snk");
            if (!string.IsNullOrEmpty(this.TxtKeyFile.Text))
            {
                this.openFile1.FileName = Path.GetFileName(this.TxtKeyFile.Text);
            }
            if (this.TxtKeyFile.Text.Length > 3)
            {
                this.openFile1.InitialDirectory = Path.GetDirectoryName(this.TxtKeyFile.Text);
            }
            if (this.openFile1.ShowDialog() == DialogResult.OK)
            {
                this.ChkSignKeyFile.Checked = true;
                this.TxtKeyFile.Text = this.openFile1.FileName;
                this.TxtKeyFile.Focus();
            }
        }

        private void SelectLogFile()
        {
            this.SetOpenFileDefaults("Log file|*.log");
            if (!string.IsNullOrEmpty(this.TxtLogFile.Text))
            {
                this.openFile1.FileName = Path.GetFileName(this.TxtLogFile.Text);
            }
            if (this.TxtLogFile.Text.Length > 3)
            {
                this.openFile1.InitialDirectory = Path.GetDirectoryName(this.TxtLogFile.Text);
            }
            if (this.openFile1.ShowDialog() == DialogResult.OK)
            {
                this.ChkGenerateLog.Checked = true;
                this.TxtLogFile.Text = this.openFile1.FileName;
                this.TxtLogFile.Focus();
            }
        }

        private void SelectOutputFile()
        {
            this.SetOpenFileDefaults("Assembly|*.dll;*.exe");
            if (this.ListAssembly.SelectedItems.Count != 0)
            {
                this.openFile1.FileName = Path.GetFileName((string) this.ListAssembly.SelectedItems[0].Tag);
            }
            if (this.TxtOutputAssembly.Text.Length > 3)
            {
                this.openFile1.InitialDirectory = Path.GetDirectoryName(this.TxtOutputAssembly.Text);
            }
            if (this.openFile1.ShowDialog() == DialogResult.OK)
            {
                this.TxtOutputAssembly.Text = this.openFile1.FileName;
                this.TxtOutputAssembly.Focus();
            }
        }

        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, uint lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, uint wParam, uint lParam);
        private void SetOpenFileDefaults(string filter)
        {
            this.openFile1.CheckFileExists = false;
            this.openFile1.Multiselect = false;
            this.openFile1.Filter = filter + "|All Files|*.*";
            this.openFile1.FileName = filter.Substring(filter.IndexOf('|') + 1);
        }

        private void SetWaterMark(bool show)
        {
            LVBKIMAGE structure = new LVBKIMAGE();
            if (show)
            {
                structure.ulFlags = LVBKIF.FLAG_ALPHABLEND | LVBKIF.TYPE_WATERMARK;
                structure.hbm = Resources.IconDropHere.GetHbitmap();
            }
            else
            {
                structure.ulFlags = LVBKIF.SOURCE_NONE;
            }
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(LVBKIMAGE)));
            Marshal.StructureToPtr(structure, ptr, false);
            SendMessage(this.ListAssembly.Handle, 0x108a, IntPtr.Zero, ptr);
            Marshal.FreeHGlobal(ptr);
            this.ListAssembly.Invalidate();
        }

        private static string TestDotnetPath(Version ver, string frameworkpath, string basepath)
        {
            if (string.IsNullOrEmpty(frameworkpath))
            {
                string path = Path.Combine(basepath, $"v{ver.Major}.{ver.Minor}");
                if (Directory.Exists(path))
                {
                    return path;
                }
            }
            if (string.IsNullOrEmpty(frameworkpath))
            {
                string str2 = Path.Combine(basepath, $"v{ver.Major}.{ver.Minor}.{ver.Build}");
                if (Directory.Exists(str2))
                {
                    return str2;
                }
            }
            return frameworkpath;
        }

        private void UpdatePrimary()
        {
            this.btnMerge.Enabled = this.ListAssembly.Items.Count > 0;
            if ((this.ListAssembly.CheckedItems != null) && (this.ListAssembly.CheckedItems.Count > 0))
            {
                this.Primary = (string) this.ListAssembly.CheckedItems[0].Tag;
                if (Path.GetFileName(this.Primary) != this.LblPrimaryAssembly.Text)
                {
                    this.LblPrimaryAssembly.Text = Path.GetFileName(this.Primary);
                }
            }
            else
            {
                this.Primary = string.Empty;
                if ("\x00b7\x00b7\x00b7" != this.LblPrimaryAssembly.Text)
                {
                    this.LblPrimaryAssembly.Text = "\x00b7\x00b7\x00b7";
                }
            }
        }

        private void visitWebsiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClickOnceUpdater.VisitWebsite("http://ilmergegui.codeplex.com//");
        }

        private void WorkerILMerge_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                DynaInvoke.GetClassReference(this.iLMergePath, this.ilMerge, null);
                Console.WriteLine("{0}.{0}()", this.ilMerge, "Merge");
                DynaInvoke.CallMethod(this.iLMergePath, this.ilMerge, "Merge", null, null);
                e.Result = null;
            }
            catch (Exception exception)
            {
                e.Result = exception;
            }
        }

        private void WorkerILMerge_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.EnableForm(true);
            this.ExitCode = 0;
            this.ExitMsg = string.Empty;
            Exception error = e.Error;
            if (e.Result != null)
            {
                string str = ((e.Result as Exception).InnerException == null) ? (e.Result as Exception).Message : (e.Result as Exception).InnerException.Message;
                this.ExitCode = 1;
                this.ExitMsg = Resources.Error_MergeException + Environment.NewLine + Environment.NewLine + str;
                if (!this.AutoClose)
                {
                    MessageBox.Show(this.ExitMsg, Resources.Error_Term, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            else if (!File.Exists(this.TxtOutputAssembly.Text) || (new FileInfo(this.TxtOutputAssembly.Text).Length == 0))
            {
                this.ExitCode = 2;
                this.ExitMsg = Resources.Error_CantMerge;
                if (!this.AutoClose)
                {
                    MessageBox.Show(this.ExitMsg, Resources.Error_Term, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
            }
            else
            {
                this.ExitMsg = Resources.AssembliesMerged;
                this.ExitCode = 0;
                if (!this.AutoClose)
                {
                    MessageBox.Show(this.ExitMsg, Resources.Done, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
            }
            if ((!this.AutoClose && this.ChkGenerateLog.Checked) && (File.Exists(this.TxtLogFile.Text) && (new FileInfo(this.TxtLogFile.Text).Length != 0)))
            {
                Process.Start(new ProcessStartInfo(this.TxtLogFile.Text));
            }
            if (this.AutoClose)
            {
                this.ExitILMergeGUI();
            }
        }

        internal static string AppDir =>
            Path.GetDirectoryName(Application.ExecutablePath);

        internal static string AppTitle =>
            "ILMergeGUI";

        internal Merger Engine { get; set; }

        public Dictionary<string, string> Extensions { get; private set; }

        public string ilMerge
        {
            get
            {
                Merger engine = this.Engine;
                if (engine != Merger.ILMerge)
                {
                    if (engine == Merger.ILRepack)
                    {
                        return "ILRepack";
                    }
                    return "None";
                }
                return "ILMerge";
            }
        }

        public string iLMergePath { get; private set; }

        public string Primary { get; private set; }

        [StructLayout(LayoutKind.Sequential)]
        public struct DotNet
        {
            public string key;
            public string name;
            public Version version;
            public string x64ProgramFilesPath;
            public string x64WindowsPath;
            public string x86ProgramFilesPath;
            public string x86WindowsPath;
            public override string ToString() => 
                this.name;
        }

        [Flags]
        internal enum LVBKIF
        {
            FLAG_ALPHABLEND = 0x20000000,
            FLAG_TILEOFFSET = 0x100,
            SOURCE_HBITMAP = 1,
            SOURCE_MASK = 3,
            SOURCE_NONE = 0,
            SOURCE_URL = 2,
            STYLE_MASK = 0x10,
            STYLE_NORMAL = 0,
            STYLE_TILE = 0x10,
            TYPE_WATERMARK = 0x10000000
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct LVBKIMAGE
        {
            public Mainform.LVBKIF ulFlags;
            public IntPtr hbm;
            public IntPtr pszImage;
            public int cchImageMax;
            public int xOffsetPercent;
            public int yOffsetPercent;
        }

        internal enum LVM
        {
            APPROXIMATEVIEWRECT = 0x1040,
            ARRANGE = 0x1016,
            CANCELEDITLABEL = 0x10b3,
            CREATEDRAGIMAGE = 0x1021,
            DELETEALLITEMS = 0x1009,
            DELETECOLUMN = 0x101c,
            DELETEITEM = 0x1008,
            EDITLABEL = 0x1076,
            EDITLABELA = 0x1017,
            EDITLABELW = 0x1076,
            ENABLEGROUPVIEW = 0x109d,
            ENSUREVISIBLE = 0x1013,
            FINDITEMA = 0x100d,
            FINDITEMW = 0x1053,
            FIRST = 0x1000,
            GETACCVERSION = 0x10c1,
            GETBKCOLOR = 0x1000,
            GETBKIMAGE = 0x108b,
            GETBKIMAGEA = 0x1045,
            GETBKIMAGEW = 0x108b,
            GETCALLBACKMASK = 0x100a,
            GETCOLUMNA = 0x1019,
            GETCOLUMNORDERARRAY = 0x103b,
            GETCOLUMNW = 0x105f,
            GETCOLUMNWIDTH = 0x101d,
            GETCOUNTPERPAGE = 0x1028,
            GETEDITCONTROL = 0x1018,
            GETEMPTYTEXT = 0x10cc,
            GETEXTENDEDLISTVIEWSTYLE = 0x1037,
            GETFOCUSEDGROUP = 0x105d,
            GETFOOTERINFO = 0x10ce,
            GETFOOTERITEM = 0x10d0,
            GETFOOTERITEMRECT = 0x10cf,
            GETFOOTERRECT = 0x10cd,
            GETGROUPCOUNT = 0x1098,
            GETGROUPINFO = 0x1095,
            GETGROUPINFOBYINDEX = 0x1099,
            GETGROUPMETRICS = 0x109c,
            GETGROUPRECT = 0x1062,
            GETGROUPSTATE = 0x105c,
            GETHEADER = 0x101f,
            GETHOTCURSOR = 0x103f,
            GETHOTITEM = 0x103d,
            GETHOVERTIME = 0x1048,
            GETIMAGELIST = 0x1002,
            GETINSERTMARK = 0x10a7,
            GETINSERTMARKCOLOR = 0x10ab,
            GETINSERTMARKRECT = 0x10a9,
            GETISEARCHSTRING = 0x1075,
            GETISEARCHSTRINGA = 0x1034,
            GETISEARCHSTRINGW = 0x1075,
            GETITEM = 0x104b,
            GETITEMA = 0x1005,
            GETITEMCOUNT = 0x1004,
            GETITEMINDEXRECT = 0x10d1,
            GETITEMPOSITION = 0x1010,
            GETITEMRECT = 0x100e,
            GETITEMSPACING = 0x1033,
            GETITEMSTATE = 0x102c,
            GETITEMTEXTA = 0x102d,
            GETITEMTEXTW = 0x1073,
            GETITEMW = 0x104b,
            GETNEXTITEM = 0x100c,
            GETNEXTITEMINDEX = 0x10d3,
            GETNUMBEROFWORKAREAS = 0x1049,
            GETORIGIN = 0x1029,
            GETOUTLINECOLOR = 0x10b0,
            GETSELECTEDCOLUMN = 0x10ae,
            GETSELECTEDCOUNT = 0x1032,
            GETSELECTIONMARK = 0x1042,
            GETSTRINGWIDTHA = 0x1011,
            GETSTRINGWIDTHW = 0x1057,
            GETSUBITEMRECT = 0x1038,
            GETTEXTBKCOLOR = 0x1025,
            GETTEXTCOLOR = 0x1023,
            GETTILEINFO = 0x10a5,
            GETTILEVIEWINFO = 0x10a3,
            GETTOOLTIPS = 0x104e,
            GETTOPINDEX = 0x1027,
            GETUNICODEFORMAT = 0x2006,
            GETVIEW = 0x108f,
            GETVIEWRECT = 0x1022,
            GETWORKAREAS = 0x1046,
            HASGROUP = 0x10a1,
            HITTEST = 0x1012,
            INSERTCOLUMNA = 0x101b,
            INSERTCOLUMNW = 0x1061,
            INSERTGROUP = 0x1091,
            INSERTGROUPSORTED = 0x109f,
            INSERTITEM = 0x104d,
            INSERTITEMA = 0x1007,
            INSERTITEMW = 0x104d,
            INSERTMARKHITTEST = 0x10a8,
            ISGROUPVIEWENABLED = 0x10af,
            ISITEMVISIBLE = 0x10b6,
            MAPIDTOINDEX = 0x10b5,
            MAPINDEXTOID = 0x10b4,
            MOVEGROUP = 0x1097,
            MOVEITEMTOGROUP = 0x109a,
            REDRAWITEMS = 0x1015,
            REMOVEALLGROUPS = 0x10a0,
            REMOVEGROUP = 0x1096,
            SCROLL = 0x1014,
            SETBKCOLOR = 0x1001,
            SETBKIMAGE = 0x108a,
            SETBKIMAGEA = 0x1044,
            SETBKIMAGEW = 0x108a,
            SETCALLBACKMASK = 0x100b,
            SETCOLUMNA = 0x101a,
            SETCOLUMNORDERARRAY = 0x103a,
            SETCOLUMNW = 0x1060,
            SETCOLUMNWIDTH = 0x101e,
            SETEXTENDEDLISTVIEWSTYLE = 0x1036,
            SETGROUPINFO = 0x1093,
            SETGROUPMETRICS = 0x109b,
            SETHOTCURSOR = 0x103e,
            SETHOTITEM = 0x103c,
            SETHOVERTIME = 0x1047,
            SETICONSPACING = 0x1035,
            SETIMAGELIST = 0x1003,
            SETINSERTMARK = 0x10a6,
            SETINSERTMARKCOLOR = 0x10aa,
            SETITEM = 0x104c,
            SETITEMA = 0x1006,
            SETITEMCOUNT = 0x102f,
            SETITEMINDEXSTATE = 0x10d2,
            SETITEMPOSITION = 0x100f,
            SETITEMPOSITION32 = 0x1031,
            SETITEMSTATE = 0x102b,
            SETITEMTEXTA = 0x102e,
            SETITEMTEXTW = 0x1074,
            SETITEMW = 0x104c,
            SETOUTLINECOLOR = 0x10b1,
            SETPRESERVEALPHA = 0x10d4,
            SETSELECTEDCOLUMN = 0x108c,
            SETSELECTIONMARK = 0x1043,
            SETTEXTBKCOLOR = 0x1026,
            SETTEXTCOLOR = 0x1024,
            SETTILEINFO = 0x10a4,
            SETTILEVIEWINFO = 0x10a2,
            SETTOOLTIPS = 0x104a,
            SETUNICODEFORMAT = 0x2005,
            SETVIEW = 0x108e,
            SETWORKAREAS = 0x1041,
            SORTGROUPS = 0x109e,
            SORTITEMS = 0x1030,
            SORTITEMSEX = 0x1051,
            SUBITEMHITTEST = 0x1039,
            UPDATE = 0x102a
        }

        internal enum LVS
        {
            EX_DOUBLEBUFFER = 0x10000
        }

        [Description("Merging Application")]
        internal enum Merger
        {
            [Description("Microsoft's IL-Merge")]
            ILMerge = 1,
            [Description("Mono Based IL-Repack")]
            ILRepack = 2,
            [Description("No Merging Application")]
            None = 0
        }
    }
}

