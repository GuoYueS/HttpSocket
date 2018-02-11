using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

//This is asynchronous server written with socket class. A little bit modified example
//from https://docs.microsoft.com/en-us/dotnet/framework/network-programming/asynchronous-server-socket-example
//Written with recursions not loops. 
namespace HttpSocket.Models
{
    // object to pass between asynchronous calls
    public class StateObject
    {
        public Socket workSocket = null;
        public const int BufferSize = 1024;
        public byte[] buffer = new byte[BufferSize];
        public StringBuilder sb = new StringBuilder();
    }

    public class AsynchronousSocketListener
    {
        public static ManualResetEvent allDone = new ManualResetEvent(false);
  
        public static bool Manager = true;

        public AsynchronousSocketListener()
        {
        }

        public static void StartListening(int port)
        {
            byte[] bytes = new Byte[1024];

            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, port);

            Socket listener = new Socket(SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (Manager)
                {
                    allDone.Reset();

                    Console.WriteLine("Waiting for a connection...");
                    //make connection asynchronously on another thread 
                    listener.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        listener);

                    allDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.WriteLine("Nothing to see here its just a bug. But wait is it Microsoft");
            listener.Close();
        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            allDone.Set();

            Socket listener = (Socket)ar.AsyncState;
            // worker thread blocks while connections established and creates new socket for that new client endpoint
            // and so on.......
            Socket handler = listener.EndAccept(ar);

            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            int bytesRead = handler.EndReceive(ar);

            state.sb.Append(Encoding.ASCII.GetString(
                state.buffer, 0, bytesRead));

            content = state.sb.ToString();
            if (content.IndexOf("\r\n\r\n") > -1)
            {
                Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                    content.Length, content);

                Send(handler, Http.createResponse(200, content));
            }
            else
            {
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
            }

        }

        private static void Send(Socket handler, byte[] data)
        { 
            handler.BeginSend(data, 0, data.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {  
                Socket handler = (Socket)ar.AsyncState;

                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void SendFile(Socket handler, String fileName)
        {
            handler.BeginSendFile(fileName, new AsyncCallback(FileSendCallback), handler);
        }

        private static void FileSendCallback(IAsyncResult ar)
        {
            Socket client = (Socket)ar;
            client.EndSendFile(ar);
        }

    }

}
