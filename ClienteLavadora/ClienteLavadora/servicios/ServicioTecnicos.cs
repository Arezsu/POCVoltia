using System.Collections.Generic;
using System.Threading.Tasks;
using ClienteLavadora.modelo;

namespace ClienteLavadora.servicios
{
    public class ServicioTecnicos : IServicioTecnicos
    {
        private const string Campos = "codigo documento nombre especialidad telefono fechaIngreso activo";
        private const string CamposListado = Campos + " codigoLavadora marcaLavadora";
        public async Task<List<TecnicoData>> ListarAsync(string texto, string especialidad, bool? activo)
        {
            const string q = "query($texto:String,$especialidad:String,$activo:Boolean){ tecnicosConLavadoras(texto:$texto,especialidad:$especialidad,activo:$activo){ " + CamposListado + " } }";
            var r = await ClienteGraphQL.Instancia.ConsultarAsync<RespuestaListaTecnicos>(q, new { texto, especialidad, activo });
            return r?.Tecnicos ?? new List<TecnicoData>();
        }
        public async Task<TecnicoData> BuscarAsync(int codigo)
        {
            const string q = "query($codigo:Int!){ tecnicoPorCodigo(codigo:$codigo){ " + Campos + " } }";
            var r = await ClienteGraphQL.Instancia.ConsultarAsync<RespuestaTecnico>(q, new { codigo }); return r?.Tecnico;
        }
        public async Task<TecnicoData> AgregarAsync(TecnicoInput input)
        {
            const string q = "mutation($input:TecnicoInput!){ addTecnico(input:$input){ " + Campos + " } }";
            var r = await ClienteGraphQL.Instancia.MutarAsync<RespuestaAgregarTecnico>(q, new { input }); return r?.Tecnico;
        }
        public async Task<TecnicoData> ActualizarAsync(int codigo, TecnicoInput input)
        {
            const string q = "mutation($codigo:Int!,$input:TecnicoInput!){ actualizarTecnico(codigo:$codigo,input:$input){ " + Campos + " } }";
            var r = await ClienteGraphQL.Instancia.MutarAsync<RespuestaActualizarTecnico>(q, new { codigo, input }); return r?.Tecnico;
        }
        public async Task<bool> EliminarAsync(int codigo)
        {
            const string q = "mutation($codigo:Int!){ eliminarTecnico(codigo:$codigo) }";
            var r = await ClienteGraphQL.Instancia.MutarAsync<RespuestaEliminarTecnico>(q, new { codigo }); return r != null && r.Eliminado;
        }
    }
}
