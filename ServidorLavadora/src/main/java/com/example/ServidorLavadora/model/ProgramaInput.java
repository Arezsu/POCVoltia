package com.example.ServidorLavadora.model;

/**
 * Datos que manda el cliente para crear un programa de lavado.
 * Es un "record": una clase de datos (solo guarda valores).
 * La fecha viaja como texto ISO (yyyy-MM-ddTHH:mm:ss).
 */
public record ProgramaInput(
        int codigo,
        String nombre,
        int minMinutos,
        int maxMinutos,
        double consumoAguaLitros,
        String fechaCreacion,
        boolean aguaCaliente
) {
}
