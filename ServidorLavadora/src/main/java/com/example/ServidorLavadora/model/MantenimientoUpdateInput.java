package com.example.ServidorLavadora.model;

/**
 * Datos para actualizar un mantenimiento. Todos son opcionales: lo que llegue
 * null se queda como estaba. El id no se puede cambiar (identifica al detalle).
 */
public record MantenimientoUpdateInput(
        String descripcion,
        String tecnico,
        Double costo,
        String fecha,
        Boolean completado
) {
}
