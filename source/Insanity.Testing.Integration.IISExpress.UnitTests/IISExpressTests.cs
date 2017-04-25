using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Insanity.Testing.Integration.IISExpress.UnitTests
{
    [TestClass]
    public class IISExpressTests
    {
        [TestMethod]
        public void IISExpress_StartIISExpress()
        {
            var solutionFolder = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)));
            var applicationPath = Path.Combine(solutionFolder, "Insanity.Testing.Integration.MVC452");

            var iisExpress = new IISExpress(applicationPath, 8099);
            try
            {
                iisExpress.Start();
                //TODO: Make a request to varify that it works.
            }
            finally
            {
                iisExpress.Stop();
            }
        }
    }
}
