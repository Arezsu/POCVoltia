using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using ClienteLavadora.modelo;
using ClienteLavadora.util;

namespace ClienteLavadora.vista.controles
{
    /// <summary>
    /// Formulario con TODOS los datos de una lavadora. Se usa tal cual en tres ventanas:
    /// Agregar (vacio), Actualizar (precargado) y como apoyo de la vista previa.
    ///
    /// POR QUE UN CONTROL APARTE:
    /// si las dos ventanas escribieran los ocho campos cada una, cualquier cambio
    /// (por ejemplo agregar una validacion) habria que hacerlo dos veces y se correria
    /// el riesgo de que una quede distinta de la otra. Aqui se escribe una vez.
    ///
    /// VALIDACION: el control lanza ArgumentException con un mensaje en espanol cuando
    /// un dato esta mal. La ventana solo atrapa y muestra: no repite la validacion.
    /// </summary>
    public class CamposLavadora : StackPanel
    {
        private readonly CampoTexto txtCodigo = new CampoTexto();
        private readonly ComboBox cmbMarca;
        private readonly CampoTexto txtFecha = new CampoTexto();
        private readonly CampoTexto txtPrecio = new CampoTexto();
        private readonly CampoTexto txtCapacidad = new CampoTexto();
        private readonly CampoTexto txtRpm = new CampoTexto();
        private readonly CampoTexto txtMinutos = new CampoTexto();
        private readonly CheckBox chkInverter = Ui.Check("Tecnologia inverter");

        private readonly Grid[] filas = new Grid[5];

        /// <summary>Se dispara cada vez que cambia un dato (para recalcular la vista previa).</summary>
        public event Action Cambio;

        public CamposLavadora()
        {
            cmbMarca = Ui.Combo();
            cmbMarca.IsEditable = true; // El usuario puede escribir una marca nueva
            string[] marcas = { "LG", "Samsung", "Whirlpool", "Mabe", "Haceb", "Electrolux", "Bosch", "Siemens" };
            foreach (string marca in marcas)
            {
                cmbMarca.Items.Add(marca);
            }

            // Los numeros y la fecha se ven en Consolas para que queden alineados.
            txtCodigo.Fuente = Tema.Numeros;
            txtFecha.Fuente = Tema.Numeros;
            txtPrecio.Fuente = Tema.Numeros;
            txtCapacidad.Fuente = Tema.Numeros;
            txtRpm.Fuente = Tema.Numeros;
            txtMinutos.Fuente = Tema.Numeros;

            txtFecha.Ayuda = "Formato yyyy-MM-ddTHH:mm:ss";

            // ---------- Filas del formulario ----------
            filas[0] = Ui.Fila(Ui.Celda("Codigo de la lavadora", txtCodigo), Ui.Celda("Marca", cmbMarca));
            filas[1] = Ui.Fila(Ui.Celda("Fecha de fabricacion", ConPista(txtFecha,
                "Formato yyyy-MM-ddTHH:mm:ss, por ejemplo 2024-03-10T09:00:00")), null);
            filas[2] = Ui.Fila(Ui.Celda("Precio base (pesos)", txtPrecio), Ui.Celda("Capacidad de carga (kg)", txtCapacidad));
            filas[3] = Ui.Fila(Ui.Celda("Velocidad de centrifugado (RPM)", txtRpm), Ui.Celda("Tiempo de lavado (minutos)", txtMinutos));
            filas[4] = Ui.Fila(Ui.Celda(null, chkInverter), null);

            foreach (Grid fila in filas)
            {
                Children.Add(fila);
            }

            // Avisar cada cambio: la ventana Agregar lo usa para la vista previa.
            txtCodigo.TextoCambiado += AlCambiarTexto;
            txtFecha.TextoCambiado += AlCambiarTexto;
            txtPrecio.TextoCambiado += AlCambiarTexto;
            txtCapacidad.TextoCambiado += AlCambiarTexto;
            txtRpm.TextoCambiado += AlCambiarTexto;
            txtMinutos.TextoCambiado += AlCambiarTexto;
            chkInverter.Checked += AlCambiarOpcion;
            chkInverter.Unchecked += AlCambiarOpcion;

            Limpiar();
        }

