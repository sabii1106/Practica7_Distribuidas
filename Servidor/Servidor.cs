using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Protocolo;

namespace Servidor
{
    class Servidor
    {
        private static TcpListener escuchador;
        private static Dictionary<string, int> listadoClientes = new Dictionary<string, int>();

        static void Main(string[] args)
        {
            try
            {
                escuchador = new TcpListener(IPAddress.Any, 8080);
                escuchador.Start();
                Console.WriteLine("Servidor iniciado en el puerto 8080...");

                while (true)
                {
                    TcpClient cliente = escuchador.AcceptTcpClient();
                    Console.WriteLine("Cliente conectado: {0}", cliente.Client.RemoteEndPoint.ToString());
                    Thread hiloCliente = new Thread(ManipuladorCliente);
                    hiloCliente.Start(cliente);
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Error al iniciar servidor: " + ex.Message);
            }
            finally
            {
                escuchador?.Stop();
            }
        }

        private static void ManipuladorCliente(object obj)
        {
            TcpClient cliente = (TcpClient)obj;
            NetworkStream flujo = null;

            try
            {
                flujo = cliente.GetStream();
                byte[] bufferRx = new byte[1024];
                byte[] bufferTx;
                int bytesRx;

                while ((bytesRx = flujo.Read(bufferRx, 0, bufferRx.Length)) > 0)
                {
                    string mensajeRx = Encoding.UTF8.GetString(bufferRx, 0, bytesRx);
                    Pedido pedido = Pedido.Procesar(mensajeRx);
                    Console.WriteLine("Se recibió: " + pedido);

                    string direccionCliente = cliente.Client.RemoteEndPoint.ToString();
                    Respuesta respuesta = ProtocoloHelper.ResolverPedido(pedido, direccionCliente, listadoClientes);
                    Console.WriteLine("Se envió: " + respuesta);

                    bufferTx = Encoding.UTF8.GetBytes(respuesta.ToString());
                    flujo.Write(bufferTx, 0, bufferTx.Length);
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Error al manejar cliente: " + ex.Message);
            }
            finally
            {
                flujo?.Close();
                cliente?.Close();
            }
        }
    }
}
