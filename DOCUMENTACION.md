# Documentacion tecnica y guia de sustentacion - Proyecto Voltia

Segundo Proyecto 2026B - Diseno de Soluciones - Universidad de Ibague.

Autores:

- Juan Andres Bermeo Alvarez (2220241053)
- Alejandro Sanchez Quimbayo (2220241077)
- David Arredondo (2220241062)

Este documento explica **como esta construido** el sistema, **por que** se tomo cada
decision y **que responder** en la sustentacion. Esta escrito en lenguaje sencillo, para
que cualquier integrante pueda defenderlo sin memorizar el codigo.

---

## 1. Idea general del sistema

Voltia es un sistema distribuido de tres piezas que se comunican por la red:

1. **Servidor Java (Spring Boot + GraphQL):** guarda todos los datos en memoria y expone
   un servicio web con consultas y mutaciones.
2. **Cliente C# (WPF sin XAML):** aplicacion de escritorio para Windows.
3. **Cliente web (JavaScript puro):** aplicacion que corre en el navegador.

Los tres hablan el mismo idioma: **GraphQL**. El cliente no pide "el recurso lavadoras/101"
como en REST, sino que describe exactamente que datos necesita, y el servidor responde
solo con esos campos.

```text
Cliente C#  ---- POST /graphql ---->  Servidor Java  <---- POST /graphql ---- Cliente web
   |                                      |
   |  MVC + Singleton + Builder           |  controller -> servicio -> repositorio
   |  Observer + async/await              |  datos en memoria (listas concurrentes)
   |                                      |
   +---- refresco automatico por versionDatos ----+
```

La comunicacion siempre va del cliente al servidor por HTTP (POST). Ningun cliente se
conecta directamente con el otro; toda la sincronizacion pasa por el servidor.

---

## 2. Por que una relacion Maestro - Detalle y no "un programa por lavadora"

La guia de la asignatura pide una relacion **1..1 a 0..*** (Figura 1) y advierte que no se
debe asumir que una lavadora tiene un solo programa fijo. En la version inicial del
proyecto se modelo al reves: el "programa de lavado" era el maestro y cada lavadora
quedaba atada a un unico programa. Eso tiene dos problemas:

1. **No representa la realidad.** Un mismo modelo de lavadora puede tener varios
   mantenimientos a lo largo de su vida, o ninguno si es nueva.
2. **No cumple la cardinalidad pedida.** La relacion maestro-detalle exige que el detalle
   sea **muchos** (0..*), no uno.

La correccion fue:

- **Maestro: Lavadora.** El objeto principal, el que se registra, se consulta y se elimina.
- **Detalle: Mantenimiento.** Una lista dentro de la lavadora, con **cero, uno o muchos**
  elementos.

Consecuencias practicas:

- Eliminar una lavadora elimina sus mantenimientos (composicion).
- Los mantenimientos no existen por fuera de su lavadora: todas las operaciones del
  detalle piden el `codigoLavadora`.
- El servidor calcula campos utiles del detalle: `cantidadMantenimientos`,
  `mantenimientosPendientes` y `costoMantenimientos`.
- Los datos de ejemplo incluyen una lavadora con **cero** mantenimientos, otra con uno y
  dos con varios, para demostrar la cardinalidad completa.

---

## 3. Modelo de clases del servidor

### 3.1 Electrodomestico (clase abstracta)

Contiene lo que comparten todos los electrodomesticos: `codigo` (int), `marca` (String),
`precioBase` (double) y `fechaFabricacion` (LocalDateTime). Define el metodo
`calcularPrecioFinal()` que cada hija implementa a su manera (polimorfismo).

### 3.2 Lavadora (maestro, 1..1)

Hereda los cuatro atributos anteriores y agrega:

- `capacidadCargaKilos` (double)
- `velocidadCentrifugadoRPM` (int)
- `tecnologiaInverter` (boolean)
- `minutosLavado` (int)
- `mantenimientos` (lista concurrente del detalle)

Ademas calcula el precio final:

```java
@Override
public double calcularPrecioFinal() {
    double precio = getPrecioBase();
    if (capacidadCargaKilos > 15) {
        precio += 120000;
    }
    if (tecnologiaInverter) {
        precio += 250000;
    }
    return precio;
}
```

### 3.3 Mantenimiento (detalle, 0..*)

