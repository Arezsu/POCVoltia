using System.Collections.Generic;
using System.Threading.Tasks;
using ClienteLavadora.model;

namespace ClienteLavadora.servicios
{
    /// <summary>
    /// Lo que se puede hacer con un objeto (el CRUD). Es una "interface": un contrato.
    /// El programa y la lavadora lo cumplen cada uno a su manera, asi las ventanas
    /// funcionan igual para los dos sin saber cual es (principio de inversion de
    /// dependencias: las ventanas dependen del contrato y no de una clase concreta).
    /// </summary>
    public interface IServicioCrud
    {
        /// <summary>
        /// Lista con filtro (lo hace el servidor). "texto" es el primer filtro (nombre o marca)
        /// y "opcion" el segundo (agua caliente o inverter). null = sin filtrar.
        /// </summary>
        Task<List<DatoBase>> ListarAsync(string texto, bool? opcion);

        /// <summary>Consulta UN objeto por codigo y/o por el segundo criterio (nombre o marca). null si no existe.</summary>
        Task<DatoBase> BuscarAsync(int? codigo, string texto);

        /// <summary>Crea uno nuevo. "datos" es un ProgramaInput o un LavadoraInput segun el tipo.</summary>
        Task<DatoBase> AgregarAsync(object datos);

        /// <summary>Cambia uno existente. "cambios" es un ProgramaUpdateInput o un LavadoraUpdateInput.</summary>
        Task<DatoBase> ActualizarAsync(int codigo, object cambios);

        Task<bool> EliminarAsync(int codigo);
    }
}
