package com.arezsu.pocvoltia.model;

import java.time.LocalDate;

public class Nevera extends Electrodomestico implements ICongelable {

    private int volumenLitros;
    private int numeroPuertas;
    private boolean dispensadorAgua;

    public Nevera(int codigo, String marca, LocalDate fechaFabricacion, double precioBase, int volumenLitros, int numeroPuertas1, boolean dispensadorAgua) throws Exception {
        super(codigo, marca, fechaFabricacion, precioBase);
        setVolumenLitros(volumenLitros);
        setNumeroPuertas(numeroPuertas);
        this.dispensadorAgua = dispensadorAgua;
    }

    @Override
    public double calcularPrecioFinal() {
        double precio = getPrecioBase();
        if (volumenLitros > 300) {
            precio += 300000;
        }
        if (dispensadorAgua) {
            precio += 150000;
        }
        return precio;
    }

    public int getVolumenLitros() {
        return volumenLitros;
    }

    public void setVolumenLitros(int volumenLitros) throws Exception {
        if (volumenLitros > 0) {
            this.volumenLitros = volumenLitros;
        } else {
            throw new Exception("El volumen en litros debe ser mayor a 0");
        }
    }

    public int getNumeroPuertas() {
        return numeroPuertas;
    }

    public void setNumeroPuertas(int numeroPuertas) throws Exception {
        if (numeroPuertas > 0) {
            this.numeroPuertas = numeroPuertas;
        } else {
            throw new Exception("El número de puertas debe ser al menos 1");
        }
    }

    public boolean isDispensadorAgua() {
        return dispensadorAgua;
    }

    public void setDispensadorAgua(boolean dispensadorAgua) {
        this.dispensadorAgua = dispensadorAgua;
    }

    // Métodos de la interfaz IAplicable
    @Override
    public double aplicarDescuentoTemporada(double porcentaje) {
        return calcularPrecioFinal() * (1 - (porcentaje / 100));
    }

    @Override
    public String obtenerCertificadoGarantia() {
        return "Certificado de Garantía Extendida Nevera POCVoltia";
    }
}