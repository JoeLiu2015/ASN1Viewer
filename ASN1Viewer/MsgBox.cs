using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ASN1Viewer {
  public partial class MsgBox : Form {
    public MsgBox() {
      InitializeComponent();
    }

    public string OKText {
      get { return this.btnOK.Text;  }
      set { this.btnOK.Text = value; }
    }
    public string CancelText {
      get { return this.btnCancel.Text; }
      set { this.btnCancel.Text = value; }
    }
    public string Message {
      get {  return this.lblMsg.Text;  }
      set {  this.lblMsg.Text = value; }
    }

    public static DialogResult Show(IWin32Window owner, string title, string message, string okText, string cancelText) {
      MsgBox m = new MsgBox();
      m.Text = title;
      m.OKText = okText;
      m.CancelText = cancelText;
      m.Message = message;
      m.StartPosition = FormStartPosition.CenterParent;
      return m.ShowDialog(owner);
    }

    private void btnOK_Click(object sender, EventArgs e) {
      this.DialogResult = DialogResult.OK;
    }

    private void btnCancel_Click(object sender, EventArgs e) {
      this.DialogResult = DialogResult.Cancel;
    }
  }
}
