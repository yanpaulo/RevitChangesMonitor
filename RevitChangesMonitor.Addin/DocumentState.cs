using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace RevitChangesMonitor.Addin
{
    internal class DocumentState
    {
        public Dictionary<ElementId, Dictionary<string, string>> Elements { get; set; }
            = new Dictionary<ElementId, Dictionary<string, string>>();

        public List<DocumentChangeInfo> Changes { get; set; }
            = new List<DocumentChangeInfo>();
    }
}