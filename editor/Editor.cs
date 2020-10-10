using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using ScintillaNET;
using System.Threading;
using System.Text.RegularExpressions;

namespace wjw.Editor
{
    public partial class Editor : UserControl
    {
        private const int BACK_COLOR = 0x2A211C;
        private const int FORE_COLOR = 0xB7B7B7;
        private const int NUMBER_MARGIN = 1;
        private const int BOOKMARK_MARGIN = 2;
        private const int BOOKMARK_MARKER = 2; 
        private const int FOLDING_MARGIN = 3;
        private const bool CODEFOLDING_CIRCULAR = true;
        private const uint mask = (1 << BOOKMARK_MARKER);
        private bool SearchIsOpen = false;
        private List<Line> _bookMarkList = new List<Line>();
        private Line SelectLineBookmark = null;
        private SearchManager searchManager = new SearchManager();

        public event Action<string> OnFileOpen;

        public enum Language
        {
            CPP,
            Log,
            XML
        }

        public WrapMode WrapMode
        {
            get { return scintillaCode.WrapMode; }
            set { scintillaCode.WrapMode = value; }
        }

        public List<string> SearchItems
        {
            set
            {
                foreach (var line in value)
                {
                    cbSearch.Items.Add(line);
                }
            }
        }

        public Language CurrentLanguage
        {
            set
            {
                switch(value)
                {
                    case Language.Log:
                        InitSyntaxColorForLog();
                        break;
                    case Language.XML:
                        InitSyntaxColorForXml();
                        break;
                    case Language.CPP:
                        InitSyntaxColorForCpp();
                        break;
                    default:

                        break;
                }
            }
        }

        public string CurrentLine
        {
            get
            {
                int currentLineIndex = scintillaCode.LineFromPosition(scintillaCode.CurrentPosition);
                Line currentLine = scintillaCode.Lines[currentLineIndex];
                return currentLine.Text;
            }
        }

        public string Content
        {
            get { return scintillaCode.Text; }
        }

        public Editor()
        {
            InitializeComponent();
            InitscintillaCode();
            pnBookmarkTitle.Dragable = false;
            scintillaCode.TextChanged += ScintillaCode_TextChanged;
        }

        public event EventHandler TextChange;

        private void ScintillaCode_TextChanged(object sender, EventArgs e)
        {
            TextChange?.Invoke(sender, e);
        }

        public int LineCount
        {
            get
            {
                return scintillaCode.Lines.Count;
            }
        }

        public void ClearText()
        {
            
            scintillaCode.Text = string.Empty;
        }

        public void AppendText(string content)
        {
            scintillaCode.AppendText(content);
            scintillaCode.GotoPosition(scintillaCode.TextLength);
            scintillaCode.ScrollCaret();
        }

        public void AddText(string content)
        {
            scintillaCode.AppendText(content);
        }

        public void LoadText(string content)
        {
            scintillaCode.Text = content;
            
        }

        public override string Text
        {
            get
            {
                return scintillaCode.Text;
            }
            set
            {
                scintillaCode.Text = value;
            }
        }

