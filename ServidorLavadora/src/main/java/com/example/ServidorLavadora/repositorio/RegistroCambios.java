package com.example.ServidorLavadora.repositorio;

import java.time.LocalDateTime;

/**
 * Observador concreto: escribe en la consola del servidor cada cambio que ocurre.
 * Sirve para ver en vivo que hacen los clientes.
 */
public class RegistroCambios implements ObservadorRepositorio {

    @Override
    public void alCambiar(String evento) {
        System.out.println("[REGISTRO " + LocalDateTime.now().withNano(0) + "] " + evento);
    }
}
