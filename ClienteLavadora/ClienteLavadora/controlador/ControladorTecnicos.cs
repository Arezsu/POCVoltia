using System.Collections.Generic;
using System.Threading.Tasks;
using ClienteLavadora.modelo;
using ClienteLavadora.servicios;

namespace ClienteLavadora.controlador
{
    public class ControladorTecnicos
    {
        private readonly IServicioTecnicos servicio;
        public ControladorTecnicos(IServicioTecnicos servicio) { this.servicio = servicio; }
        public Task<List<TecnicoData>> ListarAsync(string texto, string especialidad, bool? activo) => servicio.ListarAsync(texto, especialidad, activo);
        public Task<TecnicoData> BuscarAsync(int codigo) => servicio.BuscarAsync(codigo);
        public Task<TecnicoData> AgregarAsync(TecnicoInput input) => servicio.AgregarAsync(input);
        public Task<TecnicoData> ActualizarAsync(int codigo, TecnicoInput input) => servicio.ActualizarAsync(codigo, input);
        public Task<bool> EliminarAsync(int codigo) => servicio.EliminarAsync(codigo);
    }
}
