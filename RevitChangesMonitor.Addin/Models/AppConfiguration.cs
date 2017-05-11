using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitChangesMonitor.Addin.Models
{
    public class AppConfiguration
    {
        public int Id { get; set; }

        public string ReportsPath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    }
}
