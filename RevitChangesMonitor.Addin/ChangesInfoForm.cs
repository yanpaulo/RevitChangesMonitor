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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RevitChangesMonitor.Addin
{
    /// <summary>
    /// The UI to show the change history logs. This class is not the main one just a assistant
    /// in this sample. If you just want to learn how to use DocumentChanges event,
    /// please pay more attention to ExternalApplication class.
    /// </summary>
    public partial class ChangesInformationForm : Form
    {
        private DataTable _dataTable;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ChangesInformationForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor with one argument
        /// </summary>
        /// <param name="dataBuffer">prepare the informations which is shown in this UI</param>
        public ChangesInformationForm(DataTable dataBuffer)
            : this()
        {
            _dataTable = dataBuffer;
            changesdataGridView.DataSource = dataBuffer;
            changesdataGridView.AutoGenerateColumns = false;
            changesdataGridView.Columns["Time"].DefaultCellStyle.Format = "O";
        }


        /// <summary>
        /// windows shown event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangesInfoForm_Shown(object sender, EventArgs e)
        {
            // set window's display location
            int left = Screen.PrimaryScreen.WorkingArea.Right - this.Width - 5;
            int top = Screen.PrimaryScreen.WorkingArea.Bottom - this.Height;
            Point windowLocation = new Point(left, top);
            this.Location = windowLocation;
        }

        /// <summary>
        /// Scroll to last line when add new log lines
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void changesdataGridView_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            changesdataGridView.CurrentCell = changesdataGridView.Rows[changesdataGridView.Rows.Count - 1].Cells[0];
        }

        private void topMostCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            TopMost = topMostCheckBox.Checked;
        }

        private void exportButton_Click(object sender, EventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                DefaultExt = "csv",
                FileName = "revit-log",
                Filter = "CSV File|*.csv"
            };
            var result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                using (var writer = new StreamWriter(dialog.OpenFile()))
                {
                    foreach (DataRow row in _dataTable.Rows)
                    {
                        writer.WriteLine(string.Join("\t", row.ItemArray.Select(c => c.ToString())));
                    }

                    writer.Close();
                }
            }
        }

        private void clearButton_Click(object sender, EventArgs e)
        {
            _dataTable.Clear();
        }

        private void reportButton_Click(object sender, EventArgs e)
        {
            var sb = new StringBuilder();
            var data = AppContext.Instance.ActiveDocumentChangesInfo;
            var totalCategoryScore = new Dictionary<string, int>
            {
                {"Geral", 0 },
                {"Architecture", 0 },
                {"Interiors", 0 },
                {"Landscape", 0 },
                {"Structural", 0 },
                {"Electrical", 0 },
                {"HVAC", 0 },
                {"Plumbing", 0 }
            };
            var changeTemplate = new List<KeyValuePair<int, DocumentChangeInfo>>
            {
                new KeyValuePair<int, DocumentChangeInfo>
                    (1,  new DocumentChangeInfo { Transaction = "Wall - Line", ChangeType = "Added", Category = "Walls" }),
                new KeyValuePair<int, DocumentChangeInfo>
                    (2,  new DocumentChangeInfo { Transaction = "Wall", ChangeType = "Added", Category = "Walls" })
            };
            
            foreach (var ctPair in changeTemplate)
            {
                int key = ctPair.Key;
                var template = ctPair.Value;

                int score = 0;
                Dictionary<string, int> categoryScore = new Dictionary<string, int>();
                int count = data.Count(c => c.Transaction == template.Transaction && c.ChangeType == template.ChangeType && c.Category == template.Category);
                
                if (key == 1 || key == 2)
                {
                    categoryScore = new Dictionary<string, int>
                        {
                            {"Geral", 1 },
                            {"Architecture", 2 },
                            {"Interiors", 1 },
                            {"Landscape", 0 },
                            {"Structural", 1 },
                            {"Electrical", 0 },
                            {"HVAC", 0 },
                            {"Plumbing", 0 }
                        };

                    score = count;
                    if (count >= 2 && count <= 5)
                    {
                        score = 2;
                    }
                    if (count >= 6 && count <= 10)
                    {
                        score = 4;
                    }
                    if (count >= 11)
                    {
                        score = 8;
                    }
                }

                foreach (var csPair in categoryScore.ToList())
                {
                    categoryScore[csPair.Key] = csPair.Value * score;
                    totalCategoryScore[csPair.Key] += csPair.Value * score;
                }

                sb.AppendLine($"{template.Transaction};\t{template.ChangeType};\t{template.Category}");
                sb.AppendLine(
                    string.Join(",", categoryScore.Select(c => $"{c.Key}: {c.Value}"))
                );
                sb.AppendLine();
            }
            sb.AppendLine($"Total:");
            sb.AppendLine(
                    string.Join(",", totalCategoryScore.Select(c => $"{c.Key}: {c.Value}"))
            );

            MessageBox.Show(sb.ToString());
        }

        private void locationButton_Click(object sender, EventArgs e)
        {
            var context = AppContext.Instance;
            var config = context.Configuration;

            var dialog = new FolderBrowserDialog()
            {
                SelectedPath = config.ReportsPath,
                ShowNewFolderButton = true,
            };
            var result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                config.ReportsPath = dialog.SelectedPath;
                context.Save();
            }
        }
    }
}
