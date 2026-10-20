using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using ClienteLavadora.model;
using ClienteLavadora.util;

namespace ClienteLavadora.servicios
{
    /// <summary>
    /// Es el UNICO lugar donde el cliente (C#) habla con el servidor GraphQL (Java).
    /// Las ventanas no arman peticiones por su cuenta: usan los servicios
    /// (ServiciosCliente.cs) y estos llaman a esta clase.
    ///
    /// Es un Singleton: existe una sola instancia del cliente HTTP para toda la aplicacion.
    /// Todos los metodos son "async": mandan la peticion por la red y esperan la
    /// respuesta sin congelar la ventana.
    /// </summary>
    public static class ClienteGraphQL
    {
        private const string UrlPorDefecto = "http://localhost:8081/graphql";

        // Los campos que se piden en cada consulta (se escriben una sola vez aqui)
        private const string CamposPrograma =
            "codigo nombre minMinutos maxMinutos consumoAguaLitros fechaCreacion aguaCaliente cantidadLavadoras";

        private const string CamposLavadora =
            "codigo marca precioBase fechaFabricacion capacidadCargaKilos velocidadCentrifugadoRPM " +
            "tecnologiaInverter minutosLavado precioFinal programaLavado { codigo nombre }";

        /// <summary>Direccion del servidor. Se lee de App.config (clave UrlServidor).</summary>
        public static string UrlServidor
        {
            get
            {
                string url = ConfigurationManager.AppSettings["UrlServidor"];
                return string.IsNullOrWhiteSpace(url) ? UrlPorDefecto : url.Trim();
            }
        }

        private static GraphQLHttpClient cliente;

        // Se crea la primera vez que se necesita y despues se reutiliza (Singleton)
        private static GraphQLHttpClient Cliente
        {
            get
            {
                if (cliente == null)
                {
                    cliente = new GraphQLHttpClient(UrlServidor, new SystemTextJsonSerializer());
                }
                return cliente;
            }
        }

        // ====================================================================
        //  PROGRAMAS DE LAVADO (maestro)
        // ====================================================================

        public static async Task<List<ProgramaData>> ListarProgramasAsync(string nombre, bool? aguaCaliente) // Promesa de resultado
        {
            GraphQLRequest peticion = new GraphQLRequest
            {
                Query = "query filtrar($nombre: String, $caliente: Boolean) { " +
                        "programasPorFiltro(nombre: $nombre, aguaCaliente: $caliente) { " + CamposPrograma + " } }",
                Variables = new { nombre = nombre, caliente = aguaCaliente }
            };

            ResponseProgramas datos = await EnviarConsultaAsync<ResponseProgramas>(peticion); // Espera la respuesta
            return datos.Programas ?? new List<ProgramaData>(); // Si es nulo, lista vacia
        }

        /// <summary>Consulta UN programa por uno o dos criterios (codigo y/o nombre). Devuelve null si no existe.</summary>
        public static async Task<ProgramaData> BuscarProgramaAsync(int? codigo, string nombre)
        {
            GraphQLRequest peticion = new GraphQLRequest
            {
                Query = "query buscar($codigo: Int, $nombre: String) { " +
                        "programaPorCriterios(codigo: $codigo, nombre: $nombre) { " + CamposPrograma + " } }",
                Variables = new { codigo = codigo, nombre = nombre }
            };

            ResponseProgramaPorCriterios datos = await EnviarConsultaAsync<ResponseProgramaPorCriterios>(peticion);
            return datos.Programa;
        }

        public static async Task<ProgramaData> AgregarProgramaAsync(ProgramaInput input)
        {
            GraphQLRequest peticion = new GraphQLRequest
            {
                Query = "mutation agregar($input: ProgramaInput!) { addPrograma(input: $input) { " + CamposPrograma + " } }",
                Variables = new { input = input }
            };

            ResponseAddPrograma datos = await EnviarMutacionAsync<ResponseAddPrograma>(peticion);
            return datos.Programa;
        }

        public static async Task<ProgramaData> ActualizarProgramaAsync(int codigo, ProgramaUpdateInput input)
        {
            GraphQLRequest peticion = new GraphQLRequest
            {
                Query = "mutation actualizar($codigo: Int!, $input: ProgramaUpdateInput!) { " +
                        "actualizarPrograma(codigo: $codigo, input: $input) { " + CamposPrograma + " } }",
                Variables = new { codigo = codigo, input = input }
            };

            ResponseActualizarPrograma datos = await EnviarMutacionAsync<ResponseActualizarPrograma>(peticion);
            return datos.Programa;
        }

        public static async Task<bool> EliminarProgramaAsync(int codigo)
        {
            GraphQLRequest peticion = new GraphQLRequest
            {
                Query = "mutation eliminar($codigo: Int!) { eliminarPrograma(codigo: $codigo) }",
                Variables = new { codigo = codigo }
            };

            ResponseEliminarPrograma datos = await EnviarMutacionAsync<ResponseEliminarPrograma>(peticion);
            return datos.Eliminado;
        }

