using System;
using System.Net.Sockets;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace Protocolo
{
    public class Pedido
    {
        public string Comando { get; set; }
        public string[] Parametros { get; set; }

        public static Pedido Procesar(string mensaje)
        {
            var partes = mensaje.Split(' ');
            return new Pedido
            {
                Comando = partes[0].ToUpper(),
                Parametros = partes.Skip(1).ToArray()
            };
        }

        public override string ToString()
        {
            return $"{Comando} {string.Join(" ", Parametros)}";
        }
    }

    public class Respuesta
    {
        public string Estado { get; set; }
        public string Mensaje { get; set; }

        public override string ToString()
        {
            return $"{Estado} {Mensaje}";
        }
    }

    public static class ProtocoloHelper
    {
        public static Respuesta HazOperacion(TcpClient remoto, NetworkStream flujo, Pedido pedido)
        {
            if (flujo == null)
                return null;

            try
            {
                byte[] bufferTx = Encoding.UTF8.GetBytes(pedido.ToString());
                flujo.Write(bufferTx, 0, bufferTx.Length);

                byte[] bufferRx = new byte[1024];
                int bytesRx = flujo.Read(bufferRx, 0, bufferRx.Length);
                string mensaje = Encoding.UTF8.GetString(bufferRx, 0, bytesRx);
                var partes = mensaje.Split(' ');

                return new Respuesta
                {
                    Estado = partes[0],
                    Mensaje = string.Join(" ", partes.Skip(1).ToArray())
                };
            }
            catch (SocketException)
            {
                return null;
            }
        }

        public static Respuesta ResolverPedido(Pedido pedido, string direccionCliente, Dictionary<string, int> listadoClientes)
        {
            if (!listadoClientes.ContainsKey(direccionCliente))
                listadoClientes[direccionCliente] = 0;

            listadoClientes[direccionCliente]++;

            if (pedido.Comando == "INGRESO")
            {
                if (pedido.Parametros[0] == "admin" && pedido.Parametros[1] == "1234")
                    return new Respuesta { Estado = "OK", Mensaje = "ACCESO_CONCEDIDO" };
                else
                    return new Respuesta { Estado = "NOK", Mensaje = "ACCESO_NEGADO" };
            }
            else if (pedido.Comando == "CALCULO")
            {
                // Simulación lógica
                return new Respuesta { Estado = "OK", Mensaje = "DIA 32" };
            }
            else if (pedido.Comando == "CONTADOR")
            {
                int contador = listadoClientes[direccionCliente];
                return new Respuesta { Estado = "OK", Mensaje = contador.ToString() };
            }

            return new Respuesta { Estado = "NOK", Mensaje = "COMANDO_DESCONOCIDO" };
        }
    }
}
