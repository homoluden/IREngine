using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using IREngine;
using System.Diagnostics;
using System.Windows.Threading;

namespace IREngineTestBed
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Consts

        // Time in milliseconds between log update
        private const double LOG_UPDATE_PERIOD_MS = 100.0;
        #endregion

        #region Fields

        private readonly DispatcherTimer _logUpdateTimer;

        private readonly TaskScheduler _uiScheduler;
        private Guid _taskKey;

        #endregion

        public MainWindow()
        {
            InitializeComponent();

            _uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            _logUpdateTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(LOG_UPDATE_PERIOD_MS), IsEnabled = false };
            _logUpdateTimer.Tick += _logUpdateTimer_Tick;
        }
        
        void _logUpdateTimer_Tick(object sender, EventArgs e)
        {
            if (WatchTimerSwitch.IsChecked != true)
                return;
            
            var logTail = IRE.Instance.OutLogTail.ToList();
            if (logTail.Count > 0)
            {
                OutputLogBox.Items.Clear();
                foreach (var line in logTail)
	            {
                    OutputLogBox.Items.Add(line);
	            }

                if (OutputLogBox.Items.Count > 0)
                {
                    OutputLogBox.ScrollIntoView(OutputLogBox.Items[OutputLogBox.Items.Count - 1]);
                }
            }
            
            logTail = IRE.Instance.ErrLogTail.ToList();
            if (logTail.Count > 0)
            {
                ErrorLogBox.Items.Clear();
                foreach (var line in logTail)
                {
                    ErrorLogBox.Items.Add(line);
                }

                if (ErrorLogBox.Items.Count > 0)
                {
                    ErrorLogBox.ScrollIntoView(ErrorLogBox.Items[ErrorLogBox.Items.Count - 1]);
                }       
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            RunExample();
        }

        private void RunExample()
        {
            string code;
            using (var reader = File.OpenText(@"scripts\game\character_test.rb"))
            {
                code = reader.ReadToEnd();                
            }

            _taskKey = IRE.Instance.RunScriptAsync(code);
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            IRE.Instance.RequestCancelation(_taskKey);
        }

        private void TimerWatcherSelector_Checked(object sender, RoutedEventArgs e)
        {
            _logUpdateTimer.Start();
        }

        private void TimerWatcherSelector_Unchecked(object sender, RoutedEventArgs e)
        {
            _logUpdateTimer.Stop();
        }

        private void ReadFullLogButtonOnClick(object sender, RoutedEventArgs e)
        {
            WatchTimerSwitch.IsChecked = false;
            OutputLogBox.Items.Clear();
            OutputLogBox.ItemsSource = IRE.Instance.FullOutLog;
        }
    }
}
