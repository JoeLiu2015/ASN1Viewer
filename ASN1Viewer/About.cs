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
      DateTime now = DateTime.Now;
      this.lblBuild.Text = String.Format(Lang.T["ABOUT_BUILD"], AssemblyVersion, now.Year * 10000 + now.Month * 100 + now.Day);
    }
    public string AssemblyVersion {
      get {
        return Assembly.GetExecutingAssembly().GetName().Version.ToString();
      }
    }

  }
}
