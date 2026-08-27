/*
 * Click nbfs://nbhost/SystemFileSystem/Templates/Licenses/license-default.txt to change this license
 * Click nbfs://nbhost/SystemFileSystem/Templates/Classes/Class.java to edit this template
 */
package com.arezsu.pocvoltia.model;

import java.time.LocalDate;

/**
 *
 * @author arezs
 */
public abstract class Electrodomestico {

    private int codigo;
    private String marca;
    private double precioBase;
    private LocalDate fechaFabricacion;

    // Constructor 1: Atributos básicos
    public Electrodomestico(int codigo, String marca) {
        this.codigo = codigo;
        this.marca = marca;
    }

    // Constructor 2: Completo con validación de excepción
    public Electrodomestico(int codigo, String marca, LocalDate fechaFabricacion, double precioBase) throws Exception {
        this(codigo, marca);
        this.fechaFabricacion = fechaFabricacion;
        setPrecioBase(precioBase);
    }

    // Validación mediante excepción (igual a setSueldo)
    public void setPrecioBase(double precioBase) throws Exception {
        if (precioBase > 0) {
            this.precioBase = precioBase;
        } else {
            throw new Exception("El precio base debe ser mayor a 0");
        }
    }

    public double getPrecioBase() {
        return this.precioBase;
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

    public LocalDate getFechaFabricacion() {
        return fechaFabricacion;
    }

    public void setFechaFabricacion(LocalDate fechaFabricacion) {
        this.fechaFabricacion = fechaFabricacion;
    }

    // Método de cálculo polimórfico
    public double calcularPrecioFinal() {
        return precioBase;
    }
}
