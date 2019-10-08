using System;
using System.Collections.Generic;
using System.Text;
using Terminal.Gui;

namespace Guit
{
    class ProgressDialog : Dialog
    {
        ProgressBar progressBar;
        TextView outputTextView;

        int line = 0;
        StringBuilder lines = new StringBuilder();

        public ProgressDialog(string title)
            : base(title, 0, 0)
        {
            Width = Dim.Fill(8);
            Height = Dim.Fill(8);

            outputTextView = new TextView
            {
                ReadOnly = false,
                Height = Dim.Fill(2),
                Width = Dim.Fill(2),
                X = 1,
                Y = 1
            };

            progressBar = new ProgressBar();

            Add(new StackPanel(outputTextView, new EmptyLine(), progressBar));
        }

        public override bool ProcessKey(KeyEvent kb)
        {
            if ((kb.KeyValue == (int)Key.Enter && progressBar.Fraction == 1) || kb.KeyValue == (int)Key.Esc)
            {
                Running = false;
                return false;
            }

            return base.ProcessKey(kb);
        }

        public void Report(string? text, float progress)
        {
            if (text != null)
            {
                lines.AppendLine(text);
                outputTextView.Text = lines.ToString();

                if (++line > outputTextView.Frame.Height)
                    outputTextView.ScrollTo(line - outputTextView.Frame.Height);
            }

            if (progress > progressBar.Fraction)
                progressBar.Fraction = progress;
        }
    }
}