package com.example.ServidorLavadora;

import com.example.ServidorLavadora.graphql.ObjectIdScalar;
import graphql.language.StringValue;
import org.bson.types.ObjectId;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.assertEquals;

class ObjectIdScalarTest {

    @Test
    void serializaYParseaObjectIdValido() {
        ObjectIdScalar scalar = new ObjectIdScalar();
        ObjectId objectId = new ObjectId("507f1f77bcf86cd799439011");

        assertEquals(objectId.toHexString(), scalar.serialize(objectId));
        assertEquals(objectId, scalar.parseValue(objectId.toHexString()));
        assertEquals(objectId, scalar.parseLiteral(new StringValue(objectId.toHexString())));
    }
}
