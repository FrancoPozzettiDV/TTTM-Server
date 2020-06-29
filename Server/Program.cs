using System;
using System.Threading;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Net;
using Newtonsoft.Json;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("Servidor iniciado");
            
            TcpListener serverSocket = new TcpListener(8000);
            serverSocket.Start();
            while (true)
            {
                TcpClient clientSocket = default(TcpClient);
                clientSocket = serverSocket.AcceptTcpClient();
                HandlerClient client = new HandlerClient();
                client.startClient(clientSocket);

            }

        }
    }

    public class HandlerClient
    {

        TcpClient clientSocket;
        List<Jugador> cola = new List<Jugador>();

        public void startClient(TcpClient clientSocket)
        {
            this.clientSocket = clientSocket;
            Thread threadClient = new Thread(doChat);
            threadClient.Start();
        }

      public string ingresarCola(Jugador jugador)
        {
            int min = jugador.puntaje - 200;
            int max = jugador.puntaje + 200;
            string nombre = jugador.usuario;
            string rival = "";
            string juga = "";
            Byte[] sendBytes = null;
            cola.Add(jugador);
            //Se agrega el usuario a la cola y con el foreach busca un posible rival
            foreach (var player in cola)
            {
                if (player.puntaje >= min && player.puntaje <= max && player.usuario != nombre)
                {
                    TcpClient conexion = player.conexion;
                    rival = JsonConvert.SerializeObject(player);
                    Console.WriteLine(rival);
                    juga = JsonConvert.SerializeObject(jugador);

                    //Se obtiene el jugador reciente de la cola y se envia un mensaje al jugador que se quedo esperando a su rival
                    string serverResponse = juga;
                    sendBytes = System.Text.Encoding.ASCII.GetBytes(serverResponse);
                    byte[] intBytes = BitConverter.GetBytes(sendBytes.Length);
                    NetworkStream networkStream = conexion.GetStream();
                    networkStream.Write(intBytes, 0, intBytes.Length);
                    networkStream.Write(sendBytes, 0, sendBytes.Length);
                    networkStream.Flush();

                    break;
                }
            }
            return rival;
        }

        private void doChat()
        {

            string dataFromClient = null;

            Byte[] sendBytes = null;
            string serverResponse = null;

            //Jugador jug1 = new Jugador(1, "num1", 1000, 3, 2);
            //ingresarCola(jug1);

            while (true)
            {
                    // Recibi mensaje
                    byte[] bytesFrom = new byte[4];
                    NetworkStream networkStream = clientSocket.GetStream();
                    networkStream.Read(bytesFrom, 0, bytesFrom.Length);
                    int buffersize = BitConverter.ToInt32(bytesFrom, 0);
                    bytesFrom = new byte[buffersize];
                    networkStream.Read(bytesFrom, 0, bytesFrom.Length);
                    dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
                    
                    //Verifico si es un usuario
                    if (dataFromClient.Contains("usuario"))
                    {
                        Jugador jugador = JsonConvert.DeserializeObject<Jugador>(dataFromClient);
                        jugador.setConexion(clientSocket);
                        //Depende del mensaje que mande un usuario, el servidor realiza una acción distinta
                        switch (jugador.mensaje)
                        {
                            case 0:
                                System.Console.WriteLine("Jugador " + jugador.usuario + " ha ingresado al servidor");
                                System.Console.WriteLine("Id: " + jugador.id + "/ Nombre: " + jugador.usuario + "/ Puntaje: " + jugador.puntaje + "/ Victorias: " + jugador.calcularPorcentaje() + "%" + "/ Mensaje: " + jugador.mensaje);
                                enviarMensaje("ok");
                                break;
                            case 1:
                                string rival = ingresarCola(jugador);
                                System.Console.WriteLine("Jugador " + jugador.usuario + " ha ingresado a la cola");
                                //Se envia un mensaje si obtiene un rival
                                if (rival != "") 
                                {
                                enviarMensaje(rival);
                                }
                                else
                                {
                                enviarMensaje("ok");
                                }
                                break;
                            case 2:
                                cola.RemoveAll(x => x.id == jugador.id);
                                System.Console.WriteLine("Jugador " + jugador.usuario + " ha salido de la cola");
                                enviarMensaje("ok");
                                break;
                        }
                    }
                    else
                    {
                        //Deserializar mensaje que no es jugador en otra cosa...
                    }

                    


                    
                    void enviarMensaje(string mensaje)
                    {
                        serverResponse = mensaje;
                        sendBytes = System.Text.Encoding.ASCII.GetBytes(serverResponse);
                        byte[] intBytes = BitConverter.GetBytes(sendBytes.Length);
                        networkStream.Write(intBytes, 0, intBytes.Length);
                        networkStream.Write(sendBytes, 0, sendBytes.Length);
                        networkStream.Flush();
                    }
                
                
            }

        }
        

    }
}
