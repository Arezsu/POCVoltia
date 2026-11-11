using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClienteLavadora.modelo;
using ClienteLavadora.servicios;
using ClienteLavadora.util;

namespace ClienteLavadora.controlador
{
    /// <summary>
    /// CONTROLADOR de los casos de uso de MANTENIMIENTO (el objeto detalle).
    ///
    /// Es el hermano de ControladorLavadoras y trabaja igual: la vista pide, el
    /// controlador llama al servicio y avisa a los demas cuando algo cambio.
    ///
    /// Cada detalle conserva su relacion con una lavadora y se identifica por su id unico.
    ///
    /// Despues de agregar, actualizar o eliminar se llama a
    /// NotificadorDatos.AvisarDatosCambiaron(). La ventana Listar y la ventana de
    /// Mantenimientos estan suscritas, entonces se recargan solas.
    /// </summary>
    public class ControladorMantenimientos
    {
        private readonly IServicioMantenimientos servicio;

        public ControladorMantenimientos(IServicioMantenimientos servicio)
        {
            if (servicio == null)
            {
                throw new ArgumentNullException("servicio");
            }
            this.servicio = servicio;
        }

        // ====================================================================
        //  Consultas
        // ====================================================================

        /// <summary>Lista todos los mantenimientos; los filtros son opcionales.</summary>
        public async Task<List<MantenimientoData>> ListarAsync(int? codigoLavadora, string tecnico, bool? completado)
        {
            return await servicio.ListarAsync(codigoLavadora, tecnico, completado);
        }

        /// <summary>Busca un mantenimiento por id. Si no existe lanza un mensaje claro.</summary>
        public async Task<MantenimientoData> BuscarAsync(int id)
        {
            MantenimientoData mantenimiento = await servicio.BuscarPorIdAsync(id);
            if (mantenimiento == null)
            {
                throw new ArgumentException(
                    "No existe un mantenimiento con el id " + id + ".");
            }
            return mantenimiento;
        }

        // ====================================================================
        //  Operaciones que cambian datos
        // ====================================================================

        public async Task<MantenimientoData> AgregarAsync(int codigoLavadora, MantenimientoInput input)
        {
            MantenimientoData creado = await servicio.AgregarAsync(codigoLavadora, input);
            NotificadorDatos.AvisarDatosCambiaron();
            return creado;
        }

        public async Task<MantenimientoData> ActualizarAsync(int id, MantenimientoUpdateInput input)
        {
            MantenimientoData actualizado = await servicio.ActualizarAsync(id, input);
            NotificadorDatos.AvisarDatosCambiaron();
            return actualizado;
        }

        public async Task EliminarAsync(int id)
        {
            bool eliminado = await servicio.EliminarAsync(id);
            if (!eliminado)
            {
                throw new Exception("El servidor no pudo eliminar el mantenimiento " + id + ".");
            }
            NotificadorDatos.AvisarDatosCambiaron();
        }

        // ====================================================================
        //  Vista previa con el patron BUILDER (sin red)
        //  Sirve para mostrarle al usuario el resumen de lo que va a guardar.
        // ====================================================================

        public MantenimientoData VistaPrevia(MantenimientoInput input)
        {
            MantenimientoData previa = new ConstructorMantenimiento()
                .ConDescripcion(input.Descripcion)
                .ConTecnico(input.Tecnico)
                .ConCosto(input.Costo)
                .ConFecha(input.Fecha)
                .ConCompletado(input.Completado)
                .Construir();

            return previa;
        }
    }
}
