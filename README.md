# Voltia - Gestor de Electrodomesticos

Segundo Proyecto 2026B - Diseno de Soluciones - Universidad de Ibague.

Aplicacion distribuida con servicios web GraphQL: un servidor en Java guarda los datos y
dos clientes de escritorio, hechos en lenguajes distintos (C# y Python), los consumen.

| Parte | Tecnologia | Carpeta |
| :--- | :--- | :--- |
| Servidor | Java 17, Spring Boot, Spring GraphQL | `ServidorLavadora` |
| Cliente 1 | C# (.NET Framework 4.8, WPF sin XAML) | `ClienteCSharp` |
| Cliente 2 | Python 3 (Tkinter) | `ClientePython` |

## Integrantes

| Nombre | Codigo |
| :--- | :--- |
| Juan Andres Bermeo Alvarez | 2220241053 |
| Alejandro Sanchez Quimbayo | 2220241077 |
| David Arredondo | 2220241062 |

## Los dos objetos (maestro - detalle)

```
ProgramaLavado  1..1  <------  0..*  Lavadora
   (maestro)                         (detalle)
```

Un programa de lavado lo usan muchas lavadoras y cada lavadora usa un solo programa
(el que cubre sus minutos de lavado).

| Objeto | Atributos |
| :--- | :--- |
| ProgramaLavado | codigo (int), nombre (String), minMinutos (int), maxMinutos (int), consumoAguaLitros (double), fechaCreacion (LocalDateTime), aguaCaliente (boolean) |
| Lavadora | codigo (int), marca (String), precioBase (double), fechaFabricacion (LocalDateTime), capacidadCargaKilos (double), velocidadCentrifugadoRPM (int), tecnologiaInverter (boolean), minutosLavado (int), programa (maestro) |

Los datos se guardan en memoria, en dos colecciones (ArrayList) dentro del servidor.
Lavadora hereda de la clase abstracta Electrodomestico (codigo, marca, precio base y fecha).

## Como ejecutarlo

Primero el servidor y despues los clientes.

### 1. Servidor (IntelliJ)

1. Abrir la carpeta `ServidorLavadora`.
2. Ejecutar `ServidorLavadoraApplication`.
3. Queda escuchando en `http://localhost:8081/graphql`.
4. Para probarlo sin cliente: `http://localhost:8081/graphiql`. Los codigos de prueba estan en
   `pruebas_graphiql_voltia.txt`.

### 2. Cliente en C# (Visual Studio)

1. Abrir `ClienteCSharp/ClienteLavadora.slnx`.
2. Ejecutar con F5. La primera vez Visual Studio descarga los paquetes de NuGet (necesita internet).
3. Si el servidor esta en otro computador, cambiar `localhost` por su IP en
   `ClienteCSharp/ClienteLavadora/App.config`.

### 3. Cliente en Python

1. Tener Python 3.9 o mas (el instalador de python.org ya trae Tkinter). No hay que instalar paquetes.
2. Abrir una terminal en la carpeta `ClientePython` y ejecutar:

```
python main.py
```

3. Si el servidor esta en otro computador, definir la variable de entorno `VOLTIA_URL`
   (ejemplo `http://192.168.1.20:8081/graphql`).

## Ventanas (una por caso de uso)

Los dos clientes tienen las mismas ventanas, para los dos objetos:

- Principal: menu (Archivo, Programa de lavado, Lavadora, Ayuda), logo y botones rapidos.
- Adicionar: crea el objeto.
- Consultar: busca UN objeto por uno o dos criterios (codigo y/o marca o nombre).
- Actualizar: primero busca, muestra todos los datos y cambia uno o varios.
- Eliminar: primero busca, muestra todos los datos y pide confirmacion.
- Listar: tabla con 2 filtros hechos en el servidor. Se recarga sola si otra ventana cambia datos.
- Ayuda > Acerca de: integrantes y version.

## Patrones de diseno y SOLID

Resumen: MVC, Singleton, Builder y Observer en el servidor y en los clientes.
El detalle de donde esta cada uno esta en `DOCUMENTACION.md`.

## Estructura de carpetas

```
ServidorLavadora/src/main/java/com/example/ServidorLavadora
  controller/   controladores GraphQL (la C de MVC)
  model/        Electrodomestico, Lavadora, ProgramaLavado y los inputs
  repositorio/  AlmacenDatos (interface), RepositorioMemoria (Singleton), observadores
  servicios/    logica de cada objeto
  util/         validaciones
  config/       configuracion de Spring

ClienteCSharp/ClienteLavadora
  gui/          ventanas y controles, todo escrito en C# (sin XAML)
  model/        datos que llegan del servidor
  servicios/    ClienteGraphQL (Singleton) y servicios por objeto
  util/         animaciones, sonidos, notificador (Observer)

ClientePython/voltia
  modelos.py, servicios.py, graphql.py, controladores.py, observador.py
  vista/        ventanas, formularios y tema
```

## Entrega en GitHub

1. Crear un repositorio nuevo en GitHub (vacio, sin README).
2. En la carpeta del proyecto ejecutar:

```
git init
git add .
git commit -m "Segundo proyecto Voltia"
git branch -M main
git remote add origin https://github.com/USUARIO/NOMBRE-DEL-REPOSITORIO.git
git push -u origin main
```

3. Compartirlo con el correo del profesor: en GitHub entrar a Settings, luego Collaborators,
   Add people y escribir `carlos.lugo@unibague.edu.co` (o dejar el repositorio publico y enviar el enlace).
