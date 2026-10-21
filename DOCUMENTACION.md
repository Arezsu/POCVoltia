# Documentacion Tecnica de Arquitectura y Patrones - Proyecto Voltia

Documento de sustentacion academica para el curso Diseno de Soluciones (2026B).

Autores:
- Juan Andres Bermeo Alvarez (2220241053)
- Alejandro Sanchez Quimbayo (2220241077)
- David Arredondo (2220241062)

---

## 1. Arquitectura General del Sistema

El proyecto esta estructurado como un sistema distribuido bajo el patron cliente-servidor mediante servicios web GraphQL sobre protocolo HTTP.

```text
+-------------------------+            +-------------------------+
|   Cliente 1 (C# WPF)    |            |   Cliente 2 (Python)    |
|     (Puro Codigo)       |            |        (Tkinter)        |
+-------------------------+            +-------------------------+
             \                                      /
              \                                    /
          GraphQL POST                        GraphQL POST
          (Queries/Mutations)                 (Queries/Mutations)
                \                                /
                 v                              v
            +----------------------------------------+
            |      Servidor Java Spring Boot 3       |
            |            (Puerto 8081)               |
            |                                        |
            |  +----------------------------------+  |
            |  |      Controladores GraphQL       |  |
            |  +----------------------------------+  |
            |  |      Servicios de Negocio        |  |
            |  +----------------------------------+  |
            |  | Repositorio en Memoria Singleton |  |
            |  +----------------------------------+  |
            +----------------------------------------+
```

---

## 2. Los Cuatro Patrones de Diseno Exigidos

### A. Patron MVC (Modelo - Vista - Controlador)
- **Modelo:** Define la estructura y el comportamiento de los datos. En el servidor (`com.example.ServidorLavadora.model`) residen las clases `Electrodomestico`, `Lavadora`, `Mantenimiento` y `ProgramaLavado`. En los clientes, son estructuras de datos que representan la respuesta del servidor.
- **Vista:** Interfaces graficas de usuario. Cada caso de uso (adicionar, consultar, actualizar, eliminar, listar) dispone de su propia ventana independiente. En C# se estructuran en `ClienteLavadora/gui/` y en Python en `voltia/vista/`.
- **Controlador:** Gestiona la comunicacion entre la vista y el modelo. En el servidor, `LavadoraController.java` mapea las operaciones `@QueryMapping` y `@MutationMapping`. En los clientes, coordinan la validacion de campos y el envio de peticiones asincronas.

