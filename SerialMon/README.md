# Thermal Printer PT-220 Simple Printing

## Background
The cheap PT-220 Bluetooth printer (made in China) comes with Android App. After longer searching, a Github repo was found, and it contains a Windows driver!
But - sometimes the printer connects, sometimes doesn't.

Tried a lot with Bluetooth frameworks to print from an app developed with .NET8.0 and C#, but wasn't able   

## Drucker �ber Bluetooth verbinden 
- Start -> Einstellungen -> Ger�te -> Bluetooth-Ge�t hinzuf�gen
- wenn "PT-220" gezeigt wird, verbinden
- Verwandte Einstellungen -> Weitere Bluetooth-Optionen -> Dialog "Bluetoth-Einstellungen"
- Tab "COM-Anschl�sse" -> Button "Hinzuf�gen" -> Radio-Button "Ausgehend" -> Button "Durchsuchen" -> PT-220 ausw�hlen
- Dem Drucker wird ein virtueller COM-Port zugeordnet (merken!)

## Drucken
- virtuellen COM-Port �ffnen 
- Ausgabe von Druckcontent und Steuerzeichen mit SerialPort.WriteLine(text) und SerialPort.Write(text)

## Drucker-Steuerzeichen
- ESC/POS Untermenge, herstellerspezifisch
- 