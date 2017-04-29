//
// (C) Copyright 2003-2016 by Autodesk, Inc.
//
// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted,
// provided that the above copyright notice appears in all copies and
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting
// documentation.
//
// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS.
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE. AUTODESK, INC.
// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.
//
// Use, duplication, or disclosure by the U.S. Government is subject to
// restrictions set forth in FAR 52.227-19 (Commercial Computer
// Software - Restricted Rights) and DFAR 252.227-7013(c)(1)(ii)
// (Rights in Technical Data and Computer Software), as applicable.
//

using System;
using System.IO;
using System.Data;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;

using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.ApplicationServices;

namespace RevitChangesMonitor.Addin
{
    /// <summary>
    /// A class inherits IExternalApplication interface and provide an entry of the sample.
    /// It create a modeless dialog to track the changes.
    /// </summary>
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]
    public class ExternalApplication : IExternalApplication
    {
        private AppContext _context = AppContext.Instance;

        #region IExternalApplication Members
        /// <summary>
        /// Implement this method to implement the external application which should be called when 
        /// Revit starts before a file or default template is actually loaded.
        /// </summary>
        /// <param name="application">An object that is passed to the external application 
        /// which contains the controlled application.</param> 
        /// <returns>Return the status of the external application. 
        /// A result of Succeeded means that the external application successfully started. 
        /// Cancelled can be used to signify that the user cancelled the external operation at 
        /// some point.
        /// If false is returned then Revit should inform the user that the external application 
        /// failed to load and the release the internal reference.</returns>
        public Result OnStartup(UIControlledApplication application)
        {
            // initialize member variables.
            _context.ControlledApp = application.ControlledApplication;
            _context.ChangesInformationForm = new ChangesInformationForm(_context.ChangesInfoTable);

            application.ViewActivated += Application_ViewActivated;

            // register the DocumentChanged event
            _context.ControlledApp.DocumentChanged += ControlledApp_DocumentChanged;

            _context.ControlledApp.DocumentClosing += ControlledApp_DocumentClosing;

            // show dialog
            _context.ChangesInformationForm.Show();

            return Result.Succeeded;
        }
        
        /// <summary>
        /// Implement this method to implement the external application which should be called when 
        /// Revit is about to exit,Any documents must have been closed before this method is called.
        /// </summary>
        /// <param name="application">An object that is passed to the external application 
        /// which contains the controlled application.</param>
        /// <returns>Return the status of the external application. 
        /// A result of Succeeded means that the external application successfully shutdown. 
        /// Cancelled can be used to signify that the user cancelled the external operation at 
        /// some point.
        /// If false is returned then the Revit user should be warned of the failure of the external 
        /// application to shut down correctly.</returns>
        public Result OnShutdown(UIControlledApplication application)
        {
            _context.ControlledApp.DocumentChanged -= ControlledApp_DocumentChanged;
            _context.ChangesInformationForm = null;
            //_context.ChangesInfoTable = null;
            return Result.Succeeded;
        }
        #endregion

        #region Event handler
        private void Application_ViewActivated(object sender, Autodesk.Revit.UI.Events.ViewActivatedEventArgs e)
        {
            var doc = e.Document;
            var dt = _context.ChangesInfoTable;

            _context.ActiveDocument = doc;
            dt.Clear();

            foreach (var row in _context.ActiveDocumentState.Changes.Select(c => c.AsDataRow(dt)))
                _context.ChangesInfoTable.Rows.Add(row);
        }

        private void ControlledApp_DocumentClosing(object sender, Autodesk.Revit.DB.Events.DocumentClosingEventArgs e)
        {
            string documentsDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                timeStamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                fileName = e.Document.Title,
                userName = Environment.UserName;

            var filename = $"{documentsDir}\\{fileName}-{userName}.csv";

            using (var writer = new StreamWriter(filename))
            {
                foreach (DataRow row in _context.DocumentStates[e.Document].Changes
                    .Select(c => c.AsDataRow(_context.ChangesInfoTable)))
                {
                    writer.WriteLine(string.Join("\t", row.ItemArray.Select(c => c.ToString())));
                }

                writer.Close();
            }
        }

        /// <summary>
        /// This method is the event handler, which will dump the change information to tracking dialog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ControlledApp_DocumentChanged(object sender, Autodesk.Revit.DB.Events.DocumentChangedEventArgs e)
        {
            // get the current document.
            Document doc = e.GetDocument();
            var transactionNames = string.Join("; ", e.GetTransactionNames());
            
            if (!_context.DocumentStates.ContainsKey(doc))
            {
                _context.DocumentStates.Add(doc, new DocumentState());
            }
            var documentState = _context.DocumentStates[doc];

            // dump the element information
            ICollection<ElementId> addedElem = e.GetAddedElementIds();
            foreach (ElementId id in addedElem)
            {
                Element elem = doc.GetElement(id);
                var info = GetElementParameterInformation(doc, elem);
                documentState.Elements.Add(id, info);

                AddChangeInfoRow(id, doc, "Added", transactionNames);
            }

            ICollection<ElementId> deletedElem = e.GetDeletedElementIds();
            foreach (ElementId id in deletedElem)
            {
                AddChangeInfoRow(id, doc, "Deleted", transactionNames);
            }

            ICollection<ElementId> modifiedElem = e.GetModifiedElementIds();
            foreach (ElementId id in modifiedElem)
            {
                Element elem = doc.GetElement(id);
                var oldInfo = documentState.Elements[id];
                var info = GetElementParameterInformation(doc, elem);
                AddChangeInfoRow(id, doc, "Modified", transactionNames);
                foreach (var item in info)
                {
                    if (oldInfo.ContainsKey(item.Key) && oldInfo[item.Key] != item.Value)
                    {
                        AddChangeInfoRow(id, doc, "Modified", $"Modify Attribute - {item.Key}");
                    }
                }
                documentState.Elements[id] = info;
            }

            if (addedElem.Count == 0 && deletedElem.Count == 0 && modifiedElem.Count == 0)
            {
                AddChangeInfoRow(null, doc, "Other", transactionNames);
            }

        }
        #endregion

        #region Class Methods
        /// <summary>
        /// This method is used to retrieve the changed element and add row to data table.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="doc"></param>
        /// <param name="changeType"></param>
        private void AddChangeInfoRow(ElementId id, Document doc, string changeType, string transactionName)
        {
            // retrieve the changed element
            Element elem = doc.GetElement(id);

            //DataRow newRow = _context.ChangesInfoTable.NewRow();
            var changeInfo = new DocumentChangeInfo();

            // set the relative information of this event into the table.
            if (elem == null)
            {
                // this branch is for deleted element due to the deleted element cannot be retrieve from the document.
                changeInfo.Time = DateTime.Now;
                changeInfo.Transaction = transactionName;
                changeInfo.ChangeType = changeType;
                changeInfo.Name = "";
                changeInfo.Category = "";

            }
            else
            {
                changeInfo.Time = DateTime.Now;
                changeInfo.Transaction = transactionName;
                changeInfo.ChangeType = changeType;
                changeInfo.Name = elem.Name;
                changeInfo.Category = elem.Category?.Name;
            }
            _context.DocumentStates[doc].Changes.Add(changeInfo);

            if (doc.Equals(_context.ActiveDocument))
            {
                var newRow = changeInfo.AsDataRow(_context.ChangesInfoTable);
                _context.ChangesInfoTable.Rows.Add(newRow); 
            }
        }
        
        Dictionary<string, string> GetElementParameterInformation(Document document, Element element)
        {
            var ret = new Dictionary<string, string>();
            // iterate element's parameters
            foreach (Parameter para in element.Parameters)
            {
                var result = GetParameterInformation(para, document);
                if (ret.ContainsKey(result.Key))
                {
                    var oldValue = ret[result.Key];
                    if (oldValue != result.Value)
                    {
                        var message = $"Element has a dupplicate parameter ({result.Key}) with different values ({oldValue} and {result.Value})";
                        Debug.WriteLine(message);
                        throw new InvalidOperationException(message);
                    }
                }
                ret[result.Key] = result.Value;
            }

            return ret;
        }

        KeyValuePair<string, string> GetParameterInformation(Parameter para, Document document)
        {
            string defName = para.Definition.Name;
            string defValue = string.Empty;
            // Use different method to get parameter data according to the storage type
            switch (para.StorageType)
            {
                case StorageType.Double:
                    //covert the number into Metric
                    defValue = para.AsValueString();
                    break;
                case StorageType.ElementId:
                    //find out the name of the element
                    Autodesk.Revit.DB.ElementId id = para.AsElementId();
                    if (id.IntegerValue >= 0)
                    {
                        defValue = document.GetElement(id).Name;
                    }
                    else
                    {
                        defValue = id.IntegerValue.ToString();
                    }
                    break;
                case StorageType.Integer:
                    if (ParameterType.YesNo == para.Definition.ParameterType)
                    {
                        if (para.AsInteger() == 0)
                        {
                            defValue = "False";
                        }
                        else
                        {
                            defValue = "True";
                        }
                    }
                    else
                    {
                        defValue = para.AsInteger().ToString();
                    }
                    break;
                case StorageType.String:
                    defValue = para.AsString();
                    break;
                default:
                    defValue = "Unexposed parameter.";
                    break;
            }

            return new KeyValuePair<string, string>(defName, defValue);
        }
        #endregion
    }

    /// <summary>
    /// This class inherits IExternalCommand interface and used to retrieve the dialog again.
    /// </summary>
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class Command : IExternalCommand
    {
        #region IExternalCommand Members
        /// <summary>
        /// Implement this method as an external command for Revit.
        /// </summary>
        /// <param name="commandData">An object that is passed to the external application
        /// which contains data related to the command,
        /// such as the application object and active view.</param>
        /// <param name="message">A message that can be set by the external application
        /// which will be displayed if a failure or cancellation is returned by
        /// the external command.</param>
        /// <param name="elements">A set of elements to which the external application
        /// can add elements that are to be highlighted in case of failure or cancellation.</param>
        /// <returns>Return the status of the external command.
        /// A result of Succeeded means that the API external method functioned as expected.
        /// Cancelled can be used to signify that the user cancelled the external operation 
        /// at some point. Failure should be returned if the application is unable to proceed with
        /// the operation.</returns>
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var context = AppContext.Instance;

            if (context.ChangesInformationForm == null)
            {
                context.ChangesInformationForm = new ChangesInformationForm(context.ChangesInfoTable);
            }
            context.ChangesInformationForm.Show();

            return Result.Succeeded;
        }
        #endregion
    }

}