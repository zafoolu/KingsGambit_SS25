# Patrol System

Ein einfaches Patrouillier-System für Einheiten, die zwischen zwei Punkten hin und her laufen.

## Komponenten

### PatrolAuthoring
- **Point A**: Startpunkt (Transform-Referenz)
- **Point B**: Zielpunkt (Transform-Referenz)
- **Speed**: Bewegungsgeschwindigkeit (1-10)
- **Wait Time**: Wartezeit an jedem Punkt (0-5 Sekunden)
- **One Way Only**: Nur einmal von A nach B laufen (für Escort-Missionen)
- **Debug Gizmos**: Zeigt Patrol-Punkte und Pfad im Scene View

### PatrolSystem
- Bewegt Einheiten automatisch zwischen den Patrol-Punkten
- Unterstützt sowohl Patrol (hin und her) als auch One-Way Bewegung
- Dreht Einheiten in Bewegungsrichtung
- Wartet an jedem Punkt für die konfigurierte Zeit

## Setup

1. Füge `PatrolAuthoring` zu einem GameObject hinzu (z.B. King)
2. Erstelle zwei leere GameObjects als Patrol-Punkte
3. Weise diese GameObjects den Feldern "Point A" und "Point B" zu
4. Konfiguriere Geschwindigkeit und Wartezeit
5. Das System startet automatisch

## Features

- **Zwei Modi**: Patrol (hin und her) oder One-Way (einmalig)
- **Automatische Bewegung**: Einheit läuft zwischen den Punkten
- **Smooth Rotation**: Einheit dreht sich in Bewegungsrichtung
- **Wartezeit**: Konfigurierbare Pause an jedem Punkt
- **Debug Visualisierung**: Gizmos zeigen Patrol-Pfad im Editor
- **Flexible Konfiguration**: Einfache Anpassung im Inspector

## Verwendung für King

### Escort-Mission (One-Way):
1. Wähle den King im Hierarchy
2. Füge "Patrol Authoring" Komponente hinzu
3. Erstelle Start- und Zielpunkt in der Szene
4. Weise die Punkte Point A und Point B zu
5. **Aktiviere "One Way Only"** für einmalige Bewegung
6. Stelle Geschwindigkeit auf ca. 2-3 für realistische Bewegung
7. Setze Wartezeit auf 0 für kontinuierliche Bewegung

### Normal Patrol (hin und her):
- Gleiche Schritte, aber "One Way Only" deaktiviert lassen
- Wartezeit auf 1-2 Sekunden für natürliches Verhalten

Der King läuft nun einmal von A nach B und stoppt dort (Escort-Modus) oder patrouilliert kontinuierlich!