package com.example.ServidorLavadora.model;

import java.time.LocalDateTime;
import com.fasterxml.jackson.annotation.JsonFormat;

/**
 * SUPERCLASE (clase padre) de los electrodomesticos de Voltia.
 * Aqui van los datos que comparten todos: codigo, marca, precio base y fecha
 * de fabricacion. Su hija es Lavadora (si manana se agrega otro electrodomestico,
 * hereda de esta clase y no hay que cambiar nada de lo que ya existe).
 * Es abstracta: no se puede crear un "Electrodomestico" suelto, solo sus hijas.
 */
public abstract class Electrodomestico {
    private int codigo;
    private String marca;
    private double precioBase;
    private LocalDateTime fechaFabricacion;

    public Electrodomestico() {
    }

    public Electrodomestico(int codigo, String marca, double precioBase, LocalDateTime fechaFabricacion) {
        this.codigo = codigo;
        this.marca = marca;
        this.precioBase = precioBase;
        this.fechaFabricacion = fechaFabricacion;
    }

    public int getCodigo() {
        return codigo;
    }

    public void setCodigo(int codigo) {
        this.codigo = codigo;
    }

    public String getMarca() {
        return marca;
    }

    public void setMarca(String marca) {
        this.marca = marca;
    }

    public double getPrecioBase() {
        return precioBase;
    }

    public void setPrecioBase(double precioBase) {
        this.precioBase = precioBase;
    }

    @JsonFormat(pattern = "yyyy-MM-dd'T'HH:mm:ss")
    public LocalDateTime getFechaFabricacion() {
        return fechaFabricacion;
    }

    public void setFechaFabricacion(LocalDateTime fechaFabricacion) {
        this.fechaFabricacion = fechaFabricacion;
    }

    /**
     * Cada hija calcula su precio final a su manera (polimorfismo).
     * Por defecto el precio final es el precio base.
     */
    public double calcularPrecioFinal() { // Cada hija lo cambia
        return precioBase;
    }

    /** GraphQL usa este getter para el campo "precioFinal". */
    public double getPrecioFinal() {
        return calcularPrecioFinal();
    }
}
