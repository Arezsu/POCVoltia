package com.example.ServidorLavadora.util;

import java.time.LocalDateTime;
import java.time.format.DateTimeFormatter;
import java.time.format.DateTimeParseException;

/**
 * Validaciones y conversiones que usan los servicios.
 * Esta clase solo hace una cosa: revisar datos (principio de responsabilidad unica).
 *
 * Todos los metodos lanzan IllegalArgumentException con un mensaje claro y ese
 * mensaje es el que le llega al usuario final en el cliente, porque el
 * ManejadorExcepcionesGraphQL lo convierte en un error de GraphQL.
 */
public final class Validaciones {

    /** Formato en que se intercambia la fecha con los clientes. */
    public static final DateTimeFormatter FORMATO_FECHA =
            DateTimeFormatter.ofPattern("yyyy-MM-dd'T'HH:mm:ss");

    private Validaciones() {
        // Clase de utilidades: no se crean objetos de ella
    }

    /** Revisa que un texto tenga contenido. */
    public static void texto(String valor, String nombreCampo) {
        if (valor == null || valor.isBlank()) {
            throw new IllegalArgumentException(nombreCampo + " es obligatorio");
        }
    }

    /** Revisa que un numero decimal sea mayor que cero (precios, capacidades). */
    public static void positivo(double valor, String nombreCampo) {
        if (valor <= 0) {
            throw new IllegalArgumentException(nombreCampo + " debe ser mayor que cero");
        }
    }

    /** Revisa que un numero decimal no sea negativo (por ejemplo, un costo que puede ser cero). */
    public static void noNegativo(double valor, String nombreCampo) {
        if (valor < 0) {
            throw new IllegalArgumentException(nombreCampo + " no puede ser negativo");
        }
    }

    /** Revisa que un numero entero sea mayor que cero (codigos). */
    public static void enteroPositivo(int valor, String nombreCampo) {
        if (valor <= 0) {
            throw new IllegalArgumentException(nombreCampo + " debe ser mayor que cero");
        }
    }

    /** Revisa que un numero entero este dentro de un rango (por ejemplo, minutos de lavado). */
    public static void rango(int valor, int minimo, int maximo, String nombreCampo) {
        if (valor < minimo || valor > maximo) {
            throw new IllegalArgumentException(
                    nombreCampo + " debe estar entre " + minimo + " y " + maximo);
        }
    }

    /**
     * Convierte el texto ISO que manda el cliente a LocalDateTime.
     * Si el texto viene mal, se responde con un ejemplo del formato correcto.
     */
    public static LocalDateTime fecha(String texto) {
        try {
            return LocalDateTime.parse(texto);
        } catch (DateTimeParseException | NullPointerException e) {
            throw new IllegalArgumentException(
                    "Fecha invalida. Use el formato yyyy-MM-ddTHH:mm:ss (ejemplo 2025-06-18T10:45:00)");
        }
    }
}
