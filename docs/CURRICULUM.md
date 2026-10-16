# Mapa del currículo

Generado a partir del banco de contenidos (`src/CSharpMastery.Content`). `[x]` = lección completa con ítems verificados por ejecución real; `[ ]` = solo temario (la app lo muestra como «contenido en preparación» y no lo usa en sesiones ni evaluaciones).

| Nivel | Título | Unidades | Habilidades | Con lección | Ítems | Ejercicios de código | Casos de prueba (ocultos) |
|---|---|---|---|---|---|---|---|
| 0 | Fundamentos de programación | 3 | 10 | 10 | 88 | 30 | 67 (32) |
| 1 | Lógica de programación | 3 | 10 | 10 | 87 | 30 | 123 (69) |
| 2 | Métodos y estructuras de datos | 4 | 13 | 13 | 105 | 39 | 147 (73) |
| 3 | Programación orientada a objetos | 3 | 12 | 12 | 98 | 36 | 140 (68) |
| 4 | C# moderno y profesional | 3 | 13 | 7 | 62 | 21 | 75 (33) |
| 5 | Herramientas profesionales | 2 | 9 | 0 | 0 | 0 | 0 (0) |
| 6 | Programación asíncrona y sistemas | 2 | 10 | 0 | 0 | 0 | 0 (0) |
| 7 | Proyectos integradores | — | — | — | 12 hitos | 12 | ver sección Proyectos |

## Nivel 0: Fundamentos de programación


### n0.u1 Tu primer programa

- [x] **Qué es un programa: compilación y ejecución** (`n0.programa`). Prerrequisitos: —
- [x] **Sintaxis: instrucciones y expresiones** (`n0.sintaxis`). Prerrequisitos: n0.programa
- [x] **Salida en consola: Write, WriteLine y secuencias de escape** (`n0.salida`). Prerrequisitos: n0.sintaxis

### n0.u2 Datos y operaciones

- [x] **Variables, constantes y tipos de datos** (`n0.variables`). Prerrequisitos: n0.salida
- [x] **Operadores aritméticos** (`n0.aritmetica`). Prerrequisitos: n0.variables
- [x] **Operadores relacionales y lógicos** (`n0.relacionales`). Prerrequisitos: n0.aritmetica
- [x] **Texto: concatenación, interpolación y formato** (`n0.texto`). Prerrequisitos: n0.variables

### n0.u3 Entrada, conversión y errores

- [x] **Entrada con Console.ReadLine** (`n0.entrada`). Prerrequisitos: n0.texto
- [x] **Conversión de tipos** (`n0.conversion`). Prerrequisitos: n0.entrada, n0.aritmetica
- [x] **Errores de compilación, de ejecución y de lógica** (`n0.errores`). Prerrequisitos: n0.conversion

## Nivel 1: Lógica de programación


### n1.u1 Decisiones

- [x] **Booleanos, expresiones y cortocircuito** (`n1.booleanos`). Prerrequisitos: n0.relacionales
- [x] **if, else if y else** (`n1.if-else`). Prerrequisitos: n1.booleanos
- [x] **switch y expresiones switch** (`n1.switch`). Prerrequisitos: n1.if-else

### n1.u2 Repetición

- [x] **Bucles while y do-while** (`n1.while`). Prerrequisitos: n1.if-else
- [x] **Bucle for** (`n1.for`). Prerrequisitos: n1.while
- [x] **break y continue** (`n1.break-continue`). Prerrequisitos: n1.for
- [x] **Contadores y acumuladores** (`n1.acumuladores`). Prerrequisitos: n1.for

### n1.u3 Resolver problemas

- [x] **Validación de datos** (`n1.validacion`). Prerrequisitos: n1.while, n0.conversion
- [x] **Resolución de problemas paso a paso** (`n1.descomposicion`). Prerrequisitos: n1.acumuladores
- [x] **Análisis intuitivo de complejidad** (`n1.complejidad`). Prerrequisitos: n1.for

## Nivel 2: Métodos y estructuras de datos


### n2.u1 Métodos

- [x] **Declaración e invocación de métodos** (`n2.metodos`). Prerrequisitos: n1.acumuladores
- [x] **Parámetros y valores de retorno** (`n2.parametros`). Prerrequisitos: n2.metodos
- [x] **Alcance de variables** (`n2.alcance`). Prerrequisitos: n2.parametros
- [x] **Sobrecarga de métodos** (`n2.sobrecarga`). Prerrequisitos: n2.parametros

