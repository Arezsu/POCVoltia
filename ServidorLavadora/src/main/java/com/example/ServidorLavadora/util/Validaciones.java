package com.example.ServidorLavadora.util;

import java.time.LocalDateTime;
import java.time.format.DateTimeFormatter;
import java.time.format.DateTimeParseException;

/**
 * Validaciones y conversiones que usan los servicios.
 * Esta clase solo hace una cosa: revisar datos (principio de responsabilidad unica).
 * Todos los metodos lanzan IllegalArgumentException con un mensaje claro
 * y ese mensaje es el que le llega al usuario en el cliente.
 */
public final class Validaciones {

    /** Formato en que se intercambia la fecha con los clientes. */
    public static final DateTimeFormatter FORMATO_FECHA =
            DateTimeFormatter.ofPattern("yyyy-MM-dd'T'HH:mm:ss");

    private Validaciones() {
        // Clase de utilidades: no se crean objetos de ella
    }

    public static void texto(String valor, String nombreCampo) {
        if (valor == null || valor.isBlank()) {
            throw new IllegalArgumentException(nombreCampo + " es obligatorio");
        }
    }

    public static void positivo(double valor, String nombreCampo) {
        if (valor <= 0) {
            throw new IllegalArgumentException(nombreCampo + " debe ser mayor que cero");
        }
    }

    public static LocalDateTime fecha(String texto) {
        try {
            return LocalDateTime.parse(texto);
        } catch (DateTimeParseException | NullPointerException e) {
            throw new IllegalArgumentException(
                    "Fecha invalida. Use el formato yyyy-MM-ddTHH:mm:ss (ejemplo 2025-06-18T10:45:00)");
        }
    }
}
