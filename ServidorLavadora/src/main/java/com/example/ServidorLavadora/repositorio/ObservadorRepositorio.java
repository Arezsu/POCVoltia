package com.example.ServidorLavadora.repositorio;

/**
 * Patron OBSERVER (lado del servidor).
 * Quien implemente esta interface se entera de cada cambio en los datos.
 */
public interface ObservadorRepositorio {

    /** Se llama cada vez que se agrega, actualiza o elimina algo. */
    void alCambiar(String evento);
}
