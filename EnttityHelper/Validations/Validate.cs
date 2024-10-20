using System;
using System.IO;

namespace EH.Validations
{
    internal static class Validate
    {
        internal static bool IsCsvValid(string csvFilePath)
        {
            if (string.IsNullOrWhiteSpace(csvFilePath))
            {
                throw new ArgumentException("CSV file path cannot be null or empty.", nameof(csvFilePath));
            }

            if (!File.Exists(csvFilePath))
            {
                throw new FileNotFoundException($"File not found: {csvFilePath}");
            }

            if (!Path.GetExtension(csvFilePath).Equals(".csv", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Invalid file extension. Only .csv files are supported.");
            }

            return true;
        }






    }
}
