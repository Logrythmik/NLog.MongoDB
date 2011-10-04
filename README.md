NLog.MongoDB Target
=============

Installation
-------------

To install, place the binaries in your application bin and add the following configuration entries to your 

	<?xml version="1.0" encoding="utf-8" ?>
	<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
			xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
		<extensions>
			<add assembly="NLog.MongoDB"/>
		</extensions>

		<targets>
			<target name="Mongo" type="MongoDB" host="localhost" port="27017"/>
		</targets>
		<rules>
			<logger name="*" minLevel="Info" appendTo="Mongo"/>
		</rules>
	</nlog>

### Target Settings:
* Host (Defaults to 'localhost')
* Port (Defaults to 27017)