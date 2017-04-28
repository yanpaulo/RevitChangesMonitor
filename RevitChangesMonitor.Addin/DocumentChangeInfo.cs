using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitChangesMonitor.Addin
{
    public class DocumentChangeInfo
    {
        public DateTime Time { get; set; }

        public string Transaction { get; set; }

        public string ChangeType { get; set; }

        public string Name { get; set; }

        public string Category { get; set; }

        public DataRow AsDataRow(DataTable dt)
        {
            var row = dt.NewRow();

            row["Time"] = Time;
            row["Transaction"] = Transaction;
            row["ChangeType"] = ChangeType;
            row["Name"] = Name;
            row["Category"] = Category;

            return row;
        }
    }
}