        private void InitscintillaCode()
        {
            // INITIAL VIEW CONFIG
            scintillaCode.CaretForeColor = Color.Red;
          //scintillaCode.CaretStyle = CaretStyle.Block;
            scintillaCode.WrapMode = WrapMode.None;
            scintillaCode.IndentationGuides = IndentView.LookBoth;
            scintillaCode.Delete += ScintillaCode_Delete;
            searchManager.TextArea = scintillaCode;
            searchManager.SearchBox = txtSearchContent;
            scintillaCode.Focus();
            InitBookmarkPanel();

            // STYLING
            InitColors();

            // NUMBER MARGIN
            InitNumberMargin();

            // BOOKMARK MARGIN
            InitBookmarkMargin();

            //CODE FOLDING MARGIN
            //InitCodeFolding();

            // DRAG DROP
            InitDragDropFile();

            // INIT HOTKEYS
            InitHotkeys();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.Q))
            {
                OpenBookMark();
                return true;
            }
            else if (keyData == (Keys.Control | Keys.Up))
            {
                PreviousMark();
                return true;
            }
            else if (keyData == (Keys.Control | Keys.Down))
            {
                NextMark();
                return true;
            }
            else if (keyData == (Keys.Control | Keys.B))
            {
                SwitchMark();
                return true;
            }
            else if (keyData == (Keys.Control | Keys.Delete))
            {
                DeleteAllBookMark();
                return true;
            }
            else if (keyData == (Keys.Control | Keys.F))
            {
                OpenSearch();
                return true;
            }
            else if (keyData == Keys.Escape)
            {
                CloseSearch();
                return true;
            }
            else if (keyData == Keys.F3)
            {
                searchManager.Find(true, false);
                return true;
            }
            else if (keyData == Keys.F2)
            {
                searchManager.Find(false, false);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void PreviousMark()
        {
            int currentLineIndex = scintillaCode.LineFromPosition(scintillaCode.CurrentPosition);
            if (currentLineIndex > 0)
            {
                int searchIndex = scintillaCode.Lines[currentLineIndex - 1].MarkerPrevious(mask);
                if (searchIndex != -1)
                    ScrollToCenter(searchIndex);
            }
        }

        private void ScrollToCenter(int index)
        {
            bool next = scintillaCode.CurrentLine <= index;

            if (index >= 0 && index < scintillaCode.Lines.Count)
            {
                scintillaCode.SetSelection(scintillaCode.Lines[index].Position, scintillaCode.Lines[index].EndPosition);
            }
            else return;

            int currentLineIndex = scintillaCode.LineFromPosition(scintillaCode.CurrentPosition);
            Line currentLine = scintillaCode.Lines[currentLineIndex];
            int linesInView = scintillaCode.LinesOnScreen;
            int halfLines = linesInView / 2;
            int lowerLine = currentLineIndex + halfLines > scintillaCode.Lines.Count ? scintillaCode.Lines.Count : currentLineIndex + halfLines;
            int upperLine = currentLineIndex - halfLines > 0 ? currentLineIndex - halfLines : 0;
            if (next)
                scintillaCode.ScrollRange(scintillaCode.Lines[lowerLine].Position, 0);
            else
            {
                scintillaCode.ScrollRange(scintillaCode.Lines[upperLine].Position, 0);
                scintillaCode.ScrollRange(scintillaCode.Lines[lowerLine].Position, 0);
            }

            scintillaCode.ScrollCaret();
        }

        private void NextMark()
        {
            int currentLineIndex = scintillaCode.LineFromPosition(scintillaCode.CurrentPosition);
            if (currentLineIndex < scintillaCode.Lines.Count - 1)
            {
                int searchIndex = scintillaCode.Lines[currentLineIndex + 1].MarkerNext(mask);
                if (searchIndex != -1)
                    ScrollToCenter(searchIndex);
            }
        }

        private void SwitchMark()
        {
            int currentLineIndex = scintillaCode.LineFromPosition(scintillaCode.CurrentPosition);
            ChangeBookMark(scintillaCode.Lines[currentLineIndex]);

        }

        private void ChangeBookMark(Line line)
        {
            if (IsContainBookMark(line))//cancel the mark
            {
                line.MarkerDelete(BOOKMARK_MARKER);

                DelBookMark(line);
                scintillaCode.IndicatorClearRange(line.Position, line.Length);
            }
            else//add the mark
            {
                line.MarkerAdd(BOOKMARK_MARKER);
                AddBookMark(line);
                scintillaCode.IndicatorFillRange(line.Position, line.Length);
            }
        }

        private bool IsContainBookMark(Line line)
        {
            if ((line.MarkerGet() & mask) > 0)
                return true;
            else
                return false;
        }

        private void InitHotkeys()
        {
            // register the hotkeys with the form
            //HotKeyManager.AddHotKey(this, OpenSearch, Keys.F, true);
            //HotKeyManager.AddHotKey(this, CloseSearch, Keys.Escape);

            scintillaCode.ClearCmdKey(Keys.Control | Keys.F);
        }

        private void ScintillaCode_Delete(object sender, ModificationEventArgs e)
        {

            if (e.LinesAdded < 0)
            {
                Line b = scintillaCode.Lines[scintillaCode.LineFromPosition(e.Position)];
                b.MarkerDelete(BOOKMARK_MARKER);
                RefreshBookMarkList();
            }

        }

        private void DeleteAllBookMark()
        {
            var result = scintillaCode.Lines.Where(q => (q.MarkerGet() & mask) > 0).ToList();
            foreach (Line line in result)
            {
                line.MarkerDelete(BOOKMARK_MARKER);
                scintillaCode.IndicatorClearRange(line.Position, line.Length);
            }
            RefreshBookMarkList();
        }

        private void OpenBookMark()
        {
            SetPanelPosition(pnBookmark);
            pnBookmark.Visible = !pnBookmark.Visible;
        }

        private void InitNumberMargin()
        {
            //scintillaCode.Styles[Style.LineNumber].BackColor = IntToColor(BACK_COLOR);
            //scintillaCode.Styles[Style.LineNumber].ForeColor = IntToColor(FORE_COLOR);
            //scintillaCode.Styles[Style.IndentGuide].ForeColor = IntToColor(FORE_COLOR);
            //scintillaCode.Styles[Style.IndentGuide].BackColor = IntToColor(BACK_COLOR);

            var nums = scintillaCode.Margins[NUMBER_MARGIN];
            nums.Width = 50;
            nums.Type = MarginType.Number;
            nums.Sensitive = true;
            nums.Mask = 0;

        }

        private void TextArea_MarginClick(object sender, MarginClickEventArgs e)
        {
            if (e.Margin == BOOKMARK_MARGIN)
            {
                var line = scintillaCode.Lines[scintillaCode.LineFromPosition(e.Position)];
                ChangeBookMark(line);
            }

        }

        private void InitBookmarkMargin()
        {
            //scintillaCode.SetFoldMarginColor(true, IntToColor(BACK_COLOR));
            var margin = scintillaCode.Margins[BOOKMARK_MARGIN];
            margin.Width = 20;
            margin.Sensitive = true;
            margin.Type = MarginType.Symbol;
            margin.Mask = (1 << BOOKMARK_MARKER);
            //margin.Cursor = MarginCursor.Arrow;

            var marker = scintillaCode.Markers[BOOKMARK_MARKER];
            marker.Symbol = MarkerSymbol.Circle;
            marker.SetBackColor(IntToColor(0xFF003B));
            marker.SetForeColor(IntToColor(0x000000));
            marker.SetAlpha(100);
            scintillaCode.MarginClick += TextArea_MarginClick;
        }

        private void InitCodeFolding()
        {

            scintillaCode.SetFoldMarginColor(true, IntToColor(BACK_COLOR));
            scintillaCode.SetFoldMarginHighlightColor(true, IntToColor(BACK_COLOR));

            // Enable code folding
            scintillaCode.SetProperty("fold", "1");
            scintillaCode.SetProperty("fold.compact", "1");

            // Configure a margin to display folding symbols
            scintillaCode.Margins[FOLDING_MARGIN].Type = MarginType.Symbol;
            scintillaCode.Margins[FOLDING_MARGIN].Mask = Marker.MaskFolders;
            scintillaCode.Margins[FOLDING_MARGIN].Sensitive = true;
            scintillaCode.Margins[FOLDING_MARGIN].Width = 20;

            // Set colors for all folding markers
            for (int i = 25; i <= 31; i++)
            {
                scintillaCode.Markers[i].SetForeColor(IntToColor(BACK_COLOR)); // styles for [+] and [-]
                scintillaCode.Markers[i].SetBackColor(IntToColor(FORE_COLOR)); // styles for [+] and [-]
            }

            // Configure folding markers with respective symbols
            scintillaCode.Markers[Marker.Folder].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CirclePlus : MarkerSymbol.BoxPlus;
            scintillaCode.Markers[Marker.FolderOpen].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CircleMinus : MarkerSymbol.BoxMinus;
            scintillaCode.Markers[Marker.FolderEnd].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CirclePlusConnected : MarkerSymbol.BoxPlusConnected;
            scintillaCode.Markers[Marker.FolderMidTail].Symbol = MarkerSymbol.TCorner;
            scintillaCode.Markers[Marker.FolderOpenMid].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CircleMinusConnected : MarkerSymbol.BoxMinusConnected;
            scintillaCode.Markers[Marker.FolderSub].Symbol = MarkerSymbol.VLine;
            scintillaCode.Markers[Marker.FolderTail].Symbol = MarkerSymbol.LCorner;

            // Enable automatic folding
            scintillaCode.AutomaticFold = (AutomaticFold.Show | AutomaticFold.Click | AutomaticFold.Change);

        }

        public void InitDragDropFile()
        {

            scintillaCode.AllowDrop = true;
            scintillaCode.DragEnter += delegate (object sender, DragEventArgs e)
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                    e.Effect = DragDropEffects.Copy;
                else
                    e.Effect = DragDropEffects.None;
            };
            scintillaCode.DragDrop += delegate (object sender, DragEventArgs e)
            {

                // get file drop
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {

                    Array a = (Array)e.Data.GetData(DataFormats.FileDrop);
                    if (a != null)
                    {

                        string path = a.GetValue(0).ToString();
                        OnFileOpen?.Invoke(path);
                    }
                }
            };

        }

