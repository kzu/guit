using Terminal.Gui;

namespace Guit
{
    class App : Toplevel
    {
        private CompositionManager manager;
        private Label spinner = new Label("Loading plugins...")
        {
            X = Pos.Center(),
            Y = Pos.Center(),
        };

        public App(CompositionManager manager)
        {
            this.manager = manager;
            Add(spinner);
        }

        public override void WillPresent()
        {
            base.WillPresent();
            RunShell();
        }

        void RunShell()
        {
            while (true)
            {
                //var wait = new SpinWait();
                //var task = Task.Run(() => manager.CreateComposition());

                //var chars = new[] { '◴', '◷', '◶', '◵' };
                //var chars = new[] { '|', '/', '-', '\\' };
                //var index = 0;

                //while (!task.IsCompleted)
                //{
                //    wait.SpinOnce();
                //    //if (index >= chars.Length)
                //    //    index = 0;
                //    // TODO: refresh spinner
                //    //index++;
                //}

                using (var composition = manager.CreateComposition())
                {
                    //Force all singletons to be instantiated.
                    composition.GetExports<ISingleton>();

                    spinner.Text = "";

                    //Obtain our first exported value
                    Application.Run(composition.GetExport<Shell>());

                    spinner.Text = "Reloading plugins...";
                }
            }
        }
    }
}
