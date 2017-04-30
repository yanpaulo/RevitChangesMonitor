using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using RevitChangesMonitor.Addin.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitChangesMonitor.Addin
{
    public class AppContext
    {
        public static AppContext Instance { get; } = new AppContext();
        private AppContext() { }

        public ExternalApplication ExternalApplication { get; set; }

        public LoginInformation LoginInfo { get; set; }
        
        /// <summary>
        /// data table for information windows.
        /// </summary>
        public DataTable ChangesInfoTable { get; } = CreateChangeInfoTable();

        /// <summary>
        /// The window is used to show changes' information.
        /// </summary>
        public ChangesInformationForm ChangesInformationForm { get; set; }

        public Dictionary<Document, DocumentState> DocumentStates { get; } = new Dictionary<Document, DocumentState>();

        public Document ActiveDocument { get; set; }
        
        public DocumentState ActiveDocumentState => DocumentStates[ActiveDocument];

        public List<DocumentChangeInfo> ActiveDocumentChangesInfo => ActiveDocumentState.Changes;
        
        /// <summary>
        /// Generate a data table with five columns for display in window
        /// </summary>
        /// <returns>The DataTable to be displayed in window</returns>
        private static DataTable CreateChangeInfoTable()
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
    }
}
