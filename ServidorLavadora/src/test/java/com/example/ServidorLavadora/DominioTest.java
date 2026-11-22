package com.example.ServidorLavadora;

import com.example.ServidorLavadora.model.Lavadora;
import com.example.ServidorLavadora.model.Mantenimiento;
import com.example.ServidorLavadora.repositorio.RepositorioMemoria;
import com.example.ServidorLavadora.util.Validaciones;
import org.junit.jupiter.api.Test;

import java.time.LocalDateTime;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertSame;
import static org.junit.jupiter.api.Assertions.assertThrows;
import static org.junit.jupiter.api.Assertions.assertTrue;

/**
 * Pruebas del modelo de dominio y del repositorio. No necesitan levantar el
 * servidor web: revisan las reglas del negocio directamente.
 */
class DominioTest {

    /** El Builder arma la lavadora paso a paso y el precio final aplica los extras. */
    @Test
    void elBuilderArmaLaLavadoraYCalculaElPrecioFinal() {
        Lavadora lavadora = Lavadora.builder()
                .codigo(900)
                .marca("Whirlpool")
                .precioBase(1000000)
                .fechaFabricacion(LocalDateTime.of(2025, 5, 1, 10, 0))
                .capacidadCargaKilos(18.0) // mas de 15 kilos: + 120.000
                .velocidadCentrifugadoRPM(1400)
                .tecnologiaInverter(true)  // inverter: + 250.000
                .minutosLavado(60)
                .build();

        assertEquals("Whirlpool", lavadora.getMarca());
        assertEquals(18.0, lavadora.getCapacidadCargaKilos());
        assertTrue(lavadora.isTecnologiaInverter());
        assertEquals(1370000.0, lavadora.calcularPrecioFinal());
    }

    /** La relacion maestro-detalle se guarda por clave, no como una lista en Lavadora. */
    @Test
    void elMantenimientoSeRelacionaConLaLavadoraPorCodigo() {
        Lavadora lavadora = Lavadora.builder()
                .codigo(901)
                .marca("Samsung")
                .precioBase(900000)
                .fechaFabricacion(LocalDateTime.of(2025, 1, 1, 8, 0))
                .capacidadCargaKilos(12.0)
                .velocidadCentrifugadoRPM(1000)
                .tecnologiaInverter(false)
                .minutosLavado(30)
                .build();
        RepositorioMemoria repositorio = RepositorioMemoria.getInstance();
        repositorio.guardar(lavadora);
        Mantenimiento primero = Mantenimiento.builder()
                .id(1)
                .descripcion("Limpieza de filtro")
                .tecnico("Carlos Perez")
                .costo(50000)
                .fecha(LocalDateTime.of(2025, 2, 1, 9, 0))
                .completado(true)
                .build();
        primero.setCodigoLavadora(lavadora.getCodigo());
        repositorio.guardarMantenimiento(primero);
        Mantenimiento segundo = Mantenimiento.builder()
                .id(2)
                .descripcion("Revision de mangueras")
                .tecnico("Laura Gomez")
                .costo(70000)
                .fecha(LocalDateTime.of(2025, 3, 1, 9, 0))
                .completado(false)
                .build();
        segundo.setCodigoLavadora(lavadora.getCodigo());
        repositorio.guardarMantenimiento(segundo);

        assertEquals(2, repositorio.mantenimientosPorLavadora(lavadora.getCodigo()).size());
        assertTrue(repositorio.buscarMantenimiento(2).isPresent());
        repositorio.eliminar(lavadora.getCodigo());
    }

    /** Las validaciones responden con un mensaje claro cuando el dato esta mal. */
    @Test
    void lasValidacionesRechazanDatosInvalidos() {
        assertThrows(IllegalArgumentException.class, () -> Validaciones.texto("   ", "La marca"));
        assertThrows(IllegalArgumentException.class, () -> Validaciones.positivo(0, "El precio"));
        assertThrows(IllegalArgumentException.class, () -> Validaciones.noNegativo(-1, "El costo"));
        assertThrows(IllegalArgumentException.class, () -> Validaciones.enteroPositivo(0, "El codigo"));
        assertThrows(IllegalArgumentException.class, () -> Validaciones.rango(400, 1, 300, "Los minutos"));
        assertThrows(IllegalArgumentException.class, () -> Validaciones.fecha("03/02/2025"));
    }

    /** El repositorio es Singleton: dos llamadas devuelven exactamente el mismo objeto. */
    @Test
    void elRepositorioEsSingleton() {
        RepositorioMemoria primero = RepositorioMemoria.getInstance();
        RepositorioMemoria segundo = RepositorioMemoria.getInstance();
        assertSame(primero, segundo);
    }
}
