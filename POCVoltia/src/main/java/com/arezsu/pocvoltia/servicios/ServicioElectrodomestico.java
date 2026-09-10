package com.arezsu.pocvoltia.servicios;

import com.arezsu.pocvoltia.model.Electrodomestico;
import java.util.HashMap;
import java.util.Map;

/**
 *
 * @author arezs
 */
public class ServicioElectrodomestico {

    private static Map<Integer, Electrodomestico> electrodomesticos = new HashMap<>();

    public static Map<Integer, Electrodomestico> getElectrodomesticos() {
        return Map.copyOf(electrodomesticos);
    }

    public static boolean addElectrodomestico(Electrodomestico elem) {
        if (electrodomesticos.containsKey(elem.getCodigo())) {
            return false;
        }
        electrodomesticos.put(elem.getCodigo(), elem);
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
