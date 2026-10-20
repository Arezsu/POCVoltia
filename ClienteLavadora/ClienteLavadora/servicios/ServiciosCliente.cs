using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClienteLavadora.model;

namespace ClienteLavadora.servicios
{
    /// <summary>Servicio del PROGRAMA DE LAVADO: llama a ClienteGraphQL con los metodos de programa.</summary>
    public class ServicioProgramaCliente : IServicioCrud
    {
        public async Task<List<DatoBase>> ListarAsync(string texto, bool? opcion)
        {
            List<ProgramaData> lista = await ClienteGraphQL.ListarProgramasAsync(texto, opcion);
            return lista.Cast<DatoBase>().ToList(); // La lista de hijas pasa a lista del padre
        }

        public async Task<DatoBase> BuscarAsync(int? codigo, string texto)
        {
            return await ClienteGraphQL.BuscarProgramaAsync(codigo, texto);
        }

        public async Task<DatoBase> AgregarAsync(object datos)
        {
            return await ClienteGraphQL.AgregarProgramaAsync((ProgramaInput)datos);
        }

        public async Task<DatoBase> ActualizarAsync(int codigo, object cambios)
        {
            return await ClienteGraphQL.ActualizarProgramaAsync(codigo, (ProgramaUpdateInput)cambios);
        }

        public async Task<bool> EliminarAsync(int codigo)
        {
            return await ClienteGraphQL.EliminarProgramaAsync(codigo);
        }
    }

    /// <summary>Servicio de la LAVADORA: igual al del programa pero con los metodos de lavadora.</summary>
    public class ServicioLavadoraCliente : IServicioCrud
    {
        public async Task<List<DatoBase>> ListarAsync(string texto, bool? opcion)
        {
            List<LavadoraData> lista = await ClienteGraphQL.ListarLavadorasAsync(texto, opcion);
            return lista.Cast<DatoBase>().ToList();
        }

        public async Task<DatoBase> BuscarAsync(int? codigo, string texto)
        {
            return await ClienteGraphQL.BuscarLavadoraAsync(codigo, texto);
        }

        public async Task<DatoBase> AgregarAsync(object datos)
        {
            return await ClienteGraphQL.AgregarLavadoraAsync((LavadoraInput)datos);
        }

        public async Task<DatoBase> ActualizarAsync(int codigo, object cambios)
        {
            return await ClienteGraphQL.ActualizarLavadoraAsync(codigo, (LavadoraUpdateInput)cambios);
        }

        public async Task<bool> EliminarAsync(int codigo)
        {
            return await ClienteGraphQL.EliminarLavadoraAsync(codigo);
        }
    }

    /// <summary>Entrega el servicio que corresponde al tipo (patron "fabrica").</summary>
    public static class FabricaServicios
    {
        public static IServicioCrud Obtener(TipoObjeto tipo)
        {
            if (tipo == TipoObjeto.Lavadora)
            {
                return new ServicioLavadoraCliente();
            }
            return new ServicioProgramaCliente();
        }
    }
}