        private void SetPanelPosition(Control control)
        {
            Point p = PointToClient(Control.MousePosition);
            if (p.X + control.Width > this.Width)
                p.X = this.Width - control.Width;
            if (p.Y + control.Height > this.Height)
                p.Y = this.Height - control.Height;
            if (p.X < 0)
                p.X = 0;
            if (p.Y < 0)
                p.Y = 0;
            control.Top = p.Y;
            control.Left = p.X;
        }

        private string GetClipboardData()
        {
            try
            {
                string str = Clipboard.GetText();
                return str;
            }
            catch
            {
                return string.Empty;
            }

        }

        private void OpenSearch()
        {
            if (!SearchIsOpen)
            {
                SearchIsOpen = true;
                InvokeIfNeeded(delegate ()
                {
                    SetPanelPosition(pnSearch);
                    pnSearch.Visible = true;
                    pnSearch.BringToFront();
                    string clipboardData = GetClipboardData();
                    txtSearchContent.Text = string.IsNullOrEmpty(clipboardData) ? searchManager.LastSearch : clipboardData;
                    txtSearchContent.Focus();
                    txtSearchContent.SelectAll();
                });
            }
            else
            {
                InvokeIfNeeded(delegate ()
                {
                    pnSearch.Visible = true;
                    pnSearch.BringToFront();
                    string clipboardData = GetClipboardData();
                    txtSearchContent.Text = clipboardData;
                    txtSearchContent.Focus();
                    txtSearchContent.SelectAll();
                });
            }
        }