        // ================= Propiedades =================

        public UIElement Vista
        {
            get { return this; }
        }

        /// <summary>Filas del formulario (para la animacion escalonada de entrada).</summary>
        public IEnumerable<UIElement> Filas
        {
            get { return filas; }
        }

        // ================= Limpiar y mostrar =================

        /// <summary>Deja el formulario listo para escribir una lavadora nueva.</summary>
        public void Limpiar()
        {
            txtCodigo.Texto = "";
            cmbMarca.Text = "";
            cmbMarca.SelectedIndex = -1;
            txtFecha.Texto = Formato.FechaServidor(DateTime.Now);
            txtPrecio.Texto = "";
            txtCapacidad.Texto = "";
            txtRpm.Texto = "";
            txtMinutos.Texto = "";
            chkInverter.IsChecked = false;
        }

        /// <summary>Precarga los datos de una lavadora que ya existe (para actualizar).</summary>
        public void Mostrar(LavadoraData lavadora)
        {
            if (lavadora == null)
            {
                Limpiar();
                return;
            }

            txtCodigo.Texto = lavadora.Codigo.ToString(CultureInfo.InvariantCulture);
            cmbMarca.Text = lavadora.Marca;
            txtFecha.Texto = lavadora.FechaFabricacion ?? "";
            txtPrecio.Texto = lavadora.PrecioBase.ToString("0.##", CultureInfo.InvariantCulture);
            txtCapacidad.Texto = lavadora.CapacidadCargaKilos.ToString("0.##", CultureInfo.InvariantCulture);
            txtRpm.Texto = lavadora.VelocidadCentrifugadoRPM.ToString(CultureInfo.InvariantCulture);
            txtMinutos.Texto = lavadora.MinutosLavado.ToString(CultureInfo.InvariantCulture);
            chkInverter.IsChecked = lavadora.TecnologiaInverter;
        }

        // ================= Bloqueos =================

        /// <summary>Permite o prohibe editar todos los campos (en Actualizar, el codigo se bloquea).</summary>
        public void PermitirEdicion(bool permitir)
        {
            txtFecha.SoloLectura = !permitir;
            txtPrecio.SoloLectura = !permitir;
            txtCapacidad.SoloLectura = !permitir;
            txtRpm.SoloLectura = !permitir;
            txtMinutos.SoloLectura = !permitir;
            cmbMarca.IsEnabled = permitir;
            chkInverter.IsEnabled = permitir;
        }

        /// <summary>El codigo es la llave de la lavadora: al actualizar no se puede cambiar.</summary>
        public void BloquearCodigo()
        {
            txtCodigo.SoloLectura = true;
        }

        /// <summary>Deja el codigo editable (caso de uso Agregar).</summary>
        public void PermitirCodigo()
        {
            txtCodigo.SoloLectura = false;
        }

        /// <summary>Lleva el cursor al primer campo (comodo despues de guardar).</summary>
        public void EnfocarCodigo()
        {
            txtCodigo.Enfocar();
        }

        // ================= Leer lo que escribio el usuario =================

