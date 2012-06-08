require 'albacore'

namespace :nuget do

	task :package => :setup_build do
		command = '.nuget/NuGet.exe'
		nuspec = 'NLog.MongoDB.nuspec'
		base_folder = 'build/'
		sh ".nuget/NuGet.exe pack -BasePath #{base_folder} -Output #{base_folder} #{nuspec}"
	end

	directory 'build'

	task :setup_build => ['build:release', 'build'] do
		cp FileList['NLog.MongoDB/bin/Release/NLog.MongoDB.*'], 'build'
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
