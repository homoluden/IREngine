﻿using System;
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
using System.Collections.Concurrent;

namespace IREngine
{
    public class LogDiffEventArgs : EventArgs
    {
        public LogDiffEventArgs(IEnumerable<string> addedLines)
        {
            AddedLines = addedLines;
        }

        public IEnumerable<string> AddedLines { get; set; }
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
            LogsTailLength = 1000;
            
            _defaultEngine = Ruby.CreateEngine((setup) => {
                setup.ExceptionDetail = true;
            });
            
            var paths = (new[] { Environment.CurrentDirectory, Environment.CurrentDirectory + @"\scripts"});
            paths.ToList().AddRange(_defaultEngine.GetSearchPaths());
            _defaultEngine.SetSearchPaths(paths);
            _defaultEngine.Runtime.LoadAssembly(typeof(IRE).Assembly);

            _outStream = new MemoryStream();

            _errStream = new MemoryStream();

            _defaultEngine.Runtime.IO.SetOutput(_outStream, Encoding.UTF8);
            _defaultEngine.Runtime.IO.SetErrorOutput(_errStream, Encoding.UTF8);
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

        // Pause in seconds between log updates
        public readonly double LOG_UPDATES_PAUSE = 1.0;
        public readonly int MAX_OUTPUT_STREAM_SIZE = 10*1024*1024;
        public readonly int MAX_ERROR_STREAM_SIZE = 10*1024*1024;

        #endregion

        #region Fields

        private readonly Dictionary<Guid, TaskRecord> _tasks = new Dictionary<Guid, TaskRecord>();

        private readonly ScriptEngine _defaultEngine;

        private readonly MemoryStream _outStream;
        private readonly MemoryStream _errStream;

        private readonly ConcurrentQueue<string> _outputLog = new ConcurrentQueue<string>();
        private readonly ConcurrentQueue<string> _errorLog = new ConcurrentQueue<string>();

        #endregion

        #region Properties

        public int LogsTailLength { get; set; }

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

        public string[] FullOutLog
        {
            get
            {
                return _outputLog.ToArray();
            }
        }

        public IEnumerable<string> OutLogTail
        {
            get
            {
                return _outputLog.Reverse().Take(LogsTailLength).Reverse();
            }
        }

        public IEnumerable<string> ErrLogTail
        {
            get
            {
                return _errorLog.Reverse().Take(LogsTailLength).Reverse();
            }
        }

        #endregion

        #region Private Methods

        #endregion

        #region Public Methods

        public void WriteMessage(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }
            
            foreach (var line in text.Split("\n\r".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            {
                _outputLog.Enqueue(line);
            }
        }

        public void WriteError(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }
            
            foreach (var line in text.Split("\n\r".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            {
                _errorLog.Enqueue(line);
            }
        }

        public Guid RunScriptAsync(string code)
        {
            var scriptScope = _defaultEngine.CreateScope();
            scriptScope.SetVariable("logger", IRE.Instance);
            
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
            task.ContinueWith((t) => Instance.RemoveTask(guid));
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