        // ====================================================================
        //  LAVADORAS (detalle)
        // ====================================================================

        public static async Task<List<LavadoraData>> ListarLavadorasAsync(string marca, bool? tecnologiaInverter)
        {
            GraphQLRequest peticion = new GraphQLRequest
            {
                Query = "query filtrar($marca: String, $inverter: Boolean) { " +
                        "lavadorasPorFiltro(marca: $marca, tecnologiaInverter: $inverter) { " + CamposLavadora + " } }",
                Variables = new { marca = marca, inverter = tecnologiaInverter }
            };

            ResponseLavadoras datos = await EnviarConsultaAsync<ResponseLavadoras>(peticion);
            return datos.Lavadoras ?? new List<LavadoraData>();
        }

        /// <summary>Consulta UNA lavadora por uno o dos criterios (codigo y/o marca). Devuelve null si no existe.</summary>
        public static async Task<LavadoraData> BuscarLavadoraAsync(int? codigo, string marca)
        {
            GraphQLRequest peticion = new GraphQLRequest
            {
                Query = "query buscar($codigo: Int, $marca: String) { " +
                        "lavadoraPorCriterios(codigo: $codigo, marca: $marca) { " + CamposLavadora + " } }",
                Variables = new { codigo = codigo, marca = marca }
            };

            ResponseLavadoraPorCriterios datos = await EnviarConsultaAsync<ResponseLavadoraPorCriterios>(peticion);
            return datos.Lavadora;
        }

        public static async Task<LavadoraData> AgregarLavadoraAsync(LavadoraInput input)
        {
            GraphQLRequest peticion = new GraphQLRequest
            {
                Query = "mutation agregar($input: LavadoraInput!) { addLavadora(input: $input) { " + CamposLavadora + " } }",
                Variables = new { input = input }
            };

            ResponseAddLavadora datos = await EnviarMutacionAsync<ResponseAddLavadora>(peticion);
            return datos.Lavadora;
        }

        public static async Task<LavadoraData> ActualizarLavadoraAsync(int codigo, LavadoraUpdateInput input)
        {
            GraphQLRequest peticion = new GraphQLRequest
            {
                Query = "mutation actualizar($codigo: Int!, $input: LavadoraUpdateInput!) { " +
                        "actualizarLavadora(codigo: $codigo, input: $input) { " + CamposLavadora + " } }",
                Variables = new { codigo = codigo, input = input }
            };

            ResponseActualizarLavadora datos = await EnviarMutacionAsync<ResponseActualizarLavadora>(peticion);
            return datos.Lavadora;
        }

        public static async Task<bool> EliminarLavadoraAsync(int codigo)
        {
            GraphQLRequest peticion = new GraphQLRequest
            {
                Query = "mutation eliminar($codigo: Int!) { eliminarLavadora(codigo: $codigo) }",
                Variables = new { codigo = codigo }
            };

            ResponseEliminarLavadora datos = await EnviarMutacionAsync<ResponseEliminarLavadora>(peticion);
            return datos.Eliminada;
        }

        // ====================================================================
        //  Metodos internos
        // ====================================================================

        // <T> = "tipo generico": el mismo metodo sirve para cualquier tipo de respuesta
        private static async Task<T> EnviarConsultaAsync<T>(GraphQLRequest peticion) // Tipo generico
        {
            GraphQLResponse<T> respuesta;
            Notificador.CambiarTrabajando(true); // La perilla empieza a girar
            try
            {
                respuesta = await Cliente.SendQueryAsync<T>(peticion);
            }
            catch (HttpRequestException)
            {
                // Pasa cuando el servidor esta apagado o la direccion esta mal
                throw ErrorDeConexion();
            }
            finally
            {
                Notificador.CambiarTrabajando(false); // y se detiene
            }
            LanzarSiHayErrores(respuesta.Errors);
            return respuesta.Data;
        }

        private static async Task<T> EnviarMutacionAsync<T>(GraphQLRequest peticion)
        {
            GraphQLResponse<T> respuesta;
            Notificador.CambiarTrabajando(true);
            try
            {
                respuesta = await Cliente.SendMutationAsync<T>(peticion);
            }
            catch (HttpRequestException)
            {
                throw ErrorDeConexion();
            }
            finally
            {
                Notificador.CambiarTrabajando(false);
            }
            LanzarSiHayErrores(respuesta.Errors);
            return respuesta.Data;
        }

        private static Exception ErrorDeConexion()
        {
            return new Exception("No se pudo conectar con el servidor GraphQL en " + UrlServidor +
                                 ".\nVerifique que el servidor (Java) esté corriendo.");
        }

        /// <summary>
        /// Si el servidor respondio con un error (codigo repetido, dato invalido, etc.)
        /// se lanza una excepcion con ese mismo mensaje para mostrarselo al usuario.
        /// </summary>
        private static void LanzarSiHayErrores(GraphQLError[] errores)
        {
            if (errores != null && errores.Length > 0)
            {
                throw new Exception(errores[0].Message);
            }
        }
    }
}
