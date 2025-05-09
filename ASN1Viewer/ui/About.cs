﻿using System;
using System.Reflection;
using System.Windows.Forms;

namespace ASN1Viewer.ui {
  partial class About : Form {
    public About() {
      InitializeComponent();
      this.Text = String.Format("About ");
    }

    private void About_Load(object sender, EventArgs e) {
      DateTime buildTime = Utils.GetAssembyBuildTime();
      this.Text = String.Format(Lang.T["ABOUT_TITLE"], Lang.T["PROD_NAME"]);
      this.lblProdName.Text = Lang.T["PROD_NAME"];
      this.lblCopyRight.Text = String.Format(Lang.T["ABOUT_COPYRIGHT"], DateTime.Now.Year.ToString());
      this.lblBuild.Text = String.Format(Lang.T["ABOUT_BUILD"], AssemblyVersion, buildTime.Year * 10000 + buildTime.Month * 100 + buildTime.Day);
    }
    public string AssemblyVersion {
      get {
        return Assembly.GetExecutingAssembly().GetName().Version.ToString();
      }
    }

  }
}
