using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ShutDown
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new ShutdownService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
    public class ShutdownService : ServiceBase
    {
        private const string Url = "http://*:8058/"; // Adjust the port number as needed
        private HttpListener httpListener;

        public ShutdownService()
        {
            this.ServiceName = "ShutdownService";
            this.CanStop = true;
            this.CanPauseAndContinue = false;
            this.AutoLog = true;
        }

        protected override void OnStart(string[] args)
        {
            httpListener = new HttpListener();
            httpListener.Prefixes.Add(Url);
            httpListener.Start();
            httpListener.BeginGetContext(HandleHttpRequest, httpListener);
        }

        protected override void OnStop()
        {
            if (httpListener != null)
            {
                httpListener.Stop();
                httpListener.Close();
            }
        }

        private void HandleHttpRequest(IAsyncResult result)
        {
            HttpListener listener = (HttpListener)result.AsyncState;
            HttpListenerContext context = listener.EndGetContext(result);
            HttpListenerRequest request = context.Request;

            // Process the request to check for the shutdown command
            string responseString = "";
            if (request.HttpMethod == "POST" && request.Url.AbsolutePath == "/shutdown")
            {
                // You can add additional checks here for authorization, if required.

                // Execute the system shutdown command
                ShutdownSystem();
                responseString = "System shutdown initiated.";
            }
            else
            {
                responseString = "Invalid request.";
            }

            // Send response to the client
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);
            context.Response.ContentLength64 = buffer.Length;
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.OutputStream.Close();

            // Continue listening for new requests
            listener.BeginGetContext(HandleHttpRequest, listener);
        }

        private void ShutdownSystem()
        {
            // You may want to add a delay or show a warning message before shutting down.
            System.Diagnostics.Process.Start("shutdown", "/s /f /t 0");
        }
    }
}