        private void CloseSearch()
        {
            if (SearchIsOpen)
            {
                SearchIsOpen = false;
                InvokeIfNeeded(delegate ()
                {
                    pnSearch.Visible = false;
                });
            }
            else if (pnBookmark.Visible)
                pnBookmark.Visible = false;
        }

        private void InitColors()
        {
            scintillaCode.SetSelectionBackColor(true, IntToColor(0x99C9EF));
            scintillaCode.Indicators[2].Style = IndicatorStyle.RoundBox;
            scintillaCode.Indicators[2].ForeColor = Color.Red;
            scintillaCode.IndicatorCurrent = 2;
        }

        private  Color IntToColor(int rgb)
        {
            return Color.FromArgb(255, (byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
        }

        private void InvokeIfNeeded(Action action)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(action);
            }
            else
            {
                action.Invoke();
            }
        }

        private void InitDefaultColor()
        {
            // scintillaCode.CaretForeColor = Color.White;
            // Configure the default style
            scintillaCode.StyleResetDefault();
            scintillaCode.Styles[Style.Default].Font = "Consolas";
            scintillaCode.Styles[Style.Default].Size = 11;
            scintillaCode.Styles[Style.Default].BackColor = IntToColor(0xFFFFFF);
            scintillaCode.Styles[Style.Default].ForeColor = Color.Black;
            scintillaCode.StyleClearAll();
        }

