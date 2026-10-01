package com.example.ServidorLavadora.model;

/**
 * Todos los campos son opcionales: el cliente solo manda lo que quiere cambiar.
 */
public record LavadoraUpdateInput(
        String marca,
        Double precioBase,
        String fechaFabricacion,
        Double capacidadKilos,
        Boolean funcionSecado
) {
}
