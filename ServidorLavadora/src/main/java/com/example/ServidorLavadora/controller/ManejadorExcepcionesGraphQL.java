package com.example.ServidorLavadora.controller;

import graphql.GraphQLError;
import graphql.GraphqlErrorBuilder;
import graphql.schema.DataFetchingEnvironment;
import org.springframework.graphql.execution.DataFetcherExceptionResolverAdapter;
import org.springframework.graphql.execution.ErrorType;
import org.springframework.stereotype.Component;

import java.util.Map;

/**
 * Convierte las excepciones de negocio (codigo repetido, lavadora no
 * encontrada, datos invalidos) en errores GraphQL con un mensaje claro
 * para que el cliente se lo muestre al usuario.
 */
@Component // Atrapa errores y responde claro
public class ManejadorExcepcionesGraphQL extends DataFetcherExceptionResolverAdapter {

    @Override
    protected GraphQLError resolveToSingleError(Throwable ex, DataFetchingEnvironment env) {
        boolean esValidacion = ex instanceof IllegalArgumentException;
        String mensaje = esValidacion
                ? (ex.getMessage() == null ? "Los datos enviados no son válidos." : ex.getMessage())
                : "Ocurrió un error interno al procesar la operación.";
        String codigo = esValidacion ? "VALIDACION" : "ERROR_INTERNO";
        return GraphqlErrorBuilder.newError(env)
                .message(mensaje)
                .errorType(esValidacion ? ErrorType.BAD_REQUEST : ErrorType.INTERNAL_ERROR)
                .extensions(Map.of("codigo", codigo))
                .build();
    }
}
