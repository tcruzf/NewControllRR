# NewControllRR
Projeto rodando no debian 12.
git clone https://github.com/tcruzf/NewControllRR.git
cd NewControllRR
Change ControllRR.Presentation/appsettings.json
Insert user, password, host and ip address of mysql/mariadb server
 Run migrations....#
 Run dotnet clean && dotnet build#
 dotnet watch --project ControllRR.Presentation/

 Generate new 'sql' file: /usr/bin/dotnet ef migrations --project ControllRR.Infrastructure script -o database.sql# ControllRR

