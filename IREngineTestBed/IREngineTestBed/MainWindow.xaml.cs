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

namespace IREngineTestBed
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Consts
        
        #endregion

        private TaskScheduler _uiScheduler;

        public MainWindow()
        {
            InitializeComponent();

            _uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            IRE.Instance.OutputUpdated += (s, args) =>
            {
                string msg = string.Format("{0}", args.Data);
                Debug.WriteLine(msg);

                var uiUpdateTask = Task.Factory.StartNew(() =>
                {
                    foreach (var str in msg.Split("\n\r".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                    {
                        var trimmed = str.Trim();
                        OutputLogBox.Items.Add(trimmed);
                        OutputLogBox.ScrollIntoView(trimmed);
                    }
                },
                    Task.Factory.CancellationToken,
                    TaskCreationOptions.None,
                    _uiScheduler);
                uiUpdateTask.Wait();
            };
            IRE.Instance.ErrorUpdated += (s, args) =>
            {
                string msg = string.Format("{0}", args.Data);
                Debug.WriteLine(msg);

                var uiUpdateTask = Task.Factory.StartNew(() =>
                {
                    foreach (var str in msg.Split("\n\r".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                    {
                        var trimmed = str.Trim();
                        ErrorLogBox.Items.Add(trimmed);
                        ErrorLogBox.ScrollIntoView(trimmed);
                    }
                },
                    Task.Factory.CancellationToken,
                    TaskCreationOptions.None,
                    _uiScheduler);
                uiUpdateTask.Wait();
            };

        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            
            IRE.Instance.StartWatching();

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
        }
    }
}
