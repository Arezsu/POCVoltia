using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using ClienteLavadora.model;
using ClienteLavadora.util;

namespace ClienteLavadora.gui
{
    /// <summary>
    /// Formulario con todos los datos de una LAVADORA (los mismos de la ventana en Java).
    /// Se dibuja en codigo: cada fila es una cuadricula con dos columnas.
    /// Cumple el contrato ICampos, por eso las ventanas lo usan sin saber que es una lavadora.
    /// </summary>
    public class CamposLavadora : StackPanel, ICampos
    {
        private readonly CampoTexto txtCodigo = new CampoTexto();
        private readonly ComboBox cmbMarca = Ui.Combo();
        private readonly DatePicker dpFecha = Ui.Fecha();
        private readonly CampoTexto txtHora = new CampoTexto();
        private readonly CampoTexto txtPrecio = new CampoTexto();
        private readonly CampoTexto txtCapacidad = new CampoTexto();
        private readonly CampoTexto txtRpm = new CampoTexto();
        private readonly CampoTexto txtMinutos = new CampoTexto();
        private readonly CheckBox chkInverter = Ui.Check("Tecnología Inverter");
        private readonly CampoTexto txtPrograma = new CampoTexto();
        private readonly CampoTexto txtPrecioFinal = new CampoTexto();

        private readonly Grid[] filas = new Grid[6];

        public CamposLavadora()
        {
            foreach (string marca in Catalogo.Marcas)
            {
                cmbMarca.Items.Add(marca);
            }

            txtPrograma.SoloLectura = true;
            txtPrecioFinal.SoloLectura = true;

            // Fila 1: codigo y marca
            filas[0] = Ui.Fila(Ui.Celda("Código", txtCodigo), Ui.Celda("Marca", cmbMarca));
            // Fila 2: fecha de fabricacion y hora (juntas forman el LocalDateTime)
            filas[1] = Ui.Fila(Ui.Celda("Fecha de fabricación", dpFecha), Ui.Celda("Hora (formato 14:30)", txtHora));
            // Fila 3: precio base y capacidad
            filas[2] = Ui.Fila(Ui.Celda("Precio base ($)", txtPrecio), Ui.Celda("Capacidad (kg)", txtCapacidad));
            // Fila 4: velocidad de centrifugado y tiempo de lavado
            filas[3] = Ui.Fila(Ui.Celda("Velocidad de centrifugado (RPM)", txtRpm), Ui.Celda("Tiempo de lavado (min)", txtMinutos));
            // Fila 5: tecnologia inverter
            filas[4] = Ui.Fila(chkInverter, null);
            // Fila 6: datos que calcula el servidor (solo se ven al buscar)
            filas[5] = Ui.Fila(Ui.Celda("Programa de lavado (automático)", txtPrograma),
                               Ui.Celda("Precio final (calculado)", txtPrecioFinal));

            foreach (Grid fila in filas)
            {
                Children.Add(fila);
            }
            Limpiar();
            ConfigurarParaAgregar();
        }

        public UIElement Vista
        {
            get { return this; }
        }

        public IEnumerable<UIElement> Filas // Lista recorrible
        {
            get { return filas; }
        }

        // ================= Modos de uso =================

        public void ConfigurarParaAgregar()
        {
            filas[5].Visibility = Visibility.Collapsed;
            txtCodigo.SoloLectura = false;
            PermitirEdicion(true);
        }

        public void ConfigurarParaBuscar()
        {
            filas[5].Visibility = Visibility.Visible;
            txtCodigo.SoloLectura = true;
            PermitirEdicion(false);
        }

        public void PermitirEdicion(bool permitir)
        {
            txtHora.SoloLectura = !permitir;
            txtPrecio.SoloLectura = !permitir;
            txtCapacidad.SoloLectura = !permitir;
            txtRpm.SoloLectura = !permitir;
            txtMinutos.SoloLectura = !permitir;
            cmbMarca.IsEnabled = permitir;
            dpFecha.IsEnabled = permitir;
            chkInverter.IsEnabled = permitir;
        }

        // ================= Limpiar y mostrar =================

