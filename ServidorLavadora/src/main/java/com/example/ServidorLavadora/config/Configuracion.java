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
 * Se entrega un repositorio como AlmacenDatos para que lo usen los servicios.
 */
@Configuration // Clase de configuracion
public class Configuracion {

    @Bean // Spring guarda este objeto y lo entrega a quien lo pida
    public MongoTemplate mongoTemplate(MongoClient mongoClient) {
        // Define el nombre de la base tambien para los componentes automaticos de
        // Spring Data (por ejemplo, la creacion de indices al iniciar).
        return new MongoTemplate(mongoClient, "voltia");
    }

    @Bean
    public AlmacenDatos almacenDatos(@Value("${voltia.almacen:mongo}") String tipoAlmacen,
                                    MongoTemplate mongoTemplate) {
        if ("mongo".equalsIgnoreCase(tipoAlmacen)) {
            mongoTemplate.executeCommand("{ ping: 1 }");
            if (!mongoTemplate.collectionExists("lavadoras")) {
                mongoTemplate.createCollection("lavadoras");
            }
            if (!mongoTemplate.collectionExists("mantenimientos")) {
                mongoTemplate.createCollection("mantenimientos");
            }
            RepositorioMongo repositorio = new RepositorioMongo(mongoTemplate);
            repositorio.agregarObservador(new RegistroCambios());
            return repositorio;
        }

        if (!"memoria".equalsIgnoreCase(tipoAlmacen)) {
            throw new IllegalArgumentException("Almacen no soportado: " + tipoAlmacen);
        }
        RepositorioMemoria repositorio = RepositorioMemoria.getInstance();
        repositorio.agregarObservador(new RegistroCambios());
        return repositorio;
    }
}
