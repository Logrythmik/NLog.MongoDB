using System;
using System.Text.RegularExpressions;

namespace NLog.MongoDB
{
    internal static class Extensions
    {
        // Note: I am not that great with regex, if someone else thinks of a better way
        static readonly Regex _regex = new Regex(@"/([a-zA-Z0-9-_]*)");

        public static string ParseDatabaseName(this string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString) || !connectionString.ToLower().StartsWith("mongodb://"))
                throw new FormatException("The connection string passed is not a valid MongoDB connection string.");
            
            try
            {
                // Parse from the connection string, as the third instance of "/<capture>"
                return _regex.Matches(connectionString)[2].Value.Trim('/');
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
