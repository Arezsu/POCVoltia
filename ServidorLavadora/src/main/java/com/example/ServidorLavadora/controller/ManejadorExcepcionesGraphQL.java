package com.example.ServidorLavadora.controller;

import graphql.GraphQLError;
import graphql.GraphqlErrorBuilder;
import graphql.schema.DataFetchingEnvironment;
import org.springframework.graphql.execution.DataFetcherExceptionResolverAdapter;
import org.springframework.stereotype.Component;

/**
 * Convierte las excepciones de negocio (codigo repetido, lavadora no
 * encontrada, datos invalidos) en errores GraphQL con un mensaje claro
 * para que el cliente se lo muestre al usuario.
 */
@Component
public class ManejadorExcepcionesGraphQL extends DataFetcherExceptionResolverAdapter {

    @Override
    protected GraphQLError resolveToSingleError(Throwable ex, DataFetchingEnvironment env) {
        String mensaje = (ex instanceof IllegalArgumentException)
                ? ex.getMessage()
                : "Error interno del servidor: " + ex.getMessage();
        return GraphqlErrorBuilder.newError(env)
                .message(mensaje)
                .build();
    }
}
