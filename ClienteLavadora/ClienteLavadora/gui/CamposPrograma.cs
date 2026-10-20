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
    /// Formulario con todos los datos de un PROGRAMA DE LAVADO (objeto maestro).
    /// Cumple el mismo contrato ICampos que CamposLavadora.
    /// </summary>
    public class CamposPrograma : StackPanel, ICampos
    {
        private readonly CampoTexto txtCodigo = new CampoTexto();
        private readonly CampoTexto txtNombre = new CampoTexto();
        private readonly CampoTexto txtMinimo = new CampoTexto();
        private readonly CampoTexto txtMaximo = new CampoTexto();
        private readonly CampoTexto txtAgua = new CampoTexto();
        private readonly CheckBox chkCaliente = Ui.Check("Usa agua caliente");
        private readonly DatePicker dpFecha = Ui.Fecha();
        private readonly CampoTexto txtHora = new CampoTexto();
        private readonly CampoTexto txtLavadoras = new CampoTexto();

        private readonly Grid[] filas = new Grid[5];

        public CamposPrograma()
        {
            txtLavadoras.SoloLectura = true;

            // Fila 1: codigo y nombre
            filas[0] = Ui.Fila(Ui.Celda("Código", txtCodigo), Ui.Celda("Nombre", txtNombre));
            // Fila 2: rango de minutos que cubre el programa
            filas[1] = Ui.Fila(Ui.Celda("Minutos mínimos", txtMinimo), Ui.Celda("Minutos máximos", txtMaximo));
            // Fila 3: consumo de agua y agua caliente
            filas[2] = Ui.Fila(Ui.Celda("Consumo de agua (litros)", txtAgua), chkCaliente);
            // Fila 4: fecha de creacion y hora (juntas forman el LocalDateTime)
            filas[3] = Ui.Fila(Ui.Celda("Fecha de creación", dpFecha), Ui.Celda("Hora (formato 14:30)", txtHora));
            // Fila 5: cuantas lavadoras usan el programa (la parte 0..* de la relacion)
            filas[4] = Ui.Fila(Ui.Celda("Lavadoras que usan este programa", txtLavadoras), null);

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
            filas[4].Visibility = Visibility.Collapsed;
            txtCodigo.SoloLectura = false;
            PermitirEdicion(true);
        }

        public void ConfigurarParaBuscar()
        {
            filas[4].Visibility = Visibility.Visible;
            txtCodigo.SoloLectura = true;
            PermitirEdicion(false);
        }

        public void PermitirEdicion(bool permitir)
        {
            txtNombre.SoloLectura = !permitir;
            txtMinimo.SoloLectura = !permitir;
            txtMaximo.SoloLectura = !permitir;
            txtAgua.SoloLectura = !permitir;
            txtHora.SoloLectura = !permitir;
            chkCaliente.IsEnabled = permitir;
            dpFecha.IsEnabled = permitir;
        }

        // ================= Limpiar y mostrar =================

        public void Limpiar()
        {
            txtCodigo.Texto = "";
            txtNombre.Texto = "";
            txtMinimo.Texto = "";
            txtMaximo.Texto = "";
            txtAgua.Texto = "";
            chkCaliente.IsChecked = false;
            dpFecha.SelectedDate = DateTime.Today;
            txtHora.Texto = DateTime.Now.ToString("HH:mm");
            txtLavadoras.Texto = "";
        }

        public void Mostrar(DatoBase dato)
        {
            ProgramaData programa = (ProgramaData)dato; // El tipo que corresponde

            txtCodigo.Texto = programa.Codigo.ToString();
            txtNombre.Texto = programa.Nombre;
            txtMinimo.Texto = programa.MinMinutos.ToString();
            txtMaximo.Texto = programa.MaxMinutos.ToString();
            txtAgua.Texto = programa.ConsumoAguaLitros.ToString("0.##", CultureInfo.InvariantCulture);
            chkCaliente.IsChecked = programa.AguaCaliente;

            DateTime fecha = programa.FechaComoDateTime;
            dpFecha.SelectedDate = fecha.Date;
            txtHora.Texto = fecha.ToString("HH:mm");

            txtLavadoras.Texto = programa.CantidadLavadoras.ToString();
        }

        // ================= Leer lo que escribio el usuario =================

        public object ObtenerInput()
        {
            ProgramaInput input = new ProgramaInput();
            input.Codigo = Lectura.Codigo(txtCodigo);
            input.Nombre = LeerNombre();
            input.MinMinutos = Lectura.Entero(txtMinimo, 1, "Los minutos mínimos deben ser un número entero de 1 o más.");
            input.MaxMinutos = Lectura.Entero(txtMaximo, 1, "Los minutos máximos deben ser un número entero de 1 o más.");
            input.ConsumoAguaLitros = Lectura.Decimal(txtAgua, "El consumo de agua debe ser un número mayor que cero (por ejemplo 55.5).");
            input.FechaCreacion = Lectura.FechaHora(dpFecha, txtHora, "fecha de creación");
            input.AguaCaliente = chkCaliente.IsChecked == true;
            return input;
        }

        /// <summary>Datos para actualizar (el codigo no cambia, por eso no va aqui).</summary>
        public object ObtenerUpdateInput()
        {
            ProgramaUpdateInput input = new ProgramaUpdateInput();
            input.Nombre = LeerNombre();
            input.MinMinutos = Lectura.Entero(txtMinimo, 1, "Los minutos mínimos deben ser un número entero de 1 o más.");
            input.MaxMinutos = Lectura.Entero(txtMaximo, 1, "Los minutos máximos deben ser un número entero de 1 o más.");
            input.ConsumoAguaLitros = Lectura.Decimal(txtAgua, "El consumo de agua debe ser un número mayor que cero (por ejemplo 55.5).");
            input.FechaCreacion = Lectura.FechaHora(dpFecha, txtHora, "fecha de creación");
            input.AguaCaliente = chkCaliente.IsChecked == true;
            return input;
        }

        private string LeerNombre()
        {
            string nombre = txtNombre.Texto.Trim();
            if (nombre.Length == 0)
            {
                throw new ArgumentException("Escriba el nombre del programa.");
            }
            return nombre;
        }
    }
}
