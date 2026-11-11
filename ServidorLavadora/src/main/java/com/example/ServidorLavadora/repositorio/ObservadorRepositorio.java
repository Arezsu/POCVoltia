package com.example.ServidorLavadora.repositorio;

/** Recibe avisos cuando el servidor guarda un cambio. */
public interface ObservadorRepositorio {
    void alCambiar(String evento);
}
