using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace _3N_LIDTS_AdesDelgado_04_1_IA_2026
{
    public partial class Form1 : Form
    {
        // Muestra un ícono de error junto al campo (no bloquea al usuario como MessageBox)
        private readonly ErrorProvider errores = new ErrorProvider();

        // Letras (con acentos y ñ) separadas por espacios simples: "José Luis", "Gómez Núñez"
        private static readonly Regex RegexTexto = new Regex(@"^\p{L}+(\s\p{L}+)*$", RegexOptions.Compiled);
        private static readonly Regex RegexTelefono = new Regex(@"^\d{10}$", RegexOptions.Compiled);

        private const string NombreArchivo = "3NAgoDic26.txt";

        public Form1()
        {
            InitializeComponent();

            Text = "Registro de datos";
            errores.BlinkStyle = ErrorBlinkStyle.NeverBlink;

            // Límites de captura
            txbTelefono.MaxLength = 10;
            txbEdad.MaxLength = 3;
            txbEstatura.MaxLength = 4;

            // Validación al salir de cada campo (no en cada tecla)
            txbNombre.Validating += (s, e) => ValidarNombre();
            txbApellidos.Validating += (s, e) => ValidarApellidos();
            txbEdad.Validating += (s, e) => ValidarEdad();
            txbEstatura.Validating += (s, e) => ValidarEstatura();
            txbTelefono.Validating += (s, e) => ValidarTelefono();

            // Filtros de teclado: impiden escribir caracteres inválidos desde el inicio
            txbEdad.KeyPress += SoloDigitos;
            txbTelefono.KeyPress += SoloDigitos;
            txbEstatura.KeyPress += SoloDecimal;

            // Limpiar no debe disparar validaciones
            btnLimpiar.CausesValidation = false;
        }

        // ---------- Reglas de validación ----------

        private bool Validar(TextBox tb, bool esValido, string mensaje)
        {
            errores.SetError(tb, esValido ? "" : mensaje);
            tb.BackColor = esValido ? Color.Honeydew : Color.MistyRose;
            return esValido;
        }

        private bool ValidarNombre() =>
            Validar(txbNombre, RegexTexto.IsMatch(txbNombre.Text.Trim()),
                    "Solo letras y espacios (se permiten acentos y ñ).");

        private bool ValidarApellidos() =>
            Validar(txbApellidos, RegexTexto.IsMatch(txbApellidos.Text.Trim()),
                    "Solo letras y espacios (se permiten acentos y ñ).");

        private bool ValidarEdad()
        {
            bool ok = int.TryParse(txbEdad.Text.Trim(), NumberStyles.None,
                                   CultureInfo.InvariantCulture, out int edad)
                      && edad >= 1 && edad <= 120;
            return Validar(txbEdad, ok, "Ingrese una edad entre 1 y 120.");
        }

        private bool TryObtenerEstatura(out decimal estatura)
        {
            // Acepta punto o coma como separador, sin depender de la configuración regional
            string texto = txbEstatura.Text.Trim().Replace(',', '.');
            return decimal.TryParse(texto, NumberStyles.AllowDecimalPoint,
                                    CultureInfo.InvariantCulture, out estatura)
                   && estatura >= 0.50m && estatura <= 2.50m;
        }

        private bool ValidarEstatura() =>
            Validar(txbEstatura, TryObtenerEstatura(out _),
                    "Ingrese la estatura en metros (entre 0.50 y 2.50). Ej: 1.70");

        private bool ValidarTelefono() =>
            Validar(txbTelefono, RegexTelefono.IsMatch(txbTelefono.Text),
                    "El teléfono debe tener exactamente 10 dígitos.");

        // ---------- Filtros de teclado ----------

        private void SoloDigitos(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        }

        private void SoloDecimal(object sender, KeyPressEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (char.IsControl(e.KeyChar) || char.IsDigit(e.KeyChar)) return;

            bool esSeparador = e.KeyChar == '.' || e.KeyChar == ',';
            if (esSeparador && tb.Text.IndexOfAny(new[] { '.', ',' }) < 0) return;

            e.Handled = true;
        }

        // ---------- Botones ----------

        private string ObtenerGenero()
        {
            if (rbFemenino.Checked) return "Femenino";
            if (rbMasculino.Checked) return "Masculino";
            if (rbOtro.Checked) return "Otro";
            return null;
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            // Se ejecutan TODAS las validaciones (sin cortocircuito) para marcar todos los errores a la vez
            bool todoValido = true;
            todoValido &= ValidarNombre();
            todoValido &= ValidarApellidos();
            todoValido &= ValidarTelefono();
            todoValido &= ValidarEdad();
            todoValido &= ValidarEstatura();

            string genero = ObtenerGenero();
            if (genero == null) todoValido = false;

            if (!todoValido)
            {
                string aviso = genero == null
                    ? "Corrija los campos marcados y seleccione un género."
                    : "Corrija los campos marcados antes de guardar.";
                MessageBox.Show(aviso, "Datos incompletos o inválidos",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            TryObtenerEstatura(out decimal estatura);

            string mensaje =
                $"Nombre: {txbNombre.Text.Trim()}\n" +
                $"Apellidos: {txbApellidos.Text.Trim()}\n" +
                $"Teléfono: {txbTelefono.Text}\n" +
                $"Edad: {txbEdad.Text.Trim()}\n" +
                $"Estatura: {estatura.ToString("0.00", CultureInfo.InvariantCulture)} m\n" +
                $"Género: {genero}";

            // Archivo en Documentos del usuario: funciona en cualquier equipo (sin ruta fija)
            string carpeta = AppContext.BaseDirectory; // junto al ejecutable (bin\Debug\net...)
            string rutaArchivo = Path.Combine(carpeta, NombreArchivo);

            try
            {
                bool archivoExiste = File.Exists(rutaArchivo);

                using (StreamWriter escritor = new StreamWriter(rutaArchivo, true, Encoding.UTF8))
                {
                    if (archivoExiste) escritor.WriteLine();
                    escritor.WriteLine($"Registro: {DateTime.Now:yyyy-MM-dd HH:mm}");
                    escritor.WriteLine(mensaje);
                    escritor.WriteLine(new string('-', 30));
                }
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                MessageBox.Show($"No se pudo guardar el archivo:\n{ex.Message}", "Error al guardar",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show($"{mensaje}\n\nGuardado en:\n{rutaArchivo}", "Confirmación de Registro",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Abre el Explorador con el archivo seleccionado para que sepas dónde quedó
            Process.Start("explorer.exe", $"/select,\"{rutaArchivo}\"");

            btnLimpiar_Click(sender, e);
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            foreach (TextBox tb in new[] { txbNombre, txbApellidos, txbTelefono, txbEdad, txbEstatura })
            {
                tb.Clear();
                tb.BackColor = SystemColors.Window;
            }

            errores.Clear();

            rbFemenino.Checked = false;
            rbMasculino.Checked = false;
            rbOtro.Checked = false;

            txbNombre.Focus();
        }
    }
}