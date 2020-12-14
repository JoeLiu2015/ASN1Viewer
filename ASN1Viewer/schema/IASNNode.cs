using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASN1Viewer.schema {
  public interface IASNNode {
    int Tag { get; }
    int ChildCount { get; }
    IASNNode GetChild(int i);

    ISchemaNode Schema { set; }
  }
}
