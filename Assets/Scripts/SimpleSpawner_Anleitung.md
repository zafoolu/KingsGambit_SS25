# Simple Spawner System - Verwendungsanleitung

## Überblick

Das SimpleSpawner-System ermöglicht es, Einheiten automatisch in bestimmten Zeitintervallen zu spawnen.

## Einrichtung

### Schritt 1: SimpleSpawnerAuthoring-Komponente hinzufügen
1. Erstelle ein leeres GameObject in der Szene
2. Benenne es z.B. "EnemySpawner"
3. Füge die "SimpleSpawnerAuthoring"-Komponente hinzu

### Schritt 2: Spawner konfigurieren

#### Spawn Settings:
- **Unit Type**: Wähle den Einheitentyp (z.B. CursedPawn, CursedKing)
- **Units Per Spawn**: Anzahl der Einheiten pro Spawn-Vorgang
- **Spawn Interval**: Zeit zwischen den Spawns (in Sekunden)
- **Max Spawns**: Maximale Anzahl von Spawn-Vorgängen (0 = unendlich)
- **Spawn On Start**: Spawne sofort beim Spielstart

#### Formation Settings:
- **Formation**: 
  - Grid: Einheiten in einem Raster
  - Circle: Einheiten in einem Kreis
  - Line: Einheiten in einer Linie
- **Unit Spacing**: Abstand zwischen den Einheiten

#### Debug:
- **Show Debug Gizmos**: Zeigt Spawn-Bereich im Scene-View
- **Debug Radius**: Größe der Debug-Visualisierung

### Schritt 3: Position festlegen
1. Positioniere das Spawner-GameObject an der gewünschten Spawn-Position
2. Die Einheiten werden um diese Position herum gespawnt

## Beispiel-Konfigurationen

### Basis Enemy Spawner:
- Unit Type: CursedPawn
- Units Per Spawn: 3
- Spawn Interval: 5.0
- Max Spawns: 10
- Formation: Grid
- Unit Spacing: 2.0

### Boss Spawner:
- Unit Type: CursedKing
- Units Per Spawn: 1
- Spawn Interval: 30.0
- Max Spawns: 1
- Formation: Circle
- Unit Spacing: 0.0

### Kontinuierlicher Spawner:
- Unit Type: CursedPawn
- Units Per Spawn: 2
- Spawn Interval: 3.0
- Max Spawns: 0 (unendlich)
- Formation: Line
- Unit Spacing: 1.5

## Verfügbare Einheitentypen

- **CursedPawn**: Basis-Feindeinheit
- **CursedKing**: Boss-Einheit
- **CarraraKing**: Freundliche König-Einheit
- Weitere Typen je nach UnitTypeSO-Konfiguration

## Tipps

1. **Debug-Gizmos verwenden**: Aktiviere "Show Debug Gizmos" um den Spawn-Bereich zu visualisieren
2. **Spawn-Timing testen**: Beginne mit kurzen Intervallen zum Testen
3. **Formation anpassen**: Verschiedene Formationen für verschiedene Einheitentypen
4. **Max Spawns setzen**: Verhindert endloses Spawning in kleinen Levels
5. **Mehrere Spawner**: Erstelle mehrere Spawner für verschiedene Bereiche

## Häufige Probleme

1. **Keine Einheiten spawnen**: Überprüfe ob EntitiesReferences korrekt konfiguriert ist
2. **Falsche Formation**: Stelle sicher, dass Unit Spacing > 0 ist
3. **Performance-Probleme**: Reduziere Spawn-Rate oder setze Max Spawns

## System-Komponenten

- **SimpleSpawnerAuthoring.cs**: Editor-Komponente für Konfiguration
- **SimpleSpawnerSystem.cs**: ECS-System für automatisches Spawning
- **SimpleSpawner**: ECS-Komponente mit Spawn-Daten