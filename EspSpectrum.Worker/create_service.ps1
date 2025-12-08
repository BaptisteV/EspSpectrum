#Requires -Version 6.0
Remove-Service -Name "EspSpectrum"
New-Service -Name "EspSpectrum" -BinaryPathName "C:\Users\Bapt\Desktop\FFT_Publish\bin\EspSpectrum.Worker.exe" -DisplayName "EspSpectrum" -StartupType Manual
Start-Service -Name "EspSpectrum"