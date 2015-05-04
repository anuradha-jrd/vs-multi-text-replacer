using EnvDTE;
using EnvDTE80;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace MultiTextReplacement
{
    public partial class ReplaceWindow : Form
    {
        private DTE2 _applicationObject;
        private AddIn _addInInstance;
        private Find _find;
        private List<ReplacerEntry> _replacements;
        private int _pointer = 0;
        private int _replacedCount = 0;

        public ReplaceWindow(DTE2 applicationObject, AddIn addInInstance)
        {
            InitializeComponent();

            this._applicationObject = applicationObject;
            this._addInInstance = addInInstance;
            this._replacements = new List<ReplacerEntry>();

            this.RefreshList();
        }

        private void RefreshList()
        {
            try
            {
                TextDocument textDoc = _applicationObject.ActiveDocument.Object("TextDocument") as TextDocument;
                EditPoint editPoint = textDoc.StartPoint.CreateEditPoint();
                _find = textDoc.DTE.Find as Find;
                editPoint.StartOfDocument();

                XmlDocument xmldoc = new XmlDocument();
                XmlNodeList xmlnode;
                int i = 0;
                //FileStream fs = new FileStream(@"C:\temp\Replacements.xml", FileMode.Open, FileAccess.Read);
                Solution solution = _applicationObject.Solution as Solution;
                string path = System.IO.Path.GetDirectoryName(solution.FullName);
                
                try
                {
                    FileStream fs = new FileStream(path + @"\Replacements.xml", FileMode.Open, FileAccess.Read);

                    xmldoc.Load(fs);
                    xmlnode = xmldoc.GetElementsByTagName("entry");

                    for (i = 0; i < xmlnode.Count; i++)
                    {
                        try
                        {
                            ReplacerEntry entry = new ReplacerEntry();
                            entry.Find = xmlnode[i].ChildNodes.Item(0).InnerText;
                            entry.Replace = xmlnode[i].ChildNodes.Item(1).InnerText;
                            entry.FullMatch = Boolean.Parse(xmlnode[i].ChildNodes.Item(2).InnerText);

                            this._replacements.Add(entry);
                        }
                        catch (Exception ex)
                        {
                            if (MessageBox.Show("Invalid XML. All entries will not be added. Exception occured in entry " + (i + 1) + " : " + ex.Message, "Exception occured", MessageBoxButtons.AbortRetryIgnore) == System.Windows.Forms.DialogResult.Abort)
                            {
                                break;
                            }
                        }
                    }

                    if (_replacements.Count > 0)
                    {
                        this.txtFind.Text = _replacements[_pointer].Find;
                        this.txtReplace.Text = _replacements[_pointer].Replace;
                    }
                }
                catch(FileNotFoundException ex)
                {
                    MessageBox.Show("Replacements.xml file is not found inside the solution folder. Sample file can be found inside c:/temp folder. Exception: " + ex.Message);
                    return;
                }                
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SearchNext()
        {
            if (_replacements.Count > 0 && _pointer < _replacements.Count)
            {
                ReplacerEntry entry = _replacements[_pointer];

                if (this.FindText(entry) == vsFindResult.vsFindResultNotFound)
                {
                    _pointer++;
                    SearchNext();
                }
            }
            else if(_replacements.Count > 0 && _pointer == _replacements.Count)
            {
                MessageBox.Show("Replace completed. " + _replacedCount.ToString() + " matches found.");
            }
        }

        private void SearchPrevious()
        {
            if (_replacements.Count > 0 && _pointer > 0)
            {
                ReplacerEntry entry = _replacements[_pointer];

                if (this.FindText(entry) == vsFindResult.vsFindResultNotFound)
                {
                    _pointer--;
                    SearchPrevious();
                }
            }
        }

        private vsFindResult FindText(ReplacerEntry entry)
        {
            vsFindResult result = vsFindResult.vsFindResultNotFound;

            if (_find != null)
            {
                this.txtFind.Text = entry.Find;
                this.txtReplace.Text = entry.Replace;
                _find.FindWhat = entry.Find;
                _find.ReplaceWith = entry.Replace;
                _find.Action = vsFindAction.vsFindActionFind;
                _find.MatchCase = true;
                _find.MatchWholeWord = entry.FullMatch;
                result = _find.Execute();
            }

            return result;
        }

        private void btnFind_Click(object sender, EventArgs e)
        {
            SearchNext();
        }

        private void btnReplace_Click(object sender, EventArgs e)
        {
            _find.Action = vsFindAction.vsFindActionReplace;
            _find.Execute();
            _replacedCount++;
        }

        private void btnSkip_Click(object sender, EventArgs e)
        {
            if (_pointer < _replacements.Count - 1)
            {
                _pointer++;
                SearchNext();
            }
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            if (_pointer > 0)
            {
                _pointer--;
                SearchPrevious();
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            this.RefreshList();
        }
    }
}
