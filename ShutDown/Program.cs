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

            // Process the request to check for the shutdown command and delay parameter
            string responseString = "";
            if (request.HttpMethod == "POST" && request.Url.AbsolutePath == "/shutdown")
            {
                // You can add additional checks here for authorization, if required.

                // Get the shutdown delay time from the query parameter
                int delaySeconds = GetShutdownDelay(request.Url.Query);

                // Execute the system shutdown command
                ShutdownSystem(delaySeconds);
                responseString = $"System shutdown initiated with a delay of {delaySeconds} seconds.";
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

        private int GetShutdownDelay(string query)
        {
            string delayParameter = "delay=";
            int defaultDelaySeconds = 60; // Default shutdown delay if the parameter is missing or invalid
            int delaySeconds = defaultDelaySeconds;

            int index = query.IndexOf(delayParameter);
            if (index != -1)
            {
                int startIndex = index + delayParameter.Length;
                int endIndex = query.IndexOf('&', startIndex);
                if (endIndex == -1)
                    endIndex = query.Length;

                string delayValue = query.Substring(startIndex, endIndex - startIndex);
                if (int.TryParse(delayValue, out int parsedDelay))
                {
                    delaySeconds = parsedDelay;
                }
            }

            return delaySeconds;
        }

        private void ShutdownSystem(int delaySeconds)
        {
            // You may want to add a delay or show a warning message before shutting down.
            System.Diagnostics.Process.Start("shutdown", $"/s /f /t {delaySeconds}");
        }

    }
}
