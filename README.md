# lib-mvc-ado-csharp

## Requirements
- .NET Framework 4.5 and upper
- Database Existed
- App.config with connection string. Eg:
```
<configuration>
//...other config
  <connectionStrings>
    <add name="SqlServer"
      connectionString="Data Source=<Your Server Name>;Initial Catalog=<Your Database Name>;Integrated Security=True"
      providerName="System.Data.SqlClient" />
  </connectionStrings>
</configuration>
```
