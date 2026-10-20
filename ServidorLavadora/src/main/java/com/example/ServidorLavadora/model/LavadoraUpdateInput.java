package com.example.ServidorLavadora.model;

/**
 * Datos para actualizar una lavadora. Todos son opcionales:
 * lo que llegue null se queda como estaba.
 */
public record LavadoraUpdateInput(
        String marca,
        Double precioBase,
        String fechaFabricacion,
        Double capacidadCargaKilos,
        Integer velocidadCentrifugadoRPM,
        Boolean tecnologiaInverter,
        Integer minutosLavado
) {
}
