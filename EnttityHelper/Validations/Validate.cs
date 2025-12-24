using System;
using System.IO;

namespace EH.Validations
{
    internal static class Validate
    {
        internal static bool IsFileValid(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            if (extension != ".csv" && extension != ".txt")
            {
                throw new ArgumentException("Invalid file extension. Only .csv and .txt files are supported.");
            }

            return true;
        }







    }
}