        private void InitSyntaxColorForLog()
        {
            InitDefaultColor();

            // Configure the log lexer styles
            scintillaCode.Styles[Style.Cpp.Identifier].ForeColor = Color.Black;
            scintillaCode.Styles[Style.Cpp.Comment].ForeColor = IntToColor(0xBD758B);
            scintillaCode.Styles[Style.Cpp.CommentLine].ForeColor = IntToColor(0x40BF57);
            scintillaCode.Styles[Style.Cpp.CommentDoc].ForeColor = IntToColor(0x2FAE35);
            scintillaCode.Styles[Style.Cpp.Number].ForeColor = IntToColor(0x0000FF);
            scintillaCode.Styles[Style.Cpp.String].ForeColor = Color.Black;
            scintillaCode.Styles[Style.Cpp.Character].ForeColor = Color.Black;
            scintillaCode.Styles[Style.Cpp.Preprocessor].ForeColor = Color.Black;
            scintillaCode.Styles[Style.Cpp.Operator].ForeColor = IntToColor(0x008000);
            //scintillaCode.Styles[Style.Cpp.Operator].ForeColor = IntToColor(0xED18EA);
            scintillaCode.Styles[Style.Cpp.Regex].ForeColor = Color.Black;
            //scintillaCode.Styles[Style.Cpp.CommentLineDoc].ForeColor = IntToColor(0x77A7DB);
            //scintillaCode.Styles[Style.Cpp.Word].ForeColor = IntToColor(0x48A8EE);
            //scintillaCode.Styles[Style.Cpp.Word2].ForeColor = IntToColor(0xF98906);
            scintillaCode.Styles[Style.Cpp.Word].ForeColor = IntToColor(0xF98906);
            scintillaCode.Styles[Style.Cpp.Word2].ForeColor = Color.Red;
            scintillaCode.Styles[Style.Cpp.CommentDocKeyword].ForeColor = Color.Black;
            scintillaCode.Styles[Style.Cpp.CommentDocKeywordError].ForeColor = Color.Black;
            scintillaCode.Styles[Style.Cpp.GlobalClass].ForeColor = Color.Black;

            scintillaCode.Lexer = Lexer.Cpp;

            scintillaCode.SetKeywords(0, "debug info warn Debug Info Warn DEBUG INFO WARN");
            scintillaCode.SetKeywords(1, "error Error ERROR");

            //scintillaCode.SetKeywords(0, "debug info warn Debug Info Warn DEBUG INFO WARN class extends implements import interface new case do while else if for in switch throw get set function var try catch finally while with default break continue delete return each const namespace package include use is as instanceof typeof author copy default deprecated eventType example exampleText exception haxe inheritDoc internal link mtasc mxmlc param private return see serial serialData serialField since throws usage version langversion playerversion productversion dynamic private public partial static intrinsic internal native override protected AS3 final super this arguments null Infinity NaN undefined true false abstract as base bool break by byte case catch char checked class const continue decimal default delegate do double descending explicit event extern else enum false finally fixed float for foreach from goto group if implicit in int interface internal into is lock long new null namespace object operator out override orderby params private protected public readonly ref return switch struct sbyte sealed short sizeof stackalloc static string  this throw true try typeof uint ulong unchecked unsafe ushort using var virtual volatile void while where yield");
            //scintillaCode.SetKeywords(1, "error Error ERROR void Null ArgumentError arguments Array Boolean Class Date DefinitionError Debug Info Error EvalError Function int Math Namespace Number Object RangeError ReferenceError RegExp SecurityError String SyntaxError TypeError uint XML XMLList Boolean Byte Char DateTime Decimal Double Int16 Int32 Int64 IntPtr SByte Single UInt16 UInt32 UInt64 UIntPtr Void Path File System Windows Forms ScintillaNET");

        }

