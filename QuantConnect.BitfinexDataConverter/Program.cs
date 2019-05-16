using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

/*
 *
 *
 *
 */
namespace QuantConnect.BitfinexDataConverter
{
    class Program
    {
        private const string RawDataFolder = @"C:\Users\sterling\source\repos\bitfinex_api\data";
        private const string LeanDataFolder = @"C:\Users\sterling\Google Drive\Data";

        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        static void Main(string[] args)
        {
            // We would like to process every file that exist in this directory.
            // Retrive their full path to the collection
            var rawDataFilesPath = Directory.GetFiles(RawDataFolder);

            // Bust the raw data
            foreach (var file in rawDataFilesPath)
            {
                // Get the symbol for the data
                var fileName = Path.GetFileNameWithoutExtension(file);
                var symbol = fileName.Split('_')[1];

                // Time the we'll use to compare the timestamp of anjacent entries
                var previousEntryTime = Epoch;

                // String Builder that will contain information for a single minute data zip file
                var sb = new StringBuilder();

                // Read from the file
                // https://stackoverflow.com/questions/8037070/whats-the-fastest-way-to-read-a-text-file-line-by-line
                // :
                var lines = File.ReadLines(fileName);

                foreach (var line in lines)
                {
                    var entries = line.Split(',');
                    var timestamp = entries[0];
                    var open = entries[1];
                    var close = entries[2];
                    var high = entries[3];
                    var low = entries[4];
                    var volume = entries[5];

                    // Convert milliseconds to int and devide to 1000 to get seconds and then to DateTime
                    var currentEntryTime = FromUnixTime(Convert.ToInt64(timestamp) / 1000);

                    // Check if there is a new day and we need to start a new file
                    if (currentEntryTime.Date != previousEntryTime.Date)
                    {
                        // Write string builder contents to a file. Reset SB. Append new entry.
                        
                    }
                    else
                    {
                        // Append an entry to SB.

                    }
                }


            }


            Console.ReadKey();
        }

        /// <summary>
        /// Converts time from Unix from Unix time to Datetime
        /// </summary>
        /// <param name="unixTime">Second from epoc time</param>
        /// <returns></returns>
        private static DateTime FromUnixTime(long unixTime)
        {
            return Epoch.AddSeconds(unixTime);
        }

        /// <summary>
        /// Converts DateTime to Unix Time Stamp
        /// </summary>
        /// <param name="dt">DateTime convert to seconds since epoc time</param>
        /// <returns></returns>
        private static long ToUnixTimeMilliseconds(DateTime dt)
        {
            // make sure dt kind is Utc
            if(dt.Kind != DateTimeKind.Utc)
                throw new ArgumentException("DateTime passed to ToUnixTimeStamp is not UTC");

            // convert to DateTimeOffset type
            var dtOffset = new DateTimeOffset(dt);

            return dtOffset.ToUnixTimeMilliseconds();
        }
    }
}
