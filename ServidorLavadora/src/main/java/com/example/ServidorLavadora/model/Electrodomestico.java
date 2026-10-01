package com.example.ServidorLavadora.model;

/**
 * Clase estructural abstracta (caso Voltia). Lavadora hereda de ella.
 */
public abstract class Electrodomestico {
    private int codigo;
    private String marca;
    private double precioBase;

    public Electrodomestico() {
    }

    public Electrodomestico(int codigo, String marca, double precioBase) {
        this.codigo = codigo;
        this.marca = marca;
        this.precioBase = precioBase;
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
}
