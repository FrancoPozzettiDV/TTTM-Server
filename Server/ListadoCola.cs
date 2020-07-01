using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    class ListadoCola
    {
        public static ListadoCola instancia = null;
        public static List<Jugador> cola = new List<Jugador>();

        public static ListadoCola getInstancia()
        {
            if (instancia == null)
            {
                instancia = new ListadoCola();
            }

            return instancia;
        }

        public string entrarCola(Jugador jugador)
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
                    //Se obtiene la conexion del rival y se setea la conexion de ambos en null para que se pueda serializar y enviar
                    TcpClient conexion = player.conexion;

                    Partida.setJugador1(player);
                    Partida.verificarConexiones1(player);
                    player.conexion = null;
                    rival = JsonConvert.SerializeObject(player);

                    Partida.setJugador2(jugador);
                    Partida.verificarConexiones2(jugador);
                    jugador.conexion = null;
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

        public void salirCola(Jugador jugador)
        {
            cola.RemoveAll(x => x.id == jugador.id);
        }
    }
}
