using System;
using System.Collections.Generic;
using System.Linq;
using NStack;
using Terminal.Gui;

namespace Guit
{
    public class CompletionTextField : TextField
    {
        readonly IEnumerable<string> completionValues;

        ustring previousText = string.Empty;
        int bestMatchPosition = -1;

        public CompletionTextField(params string[]? completionValues) : base(ustring.Empty)
        {
            this.completionValues = completionValues ?? Enumerable.Empty<string>();

            Changed += OnTextChanged;
        }

        bool IsCompletionEnabled { get; set; } = true;

        public override bool ProcessKey(KeyEvent kb)
        {
            switch (kb.Key)
            {
                case Key.ControlK:
                    IsCompletionEnabled = true;
                    CompleteWord();
                    return true;
                case Key.Backspace:
                    if (IsCompletionEnabled && CursorPosition == bestMatchPosition && !Text.IsEmpty)
                    {
                        Text = Text[0, CursorPosition];
                        IsCompletionEnabled = false;
                        return true;
                    }
                    break;
            }

            return base.ProcessKey(kb);
        }

        void OnTextChanged(object sender, ustring text)
        {
            try
            {
                if (previousText.Length < Text.Length && IsCompletionEnabled)
                    CompleteWord();
                else if (Text.IsEmpty) // Re-enable completion
                    IsCompletionEnabled = true;
            }
            finally
            {
                previousText = Text;
            }
        }

        void CompleteWord()
        {
            var enteredText = Text[0, CursorPosition];

            if (!enteredText.IsEmpty)
            {
                bestMatchPosition = -1;

                if (completionValues.FirstOrDefault(x => x.StartsWith(enteredText.ToString(), StringComparison.OrdinalIgnoreCase)) is string bestMatch)
                {
                    bestMatchPosition = CursorPosition;
                    Text = bestMatch;
                    CursorPosition = bestMatchPosition;
                }
                else
                {
                    Text = enteredText;
                }
            }
        }
    }
}