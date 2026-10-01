package com.example.ServidorLavadora.model;

/**
 * Datos que envia el cliente para crear una lavadora.
 * fechaFabricacion viaja como texto ISO (yyyy-MM-ddTHH:mm:ss) y el servicio
 * lo convierte a LocalDateTime.
 */
public record LavadoraInput(
        int codigo,
        String marca,
        double precioBase,
        String fechaFabricacion,
        double capacidadKilos,
        boolean funcionSecado
) {
}
