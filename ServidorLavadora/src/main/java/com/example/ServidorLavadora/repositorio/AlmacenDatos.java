package com.example.ServidorLavadora.repositorio;

import com.example.ServidorLavadora.model.Lavadora;
import com.example.ServidorLavadora.model.Mantenimiento;

import java.util.List;
import java.util.Optional;

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
     * Lavadoras maestras. Los detalles se administran mediante operaciones separadas.
     */
    List<Lavadora> lavadoras();

    /** Devuelve el listado aplicando los criterios en el almacen de datos. */
    List<Lavadora> listarPorFiltro(String marca, Boolean tecnologiaInverter, Boolean conMantenimientos);

    /** Busca un maestro por su clave sin cargar toda la coleccion. */
    Optional<Lavadora> buscarPorCodigo(int codigo);

    /** Busca una lavadora aplicando los criterios recibidos por el usuario. */
    Optional<Lavadora> buscarPorCriterios(Integer codigo, String marca);

    /** Persiste una lavadora nueva o actualizada en el almacen activo. */
    Lavadora guardar(Lavadora lavadora);

    /** Elimina una lavadora por su codigo y sus detalles asociados. */
    boolean eliminar(int codigo);

    /** Registra un cambio y avisa a los observadores del servidor. */
    void registrarCambio(String evento);

    /**
     * Numero que sube en 1 cada vez que cambian los datos. Los clientes lo
     * consultan cada pocos segundos para darse cuenta de que otro cliente
     * modifico algo y refrescar su ventana de listado.
     */
    int version();

    List<Mantenimiento> mantenimientos();
    List<Mantenimiento> mantenimientosPorLavadora(int codigoLavadora);
    Optional<Mantenimiento> buscarMantenimiento(int id);
    Mantenimiento guardarMantenimiento(Mantenimiento mantenimiento);
    boolean eliminarMantenimiento(int id);
    boolean tieneMantenimientos(int codigoLavadora);
}