Es el registro hijo. Tiene seis atributos: `id` (int), `descripcion` (String),
`tecnico` (String), `costo` (double), `fecha` (LocalDateTime) y `completado` (boolean).
El `id` no lo manda el cliente: lo calcula el servidor tomando el mayor id de esa lavadora
y sumandole uno. Asi nunca se repite dentro de la misma lavadora.

### 3.4 Patron Builder en el modelo

Los dos objetos tienen muchos atributos. Un constructor con ocho parametros seguidos es
dificil de leer y de mantener, y ademas es facil equivocarse en el orden. El Builder
permite escribir el codigo asi:

```java
Lavadora lavadora = Lavadora.builder()
    .codigo(101)
    .marca("LG")
    .precioBase(1450000)
    .fechaFabricacion(LocalDateTime.of(2024, 3, 10, 9, 0))
    .capacidadCargaKilos(18.0)
    .velocidadCentrifugadoRPM(1400)
    .tecnologiaInverter(true)
    .minutosLavado(45)
    .build();
```

Cada metodo del Builder devuelve el mismo Builder (`return this`), por eso se pueden
encadenar las llamadas. El constructor de la clase es privado: la unica forma de crear un
objeto es a traves de `build()`.

---

## 4. El contrato GraphQL

El archivo `ServidorLavadora/src/main/resources/graphql/schema.graphqls` es el contrato:
ahi se define que tipos de datos existen, que consultas se pueden hacer y que mutaciones
se pueden ejecutar.

### 4.1 Una sola direccion, un solo metodo HTTP

Todas las peticiones son `POST` a `http://localhost:8081/graphql`. Lo que cambia es el
cuerpo JSON:

```json
{
  "query": "query { lavadoras { codigo marca precioFinal } }"
}
```

O con variables, que es la forma recomendada porque evita errores de comillas:

```json
{
  "query": "query Buscar($codigo: Int!) { lavadoraPorCodigo(codigo: $codigo) { codigo marca } }",
  "variables": { "codigo": 101 }
}
```

### 4.2 Queries y mutations

- **Queries:** solo leen. No cambian nada en el servidor.
- **Mutations:** crean, actualizan o eliminan. Siempre responden con el objeto afectado o
  con un booleano.

### 4.3 Filtros en el servidor

El cliente no descarga toda la lista para filtrarla en memoria. Envia los criterios y el
servidor filtra:

- `lavadorasPorFiltro(marca, tecnologiaInverter, conMantenimientos)`
- `mantenimientosPorFiltro(codigoLavadora, tecnico, completado)`

Ventajas: menos datos por la red, menos trabajo en el cliente y una sola regla de filtrado
para los dos clientes.

### 4.4 Como se representan las fechas

GraphQL no tiene un tipo de dato "fecha". Por eso `fechaFabricacion` y `fecha` se declaran
como `String` en el esquema y el servidor las entrega con el formato
`yyyy-MM-ddTHH:mm:ss`. En Java eso se logra con `@SchemaMapping` en el controlador:

```java
@SchemaMapping(typeName = "Lavadora", field = "fechaFabricacion")
public String fechaFabricacion(Lavadora lavadora) {
    return lavadora.getFechaFabricacion().format(Validaciones.FORMATO_FECHA);
}
```

El cliente debe enviar las fechas en ese mismo formato. Si llega otro formato, el servidor
responde con un mensaje de error que explica el formato correcto.

### 4.5 Manejo de errores

`ManejadorExcepcionesGraphQL` convierte cualquier `IllegalArgumentException` (codigo
repetido, fecha invalida, lavadora inexistente) en un error GraphQL con el mensaje de
negocio. Los clientes leen ese mensaje y lo muestran tal cual, por ejemplo:

```text
Ya existe una lavadora con el codigo 101
La velocidad de centrifugado minima debe ser 600 RPM
Escriba al menos un criterio de busqueda (codigo o marca)
```

### 4.6 versionDatos: la senal de "algo cambio"

Cada vez que el repositorio notifica un cambio, sube en 1 un contador `AtomicInteger`.
La query `versionDatos` devuelve ese numero. Los clientes lo consultan cada pocos segundos:
si el numero cambio, vuelven a pedir la lista. Esto permite que los dos clientes se vean
entre si sin necesidad de que uno avise al otro.

