package com.example.ServidorLavadora.servicios;

import com.example.ServidorLavadora.model.Lavadora;
import com.example.ServidorLavadora.model.LavadoraInput;
import com.example.ServidorLavadora.model.LavadoraUpdateInput;

import java.time.LocalDateTime;
import java.time.format.DateTimeFormatter;
import java.time.format.DateTimeParseException;
import java.util.ArrayList;
import java.util.List;
import java.util.Optional;

/**
 * Logica de negocio. Las lavadoras se guardan en memoria principal en una
 * lista (ArrayList). Como varios clientes pueden llamar al servidor al mismo
 * tiempo, todos los metodos son synchronized.
 */
public class ServicioLavadora {

    /** Formato de fecha que se intercambia con el cliente. */
    public static final DateTimeFormatter FORMATO_FECHA =
            DateTimeFormatter.ofPattern("yyyy-MM-dd'T'HH:mm:ss");

    private static final List<Lavadora> lavadoras = new ArrayList<>(List.of(
            new Lavadora(101, "LG", 1450000, LocalDateTime.of(2024, 3, 10, 9, 0), 18.0, true),
            new Lavadora(102, "Samsung", 1290000, LocalDateTime.of(2023, 11, 22, 14, 30), 16.0, false),
            new Lavadora(103, "Whirlpool", 980000, LocalDateTime.of(2025, 1, 5, 8, 15), 14.0, false),
            new Lavadora(104, "LG", 1650000, LocalDateTime.of(2025, 6, 18, 10, 45), 20.0, true)
    ));

    // ---------------- Read ----------------

    public static synchronized List<Lavadora> listarLavadoras() {
        return new ArrayList<>(lavadoras);
    }

    public static synchronized Optional<Lavadora> buscarPorCodigo(int codigo) {
        return lavadoras.stream()
                .filter(l -> l.getCodigo() == codigo)
                .findFirst();
    }

    /**
     * Listado filtrado en el servidor por 2 parametros opcionales:
     * marca (si contiene el texto, sin importar mayusculas) y funcionSecado.
     * Si un parametro llega vacio o null, no se usa para filtrar.
     */
    public static synchronized List<Lavadora> listarPorFiltro(String marca, Boolean funcionSecado) {
        String texto = (marca == null) ? "" : marca.trim().toLowerCase();
        return lavadoras.stream()
                .filter(l -> texto.isEmpty() || l.getMarca().toLowerCase().contains(texto))
                .filter(l -> funcionSecado == null || l.isFuncionSecado() == funcionSecado)
                .toList();
    }

    // ---------------- Create ----------------

    public static synchronized Optional<Lavadora> addLavadora(LavadoraInput input) {
        if (buscarPorCodigo(input.codigo()).isPresent()) {
            throw new IllegalArgumentException("Ya existe una lavadora con el codigo " + input.codigo());
        }
        validarMarca(input.marca());
        validarPositivo(input.precioBase(), "El precio base");
        validarPositivo(input.capacidadKilos(), "La capacidad en kilos");

        Lavadora lavadora = new Lavadora(
                input.codigo(),
                input.marca().trim(),
                input.precioBase(),
                parsearFecha(input.fechaFabricacion()),
                input.capacidadKilos(),
                input.funcionSecado()
        );
        lavadoras.add(lavadora);
        return Optional.of(lavadora);
    }

    // ---------------- Update ----------------

    /** Cambia solo los atributos que vengan en el input (los null se ignoran). */
    public static synchronized Optional<Lavadora> actualizarLavadora(int codigo, LavadoraUpdateInput input) {
        Lavadora lavadora = buscarPorCodigo(codigo)
                .orElseThrow(() -> new IllegalArgumentException("No existe una lavadora con el codigo " + codigo));

        if (input.marca() != null) {
            validarMarca(input.marca());
            lavadora.setMarca(input.marca().trim());
        }
        if (input.precioBase() != null) {
            validarPositivo(input.precioBase(), "El precio base");
            lavadora.setPrecioBase(input.precioBase());
        }
        if (input.fechaFabricacion() != null) {
            lavadora.setFechaFabricacion(parsearFecha(input.fechaFabricacion()));
        }
        if (input.capacidadKilos() != null) {
            validarPositivo(input.capacidadKilos(), "La capacidad en kilos");
            lavadora.setCapacidadKilos(input.capacidadKilos());
        }
        if (input.funcionSecado() != null) {
            lavadora.setFuncionSecado(input.funcionSecado());
        }
        return Optional.of(lavadora);
    }

    // ---------------- Delete ----------------

    public static synchronized boolean eliminarLavadora(int codigo) {
        Lavadora lavadora = buscarPorCodigo(codigo)
                .orElseThrow(() -> new IllegalArgumentException("No existe una lavadora con el codigo " + codigo));
        return lavadoras.remove(lavadora);
    }

    // ---------------- Utilidades ----------------

    private static void validarMarca(String marca) {
        if (marca == null || marca.isBlank()) {
            throw new IllegalArgumentException("La marca es obligatoria");
        }
    }

    private static void validarPositivo(double valor, String nombre) {
        if (valor <= 0) {
            throw new IllegalArgumentException(nombre + " debe ser mayor que cero");
        }
    }

    private static LocalDateTime parsearFecha(String texto) {
        try {
            return LocalDateTime.parse(texto);
        } catch (DateTimeParseException | NullPointerException e) {
            throw new IllegalArgumentException(
                    "Fecha invalida. Use el formato yyyy-MM-ddTHH:mm:ss (ejemplo 2025-06-18T10:45:00)");
        }
    }
}
