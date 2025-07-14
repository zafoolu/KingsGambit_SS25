# Formation System für DOTS RTS

## Übersicht

Dieses Formation-System ersetzt das individuelle Unit-Movement durch ein Flag-Bearer/Commander-System mit Formation. Anstatt dass jede Unit einzeln zu einer Klickposition läuft, bewegt sich nur der Flag-Bearer zum Ziel, und alle anderen Units folgen in einer Formation.

## Neue Components

### 1. FlagBearer
- **Zweck**: Führende Entity einer Formation
- **Eigenschaften**: Formation-Größe, Unit-Abstand, Bewegungsgeschwindigkeit
- **Verhalten**: Bewegt sich zu Klickpositionen, andere Units folgen

### 2. FormationFollower
- **Zweck**: Units, die einer Formation folgen
- **Eigenschaften**: Referenz zum Flag-Bearer, Position in Formation
- **Verhalten**: Bewegt sich zu berechneter Position relativ zum Flag-Bearer

## Neue Systeme

### 1. FlagBearerMovementSystem
- Bewegt Flag-Bearer zu ihren Zielpositionen
- Läuft vor dem FormationSystem

### 2. FormationSystem
- Berechnet Ziel-Positionen für alle Formation-Follower
- Berücksichtigt Flag-Bearer Position und Rotation
- Läuft zwischen Flag-Bearer und Follower Movement

### 3. FormationFollowerMovementSystem
- Bewegt Formation-Follower zu ihren berechneten Positionen
- Läuft nach dem FormationSystem

### 4. FormationInputSystem
- Verarbeitet Mausklicks für Formation-Bewegung
- Setzt nur Flag-Bearer Ziele, nicht einzelne Units

## Setup im Editor

### Methode 1: Einzelne Components

1. **Flag-Bearer erstellen**:
   - Füge `FlagBearerAuthoring` zu einem GameObject hinzu
   - Stelle Formation-Parameter ein (Breite, Höhe, Abstände)
   - Stelle Bewegungsgeschwindigkeit ein

2. **Formation-Follower erstellen**:
   - Füge `FormationFollowerAuthoring` zu Unit-GameObjects hinzu
   - Weise den Flag-Bearer GameObject zu
   - Stelle Formation-Position ein (X=Spalte, Y=Reihe)

### Methode 2: Formation Group (Empfohlen)

1. **FormationGroupAuthoring verwenden**:
   - Erstelle ein leeres GameObject
   - Füge `FormationGroupAuthoring` hinzu
   - Weise Flag-Bearer Prefab zu
   - Weise Follower-Prefabs Array zu
   - Formation wird automatisch berechnet

## Integration mit bestehendem System

### Deaktivierung des alten Systems

Das bestehende `UnitMoverSystem` sollte für Formation-Units deaktiviert werden:

```csharp
// Im UnitMoverSystem, OnUpdate Methode:
// Füge Ausschluss für Formation-Units hinzu
[WithNone(typeof(FormationFollower), typeof(FlagBearer))]
public partial struct UnitMoverJob : IJobEntity
{
    // Bestehender Code...
}
```

### Input-System Anpassung

Das bestehende Input-System in `UnitSelectionManager` sollte angepasst werden:

```csharp
// Prüfe ob ausgewählte Units Flag-Bearer sind
if (HasFlagBearer(selectedUnits))
{
    // Verwende FormationInputSystem Logik
    SetFlagBearerTargets(mouseWorldPosition);
}
else
{
    // Verwende bestehende Unit-Movement Logik
    SetIndividualUnitTargets(mouseWorldPosition);
}
```

## Formation-Berechnung

### Grid-Layout
Die Formation verwendet ein Grid-Layout:
- **X-Achse**: Spalten (links-rechts)
- **Y-Achse**: Reihen (vorne-hinten)
- **Zentrierung**: Formation ist um Flag-Bearer zentriert

### Beispiel 3x3 Formation:
```
[2,0] [2,1] [2,2]  <- Reihe 2 (hinten)
[1,0] [1,1] [1,2]  <- Reihe 1 (mitte)
[0,0] [0,1] [0,2]  <- Reihe 0 (vorne)
  ^     ^     ^
 Spalte Spalte Spalte
   0     1     2

      [FB]         <- Flag-Bearer (vorne)
```

### Offset-Berechnung
```csharp
// X-Offset (links-rechts, zentriert)
float xOffset = (spalte - (breite - 1) * 0.5f) * unitSpacing;

// Z-Offset (hinter Flag-Bearer)
float zOffset = -(reihe + 1) * unitSpacing - formationDistance;
```

## Utility-Funktionen

Die `FormationUtility` Klasse bietet hilfreiche Funktionen:

- `GenerateFormationPositions()`: Automatische Position-Generierung
- `CalculateOptimalFormationWidth()`: Optimale Breite basierend auf Unit-Anzahl
- `CalculateFormationOffset()`: Offset-Berechnung
- `IsValidFormationPosition()`: Position-Validierung

## Testing und Debugging

### Editor-Preview
- `FormationGroupAuthoring` zeigt Formation-Positionen im Scene-View
- Rote Kugel = Flag-Bearer
- Gelbe Kugeln = Formation-Positionen
- Graue Linien = Verbindungen

### Debug-Informationen
- Formation-Indices werden als Labels angezeigt
- Context-Menu Funktionen für Validierung

## Erweiterte Features (Optional)

### Dynamische Formation-Anpassung
- Bei Unit-Verlust: Formation automatisch neu berechnen
- Bei neuen Units: Automatisch in Formation einreihen

### Formation-Varianten
- Linie-Formation
- Keil-Formation
- Kreis-Formation

### Kollisions-Vermeidung
- Integration mit bestehendem FlowField-System
- Pathfinding für Flag-Bearer

## Nächste Schritte

1. **Testen**: Erstelle Test-Szene mit FormationGroupAuthoring
2. **Integration**: Passe bestehendes Input-System an
3. **Optimierung**: Performance-Tests mit vielen Formationen
4. **Erweiterung**: Zusätzliche Formation-Typen implementieren

## Troubleshooting

### Problem: Units bewegen sich nicht
- Prüfe ob Flag-Bearer korrekt zugewiesen ist
- Prüfe ob FormationFollower.flagBearerEntity != Entity.Null
- Prüfe System-Update-Reihenfolge

### Problem: Formation ist falsch ausgerichtet
- Prüfe Flag-Bearer Rotation
- Prüfe Formation-Offset Berechnung
- Prüfe unitSpacing und formationDistance Werte

### Problem: Performance-Issues
- Verwende [BurstCompile] auf allen Jobs
- Prüfe ComponentLookup Updates
- Optimiere Formation-Berechnungen