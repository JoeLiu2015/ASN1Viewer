namespace ASN1Viewer
{
  partial class MainForm
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
      this.treeView1 = new System.Windows.Forms.TreeView();
      this.splitContainer1 = new System.Windows.Forms.SplitContainer();
      this.tabControl1 = new System.Windows.Forms.TabControl();
      this.tabPageInput = new System.Windows.Forms.TabPage();
      this.txtInput = new System.Windows.Forms.TextBox();
      this.tabPageBytes = new System.Windows.Forms.TabPage();
      this.panel1 = new System.Windows.Forms.Panel();
      this.hexViewer1 = new ASN1Viewer.HexViewer();
      this.menuStrip1 = new System.Windows.Forms.MenuStrip();
      this.menuFile = new System.Windows.Forms.ToolStripMenuItem();
      this.menuOpen = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
      this.menuExit = new System.Windows.Forms.ToolStripMenuItem();
      this.menuRecent = new System.Windows.Forms.ToolStripMenuItem();
      this.menuOptions = new System.Windows.Forms.ToolStripMenuItem();
      this.menuHelp = new System.Windows.Forms.ToolStripMenuItem();
      this.menuLanguage = new System.Windows.Forms.ToolStripMenuItem();
      this.menuChinese = new System.Windows.Forms.ToolStripMenuItem();
      this.menuEnglish = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
      this.menuAbout = new System.Windows.Forms.ToolStripMenuItem();
      this.statusStrip1 = new System.Windows.Forms.StatusStrip();
      this.lbStatus = new System.Windows.Forms.ToolStripStatusLabel();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
      this.splitContainer1.Panel1.SuspendLayout();
      this.splitContainer1.Panel2.SuspendLayout();
      this.splitContainer1.SuspendLayout();
      this.tabControl1.SuspendLayout();
      this.tabPageInput.SuspendLayout();
      this.tabPageBytes.SuspendLayout();
      this.panel1.SuspendLayout();
      this.menuStrip1.SuspendLayout();
      this.statusStrip1.SuspendLayout();
      this.SuspendLayout();
      // 
      // treeView1
      // 
      this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.treeView1.Location = new System.Drawing.Point(0, 0);
      this.treeView1.Name = "treeView1";
      this.treeView1.Size = new System.Drawing.Size(228, 394);
      this.treeView1.TabIndex = 0;
      this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
      // 
      // splitContainer1
      // 
      this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer1.Location = new System.Drawing.Point(0, 24);
      this.splitContainer1.Name = "splitContainer1";
      // 
      // splitContainer1.Panel1
      // 
      this.splitContainer1.Panel1.Controls.Add(this.treeView1);
      // 
      // splitContainer1.Panel2
      // 
      this.splitContainer1.Panel2.Controls.Add(this.tabControl1);
      this.splitContainer1.Size = new System.Drawing.Size(853, 394);
      this.splitContainer1.SplitterDistance = 228;
      this.splitContainer1.TabIndex = 1;
      // 
      // tabControl1
      // 
      this.tabControl1.Controls.Add(this.tabPageInput);
      this.tabControl1.Controls.Add(this.tabPageBytes);
      this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tabControl1.Location = new System.Drawing.Point(0, 0);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(621, 394);
      this.tabControl1.TabIndex = 0;
      // 
      // tabPageInput
      // 
      this.tabPageInput.Controls.Add(this.txtInput);
      this.tabPageInput.Location = new System.Drawing.Point(4, 22);
      this.tabPageInput.Name = "tabPageInput";
      this.tabPageInput.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageInput.Size = new System.Drawing.Size(613, 368);
      this.tabPageInput.TabIndex = 0;
      this.tabPageInput.Text = "Input Text";
      this.tabPageInput.UseVisualStyleBackColor = true;
      // 
      // txtInput
      // 
      this.txtInput.Dock = System.Windows.Forms.DockStyle.Fill;
      this.txtInput.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.txtInput.Location = new System.Drawing.Point(3, 3);
      this.txtInput.Multiline = true;
      this.txtInput.Name = "txtInput";
      this.txtInput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.txtInput.Size = new System.Drawing.Size(607, 362);
      this.txtInput.TabIndex = 0;
      this.txtInput.WordWrap = false;
      this.txtInput.TextChanged += new System.EventHandler(this.txtInput_TextChanged);
      // 
      // tabPageBytes
      // 
      this.tabPageBytes.Controls.Add(this.panel1);
      this.tabPageBytes.Location = new System.Drawing.Point(4, 22);
      this.tabPageBytes.Name = "tabPageBytes";
      this.tabPageBytes.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageBytes.Size = new System.Drawing.Size(613, 368);
      this.tabPageBytes.TabIndex = 1;
      this.tabPageBytes.Text = "ASN.1 Bytes";
      this.tabPageBytes.UseVisualStyleBackColor = true;
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.hexViewer1);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel1.Location = new System.Drawing.Point(3, 3);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(607, 362);
      this.panel1.TabIndex = 0;
      // 
      // hexViewer1
      // 
      this.hexViewer1.BackColor = System.Drawing.SystemColors.Window;
      this.hexViewer1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.hexViewer1.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.hexViewer1.ForeColor = System.Drawing.Color.Gray;
      this.hexViewer1.Location = new System.Drawing.Point(0, 0);
      this.hexViewer1.Name = "hexViewer1";
      this.hexViewer1.ReadOnly = true;
      this.hexViewer1.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
      this.hexViewer1.Size = new System.Drawing.Size(607, 362);
      this.hexViewer1.TabIndex = 0;
      this.hexViewer1.Text = "";
      this.hexViewer1.WordWrap = false;
      // 
      // menuStrip1
      // 
      this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
      this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuFile,
            this.menuOptions,
            this.menuHelp});
      this.menuStrip1.Location = new System.Drawing.Point(0, 0);
      this.menuStrip1.Name = "menuStrip1";
      this.menuStrip1.Size = new System.Drawing.Size(853, 24);
      this.menuStrip1.TabIndex = 2;
      this.menuStrip1.Text = "menuStrip1";
      // 
      // menuFile
      // 
      this.menuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuOpen,
            this.toolStripMenuItem1,
            this.menuExit,
            this.menuRecent});
      this.menuFile.Name = "menuFile";
      this.menuFile.Size = new System.Drawing.Size(37, 20);
      this.menuFile.Text = "&File";
      // 
      // menuOpen
      // 
      this.menuOpen.Name = "menuOpen";
      this.menuOpen.Size = new System.Drawing.Size(180, 22);
      this.menuOpen.Text = "&Open...";
      this.menuOpen.Click += new System.EventHandler(this.menuOpen_Click);
      // 
      // toolStripMenuItem1
      // 
      this.toolStripMenuItem1.Name = "toolStripMenuItem1";
      this.toolStripMenuItem1.Size = new System.Drawing.Size(177, 6);
      // 
      // menuExit
      // 
      this.menuExit.Name = "menuExit";
      this.menuExit.Size = new System.Drawing.Size(180, 22);
      this.menuExit.Text = "&Exit";
      this.menuExit.Click += new System.EventHandler(this.menuExit_Click);
      // 
      // menuRecent
      // 
      this.menuRecent.Name = "menuRecent";
      this.menuRecent.Size = new System.Drawing.Size(180, 22);
      this.menuRecent.Text = "Recent Files";
      // 
      // menuOptions
      // 
      this.menuOptions.Name = "menuOptions";
      this.menuOptions.Size = new System.Drawing.Size(61, 20);
      this.menuOptions.Text = "&Options";
      // 
      // menuHelp
      // 
      this.menuHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuLanguage,
            this.toolStripMenuItem2,
            this.menuAbout});
      this.menuHelp.Name = "menuHelp";
      this.menuHelp.Size = new System.Drawing.Size(44, 20);
      this.menuHelp.Text = "&Help";
      // 
      // menuLanguage
      // 
      this.menuLanguage.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuChinese,
            this.menuEnglish});
      this.menuLanguage.Name = "menuLanguage";
      this.menuLanguage.Size = new System.Drawing.Size(126, 22);
      this.menuLanguage.Text = "Language";
      // 
      // menuChinese
      // 
      this.menuChinese.Name = "menuChinese";
      this.menuChinese.Size = new System.Drawing.Size(116, 22);
      this.menuChinese.Text = "Chinese";
      this.menuChinese.Click += new System.EventHandler(this.menuChinese_Click);
      // 
      // menuEnglish
      // 
      this.menuEnglish.Checked = true;
      this.menuEnglish.CheckState = System.Windows.Forms.CheckState.Checked;
      this.menuEnglish.Name = "menuEnglish";
      this.menuEnglish.Size = new System.Drawing.Size(116, 22);
      this.menuEnglish.Text = "English";
      this.menuEnglish.Click += new System.EventHandler(this.menuEnglish_Click);
      // 
      // toolStripMenuItem2
      // 
      this.toolStripMenuItem2.Name = "toolStripMenuItem2";
      this.toolStripMenuItem2.Size = new System.Drawing.Size(123, 6);
      // 
      // menuAbout
      // 
      this.menuAbout.Name = "menuAbout";
      this.menuAbout.Size = new System.Drawing.Size(126, 22);
      this.menuAbout.Text = "&About";
      this.menuAbout.Click += new System.EventHandler(this.menuAbout_Click);
      // 
      // statusStrip1
      // 
      this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
      this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lbStatus});
      this.statusStrip1.Location = new System.Drawing.Point(0, 418);
      this.statusStrip1.Name = "statusStrip1";
      this.statusStrip1.Size = new System.Drawing.Size(853, 26);
      this.statusStrip1.TabIndex = 3;
      this.statusStrip1.Text = "statusStrip1";
      // 
      // lbStatus
      // 
      this.lbStatus.AutoSize = false;
      this.lbStatus.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
      this.lbStatus.Name = "lbStatus";
      this.lbStatus.Size = new System.Drawing.Size(400, 21);
      this.lbStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // MainForm
      // 
      this.AllowDrop = true;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(853, 444);
      this.Controls.Add(this.splitContainer1);
      this.Controls.Add(this.menuStrip1);
      this.Controls.Add(this.statusStrip1);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MainMenuStrip = this.menuStrip1;
      this.MinimumSize = new System.Drawing.Size(869, 482);
      this.Name = "MainForm";
      this.Text = "ASN.1 Viewer";
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
      this.Load += new System.EventHandler(this.MainForm_Load);
      this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
      this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
      this.splitContainer1.Panel1.ResumeLayout(false);
      this.splitContainer1.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
      this.splitContainer1.ResumeLayout(false);
      this.tabControl1.ResumeLayout(false);
      this.tabPageInput.ResumeLayout(false);
      this.tabPageInput.PerformLayout();
      this.tabPageBytes.ResumeLayout(false);
      this.panel1.ResumeLayout(false);
      this.menuStrip1.ResumeLayout(false);
      this.menuStrip1.PerformLayout();
      this.statusStrip1.ResumeLayout(false);
      this.statusStrip1.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TreeView treeView1;
    private System.Windows.Forms.SplitContainer splitContainer1;
    private System.Windows.Forms.TabControl tabControl1;
    private System.Windows.Forms.TabPage tabPageInput;
    private System.Windows.Forms.TabPage tabPageBytes;
    private System.Windows.Forms.MenuStrip menuStrip1;
    private System.Windows.Forms.ToolStripMenuItem menuFile;
    private System.Windows.Forms.ToolStripMenuItem menuOptions;
    private System.Windows.Forms.ToolStripMenuItem menuHelp;
    private System.Windows.Forms.StatusStrip statusStrip1;
    private System.Windows.Forms.TextBox txtInput;
    private System.Windows.Forms.Panel panel1;
    private HexViewer hexViewer1;
    private System.Windows.Forms.ToolStripMenuItem menuOpen;
    private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
    private System.Windows.Forms.ToolStripMenuItem menuExit;
    private System.Windows.Forms.ToolStripMenuItem menuAbout;
    private System.Windows.Forms.ToolStripStatusLabel lbStatus;
    private System.Windows.Forms.ToolStripMenuItem menuLanguage;
    private System.Windows.Forms.ToolStripMenuItem menuChinese;
    private System.Windows.Forms.ToolStripMenuItem menuEnglish;
    private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
    private System.Windows.Forms.ToolStripMenuItem menuRecent;
  }
}

