using System.Collections.Generic;
using System.Threading.Tasks;
using ClienteLavadora.modelo;

namespace ClienteLavadora.servicios
{
    /// <summary>
    /// Contrato de los servicios de MANTENIMIENTO (el objeto DETALLE).
    ///
    /// Cada mantenimiento pertenece a una lavadora, pero su id es unico en el sistema;
    /// por eso se puede consultar, actualizar o eliminar directamente con ese id.
    /// </summary>
    public interface IServicioMantenimientos
    {
        /// <summary>Lista todos los mantenimientos, con filtros opcionales.</summary>
        Task<List<MantenimientoData>> ListarAsync(int? codigoLavadora, string tecnico, bool? completado);

        /// <summary>Busca UN mantenimiento por su id. Devuelve null si no existe.</summary>
        Task<MantenimientoData> BuscarPorIdAsync(int id);

        /// <summary>Agrega un mantenimiento con el id elegido por el usuario.</summary>
        Task<MantenimientoData> AgregarAsync(int codigoLavadora, MantenimientoInput input);

        /// <summary>Actualiza un mantenimiento. Los campos en null no se cambian.</summary>
        Task<MantenimientoData> ActualizarAsync(int id, MantenimientoUpdateInput input);

        /// <summary>Elimina un mantenimiento de su lavadora. Devuelve true si lo borro.</summary>
        Task<bool> EliminarAsync(int id);
    }
}
