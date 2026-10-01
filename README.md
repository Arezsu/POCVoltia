# Voltia - Gestor de Lavadoras

Segundo Taller 2026B - Diseño de Soluciones  
Facultad de Ingeniería - Universidad de Ibagué

Aplicación web distribuida para gestionar lavadoras (CRUD). Un **servidor en Java** guarda los datos y los expone con **servicios web GraphQL**. Un **cliente en C#** con interfaz gráfica los consume. Cada parte usa un lenguaje distinto, así se comprueba que la comunicación funciona entre tecnologías diferentes.

## Integrantes

| Nombre | Código |
|---|---|
| Juan Andres Bermeo Alvarez | 2220241053 |
| Alejandro Sanchez Quimbayo | 2220241077 |
| David Arredondo | 2220241062 |

Versión de la aplicación: **1.0.0**

## Tecnologías

| Parte | Tecnología | IDE |
|---|---|---|
| Servidor | Java 17+, Spring Boot 4.1.1, Spring GraphQL, Maven | IntelliJ IDEA |
| Cliente | C# WinForms, .NET Framework 4.8, GraphQL.Client | Visual Studio 2026 |

## Cómo funciona

```
 Cliente C# (ventanas)  --- HTTP + GraphQL --->  Servidor Java (Spring Boot)
 Visual Studio 2026      <------ JSON --------   IntelliJ  -  lista en memoria
                          http://localhost:8081/graphql
```

El cliente nunca guarda datos. Todo se guarda en una lista (`ArrayList`) dentro del servidor, en memoria principal. Por eso, si se apaga el servidor, los datos se pierden y vuelven las 4 lavadoras de ejemplo. Varios clientes pueden conectarse al mismo servidor al tiempo.

## Atributos de Lavadora

| Atributo | Tipo |
|---|---|
| codigo | int |
| marca | String |
| precioBase | double |
| fechaFabricacion | LocalDateTime |
| capacidadKilos | double |
| funcionSecado | boolean |

`Lavadora` hereda de la clase abstracta `Electrodomestico`.

## Casos de uso

Cada caso de uso tiene su propia ventana.

| Caso de uso | Qué hace |
|---|---|
| Adicionar | Crea una lavadora nueva. Suena una lavadora cuando se guarda. |
| Consultar | Busca **una** lavadora por su código y muestra todos sus datos. |
| Listar | Muestra todas las lavadoras en una grilla. Se puede filtrar por marca y por función de secado. El filtro lo hace el servidor. |
| Actualizar | Busca por código, muestra todo y deja cambiar uno o varios campos. |
| Eliminar | Busca por código, muestra todo y pide confirmación antes de borrar. |

La ventana principal tiene un menú para ir a cada uno y el menú **Ayuda > Acerca de...** muestra los integrantes y la versión.

## Estructura del proyecto

```
ProyectoLavadora/
├── ServidorLavadora/                  (Java - IntelliJ)
│   ├── pom.xml
│   └── src/main/
│       ├── java/com/example/ServidorLavadora/
│       │   ├── ServidorLavadoraApplication.java
│       │   ├── controller/   LavadoraController, ManejadorExcepcionesGraphQL
│       │   ├── model/        Electrodomestico, Lavadora, LavadoraInput, LavadoraUpdateInput
│       │   └── servicios/    ServicioLavadora (lista en memoria)
│       └── resources/
│           ├── application.properties
│           └── graphql/schema.graphqls
└── ClienteLavadora/                   (C# - Visual Studio)
    ├── ClienteLavadora.slnx
    ├── packages/                      (paquetes NuGet incluidos)
    └── ClienteLavadora/
        ├── Form*.cs                   (una ventana por caso de uso)
        ├── ClienteGraphQL.cs          (llamadas al servidor)
        ├── Tema.cs, PanelMetalico.cs  (colores y estilo)
        ├── model/                     (clases de datos)
        ├── Assets/                    (logo y sonido)
        └── App.config                 (dirección del servidor)
```

## Cómo ejecutarlo

Primero se inicia el servidor y después el cliente.

### 1. Servidor (IntelliJ IDEA)

1. Abrir la carpeta `ServidorLavadora` en IntelliJ (File > Open y elegir el `pom.xml`).
2. Esperar a que Maven descargue las dependencias. La primera vez necesita internet.
3. Ejecutar la clase `ServidorLavadoraApplication`.
4. El servidor queda en `http://localhost:8081/graphql`.

Para probarlo sin el cliente se puede abrir `http://localhost:8081/graphiql` en el navegador.

Se necesita un JDK 17 o superior.

### 2. Cliente (Visual Studio 2026)

1. Abrir `ClienteLavadora/ClienteLavadora.slnx`.
2. Presionar **F5**.

Los paquetes NuGet ya vienen en la carpeta `packages`. Se necesita .NET Framework 4.8.

Si el servidor está en otro computador, se cambia `localhost` por su IP en `ClienteLavadora/ClienteLavadora/App.config`:

```xml
<add key="UrlServidor" value="http://192.168.1.10:8081/graphql" />
```

## Ejemplos de GraphQL

Se pueden probar en `http://localhost:8081/graphiql`.

```graphql
# Listar todas
{ lavadoras { codigo marca precioBase fechaFabricacion capacidadKilos funcionSecado } }

# Consultar una por código
{ lavadoraPorCodigo(codigo: 101) { codigo marca } }

# Listar con filtro (los dos parámetros son opcionales)
{ lavadorasPorFiltro(marca: "lg", funcionSecado: true) { codigo marca } }

# Adicionar
mutation {
  addLavadora(input: {
    codigo: 200, marca: "Mabe", precioBase: 900000,
    fechaFabricacion: "2025-02-01T10:00:00",
    capacidadKilos: 12.5, funcionSecado: false
  }) { codigo marca }
}

# Actualizar (solo se envía lo que cambia)
mutation { actualizarLavadora(codigo: 200, input: { marca: "Mabe Pro" }) { codigo marca } }

# Eliminar
mutation { eliminarLavadora(codigo: 200) }
```

El servidor valida los datos. Por ejemplo, rechaza códigos repetidos, marcas vacías, valores menores o iguales a cero y fechas mal escritas. El mensaje de error llega hasta el cliente.

## Problemas comunes

**Windows bloquea el programa** (mensajes como "Una directiva de Control de aplicaciones bloqueó este archivo" o "No se pudo procesar el archivo porque se encuentra en Internet")  
Windows marca como peligrosos los archivos que vienen de un zip descargado. Se arregla así:
1. Borrar la carpeta del proyecto.
2. Clic derecho sobre el zip > Propiedades > marcar **Desbloquear** > Aplicar.
3. Descomprimir en una carpeta simple, por ejemplo `C:\Taller2`, y no en Descargas.

También sirve abrir PowerShell en la carpeta y ejecutar `Get-ChildItem -Recurse | Unblock-File`.
Si el proyecto viene de `git clone`, este problema no aparece.

**"No se pudo conectar con el servidor GraphQL"**  
El servidor no está corriendo. Hay que iniciarlo primero en IntelliJ. Si está en otro computador, revisar la IP en `App.config` y permitir el puerto 8081 en el firewall.

**El puerto 8081 está ocupado**  
Se cambia `server.port` en `ServidorLavadora/src/main/resources/application.properties` y la misma dirección en `App.config` del cliente.

**Maven no descarga las dependencias**  
Revisar la conexión a internet y volver a cargar el proyecto (clic derecho sobre `pom.xml` > Maven > Reload project).
