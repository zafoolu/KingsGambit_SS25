# Patrol System - Einrichtungsanleitung

## Problem: König läuft zum Nullpunkt (0,0,0)

Dieses Problem tritt auf, wenn die Patrol-Punkte nicht korrekt eingerichtet sind.

## Lösung: Korrekte Einrichtung

### Schritt 1: Patrol-Punkte erstellen
1. Erstelle zwei leere GameObjects in der Szene
2. Benenne sie z.B. "PatrolPoint_A" und "PatrolPoint_B"
3. Positioniere sie an den gewünschten Patrol-Positionen

### Schritt 2: PatrolAuthoring-Komponente hinzufügen
1. Wähle den König (oder die Einheit, die patrouillieren soll)
2. Füge die "PatrolAuthoring"-Komponente hinzu
3. Ziehe "PatrolPoint_A" in das "Point A"-Feld
4. Ziehe "PatrolPoint_B" in das "Point B"-Feld
5. Stelle Geschwindigkeit und Wartezeit ein
6. Aktiviere "One Way Only" für Eskort-Missionen

### Schritt 3: Testen
1. Starte das Spiel
2. Überprüfe die Console auf Debug-Meldungen
3. Der König sollte zwischen den beiden Punkten patrouillieren

## Debug-Informationen

Das System gibt jetzt Debug-Informationen aus:
- Aktuelle Position
- Zielposition
- Patrol-Punkte A und B
- Bewegungsrichtung

## Häufige Probleme

1. **Patrol-Punkte nicht zugewiesen**: Fallback-Positionen werden verwendet
2. **Patrol-Punkte bei (0,0,0)**: System stoppt Bewegung automatisch
3. **Ungültige Richtungsberechnung**: System überspringt Frame

## Tipps

- Verwende die Debug-Gizmos im Scene-View zur Visualisierung
- Überprüfe die Console auf Warnmeldungen
- Stelle sicher, dass die Patrol-Punkte nicht zu nah beieinander liegen