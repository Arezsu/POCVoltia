package com.example.ServidorLavadora.model;

/**
 * Datos para actualizar un programa. Todos son opcionales:
 * lo que llegue null se queda como estaba (por eso Integer, Double y Boolean).
 */
public record ProgramaUpdateInput(
        String nombre,
        Integer minMinutos,
        Integer maxMinutos,
        Double consumoAguaLitros,
        String fechaCreacion,
        Boolean aguaCaliente
) {
}
