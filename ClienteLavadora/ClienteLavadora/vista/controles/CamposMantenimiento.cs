using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using ClienteLavadora.modelo;
using ClienteLavadora.util;

namespace ClienteLavadora.vista.controles
{
    /// <summary>
    /// Formulario con los datos de un MANTENIMIENTO (objeto detalle).
    ///
    /// El ID se ingresa al crear y luego se conserva para consultar, actualizar o eliminar.
    ///
    /// Se usa en la ventana de Mantenimientos para agregar y para actualizar.
    /// </summary>
    public class CamposMantenimiento : StackPanel
    {
        private readonly CampoTexto txtId = new CampoTexto();
        private readonly CampoTexto txtDescripcion = new CampoTexto();
        private readonly CampoTexto txtTecnico = new CampoTexto();
        private readonly CampoTexto txtCosto = new CampoTexto();
        private readonly CampoFecha txtFecha = new CampoFecha();
        private readonly CheckBox chkCompletado = Ui.Check("Mantenimiento completado");
        private bool idBloqueado;

        private readonly Grid[] filas = new Grid[5];

        public CamposMantenimiento()
        {
            txtCosto.SinNegativos = true;
            txtCosto.Fuente = Tema.Numeros;

            filas[0] = Ui.Fila(Ui.Celda("ID del mantenimiento", txtId), null);
            filas[1] = Ui.Fila(Ui.Celda("Descripcion del trabajo", txtDescripcion), null);
            filas[2] = Ui.Fila(Ui.Celda("Tecnico", txtTecnico), Ui.Celda("Costo (pesos)", txtCosto));
            filas[3] = Ui.Fila(Ui.Celda("Fecha", txtFecha), null);
            filas[4] = Ui.Fila(Ui.Celda(null, chkCompletado), null);

            foreach (Grid fila in filas)
            {
                Children.Add(fila);
            }

            Limpiar();
        }

        /// <summary>Filas del formulario (para la animacion de entrada).</summary>
        public IEnumerable<UIElement> Filas
        {
            get { return filas; }
        }

        // ================= Limpiar y mostrar =================

        public void Limpiar()
        {
            txtId.Texto = "";
            idBloqueado = false;
            txtId.SoloLectura = false;
            txtDescripcion.Texto = "";
            txtTecnico.Texto = "";
            txtCosto.Texto = "";
            txtFecha.Texto = Formato.FechaServidor(DateTime.Now);
            chkCompletado.IsChecked = false;
        }

        /// <summary>Precarga un mantenimiento existente para poder actualizarlo.</summary>
        public void Mostrar(MantenimientoData mantenimiento)
        {
            if (mantenimiento == null)
            {
                Limpiar();
                return;
            }

            txtId.Texto = mantenimiento.Id.ToString();
            idBloqueado = true;
            txtId.SoloLectura = true;
            txtDescripcion.Texto = mantenimiento.Descripcion ?? "";
            txtTecnico.Texto = mantenimiento.Tecnico ?? "";
            txtCosto.Texto = mantenimiento.Costo.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
            txtFecha.Texto = mantenimiento.Fecha ?? "";
            chkCompletado.IsChecked = mantenimiento.Completado;
        }

        /// <summary>Bloquea o permite editar (por ejemplo mientras se espera al servidor).</summary>
        public void PermitirEdicion(bool permitir)
        {
            txtDescripcion.SoloLectura = !permitir;
            txtTecnico.SoloLectura = !permitir;
            txtId.SoloLectura = !permitir || idBloqueado;
            txtCosto.SoloLectura = !permitir;
            txtFecha.SoloLectura = !permitir;
            chkCompletado.IsEnabled = permitir;
        }

        // ================= Leer lo que escribio el usuario =================

        /// <summary>Datos para crear un mantenimiento con ID ingresado por el usuario.</summary>
        public MantenimientoInput LeerInput()
        {
            MantenimientoInput input = new MantenimientoInput();
            if (!int.TryParse(txtId.Texto, out input.Id) || input.Id < 1)
                throw new ArgumentException("Escriba un ID de mantenimiento mayor que cero.");
            input.Descripcion = Formato.LeerTexto(txtDescripcion.Texto,
                "Escriba la descripcion del mantenimiento.");
            input.Tecnico = Formato.LeerTexto(txtTecnico.Texto, "Escriba el nombre del tecnico.");
            input.Costo = Formato.LeerDecimalCeroPermitido(txtCosto.Texto,
                "El costo debe ser un numero mayor o igual a cero (por ejemplo 185000).");
            input.Fecha = Formato.LeerFechaServidor(txtFecha.Texto,
                "Seleccione una fecha valida en el calendario.");
            input.Completado = chkCompletado.IsChecked == true;
            return input;
        }

        /// <summary>Datos para actualizar un mantenimiento (los mismos campos, todos opcionales).</summary>
        public MantenimientoUpdateInput LeerUpdateInput()
        {
            MantenimientoInput completo = LeerInput(); // Se valida una sola vez

            MantenimientoUpdateInput input = new MantenimientoUpdateInput();
            input.Descripcion = completo.Descripcion;
            input.Tecnico = completo.Tecnico;
            input.Costo = completo.Costo;
            input.Fecha = completo.Fecha;
            input.Completado = completo.Completado;
            return input;
        }

        /// <summary>Pone el campo y debajo una pista con el formato esperado.</summary>
        private static StackPanel ConPista(UIElement campo, string pista)
        {
            StackPanel panel = new StackPanel();
            panel.Children.Add(campo);
            panel.Children.Add(Ui.Pista(pista));
            return panel;
        }
    }
}
