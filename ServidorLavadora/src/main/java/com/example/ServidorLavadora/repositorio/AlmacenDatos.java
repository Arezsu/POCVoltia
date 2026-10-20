package com.example.ServidorLavadora.repositorio;

import com.example.ServidorLavadora.model.Lavadora;
import com.example.ServidorLavadora.model.ProgramaLavado;

import java.util.List;

/**
 * Abstraccion del lugar donde se guardan los datos.
 * Los servicios dependen de esta interface y no de una clase concreta
 * (principio de inversion de dependencias): si manana los datos van a una base de datos,
 * se crea otra clase que cumpla esta interface y los servicios no cambian.
 */
public interface AlmacenDatos {

    /** Objeto que se usa como candado: entra un cliente a la vez a modificar los datos. */
    Object candado();

    /** Coleccion de programas (maestro). Usar siempre dentro del candado. */
    List<ProgramaLavado> programas();

    /** Coleccion de lavadoras (detalle). Usar siempre dentro del candado. */
    List<Lavadora> lavadoras();

    /** Avisa a los observadores que algo cambio. */
    void notificar(String evento);
}