        /// <summary>Datos para crear una lavadora. Valida todo y lanza ArgumentException si algo esta mal.</summary>
        public LavadoraInput LeerInput()
        {
            LavadoraInput input = new LavadoraInput();
            input.Codigo = Formato.LeerEntero(txtCodigo.Texto, 1,
                "El codigo debe ser un numero entero mayor que cero.");
            input.Marca = Formato.LeerTexto(cmbMarca.Text, "Escriba la marca de la lavadora.");
            input.FechaFabricacion = Formato.LeerFechaServidor(txtFecha.Texto,
                "La fecha debe tener el formato yyyy-MM-ddTHH:mm:ss (por ejemplo 2024-03-10T09:00:00).");
            input.PrecioBase = Formato.LeerDecimal(txtPrecio.Texto,
                "El precio base debe ser un numero mayor que cero (por ejemplo 1450000).");
            input.CapacidadCargaKilos = Formato.LeerDecimal(txtCapacidad.Texto,
                "La capacidad de carga debe ser un numero mayor que cero (por ejemplo 18).");
            input.VelocidadCentrifugadoRPM = Formato.LeerEntero(txtRpm.Texto, 600,
                "La velocidad de centrifugado debe ser un numero entero de 600 RPM o mas.");
            input.MinutosLavado = Formato.LeerEnteroEnRango(txtMinutos.Texto, 1, 300,
                "El tiempo de lavado debe estar entre 1 y 300 minutos.");
            input.TecnologiaInverter = chkInverter.IsChecked == true;
            return input;
        }

        /// <summary>Datos para actualizar (sin el codigo, que no cambia).</summary>
        public LavadoraUpdateInput LeerUpdateInput()
        {
            LavadoraInput completo = LeerInput(); // Se valida una sola vez

            LavadoraUpdateInput input = new LavadoraUpdateInput();
            input.Marca = completo.Marca;
            input.PrecioBase = completo.PrecioBase;
            input.FechaFabricacion = completo.FechaFabricacion;
            input.CapacidadCargaKilos = completo.CapacidadCargaKilos;
            input.VelocidadCentrifugadoRPM = completo.VelocidadCentrifugadoRPM;
            input.TecnologiaInverter = completo.TecnologiaInverter;
            input.MinutosLavado = completo.MinutosLavado;
            return input;
        }

        /// <summary>
        /// Datos "a medias" para la vista previa del precio final: no valida ni lanza
        /// excepciones. Devuelve null si todavia falta un numero por escribir.
        /// </summary>
        public LavadoraInput LeerParaVistaPrevia()
        {
            double precio;
            double capacidad;
            int rpm;
            int minutos;
            int codigo;

            if (!double.TryParse((txtPrecio.Texto ?? "").Trim().Replace(',', '.'),
                    NumberStyles.Float, CultureInfo.InvariantCulture, out precio))
            {
                return null;
            }
            if (!double.TryParse((txtCapacidad.Texto ?? "").Trim().Replace(',', '.'),
                    NumberStyles.Float, CultureInfo.InvariantCulture, out capacidad))
            {
                return null;
            }
            if (!int.TryParse((txtRpm.Texto ?? "").Trim(), out rpm))
            {
                return null;
            }
            if (!int.TryParse((txtMinutos.Texto ?? "").Trim(), out minutos))
            {
                return null;
            }
            if (!int.TryParse((txtCodigo.Texto ?? "").Trim(), out codigo))
            {
                codigo = 0;
            }

            LavadoraInput input = new LavadoraInput();
            input.Codigo = codigo;
            input.Marca = cmbMarca.Text;
            input.FechaFabricacion = txtFecha.Texto;
            input.PrecioBase = precio;
            input.CapacidadCargaKilos = capacidad;
            input.VelocidadCentrifugadoRPM = rpm;
            input.MinutosLavado = minutos;
            input.TecnologiaInverter = chkInverter.IsChecked == true;
            return input;
        }

        // ================= Apoyo =================

        /// <summary>Pone el campo y debajo una pista con el formato esperado.</summary>
        private static StackPanel ConPista(UIElement campo, string pista)
        {
            StackPanel panel = new StackPanel();
            panel.Children.Add(campo);
            panel.Children.Add(Ui.Pista(pista));
            return panel;
        }

        private void AlCambiarTexto(object remitente, TextChangedEventArgs argumentos)
        {
            AvisarCambio();
        }

        private void AlCambiarOpcion(object remitente, RoutedEventArgs argumentos)
        {
            AvisarCambio();
        }

        private void AvisarCambio()
        {
            if (Cambio != null)
            {
                Cambio();
            }
        }
    }
}
