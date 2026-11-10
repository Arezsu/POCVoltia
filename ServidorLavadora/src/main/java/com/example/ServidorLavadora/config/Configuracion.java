package com.example.ServidorLavadora.config;

import com.example.ServidorLavadora.repositorio.AlmacenDatos;
import com.example.ServidorLavadora.repositorio.RegistroCambios;
import com.example.ServidorLavadora.repositorio.RepositorioMemoria;
import com.example.ServidorLavadora.repositorio.RepositorioMongo;
import com.mongodb.client.MongoClient;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import org.springframework.data.mongodb.core.MongoTemplate;

/**
 * Aqui se arma lo que Spring le entrega a los servicios.
 * Se entrega el Singleton del repositorio (como AlmacenDatos) con el observador
 * de registro ya conectado.
 */
@Configuration // Clase de configuracion
public class Configuracion {

    @Bean // Spring guarda este objeto y lo entrega a quien lo pida
    public AlmacenDatos almacenDatos(@Value("${voltia.almacen:mongo}") String tipoAlmacen,
                                    MongoTemplate mongoTemplate,
                                    MongoClient mongoClient) {
        if ("mongo".equalsIgnoreCase(tipoAlmacen)) {
            // Fijar la base de datos de la aplicacion evita que una propiedad
            // externa (por ejemplo, SPRING_DATA_MONGODB_DATABASE=test en el IDE)
            // desvie las operaciones a otra base. Se conserva el cliente Mongo
            // configurado por Spring, incluyendo host, credenciales y opciones.
            MongoTemplate mongoVoltia = new MongoTemplate(mongoClient, "voltia");
            mongoVoltia.executeCommand("{ ping: 1 }");
            RepositorioMongo repositorio = new RepositorioMongo(mongoVoltia);
            repositorio.agregarObservador(new RegistroCambios());
            return repositorio;
        }

        if (!"memoria".equalsIgnoreCase(tipoAlmacen)) {
            throw new IllegalArgumentException("Almacen no soportado: " + tipoAlmacen);
        }
        RepositorioMemoria repositorio = RepositorioMemoria.getInstance();
        repositorio.agregarObservador(new RegistroCambios()); // Patron Observer
        return repositorio;
    }
}
