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
        
        #endregion

        #region Fields

        private DispatcherTimer _logUpdateTimer;

        #endregion

        private TaskScheduler _uiScheduler;

        public MainWindow()
        {
            InitializeComponent();

            _uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            _logUpdateTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100.0), IsEnabled = false };
            _logUpdateTimer.Tick += new EventHandler(_logUpdateTimer_Tick);
            IRE.Instance.OutputUpdated += OnOutputUpdated;
            IRE.Instance.ErrorUpdated += OnErrorUpdated;

        }
        private void OnOutputUpdated(object s, LogDiffEventArgs args)
        {
            var uiUpdateTask = Task.Factory.StartNew(() =>
            {
                if (TimerWatcherSelector.IsChecked == true)
                    return;

                foreach (var line in args.AddedLines)
                {
                    OutputLogBox.Items.Add(line);
                    if (OutputLogBox.Items.Count >= IRE.Instance.LogsTailLength)
                    {
                        //OutputLogBox.Items.RemoveAt(0);
                    }

                    OutputLogBox.ScrollIntoView(line);
                }
            },
                     Task.Factory.CancellationToken,
                     TaskCreationOptions.None,
                     _uiScheduler);
            uiUpdateTask.Wait();
        }
        private void OnErrorUpdated(object s, LogDiffEventArgs args)
        {
            if (TimerWatcherSelector.IsChecked == true)
                return;

            var uiUpdateTask = Task.Factory.StartNew(() =>
            {
                foreach (var line in args.AddedLines)
                {
                    ErrorLogBox.Items.Add(line);
                    if (ErrorLogBox.Items.Count >= IRE.Instance.LogsTailLength)
                    {
                        ErrorLogBox.Items.RemoveAt(0);
                    }

                    ErrorLogBox.ScrollIntoView(line);
                }
            },
                Task.Factory.CancellationToken,
                TaskCreationOptions.None,
                _uiScheduler);
            uiUpdateTask.Wait();
        }
        void _logUpdateTimer_Tick(object sender, EventArgs e)
        {
            if (TimerWatcherSelector.IsChecked != true)
                return;
            
            OutputLogBox.Items.Clear();
            foreach (var line in IRE.Instance.OutLogTail)
	        {
                OutputLogBox.Items.Add(line);
	        }

            if (OutputLogBox.Items.Count > 0)
            {
                OutputLogBox.ScrollIntoView(OutputLogBox.Items[OutputLogBox.Items.Count - 1]);
            }


            ErrorLogBox.Items.Clear();
            foreach (var line in IRE.Instance.ErrLogTail)
            {
                ErrorLogBox.Items.Add(line);
            }

            if (ErrorLogBox.Items.Count > 0)
            {
                ErrorLogBox.ScrollIntoView(ErrorLogBox.Items[ErrorLogBox.Items.Count - 1]);
            }            
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            
            IRE.Instance.StartWatching();
            //_logUpdateTimer.Start();

            RunExample();
        }

        private void RunExample()
        {
            string code = string.Empty;
            using (var reader = File.OpenText(@"scripts\game\character_test.rb"))
            {
                code = reader.ReadToEnd();                
            }

            var taskKey = IRE.Instance.RunScriptAsync(code);
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            IRE.Instance.StopWatching();

            //_logUpdateTimer.Stop();
        }

        private void TimerWatcherSelector_Checked(object sender, RoutedEventArgs e)
        {
            IRE.Instance.OutputUpdated -= OnOutputUpdated;
            IRE.Instance.ErrorUpdated -= OnErrorUpdated;

            _logUpdateTimer.Start();
        }

        private void TimerWatcherSelector_Unchecked(object sender, RoutedEventArgs e)
        {
            IRE.Instance.OutputUpdated -= OnOutputUpdated;
            IRE.Instance.ErrorUpdated -= OnErrorUpdated;

            IRE.Instance.OutputUpdated += OnOutputUpdated;
            IRE.Instance.ErrorUpdated += OnErrorUpdated;

            _logUpdateTimer.Stop();
        }
    }
}
