using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using IronRuby;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Scripting.Runtime;

namespace IREngine
{
    public class StringEventArgs : EventArgs
    {
        public StringEventArgs(string data)
        {
            Data = data;
        }
    
        public  string Data { get; set; }
    }

    public struct TaskRecord 
    {
        public CancellationTokenSource TokenSource;
        public Task Task;
    }

    public sealed class IRE
    {
        #region Singleton

        private static volatile IRE _instance;
        private static readonly object SyncRoot = new Object();

        private IRE()
        {
            _defaultEngine = Ruby.CreateEngine((setup) => {
                setup.ExceptionDetail = true;
            });
            _defaultEngine.Runtime.LoadAssembly(typeof(IRE).Assembly);

            _outStream = new MemoryStream();
            _outStringBuilder = new StringBuilder();

            _errStream = new MemoryStream();
            _errStringBuilder = new StringBuilder();

            _defaultEngine.Runtime.IO.SetOutput(_outStream, Encoding.UTF8);
            _defaultEngine.Runtime.IO.SetErrorOutput(_errStream, Encoding.UTF8);

            _outWatchAction = () =>
                                  {
                                      int i = 0;
                                      while (IsConsoleOutputWatchingEnabled)
                                      {
                                          string msg = string.Format("***\t_outWatchTask >> tick ({0})\t***", i++);
                                          Debug.WriteLine(msg);
                                          WriteMessage(msg);
                                          
                                          Task.Factory.CancellationToken.ThrowIfCancellationRequested();

                                          int currentLength = OutputBuilder.Length;
                                          if (OutputUpdated != null && currentLength != _lastOutSize)
                                          {
                                              OutputUpdated.Invoke(_outWatchTask,
                                                                   new StringEventArgs(OutputBuilder.
                                                                                           ToString(_lastOutSize,
                                                                                                    currentLength - _lastOutSize)));

                                              _lastOutSize = currentLength;
                                          }
                                          Thread.Sleep(TIME_BETWEEN_CONSOLE_OUTPUT_UPDATES);
                                      }
                                  };
            _outWatchExcHandler = (t) =>
                                      {
                                          if (t.Exception == null)
                                              return;

                                          Instance.WriteError(
                                              string.Format(
                                                  "!!!\tException raised in Output Watch Task\t!!!\n{0}",
                                                  t.Exception.InnerException.Message));
                                      };
            _errWatchAction = () =>
                                  {
                                      int i = 0;
                                      while (IsConsoleErrorWatchingEnabled)
                                      {
                                          string msg = string.Format("***\t_errWatchTask >> tick ({0})\t***", i++);
                                          Debug.WriteLine(msg);
                                          WriteError(msg);

                                          Task.Factory.CancellationToken.ThrowIfCancellationRequested();

                                          int currentLength = ErrorBuilder.Length;
                                          if (ErrorUpdated != null && currentLength != _lastErrSize)
                                          {
                                                ErrorUpdated.Invoke(_errWatchTask,
                                                                    new StringEventArgs(ErrorBuilder.
                                                                                        ToString(_lastErrSize, currentLength - _lastErrSize)));
                                              
                                              _lastErrSize = currentLength;
                                          }
                                          Thread.Sleep(TIME_BETWEEN_CONSOLE_OUTPUT_UPDATES);
                                      }
                                  };
            _errWatchExcHandler = (t) =>
                                      {
                                          if (t.Exception == null)
                                              return;

                                          Instance.WriteError(
                                              string.Format(
                                                  "!!!\tException raised in Error Watch Task\t!!!{0}",
                                                  t.Exception.InnerException.Message));
                                      };

            //StartWatching();
        }

        public static IRE Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (SyncRoot)
                    {
                        if (_instance == null)
                            _instance = new IRE();
                    }
                }

