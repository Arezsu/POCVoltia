package com.example.ServidorLavadora.model;

import java.time.LocalDateTime;

/**
 * Objeto seleccionado para el taller. Atributos:
 * codigo (int), marca (String), precioBase (double),
 * fechaFabricacion (LocalDateTime), capacidadKilos (double)
 * y funcionSecado (boolean, el atributo de libre eleccion).
 */
public class Lavadora extends Electrodomestico {
    private LocalDateTime fechaFabricacion;
    private double capacidadKilos;
    private boolean funcionSecado;

    public Lavadora() {
    }

    public Lavadora(int codigo, String marca, double precioBase,
                    LocalDateTime fechaFabricacion, double capacidadKilos, boolean funcionSecado) {
        super(codigo, marca, precioBase);
        this.fechaFabricacion = fechaFabricacion;
        this.capacidadKilos = capacidadKilos;
        this.funcionSecado = funcionSecado;
    }

    public LocalDateTime getFechaFabricacion() {
        return fechaFabricacion;
    }

    public void setFechaFabricacion(LocalDateTime fechaFabricacion) {
        this.fechaFabricacion = fechaFabricacion;
    }

    public double getCapacidadKilos() {
        return capacidadKilos;
    }

    public void setCapacidadKilos(double capacidadKilos) {
        this.capacidadKilos = capacidadKilos;
    }

    public boolean isFuncionSecado() {
        return funcionSecado;
    }

    public void setFuncionSecado(boolean funcionSecado) {
        this.funcionSecado = funcionSecado;
    }
}
