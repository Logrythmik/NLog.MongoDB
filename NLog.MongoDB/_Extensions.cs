using System;
using System.Text.RegularExpressions;

namespace NLog.MongoDB
{
    internal static class Extensions
    {
        static readonly Regex _regex = new Regex(@"/([a-zA-Z0-9-_]*)");

        public static string ParseDatabaseName(this string connectionString)
        {
            // Parse from the connection string, as the third instance of "/<capture>"
            try
            {
                return _regex.Matches(connectionString)[2].Value.Trim('/');
            }
            catch (Exception)
            {
                return null;
            }
            
        }
    }
}
