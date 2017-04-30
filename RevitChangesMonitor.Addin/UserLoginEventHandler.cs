using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitChangesMonitor.Addin
{
    public class UserLoginEventHandler : IExternalEventHandler

    {
        public void Execute(UIApplication app) =>
            AppContext.Instance.ExternalApplication.Listen();

        public string GetName() => 
            "User Log In";
    }
}