        public void Limpiar()
        {
            txtCodigo.Texto = "";
            cmbMarca.SelectedIndex = 0;
            dpFecha.SelectedDate = DateTime.Today;
            txtHora.Texto = DateTime.Now.ToString("HH:mm");
            txtPrecio.Texto = "";
            txtCapacidad.Texto = "";
            txtRpm.Texto = "";
            txtMinutos.Texto = "";
            chkInverter.IsChecked = false;
            txtPrograma.Texto = "";
            txtPrecioFinal.Texto = "";
        }

        public void Mostrar(DatoBase dato)
        {
            LavadoraData lavadora = (LavadoraData)dato; // El tipo que corresponde

            txtCodigo.Texto = lavadora.Codigo.ToString();

            // Si la marca no esta en el combo (por ejemplo viene de otro cliente), se agrega
            if (!cmbMarca.Items.Contains(lavadora.Marca))
            {
                cmbMarca.Items.Add(lavadora.Marca);
            }
            cmbMarca.SelectedItem = lavadora.Marca;

            DateTime fecha = lavadora.FechaComoDateTime;
            dpFecha.SelectedDate = fecha.Date;
            txtHora.Texto = fecha.ToString("HH:mm");

            // Los numeros se escriben con punto decimal (cultura invariante) para poder leerlos de nuevo
            txtPrecio.Texto = lavadora.PrecioBase.ToString("0.##", CultureInfo.InvariantCulture);
            txtCapacidad.Texto = lavadora.CapacidadCargaKilos.ToString("0.##", CultureInfo.InvariantCulture);
            txtRpm.Texto = lavadora.VelocidadCentrifugadoRPM.ToString();
            txtMinutos.Texto = lavadora.MinutosLavado.ToString();
            chkInverter.IsChecked = lavadora.TecnologiaInverter;

            txtPrograma.Texto = lavadora.ProgramaTexto;
            txtPrecioFinal.Texto = lavadora.PrecioFinalTexto;
        }

        // ================= Leer lo que escribio el usuario =================

        public object ObtenerInput()
        {
            LavadoraInput input = new LavadoraInput();
            input.Codigo = Lectura.Codigo(txtCodigo);
            input.Marca = LeerMarca();
            input.FechaFabricacion = Lectura.FechaHora(dpFecha, txtHora, "fecha de fabricación");
            input.PrecioBase = Lectura.Decimal(txtPrecio, "El precio base debe ser un número mayor que cero (por ejemplo 1450000).");
            input.CapacidadCargaKilos = Lectura.Decimal(txtCapacidad, "La capacidad debe ser un número mayor que cero (por ejemplo 15.5).");
            input.VelocidadCentrifugadoRPM = Lectura.Entero(txtRpm, 600, "La velocidad de centrifugado debe ser un número entero de 600 RPM o más.");
            input.MinutosLavado = Lectura.Entero(txtMinutos, 1, "El tiempo de lavado debe ser un número entero de minutos (por ejemplo 45).");
            input.TecnologiaInverter = chkInverter.IsChecked == true;
            return input;
        }

        /// <summary>Datos para actualizar (el codigo no cambia, por eso no va aqui).</summary>
        public object ObtenerUpdateInput()
        {
            LavadoraUpdateInput input = new LavadoraUpdateInput();
            input.Marca = LeerMarca();
            input.FechaFabricacion = Lectura.FechaHora(dpFecha, txtHora, "fecha de fabricación");
            input.PrecioBase = Lectura.Decimal(txtPrecio, "El precio base debe ser un número mayor que cero (por ejemplo 1450000).");
            input.CapacidadCargaKilos = Lectura.Decimal(txtCapacidad, "La capacidad debe ser un número mayor que cero (por ejemplo 15.5).");
            input.VelocidadCentrifugadoRPM = Lectura.Entero(txtRpm, 600, "La velocidad de centrifugado debe ser un número entero de 600 RPM o más.");
            input.MinutosLavado = Lectura.Entero(txtMinutos, 1, "El tiempo de lavado debe ser un número entero de minutos (por ejemplo 45).");
            input.TecnologiaInverter = chkInverter.IsChecked == true;
            return input;
        }

        private string LeerMarca()
        {
            string marca = cmbMarca.SelectedItem as string;
            if (string.IsNullOrWhiteSpace(marca))
            {
                throw new ArgumentException("Escoja una marca.");
            }
            return marca;
        }
    }
}
