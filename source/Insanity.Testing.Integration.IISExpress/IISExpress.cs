using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Insanity.Testing.Integration.IISExpress
{
    public class IISExpress : IDisposable
    {
        private int port;
        private string applicationPath;
        private Process iisProcess;

        public IISExpress(string applicationPath, int port)
        {
            if (String.IsNullOrWhiteSpace(applicationPath)) throw new ArgumentException("Path to applicaiton path is required", nameof(applicationPath));
            if (!Directory.Exists(applicationPath)) throw new ArgumentException($"The path {applicationPath} does not exist.", nameof(applicationPath));
            if (port < 0 || port >= 65535) throw new ArgumentOutOfRangeException(nameof(port), "Port range must be between 8080 and 8099.");

            this.applicationPath = applicationPath;
            this.port = port;
        }

        public void Start()
        {
            var programFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

            iisProcess = new Process();
            iisProcess.StartInfo.FileName = $"{programFilesPath}\\IIS Express\\iisexpress.exe";
            iisProcess.StartInfo.Arguments = $"/path:\"{applicationPath}\" /port:{port}";
            iisProcess.StartInfo.UseShellExecute = false;
            iisProcess.Start();
        }

        public void Stop()
        {
            if (iisProcess != null)
            {
                if (iisProcess.HasExited == false)
                {
                    iisProcess.Kill();
                }

                iisProcess.Dispose();
                iisProcess = null;
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Stop();
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}