using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace wjw.Editor{
	internal class SearchManager {

		public  ScintillaNET.Scintilla TextArea;
		public  TextBox SearchBox;
		public  string LastSearch = "";
		public  int LastSearchIndex;

        public void Find(bool next, bool incremental) {
			bool first = LastSearch != SearchBox.Text;

			
			if (SearchBox.Text.Length > 0) {

				if (next) {
                    if (incremental && first)
                        TextArea.TargetStart = 0;
                    else
                        TextArea.TargetStart = TextArea.CurrentPosition;
                    LastSearch = SearchBox.Text;
                    TextArea.TargetEnd = TextArea.TextLength;

					// Search, and if not found..
					if (TextArea.SearchInTarget(LastSearch) == -1) {

                        PopUp.Information("Search finished!");
                        LastSearch = "";
                        return;

					}

				} else {
                    if (LastSearchIndex != 0 && TextArea.SelectedText.Length>0)
                        TextArea.TargetStart = LastSearchIndex;
                    else if(LastSearchIndex==0 && TextArea.SelectedText==LastSearch && TextArea.SelectedText.Length>0)
                    {
                        TextArea.TargetStart = 0;
                    }
                    else
                        TextArea.TargetStart = TextArea.CurrentPosition;
                    TextArea.TargetEnd = 0;
                    LastSearch = SearchBox.Text;

                    // Search, and if not found..
                    if (TextArea.SearchInTarget(LastSearch) == -1) {

                        PopUp.Information("Search finished!");
                        LastSearch = "";
                        return;
					}

				}

				// Select the occurance
				LastSearchIndex = TextArea.TargetStart;
               	TextArea.SetSelection(TextArea.TargetEnd, TextArea.TargetStart);
                int currentLineIndex = TextArea.LineFromPosition(TextArea.CurrentPosition);
                Line currentLine = TextArea.Lines[currentLineIndex];
                int linesInView = TextArea.LinesOnScreen;
                int halfLines = linesInView / 2;
                int lowerLine = currentLineIndex + halfLines > TextArea.Lines.Count ? TextArea.Lines.Count : currentLineIndex + halfLines;
                int upperLine= currentLineIndex - halfLines > 0 ? currentLineIndex - halfLines : 0;
                if(next)
                    TextArea.ScrollRange(TextArea.Lines[lowerLine].Position, 0);
                else
                {
                    TextArea.ScrollRange(TextArea.Lines[upperLine].Position, 0);
                    TextArea.ScrollRange(TextArea.Lines[lowerLine].Position, 0);
                }

                TextArea.ScrollCaret();
            }

			SearchBox.Focus();
		}


	}
}
