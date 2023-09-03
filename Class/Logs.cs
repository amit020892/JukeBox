using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace JukeBoxSolutions.Class
{
    class Logs
    {
        public static List<string> ActionSet { get; set; }
        public static void StartActionSet()
        {
            ActionSet = new List<string>();
        }

        public static void ActionSetFail(string message = "")
        {
            if (!String.IsNullOrEmpty(message))
            {
                ActionSet.Add(message);
            }

            NewEntry("Failed Action Set");
            WriteEntry(ActionSet);
            StartActionSet();
        }
        public static void WriteActionset(string message)
        {
            ActionSet.Add(message);
        }

        private static string logpath = "";
        public static void ResetLog()
        {
            string dir = Directory.GetCurrentDirectory();

            logpath = dir + "LogFile.txt";

            // This text is added only once to the file.
            if (!File.Exists(logpath))
            {
                // Create a file to write to.
                File.WriteAllText(logpath, String.Empty);
                using (StreamWriter sw = File.CreateText(logpath))
                {
                    sw.WriteLine("Hello");
                }
            }

            NewEntry("Welcome");
        }
        public static void NewEntry(string message = "New Entry")
        {
            using (StreamWriter sw = File.AppendText(logpath))
            {
                sw.WriteLine("------------------------------------------------------");
                sw.WriteLine(message);
                sw.WriteLine("------------------------------------------------------");
            }
        }
        public static void WriteEntry(string message)
        {
            // This text is always added, making the file longer over time
            // if it is not deleted.
            using (StreamWriter sw = File.AppendText(logpath))
            {
                sw.WriteLine(message);
            }
        }
        public static void WriteEntry(List<string> messages)
        {
            // This text is always added, making the file longer over time
            // if it is not deleted.
            using (StreamWriter sw = File.AppendText(logpath))
            {
                foreach (string s in messages)
                {
                    sw.WriteLine(s);
                }
            }
        }


    }

    internal static class LogSystem
    {
        internal enum Processes
        {
            iTunesImport,
            iTunesUpdate,
            ImportDataDump,
            VLCCorePlayer,
            Timer
        }

        internal static int StartProcess(Processes processes, string datafilestype)
        {
            return StartProcess(processes, new List<string>(), datafilestype);
        }

        internal static int StartProcess(Processes processes, List<string> datafiles, string datafilestype)
        {

            int logID = sbBuffer.Count();
            StringBuilder sb = new StringBuilder();

            switch (processes)
            {
                case Processes.iTunesImport:
                    sb.AppendLine(DateTime.Now.ToLongTimeString() + ":  Starting iTunes Import Process with " + datafiles.Count() + " files");
                    sb.AppendLine("----------------------------------------------");
                    break;
                case Processes.VLCCorePlayer:
                    sb.AppendLine(DateTime.Now.ToLongTimeString() + ":  Playing new media");
                    sb.AppendLine(datafilestype);
                    foreach (var s in datafiles)
                        sb.AppendLine(s);
                    sb.AppendLine("----------------------------------------------");
                    break;
                case Processes.Timer:
                    sb.AppendLine(DateTime.Now.ToLongTimeString() + ":  Starting " + datafilestype);
                    sb.AppendLine("----------------------------------------------");
                    break;
            }

            sbBuffer.Add(new StringBuilderExtended() { Id = logID, Process = processes, StringBuilder = sb, DataFilesType = datafilestype });

            UpdateFile(processes, logID);



            // Override relevant physical file
            // Capture start details
            // Capture dataset

            // Date + Time + Start iTunes Import Process
            // 

            return logID;
        }

        internal static void EndProcess(int logID)
        {
            EndProcess(logID, new List<string>());
        }
        internal static void EndProcess(int logID, List<string> variables)
        {
            var buffer = sbBuffer.First(f => f.Id == logID);

            foreach (string s in variables)
                buffer.StringBuilder.AppendLine(s);

            buffer.StringBuilder.AppendLine("----------------------------------------------");
            buffer.StringBuilder.AppendLine(DateTime.Now.ToLongTimeString() + ":  End of Process");
            UpdateFile(buffer.Process, logID);

            sbBuffer.Remove(buffer);
        }

        internal static void AddEvent(int logID, string v, bool Timestamp = false)
        {
            try
            {
                if (Timestamp)
                    sbBuffer.First(f => f.Id == logID).StringBuilder.AppendLine(DateTime.Now.ToLongTimeString() + " :: " + v);
                else
                    sbBuffer.First(f => f.Id == logID).StringBuilder.AppendLine(v);
                UpdateFile(sbBuffer.First(f => f.Id == logID).Process, logID);
            }
            catch
            {

            }
        }

        private static void UpdateFile(Processes processes, int processId, StringBuilder newSb = null, string fileNameOverride = "")
        {
            StringBuilder sb = new StringBuilder();
            string fileName = "";
            string filePath = @"Logs\";

            switch (processes)
            {
                case Processes.iTunesImport:
                    fileName = "iTunesImport Summary";
                    filePath += "Importer";
                    sb = sbBuffer.First(f => f.Id == processId).StringBuilder;
                    break;
                case Processes.ImportDataDump:
                    if (fileNameOverride == "")
                        fileName = "Import_DataDump";
                    else
                        fileName = fileNameOverride;

                    filePath += "Importer";
                    sb = newSb;
                    break;
                case Processes.VLCCorePlayer:
                    fileName = "VLC Core Player";
                    filePath += "CoreSystems";
                    sb = sbBuffer.First(f => f.Id == processId).StringBuilder;
                    break;
                case Processes.Timer:
                    fileName = "Timer_" + sbBuffer.First(f => f.Id == processId).DataFilesType;
                    filePath += "Timers";
                    sb = sbBuffer.First(f => f.Id == processId).StringBuilder;
                    break;
            }

            /// Write Data to file
            string path = string.Format("{0}\\{1}\\", Environment.CurrentDirectory, filePath);
            string file = string.Format("{0}\\{1}\\{2}.txt", Environment.CurrentDirectory, filePath, fileName);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            System.IO.File.WriteAllText(file, sb.ToString());
            //Process.Start(string.Format("{0}\\{1}", Environment.CurrentDirectory, filePath));
        }

        internal static void AddDataDump(int logID, string v, List<string> list)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + " " + v + " - " + list.Count() + " files");
            sb.AppendLine("----------------------------------------------");
            foreach (string s in list)
                sb.AppendLine(s);

            UpdateFile(Processes.ImportDataDump, logID, sb, v);
        }

        private static List<StringBuilderExtended> sbBuffer = new List<StringBuilderExtended>();
        private class StringBuilderExtended
        {
            public int Id { get; set; }
            public StringBuilder StringBuilder { get; set; }
            public Processes Process { get; set; }
            public string DataFilesType { get; internal set; }
        }

        internal static void AddError(int logID, string v)
        {
            throw new NotImplementedException();
        }

        internal static void AddCriticalError(int logID, string v)
        {
            throw new NotImplementedException();
        }
    }
}
