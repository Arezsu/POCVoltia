using System.Collections.Generic;
using System.Threading.Tasks;
using ClienteLavadora.modelo;

namespace ClienteLavadora.servicios
{
    public interface IServicioTecnicos
    {
        Task<List<TecnicoData>> ListarAsync(string texto, string especialidad, bool? activo);
        Task<TecnicoData> BuscarAsync(int codigo);
        Task<TecnicoData> AgregarAsync(TecnicoInput input);
        Task<TecnicoData> ActualizarAsync(int codigo, TecnicoInput input);
        Task<bool> EliminarAsync(int codigo);
    }
}
