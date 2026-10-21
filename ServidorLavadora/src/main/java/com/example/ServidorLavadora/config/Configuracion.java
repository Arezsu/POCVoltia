package com.example.ServidorLavadora.config;

import com.example.ServidorLavadora.repositorio.AlmacenDatos;
import com.example.ServidorLavadora.repositorio.RegistroCambios;
import com.example.ServidorLavadora.repositorio.RepositorioMemoria;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;

/**
 * Aqui se arma lo que Spring le entrega a los servicios.
 * Se entrega el Singleton del repositorio (como AlmacenDatos) con el observador
 * de registro ya conectado.
 */
@Configuration // Clase de configuracion
public class Configuracion {

    @Bean // Spring guarda este objeto y lo entrega a quien lo pida
    public AlmacenDatos almacenDatos(@Value("${voltia.datos-prueba:true}") boolean cargarDatosPrueba) {
        RepositorioMemoria repositorio = RepositorioMemoria.getInstance();
        repositorio.agregarObservador(new RegistroCambios()); // Patron Observer
        if (cargarDatosPrueba) {
            repositorio.cargarDatosPruebaSiVacio();
        }
        return repositorio;
    }
}
