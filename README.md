# local-voice-to-text-vosk
 Local integration of the Vosk voice to text dictation tool

# Quick guide
 Update the template.config.json file name to config.json and write the path to your [Vosk model](https://alphacephei.com/vosk/models)
 ```
 cd .\speech-to-text-win-console-vosk\
 dotnet run --project .\speech-to-text-win-console-vosk.csproj
```
 The Windows console app will now convert the speech detected from your microphone to whichever which ever application you are in.
 Press "Enter" to stop.

# Features
A few custom commands ("comma", "period", "new line", "send message") were added for English.
You can add your own commands by adding simple commands for specific text, and by using the [Windows API](https://learn.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes) for advanced commands.

# Credits
 This integration relies primarily on open source repository [Vosk](https://github.com/alphacep/vosk-api), licensed under the Apache 2.0 License.
