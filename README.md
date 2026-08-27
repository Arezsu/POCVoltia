# ⚡ POCVoltia - Sistema de Gestión de Electrodomésticos

<div align="center">

![Java](https://img.shields.io/badge/Java-ED8B00?style=for-the-badge&logo=openjdk&logoColor=white)
![NetBeans](https://img.shields.io/badge/NetBeans-1B6AC6?style=for-the-badge&logo=apache-netbeans&logoColor=white)
![Git](https://img.shields.io/badge/GIT-F05032?style=for-the-badge&logo=git&logoColor=white)

Un sistema robusto desarrollado en Java bajo la **Arquitectura Modelo-Vista-Controlador (MVC)** y principios de Programación Orientada a Objetos (POO), diseñado para la gestión y control de electrodomésticos (con enfoque especial en la clase `Lavadora` y su respectiva asociación a programas de lavado).

</div>

---

## 👥 Integrantes del Equipo

| Nombre | Código / ID |
| :--- | :--- |
| **Juan Andres Bermeo Alvarez** | `2220241053` |
| **Alejandro Sanchez Quimbayo** | `2220241077` |
| **David Arredondo** | `2220241062` |

---

## 🚀 Características Principales

- **Arquitectura MVC estricta:** Separación limpia entre el Modelo de clases (`Electrodomestico`, `Lavadora`, `ProgramaLavado`), la Capa de Lógica/Servicios y la Interfaz Gráfica (`JFrame`).
- **Polimorfismo:** Implementación del método `calcularPrecioFinal()` adaptado con reglas de negocio específicas para cada tipo de electrodoméstico.
- **Asociación de Clases:** Integración formal entre la clase `Lavadora` (Clase B) y `ProgramaLavado` (Clase D) cumpliendo con los lineamientos de diseño de software.
- **Interfaz Gráfica (GUI):** Formulario interactivo en Java Swing con validación de entradas de datos y manejo de excepciones.

---

## 📐 Diagrama UML del Proyecto

```mermaid
classDiagram
    class ICongelable {
        <<interface>>
        +activarSuperCongelacion() void
        +activarModoVacacionesNevera() void
    }

    class Electrodomestico {
        <<abstract>>
        -int codigo
        -String marca
        -LocalDate fechaFabricacion
        -double precioBase
        +getCodigo() int
        +setCodigo(int codigo) void
        +getMarca() String
        +setMarca(String marca) void
        +getFechaFabricacion() LocalDate
        +setFechaFabricacion(LocalDate fecha) void
        +getPrecioBase() double
        +setPrecioBase(double precioBase) void
        +calcularPrecioFinal() double
    }

    class Lavadora {
        -double capacidadKilos
        -boolean funcionSecado
        +getCapacidadKilos() double
        +setCapacidadKilos(double capacidadKilos) void
        +isFuncionSecado() boolean
        +setFuncionSecado(boolean funcionSecado) void
        +calcularPrecioFinal() double
    }

    class Nevera {
        -int capacidadLitros
        -boolean tieneNoFrost
        +getCapacidadLitros() int
        +setCapacidadLitros(int capacidadLitros) void
        +isTieneNoFrost() boolean
        +setTieneNoFrost(boolean tieneNoFrost) void
        +calcularPrecioFinal() double
        +activarSuperCongelacion() void
        +activarModoVacacionesNevera() void
    }

    class ProgramaLavado {
        -String nombre
        -int duracionMinutos
        +getNombre() String
        +setNombre(String nombre) void
        +getDuracionMinutos() int
        +setDuracionMinutos(int duracionMinutos) void
    }

    %% Relaciones basadas en tu base
    Electrodomestico <|-- Lavadora
    Electrodomestico <|-- Nevera
    ICongelable <|.. Nevera
    Lavadora "1" *-- "1..*" ProgramaLavado : asocia
