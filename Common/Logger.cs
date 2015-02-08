using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Common
{
		public class Logger
		{
			private const long MAX_SIZE = 512 * 1024; // Max Log File Size = Half MB
			private static readonly object _locker = new object();
			private static readonly string _fileName = Assembly.GetEntryAssembly().GetName().Name + ".log";
			private static readonly FileMode _mode = FileMode.Append;
			private const string LINE_FORMAT = "{0,-7}{1}: Line: {3,-2}, Member = {4}, Msg = '{2}'";
			
            public static Logger Instance { get; set; }

			public static Logger GetLogger(bool writeToConsole)
			{
				Instance.WriteToConsole = writeToConsole;
				return Instance;
			}

			public bool WriteToConsole { get; set; }

			public void Write(string s)
			{
				if ( string.IsNullOrEmpty(s) ) return;

				lock ( _locker )
					using ( var fs = File.Open(_fileName, _mode) )
					using ( var sw = new StreamWriter(fs) )
						sw.WriteLine(s);

				if(WriteToConsole) Console.WriteLine(s);
			}

			static Logger()
			{
				// Check if log file length has exceded MaxSize then reset file
				var fileExists = File.Exists(_fileName);
				lock ( _locker ) if ( fileExists )
						using ( var f = File.OpenRead(_fileName) )
							if ( f.Length > MAX_SIZE )
								_mode = FileMode.Create;

				var skipLine = _mode == FileMode.Append && fileExists ? "\n" : "";
				var logStr = string.Format("{1}{0,-7}{2}: Application Started", "INFO:", skipLine, DateTime.Now);
				lock ( _locker )
					using ( var fs = File.Open(_fileName, _mode) )
					using ( var sw = new StreamWriter(fs) )
						sw.WriteLine(logStr);

				Instance = new Logger();
			}

			public void Log(string msg, LogEvent eventType = LogEvent.Info, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
			{
				var logStr = string.Format(LINE_FORMAT, eventType.ToString().ToUpper() + ":", DateTime.Now, msg, lineNumber, caller);
				Write(logStr);
			}


			public void Log(string format, LogEvent eventType, params object[] p)
			{
				Log(String.Format(format, p), eventType);
			}


			public void Log(Exception e, LogEvent eventType = LogEvent.Error, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
			{
				var sb = new StringBuilder();
				var n = e.GetType().Name;
				string name = n.Aggregate("", (current, c) => current + ((c < 'a') ? ("_" + c) : c.ToString(CultureInfo.InvariantCulture)));

				name = name.Substring(1);
				sb.AppendFormat(LINE_FORMAT, eventType.ToString().ToUpper() + ":", DateTime.Now, name.ToUpper(), lineNumber, caller);

				if ( !String.IsNullOrEmpty(e.Source) )
				{
					sb.AppendLine(" Source");
					sb.AppendLine("    " + e.Source);
				}
				if ( !String.IsNullOrEmpty(e.Message) )
				{
					sb.AppendLine(" Message");
					sb.AppendLine("    " + e.Message);
				}
				if ( !String.IsNullOrEmpty(e.StackTrace) )
				{
					sb.AppendLine(" StackTrace");
					sb.AppendLine(e.StackTrace.Replace("  ", "   "));
				}
				var logStr = sb.ToString();
				Write(logStr);
			}


			public void Log(Type classType, LogEvent eventType = LogEvent.Info, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
			{
				var logStr = string.Format(LINE_FORMAT, eventType.ToString().ToUpper() + ":", DateTime.Now, classType.FullName, lineNumber, caller);
				Write(logStr);
			}
		}

		public enum LogEvent
		{
			Info, Debug, Warn, Error, Verbose, Critical, Fatal
		}

	}