                return _instance;
            }
        }

        #endregion

        #region Consts

        public readonly int TIME_BETWEEN_CONSOLE_OUTPUT_UPDATES = 1000;
        public readonly int MAX_OUTPUT_STREAM_SIZE = 10*1024*1024;
        public readonly int MAX_ERROR_STREAM_SIZE = 10*1024*1024;

        #endregion

        #region Fields

        private Dictionary<Guid, TaskRecord> _tasks = new Dictionary<Guid, TaskRecord>();

        private bool _outWatchEnabled;
        private bool _errWatchEnabled;
        private readonly ScriptEngine _defaultEngine;

        private readonly MemoryStream _outStream;
        private Task _outWatchTask;
        private readonly CancellationTokenSource _outWatchTaskToken = new CancellationTokenSource();
        private int _lastOutSize = 0;
        private readonly MemoryStream _errStream;
        private Task _errWatchTask;
        private readonly CancellationTokenSource _errWatchTaskToken = new CancellationTokenSource();
        private int _lastErrSize = 0;
        private readonly StringBuilder _outStringBuilder;
        private readonly StringBuilder _errStringBuilder;

        private readonly Action _outWatchAction;
        private readonly Action<Task> _outWatchExcHandler;
        private readonly Action _errWatchAction;
        private readonly Action<Task> _errWatchExcHandler;

        private readonly CancellationTokenSource _scriptsToken = new CancellationTokenSource();
        #endregion

        #region Properties

        public ScriptEngine DefaultEngine
        {
            get
            {
                lock (SyncRoot)
                {
                    return _defaultEngine;
                }
            }
        }

        public StringBuilder OutputBuilder
        {
            get
            {
                lock (SyncRoot)
                {
                    return _outStringBuilder;
                }
            }
        }

        public StringBuilder ErrorBuilder
        {
            get
            {
                lock (SyncRoot)
                {
                    return _errStringBuilder;
                }
            }
        }

        public bool IsConsoleOutputWatchingEnabled
        {
            get
            {
                lock (SyncRoot)
                {
                    return _outWatchEnabled;
                }
            }
            set
            {
                lock (SyncRoot)
                {
                    _outWatchEnabled = value;
                }
            }
        }

        public bool IsConsoleErrorWatchingEnabled
        {
            get
            {
                lock (SyncRoot)
                {
                    return _errWatchEnabled;
                }
            }
            set
            {
                lock (SyncRoot)
                {
                    _errWatchEnabled = value;
                }
            }
        }

        public event EventHandler<StringEventArgs> OutputUpdated;
        public event EventHandler<StringEventArgs> ErrorUpdated;

        #endregion

        #region Private Methods

        #endregion

        #region Public Methods

        public void WriteMessage(string text)
        {
            lock (SyncRoot)
            {
                _outStringBuilder.AppendLine(text);
            }
        }

        public void WriteError(string text)
        {
            lock (SyncRoot)
            {
                _errStringBuilder.AppendLine(text);
            }
        }

        public void StartWatching()
        {
            StopWatching();
            if (_outWatchTask != null)
                _outWatchTask.Wait();

            if (_errWatchTask != null)
                _errWatchTask.Wait();
            
            IsConsoleOutputWatchingEnabled = IsConsoleErrorWatchingEnabled = true;

            _outWatchTask = Task.Factory.StartNew(_outWatchAction, _outWatchTaskToken.Token);
            _outWatchTask.ContinueWith(_outWatchExcHandler, TaskContinuationOptions.OnlyOnFaulted);
            _errWatchTask = Task.Factory.StartNew(_errWatchAction, _errWatchTaskToken.Token);
            _errWatchTask.ContinueWith(_errWatchExcHandler, TaskContinuationOptions.OnlyOnFaulted);
        }

        public void StopWatching()
        {
            IsConsoleOutputWatchingEnabled = IsConsoleErrorWatchingEnabled = false;
        }

        public Guid RunScriptAsync(string code)
        {
            var scriptScope = _defaultEngine.CreateScope();
            CompiledCode compiledCode = null;
            try
            {
                ScriptSource scriptSource = _defaultEngine.CreateScriptSourceFromString(code, SourceCodeKind.AutoDetect);

                var errListner = new ErrorSinkProxyListener(ErrorSink.Default);
                compiledCode = scriptSource.Compile(errListner);
            }
            catch (Exception ex)
            {
                WriteError(ex.Message);
                return Guid.Empty;
            }
            
            var action = new Action(() => compiledCode.Execute(scriptScope));
            var tokenSource = new CancellationTokenSource();
            var task = new Task(action, tokenSource.Token, TaskCreationOptions.LongRunning);
            var guid = Guid.NewGuid();
            AddTask(guid, new TaskRecord { TokenSource = tokenSource, Task = task });

            task.Start();
            task.ContinueWith((t) =>
                                  {
                                      // Actually we don't needed this due to "TaskContinuationOptions.OnlyOnFaulted" is set
                                      if (t.Exception == null)
                                          return;

                                      t.Exception.Flatten().Handle((ex) =>
                                                             {
                                                                 Instance.WriteError(t.Exception.InnerException.Message);
                                                                 return true;
                                                             });
                                  }, TaskContinuationOptions.OnlyOnFaulted);
            task.ContinueWith((t) =>
                                {
                                    Instance.RemoveTask(guid);
                                });
            return guid;
        }

        public void AddTask(Guid key, TaskRecord record)
        {
            _tasks.Add(key, record);
        }

        public void RemoveTask(Guid key)
        {
            _tasks.Remove(key);
        }

        public Task GetTask(Guid key)
        {
            return _tasks.ContainsKey(key) ? _tasks[key].Task : null;
        }

        public void RequestCancelation(Guid key)
        {
            var tokenSource = _tasks.ContainsKey(key) ? _tasks[key].TokenSource : null;

            if (tokenSource == null)
                return;

            tokenSource.Cancel();
        }

        #endregion

    }
}
