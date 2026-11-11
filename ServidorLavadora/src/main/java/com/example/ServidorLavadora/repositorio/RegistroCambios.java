package com.example.ServidorLavadora.repositorio;

import java.time.LocalDateTime;

/** Muestra en la consola del servidor cada lavadora o mantenimiento modificado. */
public class RegistroCambios implements ObservadorRepositorio {
    @Override
    public void alCambiar(String evento) {
        System.out.println("[REGISTRO " + LocalDateTime.now().withNano(0) + "] " + evento);
    }
}
