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
    /// El id NO aparece en el formulario porque lo calcula el servidor: el cliente
    /// nunca lo inventa. Cuando se actualiza un mantenimiento, su id viaja como
    /// parametro de la mutacion (ver IServicioMantenimientos).
    ///
    /// Se usa en la ventana de Mantenimientos para agregar y para actualizar.
    /// </summary>
    public class CamposMantenimiento : StackPanel
    {
        private readonly CampoTexto txtDescripcion = new CampoTexto();
        private readonly CampoTexto txtTecnico = new CampoTexto();
        private readonly CampoTexto txtCosto = new CampoTexto();
        private readonly CampoTexto txtFecha = new CampoTexto();
        private readonly CheckBox chkCompletado = Ui.Check("Mantenimiento completado");

        private readonly Grid[] filas = new Grid[4];

        public CamposMantenimiento()
        {
            txtCosto.Fuente = Tema.Numeros;
            txtFecha.Fuente = Tema.Numeros;
            txtFecha.Ayuda = "Formato yyyy-MM-ddTHH:mm:ss";

            filas[0] = Ui.Fila(Ui.Celda("Descripcion del trabajo", txtDescripcion), null);
            filas[1] = Ui.Fila(Ui.Celda("Tecnico", txtTecnico), Ui.Celda("Costo (pesos)", txtCosto));
            filas[2] = Ui.Fila(Ui.Celda("Fecha", ConPista(txtFecha,
                "Formato yyyy-MM-ddTHH:mm:ss, por ejemplo 2024-06-12T10:30:00")), null);
            filas[3] = Ui.Fila(Ui.Celda(null, chkCompletado), null);

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
            txtCosto.SoloLectura = !permitir;
            txtFecha.SoloLectura = !permitir;
            chkCompletado.IsEnabled = permitir;
        }

        // ================= Leer lo que escribio el usuario =================

        /// <summary>Datos para crear un mantenimiento (sin id: lo calcula el servidor).</summary>
        public MantenimientoInput LeerInput()
        {
            MantenimientoInput input = new MantenimientoInput();
            input.Descripcion = Formato.LeerTexto(txtDescripcion.Texto,
                "Escriba la descripcion del mantenimiento.");
            input.Tecnico = Formato.LeerTexto(txtTecnico.Texto, "Escriba el nombre del tecnico.");
            input.Costo = Formato.LeerDecimalCeroPermitido(txtCosto.Texto,
                "El costo debe ser un numero mayor o igual a cero (por ejemplo 185000).");
            input.Fecha = Formato.LeerFechaServidor(txtFecha.Texto,
                "La fecha debe tener el formato yyyy-MM-ddTHH:mm:ss (por ejemplo 2024-06-12T10:30:00).");
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
