package com.example.ServidorLavadora.model;

/**
 * Datos que manda el cliente para crear una lavadora.
 * La fecha viaja como texto ISO (yyyy-MM-ddTHH:mm:ss).
 */
public record LavadoraInput(
        int codigo,
        String marca,
        double precioBase,
        String fechaFabricacion,
        double capacidadCargaKilos,
        int velocidadCentrifugadoRPM,
        boolean tecnologiaInverter,
        int minutosLavado
) {
}
