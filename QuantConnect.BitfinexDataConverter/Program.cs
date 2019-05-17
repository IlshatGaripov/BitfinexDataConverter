using System;
using System.IO;
using System.IO.Compression;
using System.Text;

/*
 *
 * This is an utility program allows you to convert historial minute bars data collected directly
 * from Bitfinex Exchange by using this code that I found on the internet prairies:
 * https://github.com/akcarsten/bitfinex_api
 *
 * This Article describes how to use the given library above to collect raw minute bar data from Bitfinex:
 * https://medium.com/coinmonks/how-to-get-historical-crypto-currency-data-954062d40d2d
 *
 * The code below converts the data obtained so to the Lean format data and stores is to Lean Data folder
 * the path to which should be specified as one of global static variables - 'LeanDataFolder'
 *
 */
namespace QuantConnect.BitfinexDataConverter
{
    class Program
    {
        // Input and Output data folders
        private const string RawDataFolder = @"C:\Users\sterling\source\repos\bitfinex_api\data";
        private const string LeanDataFolder = @"C:\Users\sterling\Google Drive\Data";

        // Epoch time
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        static void Main(string[] args)
        {
            // We would like to process every file that exist in directory with raw data.
            // Retrive their full path to the collection
            var rawDataFilesPath = Directory.GetFiles(RawDataFolder);

            // Bust the raw data
            foreach (var file in rawDataFilesPath)
            {
                // Get the symbol for the data
                var fileName = Path.GetFileNameWithoutExtension(file);
                var symbol = fileName.Split('_')[1];

                // Time the we'll use to compare the timestamp of anjacent entries
                var previousLineDateTime = Epoch;

                // String Builder that will contain information for a single minute data zip file
                var sb = new StringBuilder();

                // Read from the file
                // https://stackoverflow.com/questions/8037070/whats-the-fastest-way-to-read-a-text-file-line-by-line
                // :
                var lines = File.ReadLines(file);

                foreach (var entry in lines)
                {
                    var lineValues = entry.Split(',');
                    var timestamp = lineValues[0];
                    var open = lineValues[1];
                    var close = lineValues[2];
                    var high = lineValues[3];
                    var low = lineValues[4];
                    var volume = lineValues[5];

                    // Convert timestamp from string to long. If there is FormatException then most probably
                    // we are trying to convert a "timestamp" word from csv header
                    long timestampInt64;
                    try
                    {
                        timestampInt64 = Convert.ToInt64(timestamp);
                    }
                    catch (FormatException)
                    {
                        Console.WriteLine($"Trying convert the header - {timestamp}? File {symbol}. Skip.");
                        continue;
                    }

                    // Convert timestamp in milliseconds to DateTime
                    var lineDateTime = FromUnixTimeMilliseconds(timestampInt64);

                    // Milliseconds have passed from 12.00 AM of current day
                    long timestampOffDayStart = timestampInt64 - ToUnixTimeMilliseconds(lineDateTime.Date);

                    // Check if there is a new day and we need to start a new file
                    if (lineDateTime.Date != previousLineDateTime.Date)
                    {
                        // Write string builder contents to a file. Reset SB.
                        // And write in new line to the cleared string builder object.

                        // If not the very first entry, i.e. SBuild. object is not empty, then dump to a zip file
                        if (previousLineDateTime != Epoch)
                        {
                            // All entries we collected in SB object are for the previous Day!
                            DumpToZipFile(symbol, sb, previousLineDateTime.Date, LeanDataFolder);
                        }
                        
                        sb.Clear();
                        sb.AppendLine($"{timestampOffDayStart},{open},{high},{low},{close},{volume}");
                    }
                    else
                    {
                        // Append a Lean format entry to SB.
                        sb.AppendLine($"{timestampOffDayStart},{open},{high},{low},{close},{volume}");
                    }

                    // Important thing - assign previous DateTime variable the current value
                    previousLineDateTime = lineDateTime;
                }
            }

            Console.WriteLine("Please press ENTER to exit the program.");
            Console.ReadKey();
        }

        /// <summary>
        /// Converts time from Unix time in milliseconds to DateTime
        /// </summary>
        /// <param name="unixTime">Milliseconds from epoc time</param>
        /// <returns></returns>
        private static DateTime FromUnixTimeMilliseconds(long unixTime)
        {
            return Epoch.AddMilliseconds(unixTime);
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

        /// <summary>
        /// Creates a zip file with minute bar data in Lean format from contents of SB object
        /// </summary>
        /// <param name="symbol">Trading symbol</param>
        /// <param name="sb">String Builder object that contains information to be written to the file</param>
        /// <param name="dt">The day minute bars happened to be</param>
        /// <param name="leanDataFolder">Folder that contains historial data in Lean format</param>
        private static void DumpToZipFile(string symbol, StringBuilder sb, DateTime dt, string leanDataFolder)
        {
            // Create directory if not exist
            string directoryForZipFile = leanDataFolder + $"\\crypto\\bitfinex\\minute\\{symbol}\\";
            Directory.CreateDirectory(directoryForZipFile);

            // Zip file Path
            var zipPath = directoryForZipFile + dt.ToString("yyyyMMdd") + "_trade.zip";

            // Check if Zip file already exists. If it does - then return, otherwise create new Zip file and csv entry
            if (File.Exists(zipPath))
            {
                Console.WriteLine($"File {zipPath} already exist");
                return;
            }
                
            // Create Zip file
            using (var zf = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                // Create CSV entry inside a Zip file
                var csvName = $"{dt:yyyyMMdd}_{symbol}_minute_trade.csv";
                var ze = zf.CreateEntry(csvName);

                // Fill up CSV with content from a String Builder
                using (var zs = ze.Open())
                {
                    using (var write = new StreamWriter(zs, Encoding.UTF8))
                    {
                        // write to 'write'
                        write.Write(sb.ToString());
                    }
                }
            }
        }

    }
}