        private void InitSyntaxColorForXml()
        {
            // Configure the default style
            InitDefaultColor();

            scintillaCode.Styles[Style.Html.Attribute].ForeColor = Color.Red;
            scintillaCode.Styles[Style.Html.Comment].ForeColor = IntToColor(0x40BF57);
          //  scintillaCode.Styles[Style.Html.Entity].ForeColor = Color.Red;
            scintillaCode.Styles[Style.Html.Script].ForeColor = Color.Blue;
           // scintillaCode.Styles[Style.Html.Value].ForeColor = Color.Green;
            scintillaCode.Styles[Style.Html.Tag].ForeColor = IntToColor(0x2469BF); 
            scintillaCode.Styles[Style.Html.DoubleString].ForeColor = IntToColor(0x248C85); 
            scintillaCode.Styles[Style.Html.AttributeUnknown].ForeColor = Color.Red;
            scintillaCode.Styles[Style.Html.Number].ForeColor = Color.Purple;
            scintillaCode.Lexer = Lexer.Html;
        }

        private void InitSyntaxColorForCpp()
        {
            // Configure the default style
            InitDefaultColor();

            // Configure the log lexer styles
            scintillaCode.Styles[Style.Cpp.Identifier].ForeColor = Color.Black;
            scintillaCode.Styles[Style.Cpp.Comment].ForeColor = IntToColor(0xBD758B);
            scintillaCode.Styles[Style.Cpp.CommentLine].ForeColor = IntToColor(0x40BF57);
            scintillaCode.Styles[Style.Cpp.CommentDoc].ForeColor = IntToColor(0x2FAE35);
            scintillaCode.Styles[Style.Cpp.Number].ForeColor = IntToColor(0x0000FF);
            scintillaCode.Styles[Style.Cpp.String].ForeColor = Color.Black;
            scintillaCode.Styles[Style.Cpp.Character].ForeColor = Color.Black;
            scintillaCode.Styles[Style.Cpp.Preprocessor].ForeColor = Color.Black;
           // scintillaCode.Styles[Style.Cpp.Operator].ForeColor = IntToColor(0x008000);
            scintillaCode.Styles[Style.Cpp.Operator].ForeColor = IntToColor(0xED18EA);
            scintillaCode.Styles[Style.Cpp.Regex].ForeColor = Color.Black;
            scintillaCode.Styles[Style.Cpp.CommentLineDoc].ForeColor = IntToColor(0x77A7DB);
            scintillaCode.Styles[Style.Cpp.Word].ForeColor = IntToColor(0x48A8EE);
            scintillaCode.Styles[Style.Cpp.Word2].ForeColor = IntToColor(0xF98906);
            scintillaCode.Styles[Style.Cpp.CommentDocKeyword].ForeColor = Color.Black;
            scintillaCode.Styles[Style.Cpp.CommentDocKeywordError].ForeColor = Color.Black;
            scintillaCode.Styles[Style.Cpp.GlobalClass].ForeColor = Color.Black;

            scintillaCode.Lexer = Lexer.Cpp;

            scintillaCode.SetKeywords(0, "class extends implements import interface new case do while else if for in switch throw get set function var try catch finally while with default break continue delete return each const namespace package include use is as instanceof typeof author copy default deprecated eventType example exampleText exception haxe inheritDoc internal link mtasc mxmlc param private return see serial serialData serialField since throws usage version langversion playerversion productversion dynamic private public partial static intrinsic internal native override protected AS3 final super this arguments null Infinity NaN undefined true false abstract as base bool break by byte case catch char checked class const continue decimal default delegate do double descending explicit event extern else enum false finally fixed float for foreach from goto group if implicit in int interface internal into is lock long new null namespace object operator out override orderby params private protected public readonly ref return switch struct sbyte sealed short sizeof stackalloc static string  this throw true try typeof uint ulong unchecked unsafe ushort using var virtual volatile void while where yield");
            scintillaCode.SetKeywords(1, "debug info error void Null ArgumentError arguments Array Boolean Class Date DefinitionError Debug Info Error EvalError Function int Math Namespace Number Object RangeError ReferenceError RegExp SecurityError String SyntaxError TypeError uint XML XMLList Boolean Byte Char DateTime Decimal Double Int16 Int32 Int64 IntPtr SByte Single UInt16 UInt32 UInt64 UIntPtr Void Path File System Windows Forms ScintillaNET");

        }

