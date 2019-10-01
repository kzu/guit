using System.Collections.Generic;
using Terminal.Gui;

namespace Guit
{
    class MinimalProgressDialog : Dialog
    {
        ProgressBar progressBar;

        readonly List<string> entries = new List<string>();

        public MinimalProgressDialog(string title)
            : base(title, 0, 0)
        {
            InitilizeComponents();
        }

        void InitilizeComponents()
        {
            Width = 60;
            Height = 6;

            progressBar = new ProgressBar() { Y = 1 };

            Add(progressBar);
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

        public void Report(float progress)
        {
            if (progress > progressBar.Fraction)
                progressBar.Fraction = progress;
        }
    }
}