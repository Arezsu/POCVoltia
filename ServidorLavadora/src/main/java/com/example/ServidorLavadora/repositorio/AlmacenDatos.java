package com.example.ServidorLavadora.repositorio;

import com.example.ServidorLavadora.model.Lavadora;

import java.util.List;

/**
 * Abstraccion del lugar donde se guardan los datos (principio de inversion de
 * dependencias: los servicios dependen de esta interface, no de una clase concreta).
 *
 * Hoy los datos viven en memoria. Si manana se pasan a una base de datos, se crea
 * otra clase que cumpla este mismo contrato y los servicios no cambian ni una linea.
 */
public interface AlmacenDatos {

    /**
     * Objeto que se usa como candado (lock). Los servicios lo usan con
     * synchronized para que dos clientes no modifiquen los datos a la vez.
     */
    Object candado();

    /**
     * Coleccion de lavadoras (los maestros). Cada lavadora lleva adentro su propia
     * lista de mantenimientos (el detalle). Usar siempre dentro del candado.
     */
    List<Lavadora> lavadoras();

    /** Avisa a los observadores que algo cambio (patron Observer). */
    void notificar(String evento);

    /**
     * Numero que sube en 1 cada vez que cambian los datos. Los clientes lo
     * consultan cada pocos segundos para darse cuenta de que otro cliente
     * modifico algo y refrescar su ventana de listado.
     */
    int version();
}
