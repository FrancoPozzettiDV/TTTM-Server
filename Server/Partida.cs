using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;

namespace Server
{
    public static class Partida
    {
        public static Jugador j1;
        public static Jugador j2;
        public static List<TcpClient> conexiones = new List<TcpClient>();
        static TcpClient tcp1;
        static TcpClient tcp2;
        static bool check;
        static bool j1check;
        static bool j2check;

        public static void interpretarMensaje(string msj, TcpClient client)
        {
            setConexion();

            if (msj.Equals("start")) //Ambos jugadores entran al tablero
            {
                if (client.Equals(j1.conexion))
                {
                    enviarMensaje("Comenzar", j1.conexion); //Jugador 1 (el que entro primero a la cola) hace la primera jugada
                }
                else if (client.Equals(j2.conexion))
                {
                    enviarMensaje("Esperar", j2.conexion); //Jugador 2 tiene que esperar a Jugador 1 para hacer su jugada
                }

            }
            else if (msj.Equals("wait")) //Jugador 2 envía un wait para que pueda recibir la primera jugada
            {
                check = true;
            }
            else if (msj.Equals("fin")) //Mensajes de finalización de partida
            {
                if (client.Equals(j1.conexion))
                {
                    j1check = true; //j1check y j2check se utilizan porque solo uno de los dos jugadores necesita el mensaje de fin. 
                    if (!j2check)
                    {
                        enviarMensaje("Fin", j2.conexion);
                    }
                    
                    enviarMensaje("ok", j1.conexion);
                }
                else
                {
                    j2check = true; //si no se ponen los checks, uno de los dos va a tener el mensaje de fin en su buffer y no va a funcionar buscar un nuevo jugador.
                    if (!j1check)
                    {
                        enviarMensaje("Fin", j1.conexion);
                    }
                    
                    enviarMensaje("ok", j2.conexion);
                }
            }
            else //Por aca van los movimientos o nombres de las fichas que los jugadores presionan
            {
                if (client == j1.conexion)
                {
                    if (check)
                    {
                        enviarMensaje(msj, j2.conexion);
                        check = false;
                    }
                    else
                    {
                        enviarMensaje(msj, j2.conexion);
                    }
                }
                else if (client == j2.conexion)
                {
                    enviarMensaje(msj, j1.conexion);
                }
            }
        }

        public static void enviarMensaje(string msj, TcpClient cli)
        {
            Byte[] sendBytes = null;
            string serverResponse = msj;
            sendBytes = System.Text.Encoding.ASCII.GetBytes(serverResponse);
            byte[] intBytes = BitConverter.GetBytes(sendBytes.Length);
            NetworkStream networkStream = cli.GetStream();
            networkStream.Write(intBytes, 0, intBytes.Length);
            networkStream.Write(sendBytes, 0, sendBytes.Length);
            networkStream.Flush();
        }

        public static void reiniciarChecks()
        {
            j1check = false;
            j2check = false;
        }

        public static void setJugador1(Jugador player)
        {
            j1 = player;
        }
        public static void setJugador2(Jugador player)
        {
            j2 = player;
        }

        public static void setTcpClients(TcpClient tcp)
        {

            if (conexiones.Count == 0)
            {
                conexiones.Add(tcp);

            }
            if (!conexiones.Contains(tcp))
            {
                conexiones.Add(tcp);

            }

        }
        public static void verificarConexiones1(Jugador player)
        {
            foreach (var conexion in conexiones)
            {
                if (conexion == player.conexion)
                {
                    tcp1 = player.conexion;
                }
            }
        }
        public static void verificarConexiones2(Jugador player)
        {
            foreach (var conexion in conexiones)
            {
                if (conexion == player.conexion)
                {
                    tcp2 = player.conexion;
                }
            }
        }

        public static void setConexion()
        {
            j1.conexion = tcp1;
            j2.conexion = tcp2;
        }



    }
}
