# ChartButlerCS <img src="./ChartButlerCS/Icon1.png" width="64" height="64" align="left" /> 
Vollständig überarbeitete Version von Jörg Pauly's ursprünglichem ChartButlerCS 
- AIP-Charts von GAT24.de einfach nutzen und aktuell halten - 

Neben vielen kleineren Fehlerkorrekturen bietet diese Version folgende Vorteile:

- Es werden nun auch Vorschaubilder der Anflugkarten herunter geladen und angezeigt

- Der Aktualisierungsmechanismus wurde vollständig überarbeitet. Anflugkarten werden nun auch dann noch korrekt aktualisiert, wenn ein oder mehrere AIRAC Cycles übersprungen wurden

- ChartButlerCS ist nun ein Single-File Executable, benötigt daher keine Installation mehr und kann von einem beliebigen Ort - z.B. einem USB-Stick - gestartet werden. Außerdem werden grundsätzlich keine Administrator-Rechte mehr benötigt

- ChartButlerCS läuft nun sowohl auf allen Windows Plattformen (mit .NET 2.0), als auch auf Linux Derivaten mit entsprechend installiertem Mono Framework CLI 2.0. Folgende .NET Pakete werden benötigt: 
System, System.Data, System.Drawing, System.Security, System.Windows.Forms, System.Xml

- Die Kartendatenbank wird nun in einer versteckten Datei im Kartenverzeichnis abgelegt, dadurch ist es möglich mehrere Kartenverzeichnisse auf dem Rechner zu pflegen  