        private void BtnPrevSearch_Click(object sender, EventArgs e)
        {
            searchManager.Find(false, false);
        }

        private void BtnNextSearch_Click(object sender, EventArgs e)
        {
            searchManager.Find(true, false);
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            //  SearchManager.Find(true, true);
        }

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {

            if (HotKeyManager.IsHotkey(e, Keys.Enter))
            {
                searchManager.Find(true, true);
            }
            if (HotKeyManager.IsHotkey(e, Keys.Enter, true) || HotKeyManager.IsHotkey(e, Keys.Enter, false, true))
            {
                searchManager.Find(false, true);
            }
        }

        private void SetSerachFlag()
        {
            SearchFlags flag = SearchFlags.None;
            if (cbMatchCase.Checked)
                flag = flag | SearchFlags.MatchCase;
            if (cbWholeWord.Checked)
                flag = flag | SearchFlags.WholeWord;
            if (cbRegular.Checked)
                flag = flag | SearchFlags.Regex;
            if (searchManager.TextArea != null)
                searchManager.TextArea.SearchFlags = flag;

        }

        private void btnSearchDown_Click(object sender, EventArgs e)
        {
            AddToSearchHistory(txtSearchContent.Text);
            searchManager.Find(true, false);
        }

        private void btnSearchUp_Click(object sender, EventArgs e)
        {
            AddToSearchHistory(txtSearchContent.Text);
            searchManager.Find(false, false);
        }

        private void btnSerachBegin_Click(object sender, EventArgs e)
        {
            AddToSearchHistory(txtSearchContent.Text);
            searchManager.Find(true, true);
        }

        private void cbMatchCase_CheckedChanged(object sender, EventArgs e)
        {
            SetSerachFlag();
        }

        private void cbWholeWord_CheckedChanged(object sender, EventArgs e)
        {
            SetSerachFlag();
        }

        private void cbRegular_CheckedChanged(object sender, EventArgs e)
        {
            SetSerachFlag();
        }