> En un sistema en produccion se usaria una suscripcion de GraphQL (WebSocket). Aqui se
> uso `versionDatos` porque es mas simple, se entiende en una sustentacion y no agrega
> dependencias.

---

## 5. Recorrido completo de una operacion

### 5.1 Listar lavadoras (cliente C#)

1. El usuario abre `VentanaListar` y presiona "Filtrar".
2. La ventana no habla con la red: llama a `ControladorLavadoras.ListarAsync(...)`.
3. El controlador envuelve la llamada en `Task.Run` para que la peticion viaje en un hilo
   de fondo y la ventana no se congele.
4. El servicio `ServicioLavadoras` arma la consulta GraphQL y la envia con el
   `ClienteGraphQL` (Singleton).
5. En el servidor, `LavadoraController.lavadorasPorFiltro` recibe los argumentos y delega
   en `ServicioLavadora.listarPorFiltro`.
6. El servicio entra al candado (`synchronized (almacen.candado())`), lee la lista
   concurrente, aplica los filtros y devuelve solo las lavadoras que cumplen.
7. GraphQL arma el JSON con los campos pedidos, incluidos los mantenimientos.
8. El controlador recibe la lista, avisa por `NotificadorDatos` y la vista actualiza la
   grilla con `Dispatcher.Invoke` (si el aviso viene de otro hilo).

### 5.2 Agregar un mantenimiento (cliente web)

1. El usuario abre el modal "Mantenimientos", elige una lavadora y llena el formulario.
2. La vista valida los campos y llama a `ControladorMantenimientos.AgregarAsync(...)`.
3. El servicio arma la mutacion `addMantenimiento` y la envia con `fetch`.
4. `MantenimientoController.addMantenimiento` busca la lavadora (si no existe, responde
   error) y delega en `ServicioMantenimiento.agregar`.
5. El servicio valida los datos, calcula el siguiente id, crea el `Mantenimiento` con el
   Builder, lo agrega a la lista de la lavadora y notifica el cambio.
6. El controlador actualiza la tabla, muestra un aviso de exito y el vigilante de
   `versionDatos` hara que el otro cliente se refresque solo.

### 5.3 Eliminar una lavadora

La regla es "eliminar previa busqueda":

1. La ventana pide el codigo (o la marca) y **busca** con `lavadoraPorCriterios`.
2. Si no existe, muestra el error y no permite continuar.
3. Si existe, muestra los datos completos y pide confirmacion.
4. Al confirmar, se ejecuta `eliminarLavadora(codigo)`; el servicio vuelve a buscar dentro
   del candado antes de borrar (para evitar que alguien la haya borrado antes).
5. Se eliminan tambien sus mantenimientos, porque el detalle esta dentro del maestro.

La misma logica se repite en las pantallas de actualizar y en las de mantenimientos.

---

## 6. Los cuatro patrones, explicados para la sustentacion

### 6.1 MVC (Modelo - Vista - Controlador)

**Problema:** si la ventana arma consultas HTTP y ademas dibuja botones, cualquier cambio
en la red obliga a tocar la interfaz, y cualquier cambio visual obliga a tocar la logica.
Eso es codigo dificil de mantener.

**Solucion:** tres responsabilidades separadas.

| Capa | En el servidor | En el cliente C# | En el cliente web |
| :--- | :--- | :--- | :--- |
| Modelo | `model/` | `modelo/` | `js/modelo.js` |
| Vista | `schema.graphqls` y GraphiQL | `vista/` | `js/vista.js` + `index.html` + CSS |
| Controlador | `controller/` | `controlador/` | `js/controlador.js` |

Los servicios y el repositorio son la capa de negocio: el controlador no valida ni guarda,
solo recibe y delega.

### 6.2 Singleton

**Problema:** si el repositorio se creara varias veces, cada cliente veria una copia
distinta de los datos. Si el cliente HTTP se creara en cada llamada, se abririan sockets
innecesarios.

**Solucion:**

- `RepositorioMemoria` tiene constructor privado y un metodo `getInstance()` que crea la
  instancia la primera vez y despues siempre devuelve la misma.
