package com.example.ServidorLavadora.model;

/**
 * Datos que manda el cliente para crear un mantenimiento DENTRO de una lavadora.
 *
 * El usuario proporciona el id y la lavadora a la que se asocia el registro.
 * La fecha viaja como texto ISO (yyyy-MM-ddTHH:mm:ss).
 */
public record MantenimientoInput(
        int id,
        int codigoLavadora,
        String descripcion,
        String tecnico,
        double costo,
        String fecha,
        boolean completado
) {
}
