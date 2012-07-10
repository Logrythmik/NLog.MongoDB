NLog.MongoDB Target
=============

Installation
-------------

To install, place the binaries in your application bin and add the following configuration entries to your NLog configuration.

### Using an existing connection string

	<?xml version="1.0" encoding="utf-8" ?>
	<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
			xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
		<extensions>
			<add assembly="NLog.MongoDB"/>
		</extensions>

		<targets>
			<target name="Mongo" type="MongoDB" connectionName="ConnectionName" />
		</targets>
		<rules>
			<logger name="*" minLevel="Info" appendTo="Mongo"/>
		</rules>
	</nlog>
	
### Using a new connection string

	<?xml version="1.0" encoding="utf-8" ?>
	<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
			xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
		<extensions>
			<add assembly="NLog.MongoDB"/>
		</extensions>

		<targets>
			<target name="Mongo" type="MongoDB" connectionString="mongodb://mongo:db@server:12345/nlog" />
		</targets>
		<rules>
			<logger name="*" minLevel="Info" appendTo="Mongo"/>
		</rules>
	</nlog>	

### Using integrated settings

	<?xml version="1.0" encoding="utf-8" ?>
	<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
			xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
		<extensions>
			<add assembly="NLog.MongoDB"/>
		</extensions>

		<targets>
			<target name="Mongo" type="MongoDB" 
				database="nlog"
				host="server" port="12348"            
				username="mongo" password="password"/>
		</targets>
		<rules>
			<logger name="*" minLevel="Info" appendTo="Mongo"/>
		</rules>
	</nlog>

### Target Settings:

* Host (Defaults to 'localhost')
* Port (Defaults to 27017)
* Username
* Password
* Database(Defaults to 'NLog')
* ConnectionString (a complete Mongo Url)
* ConnectionName (the name configured in the configuration/connectionString node)