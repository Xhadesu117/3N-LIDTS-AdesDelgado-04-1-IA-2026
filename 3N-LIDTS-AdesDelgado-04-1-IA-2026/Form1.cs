using System;
using System.IO;
using System.Windows.Forms;

namespace _3N_LIDTS_AdesDelgado_04_1_IA_2026
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            string nombre = txbNombre.Text.Trim();
            string apellidos = txbApellidos.Text.Trim();
            string telefono = txbTelefono.Text.Trim();
            string edadTexto = txbEdad.Text.Trim();
            string estaturaTexto = txbEstatura.Text.Trim();

            // Validar que los campos de texto no estén vacíos
            if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(apellidos) || string.IsNullOrEmpty(telefono))
            {
                MessageBox.Show("Por favor completa Nombre, Apellidos y Teléfono.", "Datos incompletos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validar que Edad sea un número entero válido
            if (!int.TryParse(edadTexto, out int edad) || edad <= 0)
            {
                MessageBox.Show("La Edad debe ser un número entero válido.", "Dato inválido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validar que Estatura sea un número decimal válido (ej. 1.75)
            if (!double.TryParse(estaturaTexto, out double estatura) || estatura <= 0)
            {
                MessageBox.Show("La Estatura debe ser un número válido (ej. 1.75).", "Dato inválido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validar que se haya seleccionado un género
            string genero = "";
            if (rbFemenino.Checked)
            {
                genero = "Femenino";
            }
            else if (rbMasculino.Checked)
            {
                genero = "Masculino";
            }
            else if (rbOtro.Checked)
            {
                genero = "Otro";
            }
            else
            {
                MessageBox.Show("Por favor selecciona un Género.", "Dato incompleto", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string mensaje = $" Nombre: {nombre}\n\r Apellidos: {apellidos}\n\r Telefono: {telefono}\n\r Edad: {edad}\n\r Estatura: {estatura}\n\r Genero: {genero}";

            // Ruta portable: guarda el archivo junto al ejecutable en vez de una ruta fija de una sola máquina
            string rutaArchivo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "3N-Formulario.txt");

            try
            {
                bool archivoExiste = File.Exists(rutaArchivo);
                using (StreamWriter escritor = new StreamWriter(rutaArchivo, true))
                {
                    if (archivoExiste)
                    {
                        escritor.WriteLine();
                    }
                    escritor.WriteLine(mensaje);
                }

                MessageBox.Show(mensaje, "Confirmación de registro", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LimpiarFormulario();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo guardar el registro:\n{ex.Message}", "Error al guardar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
        }

        private void LimpiarFormulario()
        {
            txbNombre.Clear();
            txbApellidos.Clear();
            txbTelefono.Clear();
            txbEstatura.Clear();
            txbEdad.Clear();
            rbFemenino.Checked = false;
            rbMasculino.Checked = false;
            rbOtro.Checked = false;
        }
    }
}