- `ClienteGraphQL` (en C# y en JavaScript) es un objeto unico que se reutiliza en todas
  las peticiones.

Asi se prueba en Java:

```java
RepositorioMemoria primero = RepositorioMemoria.getInstance();
RepositorioMemoria segundo = RepositorioMemoria.getInstance();
assertSame(primero, segundo); // es el mismo objeto
```

### 6.3 Builder

**Problema:** objetos con muchos atributos generan constructores enormes y llamadas
confusas del tipo `new Lavadora(101, "LG", 1450000, ..., true, 45)`.

**Solucion:** construir paso a paso, con metodos con nombre. Se aplica en:

- `Lavadora.builder()` y `Mantenimiento.builder()` en Java.
- `ConstructorLavadora` y `ConstructorMantenimiento` en el cliente C#.
- `ConstructorLavadora` y `ConstructorMantenimiento` en el cliente web.

### 6.4 Observer

**Problema:** la ventana de listado debe mostrar los cambios que ocurren en otras ventanas
y en el otro cliente. Si no hay observador, el usuario tendria que presionar "recargar".

**Solucion:** dos niveles de observacion.

1. **Dentro de cada cliente:** un notificador con eventos (`DatosCambiaron` y `Trabajando`
   en C#; `suscribir` / `notificar` en JavaScript). La grilla se suscribe al abrirse y se
   desuscribe al cerrarse.
2. **Entre clientes:** el servidor sube `versionDatos` y cada cliente lo vigila cada pocos
   segundos. Cuando el numero cambia, dispara el mismo aviso interno.

En el servidor tambien hay Observer: `RepositorioMemoria` notifica a
`ObservadorRepositorio`, y `RegistroCambios` escribe cada cambio en la consola.

---

## 7. Principios SOLID en la practica

- **S (responsabilidad unica):** `Validaciones` solo valida; `RepositorioMemoria` solo
  guarda; `LavadoraController` solo mapea GraphQL; `ServicioLavadora` solo aplica reglas.
- **O (abierto/cerrado):** para agregar una `Secadora` se hereda de `Electrodomestico` y
  se agrega su servicio; no hay que modificar `Lavadora`.
- **L (sustitucion de Liskov):** el codigo que trabaja con `Electrodomestico` puede recibir
  una `Lavadora` y llamar `calcularPrecioFinal()` sin sorpresas.
- **I (segregacion de interfaces):** `AlmacenDatos` expone solo lo que los servicios
  necesitan; `IServicioLavadoras` e `IServicioMantenimientos` separan los dos casos de uso.
- **D (inversion de dependencias):** los servicios reciben `AlmacenDatos` (interface) y no
  `RepositorioMemoria` (clase concreta). Si manana los datos van a una base de datos, se
  crea otra implementacion y los servicios no cambian.

---

## 8. Concurrencia e hilos

### 8.1 En el servidor

El servidor atiende varias peticiones al mismo tiempo (Tomcat usa un hilo por peticion).
Dos clientes pueden estar creando o borrando lavadoras a la vez. Para que los datos no se
corrompan:

- Las listas son `CopyOnWriteArrayList`: se pueden recorrer mientras otro hilo las
  modifica. Se usan para las lavadoras y para los mantenimientos de cada lavadora.
- Las operaciones que cambian datos se hacen dentro de `synchronized (candado)`. El
  candado es un unico objeto del repositorio, asi que solo un hilo a la vez entra a la
  seccion critica.
- `versionDatos` es un `AtomicInteger`, que se puede incrementar sin candado.
- La lista de observadores tambien es concurrente.

Se probo con 40 mutaciones simultaneas: se crearon las 40 lavadoras, sin errores y sin
perdidas, y despues se eliminaron todas.

### 8.2 En el cliente C#

La interfaz grafica tiene un solo hilo. Si una peticion de red se hiciera en ese hilo, la
ventana se congelaria y el tambor dejaria de girar. Por eso:

- El controlador lanza la peticion con `Task.Run`, que la ejecuta en un hilo del grupo de
  trabajo.
- El codigo usa `async` / `await`: la ventana sigue respondiendo mientras espera.
- Cuando la respuesta llega y hay que tocar controles, se usa `Dispatcher.Invoke` para
  volver al hilo de la interfaz, porque WPF no permite modificar controles desde otro hilo.
- Mientras hay peticiones pendientes, `NotificadorDatos.Trabajando` activa el estado
  "PROCESANDO" y acelera la animacion del tambor.

### 8.3 En el cliente web

JavaScript tiene un solo hilo, pero `fetch` es asincrono: la peticion viaja y el navegador
sigue dibujando. La animacion del tambor es CSS (corre en el compositor del navegador), asi
que no se detiene mientras espera la respuesta. El controlador marca el estado
`trabajando`, la vista acelera el tambor y lo devuelve a la normalidad cuando la promesa se
resuelve o falla.

---

## 9. Diseno visual

La metafora es una lavadora industrial moderna, igual en los dos clientes:

- **Compuerta circular frontal:** aro con gradiente metalico, vidrio con gradiente radial,
  agua y ropa girando. La rotacion es continua y se acelera durante las operaciones de red,
  asi el usuario "ve" que el sistema esta trabajando.
- **Panel de control superior:** pantalla digital con el estado (LISTO, PROCESANDO, ERROR)
  y la version de los datos, en fuente monoespaciada y color cian.
- **Panel de cristal (glassmorphism tenue):** fondo oscuro semitransparente, borde claro de
  un pixel, esquinas redondeadas y desenfoque. Se usa en la vista de listado, en los
  formularios y en los mensajes.
- **Contraste limpio:** texto claro sobre fondo oscuro, acentos cian y ambar, rojo apagado
  solo para acciones destructivas. Nada de colores saturados ni plantillas genericas.
- **Cero XAML en C#:** todo se construye con objetos (`Grid`, `Border`, `Ellipse`, `Path`,
  `DataGrid`), pinceles (`LinearGradientBrush`, `RadialGradientBrush`) y animaciones
  (`DoubleAnimation` sobre `RotateTransform`).

---

## 10. Reglas de validacion (resumen)

| Dato | Regla | Mensaje si falla |
| :--- | :--- | :--- |
| codigo de lavadora | entero mayor que cero y no repetido | "Ya existe una lavadora con el codigo ..." |
| marca | texto obligatorio | "La marca es obligatoria" |
| precioBase | mayor que cero | "El precio base debe ser mayor que cero" |
| fechaFabricacion | formato `yyyy-MM-ddTHH:mm:ss` y fecha real | "Fecha invalida. Use el formato ..." |
| capacidadCargaKilos | mayor que cero | "La capacidad de carga en kilos debe ser mayor que cero" |
| velocidadCentrifugadoRPM | mayor o igual a 600 | "La velocidad de centrifugado minima debe ser 600 RPM" |
| minutosLavado | entre 1 y 300 | "Los minutos de lavado deben estar entre 1 y 300" |
| descripcion del mantenimiento | texto obligatorio | "La descripcion del mantenimiento es obligatoria" |
| tecnico | texto obligatorio | "El tecnico es obligatorio" |
| costo del mantenimiento | mayor o igual a cero | "El costo del mantenimiento no puede ser negativo" |
| fecha del mantenimiento | formato `yyyy-MM-ddTHH:mm:ss` | "Fecha invalida. Use el formato ..." |

Las mismas reglas se validan en el cliente (para responder rapido) y en el servidor (para
que no se pueda saltar la validacion con una peticion manual).

---

## 11. Pruebas realizadas

1. **Pruebas unitarias del dominio** (`mvnw test`):
   - el Builder arma la lavadora y el precio final aplica los dos extras;
   - una lavadora admite cero, uno y varios mantenimientos;
   - las validaciones rechazan datos invalidos;
   - el repositorio es Singleton;
   - el contexto de Spring arranca y el esquema GraphQL no tiene campos sin resolver.
2. **Pruebas manuales en GraphiQL:** consultas con filtros, altas, cambios, bajas y casos
   de error (ver `pruebas_graphiql_voltia.txt`).
3. **Prueba de concurrencia:** 40 mutaciones simultaneas sin errores ni perdida de datos.
4. **Pruebas de los clientes:** los dos clientes se conectan al servidor, listan, crean,
   actualizan y eliminan; el cambio hecho en uno se ve en el otro por `versionDatos`.

---

## 12. Preguntas tipicas de sustentacion (con respuesta corta)

**1. Por que GraphQL y no REST?**
Porque el cliente pide exactamente los campos que necesita. La vista de listado pide solo
las columnas de la tabla y la de consulta pide todos los campos, usando la misma URL. Con
REST habria que crear varias rutas o devolver datos que no se usan.

**2. Donde esta el maestro y donde el detalle?**
El maestro es `Lavadora` (1..1) y el detalle es `Mantenimiento` (0..*). El detalle vive
dentro de la lista de la lavadora y no existe por fuera de ella.

**3. Por que el detalle no tiene su propio repositorio?**
Porque es una composicion: los mantenimientos se guardan dentro de su lavadora. Eso
garantiza que no queden mantenimientos huerfanos y que al borrar la lavadora se borre todo
su detalle.

**4. Como se genera el id de un mantenimiento?**
El cliente no lo envia. El servidor busca el mayor id de esa lavadora, le suma uno y lo
asigna dentro del candado, para que dos peticiones simultaneas no repitan id.

**5. Que pasa si dos clientes crean la misma lavadora al mismo tiempo?**
El servicio entra al candado y verifica si el codigo existe. Como la verificacion y la
insercion estan dentro del mismo bloque sincronizado, solo una pasa; la otra recibe el
error "Ya existe una lavadora con el codigo ...".

**6. Como se evita que la ventana se congele?**
Las peticiones salen con `Task.Run` y `async`/`await`, en un hilo de fondo. La interfaz se
actualiza con `Dispatcher.Invoke`, que devuelve el control al hilo de la ventana solo
cuando hay que pintar.

**7. Por que hay un contador de version si ya existe el Observer?**
El Observer funciona dentro de un mismo proceso. Los dos clientes son procesos distintos,
asi que necesitan una senal del servidor: `versionDatos`. Cuando cambia, cada cliente
dispara su propio Observer interno y refresca la grilla.

**8. Que pasa si el servidor esta apagado?**
Los dos clientes muestran el error de conexion y el estado "sin conexion". No se pierde
nada porque los datos son del servidor; al encenderlo, el cliente vuelve a listar.

**9. Por que no hay base de datos?**
La guia pide datos en memoria. El repositorio esta detras de la interface `AlmacenDatos`,
asi que cambiar a una base de datos solo requiere una nueva implementacion.

**10. Donde se ve el patron Builder?**
En `Lavadora.builder()` y `Mantenimiento.builder()` del servidor, y en los constructores
encadenados de los dos clientes. El constructor de la clase es privado: todo pasa por el
Builder.

**11. Que es el glassmorphism y por que se uso?**
Es un estilo de paneles semitransparentes con desenfoque de fondo y borde claro. Se uso
para el panel de listado porque da sensacion de cristal tecnologico sin perder legibilidad:
fondo oscuro, texto claro y acentos medidos.

**12. Como se dibuja la lavadora sin XAML?**
Con objetos de WPF creados en C#: `Ellipse` para el aro y el vidrio, `Path` para el agua,
`Rectangle` para la ropa, gradientes para el metal y `DoubleAnimation` sobre un
`RotateTransform` para girar el tambor. No hay ningun archivo `.xaml`.

**13. Que pasa si el usuario escribe una fecha en otro formato?**
El servidor valida con `LocalDateTime.parse` y responde "Fecha invalida. Use el formato
yyyy-MM-ddTHH:mm:ss". El cliente muestra ese mensaje en un aviso.

**14. Como se cumple el principio de responsabilidad unica?**
Cada clase tiene un motivo para cambiar: `Validaciones` cambia si cambian las reglas;
`RepositorioMemoria` si cambia el almacenamiento; las ventanas si cambia el diseno. Ninguna
clase mezcla esas tres cosas.

**15. Que mejoraria si el proyecto creciera?**
Cambiar el sondeo de `versionDatos` por suscripciones GraphQL, mover los datos a una base
de datos, agregar autenticacion de usuarios y pruebas automatizadas de los clientes.

---

## 13. Limitaciones conocidas y trabajo futuro

- Los datos viven en memoria: al reiniciar el servidor se vuelve a la carga inicial.
- El refresco entre clientes usa sondeo cada pocos segundos; en produccion conviene una
  suscripcion de GraphQL.
- No hay autenticacion ni roles: cualquier cliente puede modificar los datos.
- Las pruebas automatizadas cubren el dominio y el arranque del servidor; los clientes se
  verifican de forma manual.
- El cliente C# apunta a .NET Framework 4.8 por compatibilidad con Visual Studio 2022; se
  podria migrar a .NET 8 sin cambiar la arquitectura.
