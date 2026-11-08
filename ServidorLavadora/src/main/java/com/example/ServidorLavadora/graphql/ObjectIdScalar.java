package com.example.ServidorLavadora.graphql;

import graphql.language.StringValue;
import graphql.schema.Coercing;
import graphql.schema.GraphQLScalarType;
import graphql.schema.idl.RuntimeWiring;
import org.bson.types.ObjectId;
import org.springframework.graphql.execution.RuntimeWiringConfigurer;
import org.springframework.stereotype.Component;

@Component
public class ObjectIdScalar implements RuntimeWiringConfigurer {

    @Override
    public void configure(RuntimeWiring.Builder builder) {
        builder.scalar(GraphQLScalarType.newScalar()
                .name("ObjectId")
                .description("MongoDB ObjectId")
                .coercing(new Coercing<>() {
                    @Override
                    public Object serialize(Object dataFetcherResult) {
                        if (dataFetcherResult == null) {
                            return null;
                        }
                        if (dataFetcherResult instanceof ObjectId objectId) {
                            return ObjectIdScalar.this.serialize(objectId);
                        }
                        throw new IllegalArgumentException("El valor debe ser un org.bson.types.ObjectId");
                    }

                    @Override
                    public Object parseValue(Object input) {
                        if (input == null) {
                            return null;
                        }
                        if (input instanceof ObjectId objectId) {
                            return objectId;
                        }
                        if (input instanceof String texto) {
                            return ObjectIdScalar.this.parseValue(texto);
                        }
                        throw new IllegalArgumentException("Mongo ObjectId invalido: " + input);
                    }

                    @Override
                    public Object parseLiteral(Object input) {
                        if (input instanceof StringValue stringValue) {
                            return ObjectIdScalar.this.parseLiteral(stringValue);
                        }
                        throw new IllegalArgumentException("ObjectId debe recibirse como hexadecimal");
                    }
                })
                .build());
    }

    public String serialize(ObjectId objectId) {
        if (objectId == null) {
            return null;
        }
        return objectId.toHexString();
    }

    public ObjectId parseValue(String texto) {
        if (texto == null) {
            return null;
        }
        if (!ObjectId.isValid(texto)) {
            throw new IllegalArgumentException("Mongo ObjectId invalido: " + texto);
        }
        return new ObjectId(texto);
    }

    public ObjectId parseLiteral(StringValue literal) {
        if (literal == null) {
            return null;
        }
        return parseValue(literal.getValue());
    }
}
