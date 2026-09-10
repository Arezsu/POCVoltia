package com.arezsu.pocvoltia.servicios;

import com.arezsu.pocvoltia.gui.ObservadorListado;
import com.arezsu.pocvoltia.model.Electrodomestico;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

/**
 *
 * @author arezs
 */
public class ServicioElectrodomestico {

    private static Map<Integer, Electrodomestico> electrodomesticos = new HashMap<>();

    // Lista de observadores (patrón Observer): las ventanas de Listar
    // que quieren enterarse cuando se agrega un nuevo Electrodomestico.
    private static List<ObservadorListado> observadores = new ArrayList<>();

    // Registrar un observador (lo llama el constructor de cada GUI de Listar)
    public static void agregarObservador(ObservadorListado obs) {
        observadores.add(obs);
    }

    // Quitar un observador (lo llama la GUI de Listar cuando se cierra)
    public static void quitarObservador(ObservadorListado obs) {
        observadores.remove(obs);
    }

    // Avisa a todos los observadores registrados que hubo un cambio
    private static void notificarObservadores() {
        for (ObservadorListado obs : observadores) {
            obs.actualizar();
        }
    }

    public static Map<Integer, Electrodomestico> getElectrodomesticos() {
        return Map.copyOf(electrodomesticos);
    }

    public static boolean addElectrodomestico(Electrodomestico elem) {
        if (electrodomesticos.containsKey(elem.getCodigo())) {
            return false;
        }
        electrodomesticos.put(elem.getCodigo(), elem);
        notificarObservadores(); // <-- Aquí se avisa a todos los Listar abiertos
        return true;
    }

    public static Electrodomestico buscarElectrodomesticoPorCodigo(int codigo) {
        return electrodomesticos.get(codigo);
    }

    public static boolean eliminarElectrodomestico(int codigo) {
        if (electrodomesticos.containsKey(codigo)) {
            electrodomesticos.remove(codigo);
            return true;
        }
        return false;
    }

    public static double calcularSumatoriaPrecios() {
        double sumaTotal = 0.0;
        if (electrodomesticos != null) {
            for (Electrodomestico elem : electrodomesticos.values()) {
                if (elem != null) {
                    sumaTotal += elem.calcularPrecioFinal();
                }
            }
        }
        return sumaTotal;
    }
}