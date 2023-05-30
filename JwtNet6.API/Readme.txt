Autor: Carlos Hinostroza Araujo

A.Para el uso de Jwt debemos y Identity Framwework debemos instalar los siguientes paquetes NuGet:

	1.Microsoft.AspNetCore.Authentication.JwtBearer
	2.Microsoft.EntityFrameworkCore.SqlServer
	3.Microsoft.EntityFrameworkCore.tools
	4.Microsoft.AspNetCore.Identity.EntityFrameworkCore
	5.Microsoft.AspNetCore.Authentication.JwtBearer

B.Agregar valores de configuración AppSettings.json para la cadena de conexión y de autenticación JWT

B.Configurar la clase Program en versiones posteriores de 6.0

	2.1.Realizar la configuración y código que se ubico dentro de la región Jwt
	2.2.Copia lo siguiente en la clase program
		app.UseAuthentication();
		app.UseAuthorization();
		app.MapControllers().RequireAuthorization();

	2.3.Creamos el metodo Token en el controlador Usuarios
