using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using Protocolo;

namespace Cliente
{
    public partial class FrmValidador : Form
    {
        private TcpClient remoto;
        private NetworkStream flujo;

        public FrmValidador()
        {
            InitializeComponent();
        }

        private void FrmValidador_Load(object sender, EventArgs e)
        {
            try
            {
                remoto = new TcpClient("127.0.0.1", 8080);
                flujo = remoto.GetStream();
            }
            catch (SocketException ex)
            {
                MessageBox.Show("No se pudo establecer conexión: " + ex.Message, "ERROR");
            }

            panPlaca.Enabled = false;
            chkLunes.Enabled = false;
            chkMartes.Enabled = false;
            chkMiercoles.Enabled = false;
            chkJueves.Enabled = false;
            chkViernes.Enabled = false;
            chkDomingo.Enabled = false;
            chkSabado.Enabled = false;
        }

        private void btnIniciar_Click(object sender, EventArgs e)
        {
            string usuario = txtUsuario.Text;
            string contraseña = txtPassword.Text;

            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(contraseña))
            {
                MessageBox.Show("Se requiere el ingreso de usuario y contraseña", "ADVERTENCIA");
                return;
            }

            Pedido pedido = new Pedido
            {
                Comando = "INGRESO",
                Parametros = new[] { usuario, contraseña }
            };
            Respuesta respuesta = ProtocoloHelper.HazOperacion(remoto, flujo, pedido);
            if (respuesta == null)
            {
                MessageBox.Show("Hubo un error", "ERROR");
                return;
            }

            if (respuesta.Estado == "OK" && respuesta.Mensaje == "ACCESO_CONCEDIDO")
            {
                panPlaca.Enabled = true;
                panLogin.Enabled = false;
                MessageBox.Show("Acceso concedido", "INFORMACIÓN");
                txtModelo.Focus();
            }
            else
            {
                panPlaca.Enabled = false;
                panLogin.Enabled = true;
                MessageBox.Show("No se pudo ingresar, revise credenciales", "ERROR");
                txtUsuario.Focus();
            }
        }

        private void btnConsultar_Click(object sender, EventArgs e)
        {
            string modelo = txtModelo.Text;
            string marca = txtMarca.Text;
            string placa = txtPlaca.Text;

            Pedido pedido = new Pedido
            {
                Comando = "CALCULO",
                Parametros = new[] { modelo, marca, placa }
            };

            Respuesta respuesta = ProtocoloHelper.HazOperacion(remoto, flujo, pedido);
            if (respuesta == null)
            {
                MessageBox.Show("Hubo un error", "ERROR");
                return;
            }

            if (respuesta.Estado == "NOK")
            {
                MessageBox.Show("Error en la solicitud.", "ERROR");
                LimpiarDias();
            }
            else
            {
                var partes = respuesta.Mensaje.Split(' ');
                MessageBox.Show("Se recibió: " + respuesta.Mensaje, "INFORMACIÓN");
                byte resultado = byte.Parse(partes[1]);

                chkLunes.Checked = (resultado & 0b00100000) != 0;
                chkMartes.Checked = (resultado & 0b00010000) != 0;
                chkMiercoles.Checked = (resultado & 0b00001000) != 0;
                chkJueves.Checked = (resultado & 0b00000100) != 0;
                chkViernes.Checked = (resultado & 0b00000010) != 0;
            }
        }

        private void btnNumConsultas_Click(object sender, EventArgs e)
        {
            Pedido pedido = new Pedido
            {
                Comando = "CONTADOR",
                Parametros = new[] { "hola" }
            };

           Respuesta respuesta = ProtocoloHelper.HazOperacion(remoto, flujo, pedido);
            if (respuesta == null)
            {
                MessageBox.Show("Hubo un error", "ERROR");
                return;
            }

            if (respuesta.Estado == "NOK")
            {
                MessageBox.Show("Error en la solicitud.", "ERROR");
            }
            else
            {
                MessageBox.Show("El número de pedidos recibidos en este cliente es " +
                    respuesta.Mensaje, "INFORMACIÓN");
            }
        }

        private void FrmValidador_FormClosing(object sender, FormClosingEventArgs e)
        {
            flujo?.Close();
            remoto?.Close();
        }

        private void LimpiarDias()
        {
            chkLunes.Checked = false;
            chkMartes.Checked = false;
            chkMiercoles.Checked = false;
            chkJueves.Checked = false;
            chkViernes.Checked = false;
        }
    }
}
