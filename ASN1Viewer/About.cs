using System;
using System.Reflection;
using System.Windows.Forms;

namespace ASN1Viewer {
  partial class About : Form {
    public About() {
      InitializeComponent();
      this.Text = String.Format("About ");
    }

    private void About_Load(object sender, EventArgs e) {
      this.Text = String.Format(Lang.T["ABOUT_TITLE"], Lang.T["PROD_NAME"]);
      this.lblProdName.Text = Lang.T["PROD_NAME"];
      this.lblCopyRight.Text = String.Format(Lang.T["ABOUT_COPYRIGHT"], DateTime.Now.Year.ToString());
      this.lblBuild.Text = String.Format(Lang.T["ABOUT_BUILD"], AssemblyVersion, (int)(DateTime.Now - new DateTime(2020, 1, 1)).TotalDays);
    }
    public string AssemblyVersion {
      get {
        return Assembly.GetExecutingAssembly().GetName().Version.ToString();
      }
    }

  }
}
