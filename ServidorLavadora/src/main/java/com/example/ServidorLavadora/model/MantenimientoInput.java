package com.example.ServidorLavadora.model;

/**
 * Datos que manda el cliente para crear un mantenimiento DENTRO de una lavadora.
 *
 * El id NO se pide: el servidor lo calcula solo (el siguiente numero libre de esa
 * lavadora). Asi se evita que dos mantenimientos de la misma lavadora repitan id.
 * La fecha viaja como texto ISO (yyyy-MM-ddTHH:mm:ss).
 */
public record MantenimientoInput(
        String descripcion,
        String tecnico,
        double costo,
        String fecha,
        boolean completado
) {
}
