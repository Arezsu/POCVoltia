using System.Collections.Generic;
using System.Threading.Tasks;
using ClienteLavadora.modelo;

namespace ClienteLavadora.servicios
{
    /// <summary>
    /// Contrato de los servicios de MANTENIMIENTO (el objeto DETALLE).
    ///
    /// Fijese que TODOS los metodos reciben el codigo de la lavadora: un mantenimiento
    /// no existe por fuera de su lavadora (relacion maestro-detalle). El id solo es
    /// unico dentro de esa lavadora, por eso hay que decir siempre a cual pertenece.
    /// </summary>
    public interface IServicioMantenimientos
    {
        /// <summary>
        /// Lista los mantenimientos de una lavadora aplicando los filtros en el SERVIDOR.
        ///  - tecnico: texto que debe aparecer en el nombre del tecnico (null = todos)
        ///  - completado: true = terminados, false = pendientes, null = todos
        /// </summary>
        Task<List<MantenimientoData>> ListarAsync(int codigoLavadora, string tecnico, bool? completado);

        /// <summary>Busca UN mantenimiento por su id dentro de la lavadora. Devuelve null si no existe.</summary>
        Task<MantenimientoData> BuscarPorIdAsync(int codigoLavadora, int id);

        /// <summary>Agrega un mantenimiento. El id lo calcula el servidor.</summary>
        Task<MantenimientoData> AgregarAsync(int codigoLavadora, MantenimientoInput input);

        /// <summary>Actualiza un mantenimiento. Los campos en null no se cambian.</summary>
        Task<MantenimientoData> ActualizarAsync(int codigoLavadora, int id, MantenimientoUpdateInput input);

        /// <summary>Elimina un mantenimiento de su lavadora. Devuelve true si lo borro.</summary>
        Task<bool> EliminarAsync(int codigoLavadora, int id);
    }
}
