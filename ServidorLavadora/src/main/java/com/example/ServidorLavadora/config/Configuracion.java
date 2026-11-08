package com.example.ServidorLavadora.config;

import com.example.ServidorLavadora.repositorio.AlmacenDatos;
import com.example.ServidorLavadora.repositorio.RegistroCambios;
import com.example.ServidorLavadora.repositorio.RepositorioMemoria;
import com.example.ServidorLavadora.repositorio.RepositorioMongo;
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
                                    MongoTemplate mongoTemplate) {
        if ("mongo".equalsIgnoreCase(tipoAlmacen)) {
            mongoTemplate.executeCommand("{ ping: 1 }");
            RepositorioMongo repositorio = new RepositorioMongo(mongoTemplate);
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
