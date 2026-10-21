using System.Collections.Generic;
using System.Threading.Tasks;
using ClienteLavadora.modelo;

namespace ClienteLavadora.servicios
{
    /// <summary>
    /// Implementacion real de IServicioLavadoras: arma las cadenas GraphQL de cada
    /// caso de uso y se las entrega a ClienteGraphQL.
    ///
    /// POR QUE AQUI Y NO EN LA VENTANA:
    /// si la ventana escribiera "query lavadorasPorFiltro(...)", la interfaz quedaria
    /// casada con GraphQL. Manana, si el servidor cambia a REST, solo se cambia este
    /// archivo y las ventanas ni se enteran.
    ///
    /// RESPONSABILIDAD UNICA: esta clase solo sabe de lavadoras (maestro). Los
    /// mantenimientos tienen su propia clase (ServicioMantenimientos).
    ///
    /// Los campos que se piden estan en la constante CamposLavadora: se escriben una
    /// vez y se reutilizan en todas las consultas, asi nunca queda una consulta
    /// pidiendo menos datos que otra.
    /// </summary>
    public class ServicioLavadoras : IServicioLavadoras
    {
        // Todos los campos del tipo Lavadora, incluida la coleccion de detalle.
        private const string CamposLavadora =
            "codigo marca precioBase fechaFabricacion capacidadCargaKilos velocidadCentrifugadoRPM " +
            "tecnologiaInverter minutosLavado precioFinal cantidadMantenimientos " +
            "mantenimientosPendientes costoMantenimientos " +
            "mantenimientos { id descripcion tecnico costo fecha completado }";

        public async Task<List<LavadoraData>> ListarAsync(string marca, bool? tecnologiaInverter, bool? conMantenimientos)
        {
            string consulta =
                "query listar($marca: String, $inverter: Boolean, $con: Boolean) { " +
                "lavadorasPorFiltro(marca: $marca, tecnologiaInverter: $inverter, conMantenimientos: $con) { " +
                CamposLavadora + " } }";

            // Los nombres de las propiedades de este objeto deben ser iguales a las
            // variables de la consulta ($marca, $inverter, $con).
            object variables = new { marca = marca, inverter = tecnologiaInverter, con = conMantenimientos };

            RespuestaListaLavadoras respuesta =
                await ClienteGraphQL.Instancia.ConsultarAsync<RespuestaListaLavadoras>(consulta, variables);

            if (respuesta == null || respuesta.Lavadoras == null)
            {
                return new List<LavadoraData>();
            }
            return respuesta.Lavadoras;
        }

        public async Task<LavadoraData> BuscarPorCriteriosAsync(int? codigo, string marca)
        {
            string consulta =
                "query buscar($codigo: Int, $marca: String) { " +
                "lavadoraPorCriterios(codigo: $codigo, marca: $marca) { " + CamposLavadora + " } }";

            object variables = new { codigo = codigo, marca = marca };

            RespuestaLavadoraPorCriterios respuesta =
                await ClienteGraphQL.Instancia.ConsultarAsync<RespuestaLavadoraPorCriterios>(consulta, variables);

            return respuesta == null ? null : respuesta.Lavadora;
        }

        public async Task<LavadoraData> AgregarAsync(LavadoraInput input)
        {
            string consulta =
                "mutation agregar($input: LavadoraInput!) { " +
                "addLavadora(input: $input) { " + CamposLavadora + " } }";

            object variables = new { input = input };

            RespuestaAgregarLavadora respuesta =
                await ClienteGraphQL.Instancia.MutarAsync<RespuestaAgregarLavadora>(consulta, variables);

            if (respuesta == null || respuesta.Lavadora == null)
            {
                throw new System.Exception("El servidor no devolvio la lavadora creada.");
            }
            return respuesta.Lavadora;
        }

        public async Task<LavadoraData> ActualizarAsync(int codigo, LavadoraUpdateInput input)
        {
            string consulta =
                "mutation actualizar($codigo: Int!, $input: LavadoraUpdateInput!) { " +
                "actualizarLavadora(codigo: $codigo, input: $input) { " + CamposLavadora + " } }";

            object variables = new { codigo = codigo, input = input };

            RespuestaActualizarLavadora respuesta =
                await ClienteGraphQL.Instancia.MutarAsync<RespuestaActualizarLavadora>(consulta, variables);

            if (respuesta == null || respuesta.Lavadora == null)
            {
                throw new System.Exception("El servidor no devolvio la lavadora actualizada.");
            }
            return respuesta.Lavadora;
        }

        public async Task<bool> EliminarAsync(int codigo)
        {
            string consulta = "mutation eliminar($codigo: Int!) { eliminarLavadora(codigo: $codigo) }";
            object variables = new { codigo = codigo };

            RespuestaEliminarLavadora respuesta =
                await ClienteGraphQL.Instancia.MutarAsync<RespuestaEliminarLavadora>(consulta, variables);

            return respuesta != null && respuesta.Eliminada;
        }

        public async Task<int> VersionDatosAsync()
        {
            string consulta = "query version { versionDatos }";

            RespuestaVersionDatos respuesta =
                await ClienteGraphQL.Instancia.ConsultarAsync<RespuestaVersionDatos>(consulta, new { });

            return respuesta == null ? 0 : respuesta.Version;
        }
    }
}
