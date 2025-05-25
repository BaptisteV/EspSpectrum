# EspSpectrum

EspSpectrum is an application that analyzes the loopback audio signal (the machine's "master" audio output) and sends the spectrum to a WebSocket server.  
The WebSocket server is hosted on an ESP32, which is connected to an LED matrix to display the spectrum.

For better accuracy, I decided to update the LED matrix on the microcontroller side only when a message is received via WebSocket, rather than in a tight loop.  
This prevents potential double-display of the same frame, ensuring optimal smoothness.  
As a consequence, this app should send data at a controlled, fixed interval.

It primarily uses [NAudio](https://github.com/naudio/NAudio), and currently only works on Windows.

The most important projects are:
- **EspSpectrum.Core** – The main application logic
- **EspSpectrum.Worker** – The Windows service that runs the app in the background
- **EspSpectrum.ConfigUI** – The Windows UI to configure and start/stop the EspSpectrum Windows service