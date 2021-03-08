using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HTTPServer 
{
    class WebServer
    {
        //Inicializamos el puerto del servidor
        public WebServer(int port, string path)
        {
            this.port = port;
            this.home = path;
            listener = new TcpListener(IPAddress.Any, port);
            this.Messaje = false;
        }

        public void Start()
        {
            listener.Start();
            Console.WriteLine(string.Format("Local server started at localhost:{0}", port));

            Console.CancelKeyPress += delegate
            {
                Console.WriteLine("Stopping server");
                StopServer();
            };
        }

        public void Listen()
        {
            try
            {
                while (true)
                {
                    Byte[] result = new Byte[MAX_SIZE];
                    string requestData;

                    TcpClient client = listener.AcceptTcpClient();
                    NetworkStream stream = client.GetStream();
                    int size = stream.Read(result, 0, result.Length);
                    requestData = System.Text.Encoding.ASCII.GetString(result, 0, size);
                    Console.WriteLine("Received: {0}", requestData);
                    

                    Request request = GetRequest(requestData);
                    ProcessRequest(request, stream);
                    client.Close();
                }
            }
            finally
            {
                listener.Stop();
                
            }
        }

        private void ProcessRequest(Request request, NetworkStream stream)
        {
            // Valida que el reques sea valido
            if (request == null)
            {
                return;
            }

           

            //Si la peticion es una peticion post
            if (request != null && request.Command.Equals("POST"))
            {
                GenerateResponse("RECIVED POST REQUEST", stream, OK200);
                this.Messaje = (bool)request.body["Message"];

                //Logica del bot


            }
            else {
                GenerateResponse("Not found", stream, NOTFOUND404);
            }
            
            Console.WriteLine("---------------------------------------");
            getMessage();
            return;

            /*
            if (request.Path.Equals("/"))
                request.Path = "/home.html";
            ParsePath(request);
            if (File.Exists(request.Path))
            {
                var fileContent = File.ReadAllText(request.Path);
                GenerateResponse(fileContent, stream, OK200);
                return;
            }

            */



            
        }

        public void getMessage() {
             Console.WriteLine(this.Messaje);
        }

        private void ParsePath(Request request)
        {
            request.Path.Replace('/', '\\');
            request.Path = home + request.Path;
        }

        private void GenerateResponse(string content, 
            NetworkStream stream,
            string responseHeader)
        {
            string response = "HTTP/1.1 200 OK\r\n\r\n\r\n";
            response = response + content;
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(response);
            stream.Write(msg, 0, msg.Length);
            return;
        }

        private void StopServer()
        {
            listener.Stop();
        }

        private Request GetRequest(string data)
        {
            //Creamos un objeto de tipo request
            Request request = new Request();
            //Dividimos la cadena con la informacion de la peticion
            var list = data.Split(' ');
            
            //Validamos que la informacion sea valida
            if (list.Length < 3)
                return null;

            //A nuestro objeto request agregamos
            //[0] -> el metodo http de la peticion
            //[1] -> la url a donde va dirigida la peticion
            //[2] -> protocolo de la peticion HTTP/1.1
            request.Command = list[0];
            request.Path = list[1];
            request.Protocol = list[2].Split('\n')[0];

            if (request.Command.Equals("POST")) {
                string[] bodyReq = data.Split('{');
               

                string jsonString = "{" + bodyReq[1];
                JObject json = JObject.Parse(jsonString);
                request.body = json;

                

              
            }

            Console.WriteLine("Instruction: {0}\nPath: {1}\nProtocol: {2}",
                request.Command,
                request.Path,
                request.Protocol);
            //Regresamos el objeto request
            return request;
        }

        private TcpListener listener;
        private int port;
        private string home;
        private static string NOTFOUND404 = "HTTP/1.1 404 Not Found";
        private static string OK200 = "HTTP/1.1 200 OK\r\n\r\n\r\n";
        private static int MAX_SIZE = 1000;
        private bool Messaje;
    }

    public class Request
    {
        public string Command;
        public string Path;
        public string Protocol;
        public JObject body;
    }

}
