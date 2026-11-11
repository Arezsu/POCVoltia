using System.Collections.Generic;
using System.Threading.Tasks;
using ClienteLavadora.modelo;

namespace ClienteLavadora.servicios
{
    /// <summary>
    /// Implementacion real de IServicioMantenimientos: arma las cadenas GraphQL del
    /// objeto DETALLE y las envia por ClienteGraphQL.
    ///
    /// Las operaciones sobre un mantenimiento existente se hacen con su id.
    /// Al crearlo, el input incluye el id elegido y el codigo de la lavadora asociada.
    /// </summary>
    public class ServicioMantenimientos : IServicioMantenimientos
    {
        private const string CamposMantenimiento =
            "codigoLavadora marcaLavadora id descripcion tecnico costo fecha completado";

        public async Task<List<MantenimientoData>> ListarAsync(int? codigoLavadora, string tecnico, bool? completado)
        {
            string consulta =
                "query listar($codigo: Int, $tecnico: String, $completado: Boolean) { " +
                "mantenimientosPorFiltro(codigoLavadora: $codigo, tecnico: $tecnico, completado: $completado) { " +
                CamposMantenimiento + " } }";

            object variables = new { codigo = codigoLavadora, tecnico = tecnico, completado = completado };

            RespuestaListaMantenimientos respuesta =
                await ClienteGraphQL.Instancia.ConsultarAsync<RespuestaListaMantenimientos>(consulta, variables);

            if (respuesta == null || respuesta.Mantenimientos == null)
            {
                return new List<MantenimientoData>();
            }
            return respuesta.Mantenimientos;
        }

        public async Task<MantenimientoData> BuscarPorIdAsync(int id)
        {
            string consulta =
                "query buscar($id: Int!) { " +
                "mantenimientoPorId(id: $id) { " + CamposMantenimiento + " } }";

            RespuestaMantenimientoPorId respuesta =
                await ClienteGraphQL.Instancia.ConsultarAsync<RespuestaMantenimientoPorId>(consulta, new { id = id });

            return respuesta == null ? null : respuesta.Mantenimiento;
        }

        public async Task<MantenimientoData> AgregarAsync(int codigoLavadora, MantenimientoInput input)
        {
            string consulta =
                "mutation agregar($input: MantenimientoInput!) { " +
                "addMantenimiento(input: $input) { " + CamposMantenimiento + " } }";

            input.CodigoLavadora = codigoLavadora;
            object variables = new { input = input };

            RespuestaAgregarMantenimiento respuesta =
                await ClienteGraphQL.Instancia.MutarAsync<RespuestaAgregarMantenimiento>(consulta, variables);

            if (respuesta == null || respuesta.Mantenimiento == null)
            {
                throw new System.Exception("El servidor no devolvio el mantenimiento creado.");
            }
            return respuesta.Mantenimiento;
        }

        public async Task<MantenimientoData> ActualizarAsync(int id, MantenimientoUpdateInput input)
        {
            string consulta =
                "mutation actualizar($id: Int!, $input: MantenimientoUpdateInput!) { " +
                "actualizarMantenimiento(id: $id, input: $input) { " +
                CamposMantenimiento + " } }";

            object variables = new { id = id, input = input };

            RespuestaActualizarMantenimiento respuesta =
                await ClienteGraphQL.Instancia.MutarAsync<RespuestaActualizarMantenimiento>(consulta, variables);

            if (respuesta == null || respuesta.Mantenimiento == null)
            {
                throw new System.Exception("El servidor no devolvio el mantenimiento actualizado.");
            }
            return respuesta.Mantenimiento;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            string consulta =
                "mutation eliminar($id: Int!) { " +
                "eliminarMantenimiento(id: $id) }";

            object variables = new { id = id };

            RespuestaEliminarMantenimiento respuesta =
                await ClienteGraphQL.Instancia.MutarAsync<RespuestaEliminarMantenimiento>(consulta, variables);

            return respuesta != null && respuesta.Eliminado;
        }
    }
}
