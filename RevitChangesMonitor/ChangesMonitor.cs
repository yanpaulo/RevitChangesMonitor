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
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;

using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.ApplicationServices;

namespace Revit.ChangesMonitor
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
        #region  Class Member Variables
        /// <summary>
        /// A controlled application used to register the DocumentChanged event. Because all trigger points
        /// in this sample come from UI, the event must be registered to ControlledApplication. 
        /// If the trigger point is from API, user can register it to application 
        /// which can retrieve from ExternalCommand.
        /// </summary>
        private static ControlledApplication m_CtrlApp;

        /// <summary>
        /// data table for information windows.
        /// </summary>
        private static DataTable m_ChangesInfoTable;

        /// <summary>
        /// The window is used to show changes' information.
        /// </summary>
        private static ChangesInformationForm m_InfoForm;


        private static Dictionary<ElementId, Dictionary<string, string>> elementsDb;
        #endregion

        #region Class Static Property
        /// <summary>
        /// Property to get and set private member variables of changes log information.
        /// </summary>
        public static DataTable ChangesInfoTable
        {
            get { return m_ChangesInfoTable; }
            set { m_ChangesInfoTable = value; }
        }

        /// <summary>
        /// Property to get and set private member variables of info form.
        /// </summary>
        public static ChangesInformationForm InfoForm
        {
            get { return ExternalApplication.m_InfoForm; }
            set { ExternalApplication.m_InfoForm = value; }
        }
        #endregion

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
            elementsDb = new Dictionary<ElementId, Dictionary<string, string>>();

            // initialize member variables.
            m_CtrlApp = application.ControlledApplication;
            m_ChangesInfoTable = CreateChangeInfoTable();
            m_InfoForm = new ChangesInformationForm(ChangesInfoTable);

            // register the DocumentChanged event
            m_CtrlApp.DocumentChanged += new EventHandler<Autodesk.Revit.DB.Events.DocumentChangedEventArgs>(CtrlApp_DocumentChanged);

            // show dialog
            m_InfoForm.Show();

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
            m_CtrlApp.DocumentChanged -= CtrlApp_DocumentChanged;
            m_InfoForm = null;
            m_ChangesInfoTable = null;
            return Result.Succeeded;
        }
        #endregion

        #region Event handler
        /// <summary>
        /// This method is the event handler, which will dump the change information to tracking dialog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CtrlApp_DocumentChanged(object sender, Autodesk.Revit.DB.Events.DocumentChangedEventArgs e)
        {
            // get the current document.
            Document doc = e.GetDocument();
            var transactionNames = string.Join("; ", e.GetTransactionNames());
            // dump the element information
            ICollection<ElementId> addedElem = e.GetAddedElementIds();
            foreach (ElementId id in addedElem)
            {
                //Element elem = doc.GetElement(id);
                //var info = GetElementParameterInformation(doc, elem);
                //elementsDb.Add(id, info);
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
                //Element elem = doc.GetElement(id);
                //var oldInfo = elementsDb[id];
                //var info = GetElementParameterInformation(doc, elem);
                AddChangeInfoRow(id, doc, "Modified", transactionNames);
                //foreach (var item in info)
                //{
                //    if (oldInfo[item.Key] != item.Value)
                //    {
                //        AddChangeInfoRow(id, doc, "Modified", $"Modified Attribute {item.Key}");
                //    }
                //}

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

            DataRow newRow = m_ChangesInfoTable.NewRow();

            // set the relative information of this event into the table.
            if (elem == null)
            {
                // this branch is for deleted element due to the deleted element cannot be retrieve from the document.
                newRow["Time"] = DateTime.Now;
                newRow["Transaction"] = transactionName;
                newRow["ChangeType"] = changeType;
                newRow["Name"] = "";
                newRow["Category"] = "";

            }
            else
            {
                newRow["Time"] = DateTime.Now;
                newRow["Transaction"] = transactionName;
                newRow["ChangeType"] = changeType;
                newRow["Name"] = elem.Name;
                newRow["Category"] = elem.Category?.Name;
            }

            m_ChangesInfoTable.Rows.Add(newRow);
        }

        /// <summary>
        /// Generate a data table with five columns for display in window
        /// </summary>
        /// <returns>The DataTable to be displayed in window</returns>
        private DataTable CreateChangeInfoTable()
        {
            // create a new dataTable
            DataTable changesInfoTable = new DataTable("ChangesInfoTable");

            // Create a "ChangeType" column. It will be "Added", "Deleted" and "Modified".
            DataColumn timeColumn = new DataColumn("Time", typeof(System.DateTime));
            timeColumn.Caption = "Time";
            changesInfoTable.Columns.Add(timeColumn);

            // Create a "Transaction" column. It will be the transaction.
            DataColumn transactionColumn = new DataColumn("Transaction", typeof(System.String));
            transactionColumn.Caption = "Transaction";
            changesInfoTable.Columns.Add(transactionColumn);

            // Create a "ChangeType" column. It will be "Added", "Deleted" and "Modified".
            DataColumn styleColumn = new DataColumn("ChangeType", typeof(System.String));
            styleColumn.Caption = "ChangeType";
            changesInfoTable.Columns.Add(styleColumn);

            // Create a "Name" column. It will be the Element Name
            DataColumn nameColum = new DataColumn("Name", typeof(System.String));
            nameColum.Caption = "Name";
            changesInfoTable.Columns.Add(nameColum);

            // Create a "Category" column. It will be the Category Name of the element.
            DataColumn categoryColum = new DataColumn("Category", typeof(System.String));
            categoryColum.Caption = "Category";
            changesInfoTable.Columns.Add(categoryColum);

            // return this data table 
            return changesInfoTable;
        }

        Dictionary<string, string> GetElementParameterInformation(Document document, Element element)
        {
            var ret = new Dictionary<string, string>();
            // iterate element's parameters
            foreach (Parameter para in element.Parameters)
            {
                var result = GetParameterInformation(para, document);
                ret.Add(result.Key, result.Value);
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
            if (ExternalApplication.InfoForm == null)
            {
                ExternalApplication.InfoForm = new ChangesInformationForm(ExternalApplication.ChangesInfoTable);
            }
            ExternalApplication.InfoForm.Show();

            return Result.Succeeded;
        }
        #endregion
    }

}
