# Visual Ternera Service (VSTS)
#### Es una solución para controlar en que estado estan las etiquetas que utiliza PiQuatro.

Este proyecto esta dividio en dos programas:
- *Client*, que es el Windows Service que estara funcionando en las maquinas de producción.
- *Server*, que es el servidor que se comunicara con las maquinas en producción para analizar en que estado estan.

### Ejemplo:
|Cliente|Descripcion|UltimaConexion|
|---|---|---|
|192.168.10.102|Archivos Sobrantes|2024-04-22 11:24:00|
|192.168.42.25|Okay|2024-04-22 11:49:00|
|192.168.78.25|Desactualizado|2024-04-22 11:50:00|
|192.168.78.40|Multiples Instalaciones|2024-04-22 11:50:00|
|192.168.28.4|Desactualizado y Sobrantes|2024-04-22 11:50:00|
|192.168.10.25|Desactualizado|2024-04-19 16:26:00|
|192.168.56.1|Archivos Sobrantes|2024-04-15 11:36:00|


Instalación
===
### • Servidor
Para poder instalar el servidor es necesario contar con:
- Un servidor http capaz de hospedar [applicaciones ASP.NET](https://learn.microsoft.com/es-mx/aspnet/core/host-and-deploy/iis/?view=aspnetcore-8.0).
- Una instancia de SQL Server capaz de utilizar [tablas en memoria](https://learn.microsoft.com/es-mx/sql/relational-databases/in-memory-oltp/requirements-for-using-memory-optimized-tables?view=sql-server-ver16#requirements).

Para que el servidor pueda comunicarse con la base de datos, se necesita:
1. Preparar la base de datos ejecutando el SQL script **'deploy_VSTS.sql'** ubicado en la carpeta raiz.
2. Modificar la cadena de conexión **'DefaultConnection'** dentro de **_Server/appsettings.json_** para que se conecte con su servidor de base de datos.
3. Asignar el rol **'vsts_server'** al usuario corrrespondiente.

---

### • Cliente
Para poder insalar el cliente es necesario contar con:
- Una versión de Windows 10 1607+ o Windows 11 22000+, 64 bits.

Para poder instalarlo se necesita ejecutar el script **'installer.ps1'** como administrador.


Funcionamiento
===
### • Cliente
> :warning: La primera vez que se ejecuta el servicio realizara un analisis para encontrar PiQuatro, en caso de no encontrarlo o encontrar multiples, el cliente enviara el reporte al servidor y finalizara.

El servicio de **cliente** realiza tres tipos de analisis diarios.
1. Envia al servidor una lista de etiquetas encontradas en PiQuatro, indicando nombre y hash. Esto se realiza cada N minutos, donde N es un valor configurable, por defecto _'180'_ minutos.
2. Reporta al servidor si se encontro mas de una, o ninguna, instalación de PiQuatro en el sistema. Para mejorar el rendimiento de este analisis, solamente se realiza la busqueda en una unidad de disco configurable, por defecto en _'C:'_. Este analisis se realiza a una cierta hora del dia, tambien configurable.
3. Consulta al servidor si hay alguna actualizacion del cliente pendiente, en caso de haberla el mismo servicio realizara la actualización. Esta consulta tambien se realiza a una cierta hora del dia configurable.

> El archivo de configuración se encuentra en la unidad principal _'C:\ProgramData\Visual Ternera Service\config.toml'_

Junto al archivo de configuración se encuentra la carpeta _'Logs'_, todo error que encuentre el servicio sera reportado ahi, separado en un archivo por dia, subdividio por hora.

---

### • Servidor
Una vez iniciado el servidor, se puede consultar las APIs disponibles mediante el enlace **'http\://{hostname}:{port}/swagger'**

El servidor escucha y responde a las peticiones realizada por los clientes mediante una comunicación de tipo _API REST_.
Todo reporte de estado que es recibido por un cliente es publicado en la base de datos, en la tabla **[service].[EstadoCliente]** y es detallado en un archivo log.

> Estos archivos logs se encuentran en la unidad principal _'C:\ProgramData\Visual Ternera Server\'_, separados en una carpeta por cliente, un archivo por dia, subdivido por hora.

> Cualquier error interno que encuentre el servidor, es reportado en el Visor de Eventos de Windows.

El servidor esta limitado a solamente recibir 100 peticiones cada 10 minutos.

---

### • Autenticación
La comunicación entre servidor-cliente es validada medianto un metodo de autenticación. <br/>
En donde, el cliente le envia al servidor una llave privada encriptada por una llave publica y la llave publica. Y el servidor valida que sean la mismas llaves que el autorizo en el archivo de configuración **_Server/appsettings.json_**.

Para el cliente las llaves se pueden configurar en _'C:\ProgramData\Visual Ternera Service\config.toml'_.

> Para poder bajar el instalador o el cliente desde el servidor, tambien se necesita ingresar una clave en la URI de la siguiente forma: **'http\://{hostname}:{port}/instalador?key={clave}'**