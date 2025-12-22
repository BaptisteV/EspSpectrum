# EspSpectrum.Worker

Windows service that runs the EspSpectrum application in the background.

It runs the main loop to:
- Read audio data
- Process FFT
- Send spectrum to ESP device via Websocket

And watches for configuration changes to eventually send them to the ESP device (bar color personalization for exemple)

## Deploy
See `create_service.ps1`
```powershell
#Requires -Version 6.0

# Publish the project
dotnet publish -c Release -f net10.0 -r win-x64 /p:PublishSingleFile=true -o ./publish

# Reinstall the Windows service
Remove-Service -Name "EspSpectrum"
New-Service -Name "EspSpectrum" -BinaryPathName "./publish" -DisplayName "EspSpectrum" -StartupType Manual
Start-Service -Name "EspSpectrum"
```
