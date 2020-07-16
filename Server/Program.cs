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

        public void startClient(TcpClient clientSocket)
        {
            this.clientSocket = clientSocket;
            Thread threadClient = new Thread(doChat);
            threadClient.Start();
        }

        private void doChat()
        {
            try
            {

                string dataFromClient = null;

                Byte[] sendBytes = null;
                string serverResponse = null;

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
                        Partida.setTcpClients(clientSocket);

                        //Depende del mensaje que mande un usuario, el servidor realiza una acción distinta
                        switch (jugador.mensaje)
                        {
                            case 0:
                                System.Console.WriteLine("Jugador " + jugador.usuario + " ha ingresado al servidor");
                                System.Console.WriteLine("Id: " + jugador.id + "/ Nombre: " + jugador.usuario + "/ Puntaje: " + jugador.puntaje + "/ Victorias: " + jugador.calcularPorcentaje() + "%");
                                enviarMensaje("ok");
                                break;
                            case 1:
                                string rival = ListadoCola.getInstancia().entrarCola(jugador);
                                System.Console.WriteLine("Jugador " + jugador.usuario + " ha ingresado a la cola");
                                if (rival != "")
                                {
                                    //Se envia al rival y se vuelve a setear la conexion para que no quede en null
                                    enviarMensaje(rival);
                                    jugador.setConexion(clientSocket);
                                }
                                break;
                            case 2:
                                ListadoCola.getInstancia().salirCola(jugador);
                                System.Console.WriteLine("Jugador " + jugador.usuario + " ha salido de la cola");
                                enviarMensaje("ok");
                                Partida.reiniciarChecks();
                                break;
                        }
                    }
                    else
                    {
                        Partida.interpretarMensaje(dataFromClient, clientSocket);
                        //Mensajes provenientes de la partida
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
            catch (System.IO.IOException)
            {
                Console.WriteLine("Un usuario se ha desconectado");
            }
        }
        

    }
}
