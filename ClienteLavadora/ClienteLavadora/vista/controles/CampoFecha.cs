using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using ClienteLavadora.util;

namespace ClienteLavadora.vista.controles
{
    /// <summary>
    /// Selector de fecha con calendario. El contrato del servidor conserva fecha y hora;
    /// cuando se edita un registro se preserva su hora, y en uno nuevo se usa la actual.
    /// </summary>
    public sealed class CampoFecha : DatePicker
    {
        private TimeSpan hora = DateTime.Now.TimeOfDay;

        public CampoFecha()
        {
            SelectedDateFormat = DatePickerFormat.Short;
            Language = XmlLanguage.GetLanguage("es-CO");
            Background = Tema.FondoFecha;
            Foreground = Tema.TextoFecha;
            BorderBrush = Tema.BordeFecha;
            BorderThickness = new System.Windows.Thickness(1);
            Padding = new System.Windows.Thickness(10, 7, 10, 7);
            CalendarStyle = CrearEstiloCalendario();
            SelectedDate = DateTime.Today;
        }

        /// <summary>
        /// El DatePicker estandar pinta su caja de texto interior con el tema de Windows,
        /// ignorando el fondo oscuro del control exterior. Se aplica la paleta al campo
        /// interno despues de crear la plantilla para que el calendario conserve su boton.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            DatePickerTextBox campo = GetTemplateChild("PART_TextBox") as DatePickerTextBox;
            if (campo != null)
            {
                campo.Background = Tema.FondoFecha;
                campo.Foreground = Tema.TextoFecha;
                campo.BorderBrush = Tema.BordeFecha;
                campo.BorderThickness = new System.Windows.Thickness(0);
                campo.CaretBrush = Tema.Cian;
            }
        }

        private static Style CrearEstiloCalendario()
        {
            Style estilo = new Style(typeof(System.Windows.Controls.Calendar));
            estilo.Setters.Add(new System.Windows.Setter(Control.BackgroundProperty, Tema.PanelOscuro));
            estilo.Setters.Add(new System.Windows.Setter(Control.ForegroundProperty, Tema.Texto));
            estilo.Setters.Add(new System.Windows.Setter(Control.BorderBrushProperty, Tema.BordeFecha));

            Style dia = new Style(typeof(CalendarDayButton));
            dia.Setters.Add(new System.Windows.Setter(Control.ForegroundProperty, Tema.Texto));
            dia.Setters.Add(new System.Windows.Setter(Control.BackgroundProperty, Tema.PanelOscuro));
            dia.Setters.Add(new System.Windows.Setter(Control.BorderBrushProperty, Tema.PanelOscuro));
            dia.Setters.Add(new System.Windows.Setter(Control.BorderThicknessProperty, new Thickness(1)));
            dia.Triggers.Add(new Trigger
            {
                Property = CalendarDayButton.IsInactiveProperty,
                Value = true,
                Setters = { new System.Windows.Setter(Control.ForegroundProperty, Tema.TextoSecundario),
                    new System.Windows.Setter(UIElement.OpacityProperty, 0.58) }
            });
            dia.Triggers.Add(new Trigger
            {
                Property = CalendarDayButton.IsMouseOverProperty,
                Value = true,
                Setters = { new System.Windows.Setter(Control.BackgroundProperty, Tema.Acero),
                    new System.Windows.Setter(Control.BorderBrushProperty, Tema.BordeFecha) }
            });
            dia.Triggers.Add(new Trigger
            {
                Property = CalendarDayButton.IsTodayProperty,
                Value = true,
                Setters = { new System.Windows.Setter(Control.BorderBrushProperty, Tema.Cian) }
            });
            dia.Triggers.Add(new Trigger
            {
                Property = CalendarDayButton.IsSelectedProperty,
                Value = true,
                Setters = { new System.Windows.Setter(Control.BackgroundProperty, Tema.Cian),
                    new System.Windows.Setter(Control.ForegroundProperty, Tema.Fondo),
                    new System.Windows.Setter(Control.BorderBrushProperty, Tema.Cian) }
            });
            estilo.Resources.Add(typeof(CalendarDayButton), dia);

            Style navegacion = new Style(typeof(CalendarButton));
            navegacion.Setters.Add(new System.Windows.Setter(Control.ForegroundProperty, Tema.Texto));
            navegacion.Setters.Add(new System.Windows.Setter(Control.BackgroundProperty, Tema.PanelOscuro));
            navegacion.Triggers.Add(new Trigger
            {
                Property = UIElement.IsMouseOverProperty,
                Value = true,
                Setters = { new System.Windows.Setter(Control.BackgroundProperty, Tema.Acero),
                    new System.Windows.Setter(Control.ForegroundProperty, Tema.Cian) }
            });
            estilo.Resources.Add(typeof(CalendarButton), navegacion);
            return estilo;
        }

        public string Texto
        {
            get
            {
                return SelectedDate.HasValue
                    ? Formato.FechaServidor(SelectedDate.Value.Date.Add(hora))
                    : "";
            }
            set
            {
                DateTime fecha;
                if (!string.IsNullOrWhiteSpace(value) && DateTime.TryParse(value,
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha))
                {
                    hora = fecha.TimeOfDay;
                    SelectedDate = fecha.Date;
                }
                else
                {
                    SelectedDate = null;
                }
            }
        }

        public bool SoloLectura
        {
            get { return !IsEnabled; }
            set { IsEnabled = !value; }
        }
    }
}