        private void TxtSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                e.Handled = true;
            }
        }

        private void InitBookmarkPanel()
        {
            pnBookmark.Visible = false;
            pnSearch.Visible = false;
            InitListView();
        }

        private void InitListView()
        {
            lvMain.Items.Clear();
            lvMain.FullRowSelect = true;
            lvMain.View = View.Details;
            lvMain.GridLines = true;
        }

        private void RefreshList(List<Line> list)
        {
            _bookMarkList = list;
            ShowID(_bookMarkList);
        }

        private void AddBookMark(Line line)
        {
            var item = _bookMarkList.FirstOrDefault(q => q.Index == line.Index);
            if (item == null)
                _bookMarkList.Add(line);
            ShowID(_bookMarkList);
        }

        private void DelBookMark(Line line)
        {
            var item = _bookMarkList.FirstOrDefault(q => q.Index == line.Index);
            if (item != null)
                _bookMarkList.Remove(item);
            ShowID(_bookMarkList);
        }

        private void ShowID(List<Line> _bookMarkList)
        {
            lvMain.Items.Clear();
            _bookMarkList = _bookMarkList.OrderBy(q => q.Index).ToList();
            foreach (Line line in _bookMarkList)
            {
                ListViewItem lv = new ListViewItem();
                lv.Text = (line.Index + 1).ToString();
                lv.SubItems.Add(line.Text);
                lv.Tag = line;
                lvMain.Items.Add(lv);
            }
            if (lvMain.Items.Count > 0)
                lvMain.Items[0].Selected = true;
        }

        private void lvMain_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (lvMain.SelectedItems.Count > 0)
            {
                ListViewItem lv = lvMain.SelectedItems[0];
                SelectLineBookmark = lv.Tag as Line;
                ScrollToCenter(SelectLineBookmark.Index);
            }
        }

        private void lvMain_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (lvMain.SelectedItems.Count > 0)
                {
                    ListViewItem lv = lvMain.SelectedItems[0];
                    SelectLineBookmark = lv.Tag as Line;
                    ScrollToCenter(SelectLineBookmark.Index);
                }
            }
            else if (e.KeyCode == Keys.Delete)
            {
                if (lvMain.SelectedItems.Count > 0)
                {
                    ListViewItem lv = lvMain.SelectedItems[0];
                    SelectLineBookmark = lv.Tag as Line;
                    ChangeBookMark(SelectLineBookmark);
                }
            }
        }

        private void txtSearch_TextChanged_1(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtSearchBookMark.Text))
            {
                List<Line> list = this._bookMarkList.Where(q => q.Text.IndexOf(txtSearchBookMark.Text, StringComparison.OrdinalIgnoreCase) != -1).ToList();
                if (list != null)
                    ShowID(list);
            }
            else
                ShowID(this._bookMarkList);
        }

        private void txtSearch_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (lvMain.SelectedItems.Count > 0)
                {
                    ListViewItem lv = lvMain.SelectedItems[0];
                    SelectLineBookmark = lv.Tag as Line;
                    ScrollToCenter(SelectLineBookmark.Index);
                }
            }
            else if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
            {
                if (lvMain.SelectedItems.Count > 0)
                {
                    lvMain.Select();
                }
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshBookMarkList();
        }

        private void RefreshBookMarkList()
        {
            List<Line> markList = new List<Line>();
            markList = scintillaCode.Lines.Where(q => (q.MarkerGet() & mask) > 0).ToList();
            RefreshList(markList);
        }

        private void scintillaCode_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void scintillaCode_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                RefreshBookMarkList();
            }
        }

        private void btnDeleAllbookMark_Click(object sender, EventArgs e)
        {
            DeleteAllBookMark();
        }

        private void cbSearch_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtSearchContent.Text = cbSearch.Text;
        }

        private void pnBookmark_Resize(object sender, EventArgs e)
        {

        }

        private void lvMain_Resize(object sender, EventArgs e)
        {
            this.columnHeader2.Width = lvMain.Width - this.columnHeader1.Width;
        }

        private void AddToSearchHistory(string str)
        {
            if (!string.IsNullOrEmpty(str) && !cbSearch.Items.Contains(str))
                cbSearch.Items.Add(str);
        }

        private void btnViewText_Click(object sender, EventArgs e)
        {
            if(lvMain.Items.Count<=0)
            {
                PopUp.Information("No text founded");
                return;
            }
            StringBuilder sb = new StringBuilder();
            foreach( ListViewItem item in lvMain.Items)
            {
                sb.Append(item.SubItems[1].Text);
            }
            FormText ft = new FormText(sb.ToString());
            ft.ShowDialog();
        }
    }
}
