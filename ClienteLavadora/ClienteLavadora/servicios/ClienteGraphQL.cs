using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using ClienteLavadora.util;

namespace ClienteLavadora.servicios
{
    /// <summary>
    /// EL UNICO lugar de todo el cliente que habla con el servidor GraphQL (Java).
    ///
    /// Es un SINGLETON: existe UNA sola instancia en toda la aplicacion y por lo
    /// tanto UN solo GraphQLHttpClient (que a su vez reutiliza un HttpClient con su
    /// grupo de conexiones). Esto importa porque:
    ///  - cada GraphQLHttpClient crea conexiones nuevas; tener muchos desperdicia
    ///    sockets y puede agotar los puertos del sistema;
    ///  - todos comparten la misma configuracion (direccion, serializador JSON).
    ///
    /// Se implementa con el "doble chequeo con candado": la primera vez que alguien
    /// pide Instancia se crea, y las demas veces se reutiliza. El candado (lock)
    /// evita que dos hilos creen dos clientes al mismo tiempo.
    ///
    /// Todos los metodos son async: mandan la peticion, devuelven el control a la
    /// ventana y avisan cuando llega la respuesta, sin congelar la interfaz.
    ///
    /// NOTA IMPORTANTE SOBRE LAS INTERFACES: los servicios (IServicioLavadoras e
    /// IServicioMantenimientos) NO construyen peticiones por su cuenta; usan estos
    /// dos metodos genericos. El tipo T dice que forma tiene la respuesta.
    /// </summary>
    public sealed class ClienteGraphQL
    {
        private const string UrlPorDefecto = "http://localhost:8081/graphql";

        private static readonly object candado = new object();
        private static ClienteGraphQL instancia;

        private readonly GraphQLHttpClient cliente;

        /// <summary>Punto de acceso al Singleton.</summary>
        public static ClienteGraphQL Instancia
        {
            get
            {
                if (instancia == null)
                {
                    lock (candado)
                    {
                        if (instancia == null)
                        {
                            instancia = new ClienteGraphQL();
                        }
                    }
                }
                return instancia;
            }
        }

        /// <summary>Direccion del servidor. Se lee de App.config (clave UrlServidor).</summary>
        public string UrlServidor
        {
            get
            {
                string url = ConfigurationManager.AppSettings["UrlServidor"];
                if (string.IsNullOrWhiteSpace(url))
                {
                    return UrlPorDefecto;
                }
                return url.Trim();
            }
        }

        /// <summary>
        /// Constructor privado: nadie de afuera puede hacer "new ClienteGraphQL()".
        /// Esa es la regla del Singleton, se usa siempre ClienteGraphQL.Instancia.
        /// </summary>
        private ClienteGraphQL()
        {
            cliente = new GraphQLHttpClient(UrlServidor, new SystemTextJsonSerializer());
        }

        // ====================================================================
        //  Los dos unicos metodos que salen a la red
        // ====================================================================

        /// <summary>
        /// Envia una CONSULTA (query) y devuelve la respuesta ya convertida a T.
        /// T es un tipo "envoltorio" de Respuestas.cs, por ejemplo RespuestaListaLavadoras.
        /// </summary>
        public async Task<T> ConsultarAsync<T>(string consulta, object variables) where T : class
        {
            return await EnviarAsync<T>(consulta, variables, false).ConfigureAwait(false);
        }

        /// <summary>
        /// Envia una MUTACION (mutation: insertar, actualizar o eliminar) y devuelve la
        /// respuesta ya convertida a T.
        /// </summary>
        public async Task<T> MutarAsync<T>(string consulta, object variables) where T : class
        {
            return await EnviarAsync<T>(consulta, variables, true).ConfigureAwait(false);
        }

        // ====================================================================
        //  Trabajo interno
        // ====================================================================

        private async Task<T> EnviarAsync<T>(string consulta, object variables, bool esMutacion) where T : class
        {
            GraphQLRequest peticion = new GraphQLRequest();
            peticion.Query = consulta;
            peticion.Variables = variables;

            GraphQLResponse<T> respuesta;
            NotificadorDatos.AvisarTrabajando(true); // El tambor acelera y la pantalla dice PROCESANDO
            try
            {
                if (esMutacion)
                {
                    respuesta = await cliente.SendMutationAsync<T>(peticion).ConfigureAwait(false);
                }
                else
                {
                    respuesta = await cliente.SendQueryAsync<T>(peticion).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                if (EsProblemaDeRed(ex))
                {
                    NotificadorDatos.AvisarConexion(false);
                    throw new Exception(
                        "No se pudo conectar con el servidor GraphQL en " + UrlServidor + "." +
                        Environment.NewLine + "Verifique que el servidor (Java) este corriendo.");
                }
                throw; // Otro error distinto: se deja subir tal cual para no esconderlo
            }
            finally
            {
                NotificadorDatos.AvisarTrabajando(false);
            }

            NotificadorDatos.AvisarConexion(true);
            LanzarSiHayErrores(respuesta.Errors);
            return respuesta.Data;
        }

        /// <summary>Distingue un problema de red (servidor apagado) de un error de negocio.</summary>
        private static bool EsProblemaDeRed(Exception ex)
        {
            return ex is HttpRequestException
                || ex is TaskCanceledException
                || ex is SocketException
                || ex is System.Net.WebException;
        }

        /// <summary>
        /// Si el servidor respondio con un error de negocio (codigo repetido, dato
        /// invalido...) se lanza una excepcion con ESE MISMO mensaje en espanol, para
        /// que la ventana se lo muestre al usuario sin inventar textos nuevos.
        /// </summary>
        private static void LanzarSiHayErrores(GraphQLError[] errores)
        {
            if (errores != null && errores.Length > 0)
            {
                string mensaje = errores[0].Message;
                if (string.IsNullOrWhiteSpace(mensaje))
                {
                    mensaje = "El servidor respondio con un error.";
                }
                throw new Exception(mensaje);
            }
        }
    }
}