### n2.u2 Colecciones

- [x] **Arrays** (`n2.arrays`). Prerrequisitos: n1.for
- [x] **Matrices (arrays bidimensionales)** (`n2.matrices`). Prerrequisitos: n2.arrays
- [x] **List<T>** (`n2.listas`). Prerrequisitos: n2.arrays
- [x] **Dictionary<TKey,TValue> y HashSet<T>** (`n2.diccionarios`). Prerrequisitos: n2.listas
- [x] **Elegir la estructura de datos** (`n2.eleccion`). Prerrequisitos: n2.diccionarios

### n2.u3 Texto y tipos simples

- [x] **Strings y manipulación de texto** (`n2.strings`). Prerrequisitos: n2.arrays
- [x] **Enumeraciones** (`n2.enums`). Prerrequisitos: n1.switch
- [x] **Tuplas** (`n2.tuplas`). Prerrequisitos: n2.parametros

### n2.u4 Recursividad

- [x] **Recursividad** (`n2.recursion`). Prerrequisitos: n2.parametros, n2.arrays

## Nivel 3: Programación orientada a objetos


### n3.u1 Clases y objetos

- [x] **Clases e instancias** (`n3.clases`). Prerrequisitos: n2.parametros
- [x] **Campos, propiedades y métodos** (`n3.propiedades`). Prerrequisitos: n3.clases
- [x] **Constructores** (`n3.constructores`). Prerrequisitos: n3.propiedades
- [x] **Encapsulación y modificadores de acceso** (`n3.encapsulacion`). Prerrequisitos: n3.constructores
- [x] **Miembros estáticos y de instancia** (`n3.static`). Prerrequisitos: n3.propiedades

### n3.u2 Relaciones entre tipos

- [x] **Composición** (`n3.composicion`). Prerrequisitos: n3.encapsulacion
- [x] **Herencia** (`n3.herencia`). Prerrequisitos: n3.composicion
- [x] **Polimorfismo, sobrescritura y ocultamiento** (`n3.polimorfismo`). Prerrequisitos: n3.herencia
- [x] **Clases abstractas e interfaces** (`n3.abstractas-interfaces`). Prerrequisitos: n3.polimorfismo

### n3.u3 Tipos modernos y diseño

- [x] **Records y tipos por valor** (`n3.records`). Prerrequisitos: n3.encapsulacion
- [x] **Nulabilidad y prevención de errores con null** (`n3.nullability`). Prerrequisitos: n3.clases
- [x] **Principios SOLID con ejemplos** (`n3.solid`). Prerrequisitos: n3.abstractas-interfaces

## Nivel 4: C# moderno y profesional


### n4.u1 Errores y genéricos

- [x] **Excepciones y manejo de errores** (`n4.excepciones`). Prerrequisitos: n3.clases
- [x] **Genéricos** (`n4.genericos`). Prerrequisitos: n3.abstractas-interfaces

### n4.u2 Funciones como datos

- [x] **Delegados y eventos** (`n4.delegados`). Prerrequisitos: n3.clases
- [x] **Expresiones lambda** (`n4.lambdas`). Prerrequisitos: n4.delegados
- [x] **LINQ** (`n4.linq`). Prerrequisitos: n4.lambdas, n2.listas
- [x] **IEnumerable<T> frente a IQueryable<T>** (`n4.ienumerable-iqueryable`). Prerrequisitos: n4.linq
- [x] **Iteradores con yield** (`n4.iteradores`). Prerrequisitos: n4.linq

### n4.u3 Idiomas modernos

- [ ] **Pattern matching y expresiones modernas** (`n4.patrones`). Prerrequisitos: n1.switch, n3.records
- [ ] **Métodos de extensión** (`n4.extension`). Prerrequisitos: n3.static
- [ ] **IDisposable y using** (`n4.idisposable`). Prerrequisitos: n4.excepciones
- [ ] **Archivos, directorios y JSON** (`n4.archivos-json`). Prerrequisitos: n4.idisposable
- [ ] **Colecciones inmutables y mutabilidad** (`n4.inmutabilidad`). Prerrequisitos: n3.records
- [ ] **Namespaces, proyectos y assemblies** (`n4.organizacion`). Prerrequisitos: n3.encapsulacion

## Nivel 5: Herramientas profesionales


