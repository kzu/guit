using System;
using System.Collections.Generic;
using Terminal.Gui;

namespace Guit
{
    class ProgressDialog : Dialog
    {
        ProgressBar progressBar;
        ListView listview;

        readonly List<string> entries = new List<string>();

        public ProgressDialog(string title)
            : base(title, 0, 0)
        {
            InitilizeComponents();
        }

        void InitilizeComponents()
        {
            Width = Dim.Fill(8);
            Height = Dim.Fill(8);

            listview = new ListView()
            {
                CanFocus = false,
                AllowsMarking = false,
                Height = Dim.Fill() - 1,
                Width = Dim.Fill()
            };

            progressBar = new ProgressBar()
            {
                Y = Pos.Bottom(listview)
            };

            Add(listview, progressBar);
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

        public void Report(string text, float progress)
        {
            if (text != null)
            {
                entries.Add(text);

                listview.SetSource(entries);

                var pageSize = listview.Bounds.Height;
                if (pageSize > 0)
                    listview.SelectedItem = ((entries.Count - 1) / pageSize) * pageSize;
            }

            if (progress > progressBar.Fraction)
                progressBar.Fraction = progress;
        }
    }
}