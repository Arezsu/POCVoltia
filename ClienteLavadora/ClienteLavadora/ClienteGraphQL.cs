using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using ClienteLavadora.model;

namespace ClienteLavadora
{
    /// <summary>
    /// Unico lugar donde el cliente habla con el servidor GraphQL (Java).
    /// Las ventanas llaman a estos metodos y no arman peticiones por su cuenta.
    /// </summary>
    internal static class ClienteGraphQL
    {
        private const string UrlPorDefecto = "http://localhost:8081/graphql";

        /// <summary>URL leida de App.config (clave UrlServidor).</summary>
        public static string UrlServidor
        {
            get
            {
                string url = ConfigurationManager.AppSettings["UrlServidor"];
                return string.IsNullOrWhiteSpace(url) ? UrlPorDefecto : url.Trim();
            }
        }

        private static GraphQLHttpClient _client;

        private static GraphQLHttpClient Client
        {
            get
            {
                if (_client == null)
                {
                    _client = new GraphQLHttpClient(UrlServidor, new SystemTextJsonSerializer());
                }
                return _client;
            }
        }

        // ---------------- Consultas ----------------

        public static async Task<List<LavadoraData>> ListarLavadorasAsync()
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    query listar {
                        lavadoras {
                            codigo marca precioBase fechaFabricacion capacidadKilos funcionSecado
                        }
                    }"
            };

            var data = await EnviarConsultaAsync<ResponseLavadoras>(request);
            return data.lavadoras ?? new List<LavadoraData>();
        }

        public static async Task<LavadoraData> BuscarPorCodigoAsync(int codigo)
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    query buscar($codigo: Int!) {
                        lavadoraPorCodigo(codigo: $codigo) {
                            codigo marca precioBase fechaFabricacion capacidadKilos funcionSecado
                        }
                    }",
                Variables = new { codigo = codigo }
            };

            var data = await EnviarConsultaAsync<ResponseLavadoraPorCodigo>(request);
            return data.lavadoraPorCodigo;
        }

        public static async Task<List<LavadoraData>> ListarPorFiltroAsync(string marca, bool? funcionSecado)
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    query filtrar($marca: String, $funcionSecado: Boolean) {
                        lavadorasPorFiltro(marca: $marca, funcionSecado: $funcionSecado) {
                            codigo marca precioBase fechaFabricacion capacidadKilos funcionSecado
                        }
                    }",
                Variables = new { marca = marca, funcionSecado = funcionSecado }
            };

            var data = await EnviarConsultaAsync<ResponseLavadorasPorFiltro>(request);
            return data.lavadorasPorFiltro ?? new List<LavadoraData>();
        }

        // ---------------- Mutaciones ----------------

        public static async Task<LavadoraData> AgregarLavadoraAsync(LavadoraInput input)
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    mutation agregar($input: LavadoraInput!) {
                        addLavadora(input: $input) {
                            codigo marca precioBase fechaFabricacion capacidadKilos funcionSecado
                        }
                    }",
                Variables = new { input = input }
            };

            var data = await EnviarMutacionAsync<ResponseAddLavadora>(request);
            return data.addLavadora;
        }

        public static async Task<LavadoraData> ActualizarLavadoraAsync(int codigo, LavadoraUpdateInput input)
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    mutation actualizar($codigo: Int!, $input: LavadoraUpdateInput!) {
                        actualizarLavadora(codigo: $codigo, input: $input) {
                            codigo marca precioBase fechaFabricacion capacidadKilos funcionSecado
                        }
                    }",
                Variables = new { codigo = codigo, input = input }
            };

            var data = await EnviarMutacionAsync<ResponseActualizarLavadora>(request);
            return data.actualizarLavadora;
        }

        public static async Task<bool> EliminarLavadoraAsync(int codigo)
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    mutation eliminar($codigo: Int!) {
                        eliminarLavadora(codigo: $codigo)
                    }",
                Variables = new { codigo = codigo }
            };

            var data = await EnviarMutacionAsync<ResponseEliminarLavadora>(request);
            return data.eliminarLavadora;
        }

        // ---------------- Internos ----------------

        private static async Task<T> EnviarConsultaAsync<T>(GraphQLRequest request)
        {
            GraphQLResponse<T> response;
            try
            {
                response = await Client.SendQueryAsync<T>(request);
            }
            catch (HttpRequestException)
            {
                throw ErrorDeConexion();
            }
            LanzarSiHayErrores(response.Errors);
            return response.Data;
        }

        private static async Task<T> EnviarMutacionAsync<T>(GraphQLRequest request)
        {
            GraphQLResponse<T> response;
            try
            {
                response = await Client.SendMutationAsync<T>(request);
            }
            catch (HttpRequestException)
            {
                throw ErrorDeConexion();
            }
            LanzarSiHayErrores(response.Errors);
            return response.Data;
        }

        private static Exception ErrorDeConexion()
        {
            return new Exception("No se pudo conectar con el servidor GraphQL en " + UrlServidor +
                                 ".\r\nVerifique que el servidor (Java) esté corriendo.");
        }

        private static void LanzarSiHayErrores(GraphQLError[] errores)
        {
            if (errores != null && errores.Length > 0)
            {
                throw new Exception(errores[0].Message);
            }
        }
    }
}
