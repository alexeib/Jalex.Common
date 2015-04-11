using System;
using System.IO;
using System.Reflection;

namespace Jalex.TestUtils.xUnit
{
    public static class XUnitUtils
    {
        public static string GetDeployedFileLocation(string relativePath)
        {
            var codeBaseUrl = new Uri(Assembly.GetExecutingAssembly().CodeBase);
            var codeBasePath = Uri.UnescapeDataString(codeBaseUrl.AbsolutePath);
            var dirPath = Path.GetDirectoryName(codeBasePath);
            return Path.Combine(dirPath, relativePath);
        }
    }
}