### n5.u1 Ecosistema .NET

- [ ] **.NET SDK y CLI** (`n5.cli`). Prerrequisitos: n2.metodos
- [ ] **NuGet** (`n5.nuget`). Prerrequisitos: n5.cli
- [ ] **Depuración con puntos de interrupción** (`n5.depuracion`). Prerrequisitos: n2.metodos

### n5.u2 Calidad

- [ ] **Pruebas unitarias y diseño de casos** (`n5.pruebas`). Prerrequisitos: n3.clases
- [ ] **Refactorización** (`n5.refactorizacion`). Prerrequisitos: n5.pruebas
- [ ] **Git y control de versiones** (`n5.git`). Prerrequisitos: —
- [ ] **Leer documentación técnica** (`n5.documentacion`). Prerrequisitos: —
- [ ] **Complejidad y rendimiento básico** (`n5.rendimiento`). Prerrequisitos: n1.complejidad
- [ ] **SOLID y separación de responsabilidades** (`n5.arquitectura`). Prerrequisitos: n3.solid

## Nivel 6: Programación asíncrona y sistemas


### n6.u1 Asincronía

- [ ] **Task y Task<T>** (`n6.tasks`). Prerrequisitos: n4.lambdas
- [ ] **async y await** (`n6.async-await`). Prerrequisitos: n6.tasks
- [ ] **CancellationToken** (`n6.cancelacion`). Prerrequisitos: n6.async-await
- [ ] **Excepciones en código asíncrono** (`n6.excepciones-async`). Prerrequisitos: n6.async-await, n4.excepciones
- [ ] **Concurrencia frente a paralelismo** (`n6.concurrencia`). Prerrequisitos: n6.async-await
- [ ] **Introducción a multithreading** (`n6.hilos`). Prerrequisitos: n6.concurrencia

### n6.u2 Datos y red

- [ ] **HTTP y consumo de APIs** (`n6.http`). Prerrequisitos: n6.async-await
- [ ] **JSON y modelos de datos** (`n6.json`). Prerrequisitos: n4.archivos-json
- [ ] **Persistencia con SQLite** (`n6.sqlite`). Prerrequisitos: n6.async-await
- [ ] **Fundamentos de seguridad y validación** (`n6.seguridad`). Prerrequisitos: n6.sqlite

## Nivel 7: Proyectos integradores





## Nivel 7: Proyectos integradores

Se trabajan desde la sección **Proyectos**. Cada uno tiene requisitos, criterios de aceptación, 4 hitos evaluados con pruebas visibles y ocultas, y una rúbrica de 4 criterios (insuficiente / adecuado / excelente). Los hitos no aparecen en sesiones ni evaluaciones.

- **Control de gastos personales** (`proyecto.gastos`): hitos 4, rubrica 4. Habilidades: n1.validacion, n2.strings, n2.diccionarios, n3.records, n2.metodos.
- **Biblioteca con préstamos y multas** (`proyecto.biblioteca`): hitos 4, rubrica 4. Habilidades: n3.encapsulacion, n3.composicion, n3.records, n3.abstractas-interfaces, n2.diccionarios.
- **Tarjetas de repaso espaciado** (`proyecto.tarjetas`): hitos 4, rubrica 4. Habilidades: n3.records, n2.listas, n3.solid, n1.validacion, n2.metodos.

## Totales

77 habilidades en la ruta, 52 con lección completa; 452 ítems (incluidos 12 hitos de proyecto), 168 ejercicios de código con 610 casos de prueba.

## Estructura de cada lección completa (10 partes)

1. Objetivo de aprendizaje.
2. Explicación breve en bloques (texto, código ejecutable, notas, advertencias, decisiones de diseño).
3. Ejemplo resuelto con notas línea por línea y salida esperada (verificada).
4. Error común: código incorrecto, por qué falla y versión corregida (compilada en las pruebas).
5. Pregunta de predicción («¿qué imprime?»), verificada contra la ejecución real.
6. Ejercicio guiado (con andamiaje en el código inicial).
7. Ejercicio independiente (sin andamiaje).
8. Ejercicio de transferencia (aplicar el concepto en otro contexto).
9. Preguntas de recuperación (al menos 2) y una de búsqueda de errores.
10. Criterio de dominio de la habilidad.

Cada habilidad completa tiene además ítems de concepto o recuperación y de búsqueda de errores que se usan en repasos, intercalado y evaluaciones.