### B. Patron Singleton
- **Proposito:** Garantizar una unica instancia de un objeto para evitar inconsistencias de estado y consumo excesivo de memoria.
- **En el Servidor (`RepositorioMemoria.java`):** Garantiza que todos los hilos del servidor lean y escriban sobre la misma lista en memoria (`List<Lavadora>`). Si hubiera multiples instancias, una lavadora creada por un cliente no seria visible para el otro.
- **En los Clientes:** La conexion HTTP GraphQL (`ClienteGraphQL` en C# y `GraphQLCliente` en Python) es un Singleton. Asi no se abren sockets de red repetitivos e innecesarios en cada llamada.

### C. Patron Builder
- **Proposito:** Separar la construccion de un objeto complejo de su representacion, permitiendo crear objetos paso a paso de forma legible.
- **Ubicacion:** `LavadoraBuilder.java` (y metodo `Lavadora.builder()` en Java, C# y Python).
- **Por que se usa:** La clase `Lavadora` tiene muchos atributos (codigo, marca, precioBase, fechaFabricacion, capacidad, RPM, inverter, minutos). Si se utilizara un constructor tradicional, tendriamos mas de 8 parametros obligatorios seguidos, lo que provoca errores de asignacion y codigo dificil de mantener. Con Builder se escriben llamadas encadenadas claras:
  ```java
  Lavadora lav = Lavadora.builder()
      .codigo(101)
      .marca("Whirlpool")
      .precioBase(1500000)
      .capacidadCargaKilos(18.5)
      .tecnologiaInverter(true)
      .build();
  ```

### D. Patron Observer
- **Proposito:** Definir una dependencia uno-a-muchos entre objetos para que cuando el estado de uno cambie, todos sus dependientes sean notificados automaticamente.
- **Ubicacion:** En la funcionalidad de listar (`VentanaListar`).
- **Como opera:** La ventana de listado se suscribe como observadora (`IObservador` o suscripcion a eventos). Cuando en otra ventana (por ejemplo, `VentanaAgregar` o `VentanaEliminar`) se confirma una operacion exitosa en el servidor, se dispara una notificacion global. La ventana de listado recibe el evento y vuelve a consultar el servidor para refrescar la grilla sin que el usuario tenga que presionar un boton manual de recargar.

---

## 3. Principios SOLID Aplicados

- **S - Responsabilidad Unica (Single Responsibility Principle):** Cada clase tiene una unica razon para cambiar. Por ejemplo, `LavadoraService` unicamente aplica reglas de negocio, mientras que `LavadoraController` unicamente serializa peticiones GraphQL.
- **O - Abierto / Cerrado (Open/Closed Principle):** La jerarquia de `Electrodomestico` esta abierta a extension para nuevos aparatos sin modificar el codigo base existente.
- **L - Sustitucion de Liskov (Liskov Substitution Principle):** Cualquier instancia de `Lavadora` puede ser tratada como un `Electrodomestico` sin alterar la correccion del sistema.
- **I - Segregacion de Interfaces (Interface Segregation Principle):** Interfaces pequenas y focalizadas (`AlmacenDatos`, `IObservador`, `ICampos`) para no forzar a una clase a implementar metodos que no utiliza.
- **D - Inversion de Dependencias (Dependency Inversion Principle):** Los controladores y servicios dependen de la abstraccion `AlmacenDatos`, no de la implementacion concreta de almacenamiento.

---

## 4. Explicacion de Funciones Avanzadas en Nivel Junior - Medio

Esta seccion explica de manera simple y directa, ideal para responder preguntas del profesor, los mecanismos internos que hacen que el proyecto sea robusto:

### 1. Manejo de Concurrencia e Hilos (Multithreading)
- **El problema comun:** Cuando una aplicacion hace una peticion a traves de internet (GraphQL), la respuesta puede demorar 200 milisegundos o 2 segundos. Si esa llamada se hace en el hilo principal (el hilo de la interfaz grafica), la ventana se congela, dice "No responde" y el usuario no puede mover el mouse ni ver animaciones.
- **Nuestra solucion:**
  - En C#: Usamos llamadas asincronas con `async` y `await` apoyadas en tareas (`Task.Run`). La peticion de red viaja en un hilo secundario en segundo plano. Cuando el servidor responde, usamos el despachador de la interfaz para actualizar los campos sin bloquear la ventana.
  - En Python: Las operaciones de red se ejecutan delegando al planificador de eventos de Tkinter (`after`) o hilos ligeros (`threading.Thread`), permitiendo que el bucle de eventos graficos continue dibujando a 60 cuadros por segundo.

### 2. Animaciones y Diseno Visual sin XAML
- **El problema comun:** Muchos proyectos usan interfaces grises o prediseñadas que parecen plantillas sin personalidad.
- **Nuestra solucion:**
  - En C#: La compuerta redonda de la lavadora (`OjoDeBuey.cs`) y el chasis (`MarcoLavadora.cs`) se dibujan directamente mediante codigo estructurado con `System.Windows.Shapes` y pinceles vectoriales de degradado (`LinearGradientBrush` y `RadialGradientBrush`).
  - Al pasar el raton por encima o al realizar operaciones, se activan animaciones continuas de rotacion (`DoubleAnimation` aplicada a un `RotateTransform`), simulando que la ropa gira adentro del tambor de lavado. Todo esto sin escribir ni una sola linea de XAML.
  - En Python: Tkinter aplica un tema oscuro uniforme con contrastes cian y dorados, marcos contenedores con borde simulado y botones estilizados.

### 3. El Cliente GraphQL y la Construccion de Consultas
- **Como funciona por dentro:** GraphQL no utiliza URLs distintas para cada accion (como haria REST). En su lugar, todas las peticiones viajan por el metodo POST a la misma URL (`/graphql`).
- Lo que cambia es el "cuerpo" (body) en formato JSON, donde enviamos una cadena de texto con la consulta exacta que necesitamos:
  - Para consultar: enviamos `query { lavadoras { codigo marca precioFinal } }`. El servidor solo nos devuelve esos tres campos exactos, ahorrando ancho de banda.
  - Para modificar: enviamos `mutation { crearLavadora(input: {...}) { codigo } }`.

### 4. Validaciones de Dominio y Calculo Polimorfico
- En el metodo `calcularPrecioFinal()` de `Lavadora`:
  - Se toma el `precioBase` heredado de `Electrodomestico`.
  - Si la capacidad supera los 15 kg, se incrementan 120.000 unidades monetarias.
  - Si cuenta con tecnologia Inverter, se adicionan 250.000 unidades.
- Esto demuestra herencia, polimorfismo y sobreescritura (`@Override` / `override`) en todas las capas del sistema.
