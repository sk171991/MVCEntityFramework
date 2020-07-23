using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace MVCYorbit
{
    public class LOG
    {
        public static void Logging(string data)
        {
            string logPath = AppDomain.CurrentDomain.BaseDirectory + "Logs\\";

            DateTime dt = DateTime.Now;
            string text = dt.ToLongDateString() + "--------" + Environment.NewLine;

            // Set a variable to the Documents path.
            string fileName = dt.ToString("yyyy-MM-dd") + "_Log.txt";
            string datetimeNow = dt.ToString("yyyy - MM - dd h: mm: ss tt");
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }
            if (!File.Exists(Path.Combine(logPath, fileName)))
            {
                // Write the text to a new file named "WriteFile.txt".
                File.WriteAllText(Path.Combine(logPath, fileName), text);
                // Create a string array with the additional lines of text
                string[] lines = { datetimeNow, data, "-----------------", Environment.NewLine };
                // Append new lines of text to the file
                File.AppendAllLines(Path.Combine(logPath, fileName), lines);
            }
            else
            {
                // Create a string array with the additional lines of text
                string[] lines = { datetimeNow, data, "-----------------", Environment.NewLine };

                // Append new lines of text to the file
                File.AppendAllLines(Path.Combine(logPath, fileName), lines);
            }
        }
    }
}