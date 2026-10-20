package com.example.ServidorLavadora;

import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;

/**
 * Punto de entrada del servidor. Al ejecutar main, Spring Boot levanta el
 * servidor web en el puerto 8081 y publica el servicio GraphQL en /graphql.
 */
@SpringBootApplication // Arranca todo automaticamente
public class ServidorLavadoraApplication {

    public static void main(String[] args) {
        SpringApplication.run(ServidorLavadoraApplication.class, args);
    }

}
