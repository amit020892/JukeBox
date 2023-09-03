using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace JukeBoxSolutions
{
    internal delegate void Invoker();
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            //ApplicationInitialize = _applicationInitialize;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }
        public static new App Current
        {
            get { return Application.Current as App; }
        }
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {

        }

        CustomSplashScreen splashScreen { get; set; }

        private bool loaded { get; set; }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            //initialize the splash screen and set it as the application main window
            splashScreen = new CustomSplashScreen();
            this.MainWindow = splashScreen;
            splashScreen.Show();

            Thread.Sleep(3000);
            splashScreen.LoadingLabel.Opacity = 1;
            Task.Run(async () =>
            {
                for (var i = 1; i <= 100; i++)
                {
                    splashScreen.Dispatcher.Invoke(() =>
                    {
                        splashScreen.ProgressBarX.Value = i; // or i / 20 for %
                    });
                    await Task.Delay(100);
                }
            })
    .ContinueWith(_ =>
    {
        var mainWindow = new MainWindow();
        this.MainWindow = mainWindow;
        mainWindow.Show();
        var anim = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = new Duration(TimeSpan.FromSeconds(2))
        };

        anim.Completed += Anim_Completed;

        splashScreen.BeginAnimation(System.Windows.Window.OpacityProperty, anim);
    }, TaskScheduler.FromCurrentSynchronizationContext());






            //Task.Factory.StartNew(() =>
            //{
            //    //simulate some work being done
            //    System.Threading.Thread.Sleep(3000);

            //    //since we're not on the UI thread
            //    //once we're done we need to use the Dispatcher
            //    //to create and show the main window
            //    this.Dispatcher.Invoke(() =>
            //    {
            //        //initialize the main window, set it as the application main window
            //        //and close the splash screen
            //        var mainWindow = new MainWindow();
            //        this.MainWindow = mainWindow;
            //        mainWindow.Show();
            //        var anim = new DoubleAnimation
            //        {
            //            From = 1,
            //            To = 0,
            //            Duration = new Duration(TimeSpan.FromSeconds(2))
            //        };

            //        anim.Completed += Anim_Completed;

            //        splashScreen.BeginAnimation(System.Windows.Window.OpacityProperty, anim);
            //    });
            //});
        }


        private void Anim_Completed(object sender, EventArgs e)
        {
            splashScreen.Close();
        }
    }
}
