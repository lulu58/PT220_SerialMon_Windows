# Thermal Printer PT-220 Simple Printing

## Background
The cheap PT-220 Bluetooth printer (made in China) comes with Android App. After longer searching, a Github repo was found, and it contains a Windows driver!
But - sometimes the printer connects, sometimes doesn't.

Tried a lot with Bluetooth frameworks to print from an app developed with .NET8.0 and C#, but wasn't able   

## Drucker über Bluetooth verbinden 
- Start -> Einstellungen -> Geräte -> Bluetooth-Geät hinzufügen
- wenn "PT-220" gezeigt wird, verbinden
- Verwandte Einstellungen -> Weitere Bluetooth-Optionen -> Dialog "Bluetoth-Einstellungen"
- Tab "COM-Anschlüsse" -> Button "Hinzufügen" -> Radio-Button "Ausgehend" -> Button "Durchsuchen" -> PT-220 auswählen
- Dem Drucker wird ein virtueller COM-Port zugeordnet (merken!)

## Drucken
- virtuellen COM-Port öffnen 
- Ausgabe von Druckcontent und Steuerzeichen mit SerialPort.WriteLine(text) und SerialPort.Write(text)

## Drucker-Steuerzeichen
- ESC/POS Untermenge, herstellerspezifisch
- 