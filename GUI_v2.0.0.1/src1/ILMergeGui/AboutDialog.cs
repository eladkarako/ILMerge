namespace ILMergeGui
{
    using ILMergeGui.Properties;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Reflection;
    using System.Windows.Forms;

    public class AboutDialog : Form
    {
        private Button button1;
        private IContainer components;
        private LinkLabel linkLabel1;
        private LinkLabel linkLabel2;
        private const string paypal_picasadownloader = "https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=84Y4JSJE47R7J";
        private const string picasadownloader_at_codeplex = "http://ilmergegui.codeplex.com";
        private const string picasadownloader_mailto = "mailto:wim@vander-vegt.nl?SUBJECT=Suggestion for ILMergeGui";
        private PictureBox pictureBox1;
        private TextBox textBox1;

        public AboutDialog()
        {
            this.InitializeComponent();
            this.textBox1.Text = string.Format(this.textBox1.Text, Assembly.GetExecutingAssembly().GetName().Version);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            ComponentResourceManager manager = new ComponentResourceManager(typeof(AboutDialog));
            this.pictureBox1 = new PictureBox();
            this.button1 = new Button();
            this.textBox1 = new TextBox();
            this.linkLabel1 = new LinkLabel();
            this.linkLabel2 = new LinkLabel();
            ((ISupportInitialize) this.pictureBox1).BeginInit();
            base.SuspendLayout();
            this.pictureBox1.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            this.pictureBox1.Cursor = Cursors.Hand;
            this.pictureBox1.Image = Resources.btn_donateCC_LG_EN;
            this.pictureBox1.Location = new Point(12, 0x8e);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new Size(0x14b, 100);
            this.pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new EventHandler(this.pictureBox1_Click);
            this.button1.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this.button1.DialogResult = DialogResult.OK;
            this.button1.Location = new Point(0x10c, 0xf8);
            this.button1.Name = "button1";
            this.button1.Size = new Size(0x4b, 0x17);
            this.button1.TabIndex = 1;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            this.textBox1.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.textBox1.BorderStyle = BorderStyle.None;
            this.textBox1.Location = new Point(12, 13);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new Size(0x14b, 0x6b);
            this.textBox1.TabIndex = 2;
            this.textBox1.Text = manager.GetString("textBox1.Text");
            this.textBox1.TextAlign = HorizontalAlignment.Center;
            this.linkLabel1.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new Point(12, 0x7b);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new Size(0x79, 13);
            this.linkLabel1.TabIndex = 3;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "ILMergeGui at Codeplex";
            this.linkLabel1.LinkClicked += new LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            this.linkLabel2.AutoSize = true;
            this.linkLabel2.Location = new Point(12, 0xf8);
            this.linkLabel2.Name = "linkLabel2";
            this.linkLabel2.Size = new Size(0x9e, 13);
            this.linkLabel2.TabIndex = 4;
            this.linkLabel2.TabStop = true;
            this.linkLabel2.Text = "Mail suggestion for improvement";
            this.linkLabel2.LinkClicked += new LinkLabelLinkClickedEventHandler(this.linkLabel2_LinkClicked);
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(0x163, 0x11b);
            base.Controls.Add(this.linkLabel2);
            base.Controls.Add(this.linkLabel1);
            base.Controls.Add(this.textBox1);
            base.Controls.Add(this.button1);
            base.Controls.Add(this.pictureBox1);
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.Name = "AboutDialog";
            this.Text = "About Ghostbuster";
            ((ISupportInitialize) this.pictureBox1).EndInit();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start("http://ilmergegui.codeplex.com");
            }
            catch (Win32Exception exception1)
            {
                MessageBox.Show(exception1.Message);
            }
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start("mailto:wim@vander-vegt.nl?SUBJECT=Suggestion for ILMergeGui");
            }
            catch (Win32Exception exception1)
            {
                MessageBox.Show(exception1.Message);
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=84Y4JSJE47R7J");
            }
            catch (Win32Exception exception1)
            {
                MessageBox.Show(exception1.Message);
            }
        }
    }
}

