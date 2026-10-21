using System.Collections.Generic;
using System.Threading.Tasks;
using ClienteLavadora.modelo;

namespace ClienteLavadora.servicios
{
    /// <summary>
    /// Implementacion real de IServicioMantenimientos: arma las cadenas GraphQL del
    /// objeto DETALLE y las envia por ClienteGraphQL.
    ///
    /// Observe que TODAS las operaciones llevan "codigoLavadora" primero: asi lo pide
    /// el schema porque un mantenimiento siempre pertenece a una lavadora concreta.
    /// Ejemplo real de la mutacion:
    ///     mutation { addMantenimiento(codigoLavadora: 101, input: {...}) { id } }
    /// </summary>
    public class ServicioMantenimientos : IServicioMantenimientos
    {
        private const string CamposMantenimiento =
            "id descripcion tecnico costo fecha completado";

        public async Task<List<MantenimientoData>> ListarAsync(int codigoLavadora, string tecnico, bool? completado)
        {
            string consulta =
                "query listar($codigo: Int!, $tecnico: String, $completado: Boolean) { " +
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

        public async Task<MantenimientoData> BuscarPorIdAsync(int codigoLavadora, int id)
        {
            string consulta =
                "query buscar($codigo: Int!, $id: Int!) { " +
                "mantenimientoPorId(codigoLavadora: $codigo, id: $id) { " + CamposMantenimiento + " } }";

            object variables = new { codigo = codigoLavadora, id = id };

            RespuestaMantenimientoPorId respuesta =
                await ClienteGraphQL.Instancia.ConsultarAsync<RespuestaMantenimientoPorId>(consulta, variables);

            return respuesta == null ? null : respuesta.Mantenimiento;
        }

        public async Task<MantenimientoData> AgregarAsync(int codigoLavadora, MantenimientoInput input)
        {
            string consulta =
                "mutation agregar($codigo: Int!, $input: MantenimientoInput!) { " +
                "addMantenimiento(codigoLavadora: $codigo, input: $input) { " + CamposMantenimiento + " } }";

            object variables = new { codigo = codigoLavadora, input = input };

            RespuestaAgregarMantenimiento respuesta =
                await ClienteGraphQL.Instancia.MutarAsync<RespuestaAgregarMantenimiento>(consulta, variables);

            if (respuesta == null || respuesta.Mantenimiento == null)
            {
                throw new System.Exception("El servidor no devolvio el mantenimiento creado.");
            }
            return respuesta.Mantenimiento;
        }

        public async Task<MantenimientoData> ActualizarAsync(int codigoLavadora, int id, MantenimientoUpdateInput input)
        {
            string consulta =
                "mutation actualizar($codigo: Int!, $id: Int!, $input: MantenimientoUpdateInput!) { " +
                "actualizarMantenimiento(codigoLavadora: $codigo, id: $id, input: $input) { " +
                CamposMantenimiento + " } }";

            object variables = new { codigo = codigoLavadora, id = id, input = input };

            RespuestaActualizarMantenimiento respuesta =
                await ClienteGraphQL.Instancia.MutarAsync<RespuestaActualizarMantenimiento>(consulta, variables);

            if (respuesta == null || respuesta.Mantenimiento == null)
            {
                throw new System.Exception("El servidor no devolvio el mantenimiento actualizado.");
            }
            return respuesta.Mantenimiento;
        }

        public async Task<bool> EliminarAsync(int codigoLavadora, int id)
        {
            string consulta =
                "mutation eliminar($codigo: Int!, $id: Int!) { " +
                "eliminarMantenimiento(codigoLavadora: $codigo, id: $id) }";

            object variables = new { codigo = codigoLavadora, id = id };

            RespuestaEliminarMantenimiento respuesta =
                await ClienteGraphQL.Instancia.MutarAsync<RespuestaEliminarMantenimiento>(consulta, variables);

            return respuesta != null && respuesta.Eliminado;
        }
    }
}
