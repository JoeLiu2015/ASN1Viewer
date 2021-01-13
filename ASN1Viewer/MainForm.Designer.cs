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
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
      this.treeView1 = new System.Windows.Forms.TreeView();
      this.imageListDataType = new System.Windows.Forms.ImageList(this.components);
      this.splitContainer1 = new System.Windows.Forms.SplitContainer();
      this.tabControl1 = new System.Windows.Forms.TabControl();
      this.tabPageInput = new System.Windows.Forms.TabPage();
      this.txtInput = new System.Windows.Forms.RichTextBox();
      this.tabPageBytes = new System.Windows.Forms.TabPage();
      this.panel1 = new System.Windows.Forms.Panel();
      this.menuMainStrip = new System.Windows.Forms.MenuStrip();
      this.menuFile = new System.Windows.Forms.ToolStripMenuItem();
      this.menuOpen = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
      this.menuExit = new System.Windows.Forms.ToolStripMenuItem();
      this.menuRecent = new System.Windows.Forms.ToolStripMenuItem();
      this.menuView = new System.Windows.Forms.ToolStripMenuItem();
      this.menuTopMost = new System.Windows.Forms.ToolStripMenuItem();
      this.menuASN1Modules = new System.Windows.Forms.ToolStripMenuItem();
      this.menuTestFiles = new System.Windows.Forms.ToolStripMenuItem();
      this.menuHelp = new System.Windows.Forms.ToolStripMenuItem();
      this.menuLanguage = new System.Windows.Forms.ToolStripMenuItem();
      this.menuChinese = new System.Windows.Forms.ToolStripMenuItem();
      this.menuEnglish = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
      this.menuAbout = new System.Windows.Forms.ToolStripMenuItem();
      this.menuCheckUpdate = new System.Windows.Forms.ToolStripMenuItem();
      this.statusStrip1 = new System.Windows.Forms.StatusStrip();
      this.lbStatus = new System.Windows.Forms.ToolStripStatusLabel();
      this.lbStatusRight = new System.Windows.Forms.ToolStripStatusLabel();
      this.ctxMenuTree = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.ctxMenuCollapse = new System.Windows.Forms.ToolStripMenuItem();
      this.ctxMenuExpand = new System.Windows.Forms.ToolStripMenuItem();
      this.toolbar = new System.Windows.Forms.ToolStrip();
      this.imageListToolbar = new System.Windows.Forms.ImageList(this.components);
      this.tbBtnOpen = new System.Windows.Forms.ToolStripButton();
      this.tbBtnRecent = new System.Windows.Forms.ToolStripDropDownButton();
      this.tbBtnTopMost = new System.Windows.Forms.ToolStripButton();
      this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
      this.tbBtnCheckUpdate = new System.Windows.Forms.ToolStripButton();
      this.tbBtnLang = new System.Windows.Forms.ToolStripDropDownButton();
      this.hexViewer1 = new ASN1Viewer.HexViewer();
      this.splitContainer1.Panel1.SuspendLayout();
      this.splitContainer1.Panel2.SuspendLayout();
      this.splitContainer1.SuspendLayout();
      this.tabControl1.SuspendLayout();
      this.tabPageInput.SuspendLayout();
      this.tabPageBytes.SuspendLayout();
      this.panel1.SuspendLayout();
      this.menuMainStrip.SuspendLayout();
      this.statusStrip1.SuspendLayout();
      this.ctxMenuTree.SuspendLayout();
      this.toolbar.SuspendLayout();
      this.SuspendLayout();
      // 
      // treeView1
      // 
      this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.treeView1.ImageIndex = 0;
      this.treeView1.ImageList = this.imageListDataType;
      this.treeView1.Location = new System.Drawing.Point(0, 0);
      this.treeView1.Name = "treeView1";
      this.treeView1.SelectedImageIndex = 0;
      this.treeView1.Size = new System.Drawing.Size(228, 369);
      this.treeView1.TabIndex = 0;
      this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
      this.treeView1.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeView1_NodeMouseClick);
      // 
      // imageListDataType
      // 
      this.imageListDataType.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListDataType.ImageStream")));
      this.imageListDataType.TransparentColor = System.Drawing.Color.Transparent;
      this.imageListDataType.Images.SetKeyName(0, "");
      this.imageListDataType.Images.SetKeyName(1, "");
      this.imageListDataType.Images.SetKeyName(2, "");
      this.imageListDataType.Images.SetKeyName(3, "");
      this.imageListDataType.Images.SetKeyName(4, "");
      this.imageListDataType.Images.SetKeyName(5, "");
      this.imageListDataType.Images.SetKeyName(6, "");
      this.imageListDataType.Images.SetKeyName(7, "");
      this.imageListDataType.Images.SetKeyName(8, "");
      this.imageListDataType.Images.SetKeyName(9, "");
      this.imageListDataType.Images.SetKeyName(10, "");
      this.imageListDataType.Images.SetKeyName(11, "");
      this.imageListDataType.Images.SetKeyName(12, "");
      this.imageListDataType.Images.SetKeyName(13, "");
      this.imageListDataType.Images.SetKeyName(14, "");
      this.imageListDataType.Images.SetKeyName(15, "");
      this.imageListDataType.Images.SetKeyName(16, "");
      this.imageListDataType.Images.SetKeyName(17, "");
      this.imageListDataType.Images.SetKeyName(18, "");
      this.imageListDataType.Images.SetKeyName(19, "");
      this.imageListDataType.Images.SetKeyName(20, "");
      this.imageListDataType.Images.SetKeyName(21, "");
      this.imageListDataType.Images.SetKeyName(22, "");
      this.imageListDataType.Images.SetKeyName(23, "");
      this.imageListDataType.Images.SetKeyName(24, "");
      this.imageListDataType.Images.SetKeyName(25, "");
      this.imageListDataType.Images.SetKeyName(26, "");
      this.imageListDataType.Images.SetKeyName(27, "");
      this.imageListDataType.Images.SetKeyName(28, "");
      this.imageListDataType.Images.SetKeyName(29, "");
      this.imageListDataType.Images.SetKeyName(30, "");
      this.imageListDataType.Images.SetKeyName(31, "");
      this.imageListDataType.Images.SetKeyName(32, "");
      this.imageListDataType.Images.SetKeyName(33, "");
      this.imageListDataType.Images.SetKeyName(34, "");
      this.imageListDataType.Images.SetKeyName(35, "");
      // 
      // splitContainer1
      // 
      this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer1.Location = new System.Drawing.Point(0, 49);
      this.splitContainer1.Name = "splitContainer1";
      // 
      // splitContainer1.Panel1
      // 
      this.splitContainer1.Panel1.Controls.Add(this.treeView1);
      // 
      // splitContainer1.Panel2
      // 
      this.splitContainer1.Panel2.Controls.Add(this.tabControl1);
      this.splitContainer1.Size = new System.Drawing.Size(853, 369);
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
      this.tabControl1.Size = new System.Drawing.Size(621, 369);
      this.tabControl1.TabIndex = 0;
      // 
      // tabPageInput
      // 
      this.tabPageInput.Controls.Add(this.txtInput);
      this.tabPageInput.Location = new System.Drawing.Point(4, 22);
      this.tabPageInput.Name = "tabPageInput";
      this.tabPageInput.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageInput.Size = new System.Drawing.Size(613, 343);
      this.tabPageInput.TabIndex = 0;
      this.tabPageInput.Text = "Input Text";
      this.tabPageInput.UseVisualStyleBackColor = true;
      // 
      // txtInput
      // 
      this.txtInput.Dock = System.Windows.Forms.DockStyle.Fill;
      this.txtInput.Location = new System.Drawing.Point(3, 3);
      this.txtInput.Name = "txtInput";
      this.txtInput.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
      this.txtInput.Size = new System.Drawing.Size(607, 337);
      this.txtInput.TabIndex = 0;
      this.txtInput.Text = "";
      this.txtInput.WordWrap = false;
      this.txtInput.TextChanged += new System.EventHandler(this.txtInput_TextChanged);
      // 
      // tabPageBytes
      // 
      this.tabPageBytes.Controls.Add(this.panel1);
      this.tabPageBytes.Location = new System.Drawing.Point(4, 22);
      this.tabPageBytes.Name = "tabPageBytes";
      this.tabPageBytes.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageBytes.Size = new System.Drawing.Size(613, 343);
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
      this.panel1.Size = new System.Drawing.Size(607, 337);
      this.panel1.TabIndex = 0;
      // 
      // menuMainStrip
      // 
      this.menuMainStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
      this.menuMainStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuFile,
            this.menuView,
            this.menuHelp});
      this.menuMainStrip.Location = new System.Drawing.Point(0, 0);
      this.menuMainStrip.Name = "menuMainStrip";
      this.menuMainStrip.Padding = new System.Windows.Forms.Padding(3, 1, 0, 1);
      this.menuMainStrip.Size = new System.Drawing.Size(853, 24);
      this.menuMainStrip.TabIndex = 2;
      this.menuMainStrip.Text = "menuMainStrip";
      // 
      // menuFile
      // 
      this.menuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuOpen,
            this.toolStripMenuItem1,
            this.menuExit,
            this.menuRecent});
      this.menuFile.Name = "menuFile";
      this.menuFile.Size = new System.Drawing.Size(37, 22);
      this.menuFile.Text = "&File";
      // 
      // menuOpen
      // 
      this.menuOpen.Name = "menuOpen";
      this.menuOpen.Size = new System.Drawing.Size(136, 22);
      this.menuOpen.Text = "&Open...";
      this.menuOpen.Click += new System.EventHandler(this.menuOpen_Click);
      // 
      // toolStripMenuItem1
      // 
      this.toolStripMenuItem1.Name = "toolStripMenuItem1";
      this.toolStripMenuItem1.Size = new System.Drawing.Size(133, 6);
      // 
      // menuExit
      // 
      this.menuExit.Name = "menuExit";
      this.menuExit.Size = new System.Drawing.Size(136, 22);
      this.menuExit.Text = "&Exit";
      this.menuExit.Click += new System.EventHandler(this.menuExit_Click);
      // 
      // menuRecent
      // 
      this.menuRecent.Name = "menuRecent";
      this.menuRecent.Size = new System.Drawing.Size(136, 22);
      this.menuRecent.Text = "Recent Files";
      // 
      // menuView
      // 
      this.menuView.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuTopMost,
            this.menuASN1Modules,
            this.menuTestFiles});
      this.menuView.Name = "menuView";
      this.menuView.Size = new System.Drawing.Size(44, 22);
      this.menuView.Text = "&View";
      // 
      // menuTopMost
      // 
      this.menuTopMost.Checked = true;
      this.menuTopMost.CheckState = System.Windows.Forms.CheckState.Checked;
      this.menuTopMost.Name = "menuTopMost";
      this.menuTopMost.Size = new System.Drawing.Size(155, 22);
      this.menuTopMost.Text = "Alwasy on top";
      this.menuTopMost.Click += new System.EventHandler(this.menuTopMost_Click);
      // 
      // menuASN1Modules
      // 
      this.menuASN1Modules.Name = "menuASN1Modules";
      this.menuASN1Modules.Size = new System.Drawing.Size(155, 22);
      this.menuASN1Modules.Text = "&ASN.1 Modules";
      this.menuASN1Modules.Click += new System.EventHandler(this.menuASN1Modules_Click);
      // 
      // menuTestFiles
      // 
      this.menuTestFiles.Name = "menuTestFiles";
      this.menuTestFiles.Size = new System.Drawing.Size(155, 22);
      this.menuTestFiles.Text = "Test Files";
      // 
      // menuHelp
      // 
      this.menuHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuLanguage,
            this.toolStripMenuItem2,
            this.menuAbout,
            this.menuCheckUpdate});
      this.menuHelp.Name = "menuHelp";
      this.menuHelp.Size = new System.Drawing.Size(44, 22);
      this.menuHelp.Text = "&Help";
      // 
      // menuLanguage
      // 
      this.menuLanguage.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuChinese,
            this.menuEnglish});
      this.menuLanguage.Name = "menuLanguage";
      this.menuLanguage.Size = new System.Drawing.Size(148, 22);
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
      this.toolStripMenuItem2.Size = new System.Drawing.Size(145, 6);
      // 
      // menuAbout
      // 
      this.menuAbout.Name = "menuAbout";
      this.menuAbout.Size = new System.Drawing.Size(148, 22);
      this.menuAbout.Text = "&About";
      this.menuAbout.Click += new System.EventHandler(this.menuAbout_Click);
      // 
      // menuCheckUpdate
      // 
      this.menuCheckUpdate.Name = "menuCheckUpdate";
      this.menuCheckUpdate.Size = new System.Drawing.Size(148, 22);
      this.menuCheckUpdate.Text = "&Check Update";
      this.menuCheckUpdate.Click += new System.EventHandler(this.menuCheckUpdate_Click);
      // 
      // statusStrip1
      // 
      this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
      this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lbStatus,
            this.lbStatusRight});
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
      // lbStatusRight
      // 
      this.lbStatusRight.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
      this.lbStatusRight.Name = "lbStatusRight";
      this.lbStatusRight.Size = new System.Drawing.Size(438, 21);
      this.lbStatusRight.Spring = true;
      this.lbStatusRight.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // ctxMenuTree
      // 
      this.ctxMenuTree.ImageScalingSize = new System.Drawing.Size(32, 32);
      this.ctxMenuTree.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ctxMenuCollapse,
            this.ctxMenuExpand});
      this.ctxMenuTree.Name = "ctxMenuTree";
      this.ctxMenuTree.Size = new System.Drawing.Size(120, 48);
      // 
      // ctxMenuCollapse
      // 
      this.ctxMenuCollapse.Name = "ctxMenuCollapse";
      this.ctxMenuCollapse.Size = new System.Drawing.Size(119, 22);
      this.ctxMenuCollapse.Text = "&Collapse";
      this.ctxMenuCollapse.Click += new System.EventHandler(this.ctxMenuCollapse_Click);
      // 
      // ctxMenuExpand
      // 
      this.ctxMenuExpand.Name = "ctxMenuExpand";
      this.ctxMenuExpand.Size = new System.Drawing.Size(119, 22);
      this.ctxMenuExpand.Text = "&Expand";
      this.ctxMenuExpand.Click += new System.EventHandler(this.ctxMenuExpand_Click);
      // 
      // toolbar
      // 
      this.toolbar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tbBtnOpen,
            this.tbBtnRecent,
            this.tbBtnTopMost,
            this.toolStripSeparator1,
            this.tbBtnLang,
            this.tbBtnCheckUpdate});
      this.toolbar.Location = new System.Drawing.Point(0, 24);
      this.toolbar.Name = "toolbar";
      this.toolbar.Size = new System.Drawing.Size(853, 25);
      this.toolbar.TabIndex = 4;
      this.toolbar.Text = "toolStrip1";
      // 
      // imageListToolbar
      // 
      this.imageListToolbar.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListToolbar.ImageStream")));
      this.imageListToolbar.TransparentColor = System.Drawing.Color.Transparent;
      this.imageListToolbar.Images.SetKeyName(0, "openFile.png");
      this.imageListToolbar.Images.SetKeyName(1, "recentFiles.png");
      this.imageListToolbar.Images.SetKeyName(2, "topMost.png");
      this.imageListToolbar.Images.SetKeyName(3, "lang.png");
      this.imageListToolbar.Images.SetKeyName(4, "checkUpdate.png");
      // 
      // tbBtnOpen
      // 
      this.tbBtnOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.tbBtnOpen.Image = ((System.Drawing.Image)(resources.GetObject("tbBtnOpen.Image")));
      this.tbBtnOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.tbBtnOpen.Name = "tbBtnOpen";
      this.tbBtnOpen.Size = new System.Drawing.Size(23, 22);
      this.tbBtnOpen.Text = "toolStripButton1";
      // 
      // tbBtnRecent
      // 
      this.tbBtnRecent.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.tbBtnRecent.Image = ((System.Drawing.Image)(resources.GetObject("tbBtnRecent.Image")));
      this.tbBtnRecent.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.tbBtnRecent.Name = "tbBtnRecent";
      this.tbBtnRecent.Size = new System.Drawing.Size(32, 22);
      this.tbBtnRecent.Text = "toolStripSplitButton1";
      // 
      // tbBtnTopMost
      // 
      this.tbBtnTopMost.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.tbBtnTopMost.Image = ((System.Drawing.Image)(resources.GetObject("tbBtnTopMost.Image")));
      this.tbBtnTopMost.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.tbBtnTopMost.Name = "tbBtnTopMost";
      this.tbBtnTopMost.Size = new System.Drawing.Size(23, 22);
      this.tbBtnTopMost.Text = "toolStripButton2";
      // 
      // toolStripSeparator1
      // 
      this.toolStripSeparator1.Name = "toolStripSeparator1";
      this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
      // 
      // tbBtnCheckUpdate
      // 
      this.tbBtnCheckUpdate.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.tbBtnCheckUpdate.Image = ((System.Drawing.Image)(resources.GetObject("tbBtnCheckUpdate.Image")));
      this.tbBtnCheckUpdate.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.tbBtnCheckUpdate.Name = "tbBtnCheckUpdate";
      this.tbBtnCheckUpdate.Size = new System.Drawing.Size(23, 22);
      this.tbBtnCheckUpdate.Text = "toolStripButton3";
      // 
      // tbBtnLang
      // 
      this.tbBtnLang.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.tbBtnLang.Image = ((System.Drawing.Image)(resources.GetObject("tbBtnLang.Image")));
      this.tbBtnLang.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.tbBtnLang.Name = "tbBtnLang";
      this.tbBtnLang.Size = new System.Drawing.Size(29, 22);
      this.tbBtnLang.Text = "toolStripDropDownButton1";
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
      this.hexViewer1.Size = new System.Drawing.Size(607, 337);
      this.hexViewer1.TabIndex = 0;
      this.hexViewer1.Text = "";
      this.hexViewer1.WordWrap = false;
      // 
      // MainForm
      // 
      this.AllowDrop = true;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(853, 444);
      this.Controls.Add(this.splitContainer1);
      this.Controls.Add(this.statusStrip1);
      this.Controls.Add(this.toolbar);
      this.Controls.Add(this.menuMainStrip);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MainMenuStrip = this.menuMainStrip;
      this.MinimumSize = new System.Drawing.Size(722, 452);
      this.Name = "MainForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "ASN.1 Viewer";
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
      this.Load += new System.EventHandler(this.MainForm_Load);
      this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
      this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
      this.splitContainer1.Panel1.ResumeLayout(false);
      this.splitContainer1.Panel2.ResumeLayout(false);
      this.splitContainer1.ResumeLayout(false);
      this.tabControl1.ResumeLayout(false);
      this.tabPageInput.ResumeLayout(false);
      this.tabPageBytes.ResumeLayout(false);
      this.panel1.ResumeLayout(false);
      this.menuMainStrip.ResumeLayout(false);
      this.menuMainStrip.PerformLayout();
      this.statusStrip1.ResumeLayout(false);
      this.statusStrip1.PerformLayout();
      this.ctxMenuTree.ResumeLayout(false);
      this.toolbar.ResumeLayout(false);
      this.toolbar.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TreeView treeView1;
    private System.Windows.Forms.SplitContainer splitContainer1;
    private System.Windows.Forms.TabControl tabControl1;
    private System.Windows.Forms.TabPage tabPageInput;
    private System.Windows.Forms.TabPage tabPageBytes;
    private System.Windows.Forms.MenuStrip menuMainStrip;
    private System.Windows.Forms.ToolStripMenuItem menuFile;
    private System.Windows.Forms.ToolStripMenuItem menuView;
    private System.Windows.Forms.ToolStripMenuItem menuHelp;
    private System.Windows.Forms.StatusStrip statusStrip1;
    private System.Windows.Forms.RichTextBox txtInput;
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
    private System.Windows.Forms.ToolStripMenuItem menuASN1Modules;
    private System.Windows.Forms.ContextMenuStrip ctxMenuTree;
    private System.Windows.Forms.ToolStripMenuItem ctxMenuCollapse;
    private System.Windows.Forms.ToolStripMenuItem ctxMenuExpand;
    private System.Windows.Forms.ToolStripMenuItem menuTestFiles;
    private System.Windows.Forms.ToolStripMenuItem menuCheckUpdate;
    private System.Windows.Forms.ToolStripStatusLabel lbStatusRight;
    private System.Windows.Forms.ToolStripMenuItem menuTopMost;
    private System.Windows.Forms.ImageList imageListDataType;
    private System.Windows.Forms.ToolStrip toolbar;
    private System.Windows.Forms.ToolStripButton tbBtnOpen;
    private System.Windows.Forms.ToolStripDropDownButton tbBtnRecent;
    private System.Windows.Forms.ToolStripButton tbBtnTopMost;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
    private System.Windows.Forms.ToolStripButton tbBtnCheckUpdate;
    private System.Windows.Forms.ImageList imageListToolbar;
    private System.Windows.Forms.ToolStripDropDownButton tbBtnLang;
  }
}

