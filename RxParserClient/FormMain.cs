#region Copyright Syncfusion Inc. 2001-2018.
// Copyright Syncfusion Inc. 2001-2018. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.WinForms.DataGrid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;   
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace RxParserClient
{
    public partial class FormMain : Form
    {
        #region Private variables
        //File object containing input text and patterns
        private RxProjFile _file = new RxProjFile();

        //File Open and Save default extention variables
        private readonly string _defaultExt = "rxproj";
        private readonly string _defaultDlgFilter = "RX Project|*.rxproj";

        //List of matched matches
        private List<Match> _matched;
        private Color _defaultInputBackColor;
        #endregion

        #region Constructor
        public FormMain()
        {
            InitializeComponent();
        }
        #endregion

        #region Form Initialize
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                gridResult.Model.Options.NumberedColHeaders = false;
                _defaultInputBackColor = txtInput.SelectionBackColor;
                //_autoEvaluateTimer.Tick += _autoEvaluateTimer_Tick;

                DisplayFile();
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region Project Save, Load & Display
        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(_file.FileName))
                {
                    var dlg = new SaveFileDialog()
                    {
                        DefaultExt = _defaultExt,
                        Filter = _defaultDlgFilter
                    };

                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        _file.FileName = dlg.FileName;
                    }
                    else
                        return;
                }

                _file.InputValues = txtInput.Lines.ToList();
                _file.Save();

                SetStatus("File successfully saved.");
            }
            catch (Exception)
            {
                SetErrorStatus("Exception saving file.");
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            try
            {
                var dlg = new OpenFileDialog()
                {
                    DefaultExt = _defaultExt,
                    Filter = _defaultDlgFilter
                };
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    _file = RxProjFile.Load(dlg.FileName);
                    DisplayFile();
                }
                SetStatus("File successfully loaded.");
            }
            catch (Exception)
            {
                SetErrorStatus("Exception loading file.");
            }
        }
        private void DisplayFile()
        {
            gridRegEx.DataSource = _file.UserExpressions;
            txtInput.Lines = _file.InputValues.ToArray();
        }
        #endregion

        #region User Patterns
        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var ue = new UserExpression();
                _file.UserExpressions.Add(ue);
                //gridRegEx.Rows.Add();
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                //var sel = (UserExpression)gridRegEx.CurrentRow.DataBoundItem;
                //_file.UserExpressions.Remove(sel);
                gridRegEx.Rows.Remove(gridRegEx.CurrentRow);
            }
            catch (Exception)
            {
                SetErrorStatus("Exception deleting user expression.");
            }
        }

        #endregion

        #region Pattern Evaluate
        private void gridRegEx_CellButtonClick(object sender, Syncfusion.WinForms.DataGrid.Events.CellButtonClickEventArgs e)
        {
            try
            {
                if (e.ColumnIndex == 0)
                {
                    Evaluate();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        private void Evaluate()
        {
            try
            {
                var pattern = ((UserExpression)gridRegEx.CurrentRow.DataBoundItem).Pattern;
                if (string.IsNullOrEmpty(pattern))
                {
                    SetStatus("Blank pattern.");
                    return;
                }

                var rx = new Regex(pattern);

                _matched = new List<Match>();

                foreach (var line in txtInput.Lines)
                {
                    var m = rx.Match(line);
                    if (m.Success)
                    {
                        _matched.Add(m);
                    }
                }

                var msg = string.Format("Successfully parsed pattern.  {0} matches found.", _matched.Count);
                SetStatus(msg);
            }
            catch (Exception)
            {
                SetErrorStatus("Exception parsing pattern.");
            }

            HighlightMatchedInput();
            DisplayMatches();
        }
        private void HighlightMatchedInput()
        {
            //Clear selection
            txtInput.SelectAll();
            txtInput.SelectionBackColor = _defaultInputBackColor;

            foreach(var m in _matched)
            {
                int o = 0;
                int s = 0;
                var txt = txtInput.Text;
                var start = 0;
                var search = m.Groups[0].Value;

                if (string.IsNullOrWhiteSpace(search))
                    continue;

                while (txt.Contains(search))
                {
                    s = txt.IndexOf(search);
                    start = s + o;
                    txtInput.Select(start, search.Length);
                    txtInput.SelectionBackColor = Color.Yellow;

                    txt = txt.Substring(s + search.Length);
                    o += (s + search.Length);
                }
            }
        }
        private void DisplayMatches()
        {
            gridResult.Clear(true);

            if (_matched.Count == 0)
                return;

            var col = (from g in _matched
                       select g.Groups.Count).Max();

            gridResult.ColCount = col-1;
            gridResult.RowCount = _matched.Count;

            for (int c = 1; c <= col; c++)
            {
                gridResult.Model[0, c].CellValue = c;
            }

            for (int r = 1; r <= _matched.Count; r++)
            {

                for (int c = 1; c <= col; c++)
                {
                    gridResult.Model[r, c].CellValue = _matched[r - 1].Groups[c].Value;
                }
            }
        }

        private void gridRegEx_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            Evaluate();
        }
        #endregion

        #region Status
        private void SetErrorStatus(string msg)
        {
            lblStatus.Text = msg;
            lblStatus.ForeColor = Color.Red;
        }

        private void SetStatus(string msg)
        {
            lblStatus.Text = msg;
            lblStatus.ForeColor = Color.Black;
        }
        #endregion

        private void gridRegEx_SelectionChanged(object sender, EventArgs e)
        {
            if (gridRegEx.SelectedRows.Count > 0)
            {
                var pattern = ((UserExpression)gridRegEx.SelectedRows[0].DataBoundItem).Pattern;

                Evaluate();
            }
        }
    }
}
