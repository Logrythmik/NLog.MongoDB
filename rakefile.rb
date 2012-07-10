require 'albacore'
require './rakefile.config'

namespace :nuget do

	command = '.nuget/NuGet.exe'
	nuspec = 'NLog.MongoDB.nuspec'
	build_directory = 'build'	
	version = '0.2.1'

	task :package => :setup_build do		
		base_folder = "#{build_directory}/"
		sh "#{command} pack -BasePath #{base_folder} -Output #{base_folder} #{nuspec}"
	end

	directory build_directory

	task :setup_build => ['build:release', 'build'] do
		cp FileList['NLog.MongoDB/bin/Release/NLog.MongoDB.*'], 'build'
	end
	
	task :push => :package do
		api_key = NLog_MongoDB::Build.api_key
		
		sh "#{command} setApiKey #{api_key}"
		sh "#{command} push #{build_directory}/NLog.MongoDB.#{version}.nupkg"
	end

end

desc "runs all unit tests in debug mode"
task :test => 'build:test'

namespace :build do

	# loop to create a couple tasks -- one for debug, and one for release
	[:debug, :release].each do |configuration|

		desc "Builds a #{configuration} version of NLog.MongoDB"
		msbuild configuration do |msb|
			msb.properties :configuration => configuration
			msb.targets = [ :Clean, :Build ]
			msb.solution = 'NLog.MongoDb.sln'
		end

	end

	nunit :test => :debug do |nunit|
		nunit.command = 'packages/NUnit.2.5.10.11092/tools/nunit-console.exe'
		nunit.assemblies 'NLog.MongoDB.Tests/bin/Debug/NLog.MongoDB.Tests.dll'
	end
end
