package com.example.ServidorLavadora.config;

import com.example.ServidorLavadora.repositorio.AlmacenDatos;
import com.example.ServidorLavadora.repositorio.RegistroCambios;
import com.example.ServidorLavadora.repositorio.RepositorioMemoria;
import com.example.ServidorLavadora.repositorio.RepositorioMongo;
import org.springframework.beans.factory.ObjectProvider;
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
    public AlmacenDatos almacenDatos(@Value("${voltia.datos-prueba:true}") boolean cargarDatosPrueba,
                                    @Value("${voltia.almacen:memoria}") String tipoAlmacen,
                                    ObjectProvider<MongoTemplate> mongoTemplateProvider) {
        if ("mongo".equalsIgnoreCase(tipoAlmacen)) {
            MongoTemplate mongoTemplate = mongoTemplateProvider.getIfAvailable();
            if (mongoTemplate != null) {
                try {
                    RepositorioMongo repositorio = new RepositorioMongo(mongoTemplate);
                    repositorio.agregarObservador(new RegistroCambios());
                    if (cargarDatosPrueba) {
                        repositorio.cargarDatosPruebaSiVacio();
                    }
                    return repositorio;
                } catch (Exception ex) {
                    // Si la instancia local de MongoDB no esta disponible, se cae a
                    // memoria para conservar la demo y no romper la entrega.
                }
            }
        }

        RepositorioMemoria repositorio = RepositorioMemoria.getInstance();
        repositorio.agregarObservador(new RegistroCambios()); // Patron Observer
        if (cargarDatosPrueba) {
            repositorio.cargarDatosPruebaSiVacio();
        }
        return repositorio;
    }
}
