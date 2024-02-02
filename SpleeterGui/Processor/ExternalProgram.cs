using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace SpleeterGui.Processor
{
    public delegate void DataLineReceivedEventHandler(string line);

    public delegate void ProgramStateChangedEventHandler(ExternalProgramState newState);

    public enum ExternalProgramState
    {
        NotStarted,
        Running,
        Terminated
    }

    public class ExternalProgram
    {
        private Process _process;

        private ExternalProgramState _state;

        public event DataLineReceivedEventHandler StdOutDataLineReceived = null;

        public event DataLineReceivedEventHandler StdErrDataLineReceived = null;

        public event ProgramStateChangedEventHandler StateChanged = null;

        public Process Process => _process;

        public bool HasExited => _process.HasExited;

        public int ExitCode { get; private set; } = 0;

        public ExternalProgramState State
        {
            get
            {
                return _state;
            }

            set
            {
                _state = value;

                StateChanged?.Invoke(_state);
            }
        }

        public ExternalProgram(string filename, string arguments, Encoding encoding = null)
        {
            _process = new Process();

            _process.StartInfo.FileName = filename;
            _process.StartInfo.Arguments = arguments;
            _process.StartInfo.CreateNoWindow = true;
            _process.StartInfo.UseShellExecute = false;
            _process.StartInfo.StandardOutputEncoding = encoding ?? Encoding.Default;
            _process.StartInfo.StandardErrorEncoding = encoding ?? Encoding.Default;
            _process.StartInfo.RedirectStandardOutput = true;
            _process.StartInfo.RedirectStandardError = true;

            _process.OutputDataReceived += Process_OutputDataReceived;
            _process.ErrorDataReceived += Process_ErrorDataReceived;

            _process.EnableRaisingEvents = true;
            _process.Exited += Process_Exited;

            State = ExternalProgramState.NotStarted;
        }

        ~ExternalProgram()
        {
            try
            {
                if (!_process.HasExited)
                {
                    _process.Kill();
                }
            }
            catch (Exception)
            {
                // do nothing
            }
        }

        public bool StartExecute(out string exceptionMessage)
        {
            ExitCode = 0;

            bool started;
            try
            {
                started = _process.Start();
                exceptionMessage = null;
            }
            catch (Exception ex)
            {
                started = false;
                exceptionMessage = ex.Message;
            }

            if (!started)
            {
                return false;
            }

            State = ExternalProgramState.Running;

            ProcessManager.Add(this);
            ChildProcessTracker.AddProcess(_process);

            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();

            return true;
        }

        public void Kill()
        {
            _process.Kill();
            ExitCode = 500;
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            // https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.datareceivedeventhandler?view=netframework-4.6.1
            // When the redirected stream is closed, a null line is sent to the event handler.
            // Ensure that your event handler checks for this condition before accessing the Data property.

            // The situation where the stream is closed is handled by external code.
            /*
            if (e.Data == null)
            {
                return;
            }
            */

            StdOutDataLineReceived?.Invoke(e.Data);
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            /*
            if (e.Data == null)
            {
                return;
            }
            */

            StdErrDataLineReceived?.Invoke(e.Data);
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            ExitCode = _process.ExitCode;

            State = ExternalProgramState.Terminated;
        }

        // https://stackoverflow.com/questions/5510343/escape-command-line-arguments-in-c-sharp/37646895#37646895
        /// <summary>
        ///     This routine appends the given argument to a command line such that
        ///     CommandLineToArgvW will return the argument string unchanged. Arguments
        ///     in a command line should be separated by spaces; this function does
        ///     not add these spaces.
        /// </summary>
        /// <param name="argument">Supplies the argument to encode.</param>
        /// <param name="force">
        ///     Supplies an indication of whether we should quote the argument even if it 
        ///     does not contain any characters that would ordinarily require quoting.
        /// </param>
        public static string EncodeParameterArgument(string argument, bool force = false)
        {
            if (argument == null) throw new ArgumentNullException(nameof(argument));

            // Unless we're told otherwise, don't quote unless we actually
            // need to do so --- hopefully avoid problems if programs won't
            // parse quotes properly
            if (force == false
                && argument.Length > 0
                && argument.IndexOfAny(" \t\n\v\"".ToCharArray()) == -1)
            {
                return argument;
            }

            var quoted = new StringBuilder();
            quoted.Append('"');

            var numberBackslashes = 0;

            foreach (var chr in argument)
            {
                switch (chr)
                {
                    case '\\':
                        numberBackslashes++;
                        continue;
                    case '"':
                        // Escape all backslashes and the following
                        // double quotation mark.
                        quoted.Append('\\', numberBackslashes * 2 + 1);
                        quoted.Append(chr);
                        break;
                    default:
                        // Backslashes aren't special here.
                        quoted.Append('\\', numberBackslashes);
                        quoted.Append(chr);
                        break;
                }
                numberBackslashes = 0;
            }

            // Escape all backslashes, but let the terminating
            // double quotation mark we add below be interpreted
            // as a metacharacter.
            quoted.Append('\\', numberBackslashes * 2);
            quoted.Append('"');

            return quoted.ToString();
        }
    }
}
