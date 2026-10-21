package com.example.ServidorLavadora.model;

import java.time.LocalDateTime;
import java.util.List;
import java.util.Optional;
import java.util.concurrent.CopyOnWriteArrayList;

/**
 * Objeto MAESTRO del proyecto (lado 1..1 de la Figura 1) e hija de Electrodomestico.
 *
 * La lavadora es el objeto principal del sistema y cada lavadora tiene CERO, UNO
 * O MUCHOS mantenimientos (lado 0..*). Queda descartada la idea equivocada de que
 * una lavadora tiene un solo programa fijo: aqui la relacion maestro-detalle
 * permite cualquier cantidad de registros hijos, incluso cero.
 *
 * Atributos (mas de cinco y de tipos variados):
 *  - codigo (int), marca (String), precioBase (double), fechaFabricacion (LocalDateTime): heredados
 *  - capacidadCargaKilos (double)
 *  - velocidadCentrifugadoRPM (int)
 *  - tecnologiaInverter (boolean)
 *  - minutosLavado (int)
 *  - mantenimientos: la coleccion de detalle
 *
 * Se construye con el patron Builder:
 * Lavadora.builder().codigo(101).marca("LG").precioBase(1450000).build()
 */
public class Lavadora extends Electrodomestico {

    // Reglas de negocio del precio final (se explican en calcularPrecioFinal).
    private static final double BONO_CAPACIDAD = 120000;
    private static final double BONO_INVERTER = 250000;
    private static final double KILOS_QUE_ACTIVAN_EL_BONO = 15;

    private double capacidadCargaKilos;
    private int velocidadCentrifugadoRPM;
    private boolean tecnologiaInverter;
    private int minutosLavado;

    /**
     * Lista de mantenimientos (el detalle). Se usa CopyOnWriteArrayList porque
     * el servidor atiende varias peticiones al mismo tiempo: esta lista se puede
     * leer mientras otro hilo la modifica sin romper nada.
     */
    private final List<Mantenimiento> mantenimientos = new CopyOnWriteArrayList<>();

    private Lavadora(Builder builder) {
        super(builder.codigo, builder.marca, builder.precioBase, builder.fechaFabricacion);
        this.capacidadCargaKilos = builder.capacidadCargaKilos;
        this.velocidadCentrifugadoRPM = builder.velocidadCentrifugadoRPM;
        this.tecnologiaInverter = builder.tecnologiaInverter;
        this.minutosLavado = builder.minutosLavado;
    }

    /** Punto de entrada del patron Builder. */
    public static Builder builder() {
        return new Builder();
    }

    // ================= Relacion maestro - detalle =================

    /** Agrega un mantenimiento a esta lavadora (la lista admite 0..* elementos). */
    public void agregarMantenimiento(Mantenimiento mantenimiento) {
        mantenimientos.add(mantenimiento);
    }

    /** Quita un mantenimiento. Devuelve true si estaba en la lista. */
    public boolean quitarMantenimiento(Mantenimiento mantenimiento) {
        return mantenimientos.remove(mantenimiento);
    }

    /** Busca un mantenimiento por su id dentro de esta lavadora. */
    public Optional<Mantenimiento> buscarMantenimiento(int id) {
        return mantenimientos.stream()
                .filter(m -> m.getId() == id)
                .findFirst();
    }

    /** Copia de la lista: quien la reciba no puede modificar los datos por accidente. */
    public List<Mantenimiento> getMantenimientos() {
        return List.copyOf(mantenimientos);
    }

    /** Campo calculado que usa GraphQL: cuantos mantenimientos tiene la lavadora. */
    public int getCantidadMantenimientos() {
        return mantenimientos.size();
    }

    /** Campo calculado que usa GraphQL: cuantos mantenimientos siguen pendientes. */
    public int getMantenimientosPendientes() {
        return (int) mantenimientos.stream().filter(m -> !m.isCompletado()).count();
    }

    /** Campo calculado que usa GraphQL: suma del costo de todos sus mantenimientos. */
    public double getCostoMantenimientos() {
        return mantenimientos.stream().mapToDouble(Mantenimiento::getCosto).sum();
    }

    // ================= Reglas de negocio =================

    /**
     * Polimorfismo: cada electrodomestico calcula su precio final a su manera.
     * Una lavadora suma 120.000 si carga mas de 15 kilos y 250.000 si es inverter.
     */
    @Override
    public double calcularPrecioFinal() {
        double precio = getPrecioBase();
        if (capacidadCargaKilos > KILOS_QUE_ACTIVAN_EL_BONO) {
            precio += BONO_CAPACIDAD;
        }
        if (tecnologiaInverter) {
            precio += BONO_INVERTER;
        }
        return precio;
    }

    // ================= Getters y setters =================

    public double getCapacidadCargaKilos() {
        return capacidadCargaKilos;
    }

    public void setCapacidadCargaKilos(double capacidadCargaKilos) {
        this.capacidadCargaKilos = capacidadCargaKilos;
    }

    public int getVelocidadCentrifugadoRPM() {
        return velocidadCentrifugadoRPM;
    }

    public void setVelocidadCentrifugadoRPM(int velocidadCentrifugadoRPM) {
        this.velocidadCentrifugadoRPM = velocidadCentrifugadoRPM;
    }

    public boolean isTecnologiaInverter() {
        return tecnologiaInverter;
    }

    public void setTecnologiaInverter(boolean tecnologiaInverter) {
        this.tecnologiaInverter = tecnologiaInverter;
    }

    public int getMinutosLavado() {
        return minutosLavado;
    }

    public void setMinutosLavado(int minutosLavado) {
        this.minutosLavado = minutosLavado;
    }

    /**
     * Patron BUILDER: la lavadora tiene ocho datos escalares. Con un constructor
     * tradicional habria que escribir ocho parametros seguidos en el orden exacto
     * (y seria facil equivocarse). El Builder permite armarla paso a paso y leer
     * claramente que valor tiene cada atributo.
     */
    public static class Builder {
        private int codigo;
        private String marca;
        private double precioBase;
        private LocalDateTime fechaFabricacion;
        private double capacidadCargaKilos;
        private int velocidadCentrifugadoRPM;
        private boolean tecnologiaInverter;
        private int minutosLavado;

        public Builder codigo(int codigo) {
            this.codigo = codigo;
            return this;
        }

        public Builder marca(String marca) {
            this.marca = marca;
            return this;
        }

        public Builder precioBase(double precioBase) {
            this.precioBase = precioBase;
            return this;
        }

        public Builder fechaFabricacion(LocalDateTime fechaFabricacion) {
            this.fechaFabricacion = fechaFabricacion;
            return this;
        }

        public Builder capacidadCargaKilos(double capacidadCargaKilos) {
            this.capacidadCargaKilos = capacidadCargaKilos;
            return this;
        }

        public Builder velocidadCentrifugadoRPM(int velocidadCentrifugadoRPM) {
            this.velocidadCentrifugadoRPM = velocidadCentrifugadoRPM;
            return this;
        }

        public Builder tecnologiaInverter(boolean tecnologiaInverter) {
            this.tecnologiaInverter = tecnologiaInverter;
            return this;
        }

        public Builder minutosLavado(int minutosLavado) {
            this.minutosLavado = minutosLavado;
            return this;
        }

        public Lavadora build() {
            return new Lavadora(this);
        }
    }
}
