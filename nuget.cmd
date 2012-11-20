%windir%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe SkyNet.sln /t:Clean,Rebuild /p:Configuration=Release /fileLogger

rd /s /q nuget

mkdir nuget\lib\net40\

copy SkyNet\bin\Release\SkyNet.dll nuget\lib\net40
copy SkyNet\bin\Release\SkyNet.xml nuget\lib\net40

nuget.exe update -self
nuget.exe pack SkyNet.nuspec -BasePath nuget -Output nuget

