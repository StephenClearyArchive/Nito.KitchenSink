// <copyright file="RolloverLogTraceListener.cs" company="Nito Programs">
//     Copyright (c) 2009-2010 Nito Programs.
// </copyright>

namespace Nito.KitchenSink
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using Nito.Linq;

    /// <summary>
    /// Implements a trace listener that writes messages to an on-disk rolling log.
    /// </summary>
    [CLSCompliant(false)]
    public sealed class RolloverLogTraceListener : TraceListener
    {
        /// <summary>
        /// The list of archive files currently in the directory, sorted by name (timestamp).
        /// </summary>
        private List<string> files;

        /// <summary>
        /// The current log file, "current.txt".
        /// </summary>
        private FileStream currentFile;

        /// <summary>
        /// The text writer for the current log file. This is null if the object has been disposed.
        /// </summary>
        private TextWriter current;

        /// <summary>
        /// Initializes a new instance of the <see cref="RolloverLogTraceListener"/> class, with the parameters passed from the application configuration.
        /// </summary>
        /// <param name="initializeData">The parameters from the application configuration file.</param>
        public RolloverLogTraceListener(string initializeData)
        {
            // Pull out our parameters from the initializer
            string[] args = initializeData.Split(';');
            if (args.Length < 3)
            {
                throw new ArgumentException("initializeData does not contain all three arguments", "initializeData");
            }

            this.LogDirectory = Path.GetFullPath(args[0]);
            this.MaxFileSize = uint.Parse(args[1], NumberStyles.Integer, NumberFormatInfo.InvariantInfo);
            this.MaxFiles = uint.Parse(args[2], NumberStyles.Integer, NumberFormatInfo.InvariantInfo);

            // Create the log directory if it doesn't already exist
            Directory.CreateDirectory(this.LogDirectory);

            // Get the list of files, sorted by filename (timestamp)
            var timestampFileName = new Regex(@"\d{4}-\d{2}-\d{2} \d{2};\d{2};\d{2}\.\d{3}\.txt");
            var allFiles = Directory.GetFiles(this.LogDirectory).Select(x => Path.GetFileName(x));
            this.files = allFiles.Where(x => timestampFileName.IsMatch(x)).ToList();
            this.files.Sort(StringComparer.OrdinalIgnoreCase);

            // Recover from a previous crash if necessary
            bool recovery = false;
            if (allFiles.Contains("current.txt", StringComparer.OrdinalIgnoreCase))
            {
                this.Rollover(DateTime.Now);
                recovery = true;
            }
            else
            {
                this.TrimArchiveFiles();
            }

            // Create the new current log file
            this.OpenCurrent();

            // Make a note of the crash, if it happened
            if (recovery)
            {
                this.WriteLine("Automatic log rollover due to crash.");
            }
        }

        /// <summary>
        /// Gets the log directory, containing the current and all archive log files.
        /// </summary>
        public string LogDirectory { get; private set; }

        /// <summary>
        /// Gets the maximum size of the current log file.
        /// </summary>
        public uint MaxFileSize { get; private set; }

        /// <summary>
        /// Gets the maximum number of archived log files.
        /// </summary>
        public uint MaxFiles { get; private set; }

        /// <summary>
        /// Writes a message to the log.
        /// </summary>
        /// <param name="message">A message to write.</param>
        public override void Write(string message)
        {
            this.WriteLine(message);
        }

        /// <summary>
        /// Writes a message to the log.
        /// </summary>
        /// <param name="message">A message to write.</param>
        public override void WriteLine(string message)
        {
            // Timestamp all messages
            DateTime now = DateTime.Now;
            this.current.WriteLine(now.ToString("yyyy-MM-dd hh:mm:ss.fff", DateTimeFormatInfo.InvariantInfo) + " " + message.Printable());

            // Check new file position and rollover as necessary
            if (this.currentFile.Position > this.MaxFileSize)
            {
                this.CloseCurrentAndRollover(now);
                this.OpenCurrent();
            }
        }

        /// <summary>
        /// Flushes the output buffer. Does not cause a rollover.
        /// </summary>
        public override void Flush()
        {
            this.current.Flush();
            this.currentFile.Flush();
        }

        /// <summary>
        /// Closes the output stream so it no longer receives tracing or debugging output.
        /// </summary>
        public override void Close()
        {
            if (this.current != null)
            {
                this.Flush();
                this.CloseCurrentAndRollover(DateTime.Now);

                this.current = null;
            }

            base.Close();
        }

        /// <summary>
        /// Releases the resources owned by this object.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Close();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Removes archived log files (oldest first) if there are too many.
        /// </summary>
        private void TrimArchiveFiles()
        {
            while (this.files.Count > this.MaxFiles)
            {
                File.Delete(Path.Combine(this.LogDirectory, this.files[0]));
                this.files.RemoveAt(0);
            }
        }

        /// <summary>
        /// Trims the archived log files if there are too many, and changes the current log file into an archived log file. The current log file must be closed before invoking this method.
        /// </summary>
        /// <param name="now">The date/time stamp of when the current log file gets archived.</param>
        private void Rollover(DateTime now)
        {
            // Rename current.txt to a timestamped name
            string newFileName = now.ToString("yyyy-MM-dd hh;mm;ss.fff", DateTimeFormatInfo.InvariantInfo) + ".txt";
            if (File.Exists(Path.Combine(this.LogDirectory, newFileName)))
            {
                // This prevents any possible naming conflicts, which are rare in the real world but common in testing
                newFileName += " " + Guid.NewGuid().ToString("N");
            }

            File.Move(Path.Combine(this.LogDirectory, "current.txt"), Path.Combine(this.LogDirectory, newFileName));
            this.files.AsSorted().Insert(newFileName);

            // Trim old log files as necessary
            this.TrimArchiveFiles();
        }

        /// <summary>
        /// Creates a new current log file.
        /// </summary>
        private void OpenCurrent()
        {
            this.currentFile = new FileStream(Path.Combine(this.LogDirectory, "current.txt"), FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read);
            this.current = new StreamWriter(this.currentFile, new UTF8Encoding(false, true)) { AutoFlush = true };
        }

        /// <summary>
        /// Closes the current log file, rolling over if necessary.
        /// </summary>
        /// <param name="now">The date/time stamp of when the current log file gets archived.</param>
        private void CloseCurrentAndRollover(DateTime now)
        {
            bool currentHasData = this.currentFile.Position != 0;
            this.current.Close();
            this.currentFile.Close();

            if (currentHasData)
            {
                this.Rollover(now);
            }
            else
            {
                File.Delete(Path.Combine(this.LogDirectory, "current.txt"));
            }
        }
    }
}
