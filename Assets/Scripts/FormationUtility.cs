using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;

/// <summary>
/// Utility-Klasse für Formation-Berechnungen und -Verwaltung.
/// Enthält statische Methoden für häufig verwendete Formation-Operationen.
/// </summary>
public static class FormationUtility
{
    /// <summary>
    /// Generiert automatisch Formation-Positionen für eine gegebene Anzahl von Units.
    /// </summary>
    /// <param name="unitCount">Anzahl der Units</param>
    /// <param name="formationWidth">Gewünschte Breite der Formation</param>
    /// <returns>Array von Formation-Positionen</returns>
    public static NativeArray<int2> GenerateFormationPositions(int unitCount, int formationWidth, Allocator allocator)
    {
        NativeArray<int2> positions = new NativeArray<int2>(unitCount, allocator);
        
        for (int i = 0; i < unitCount; i++)
        {
            int row = i / formationWidth;
            int col = i % formationWidth;
            positions[i] = new int2(col, row);
        }
        
        return positions;
    }
    
    /// <summary>
    /// Berechnet die optimale Formation-Breite basierend auf der Anzahl der Units.
    /// </summary>
    /// <param name="unitCount">Anzahl der Units</param>
    /// <returns>Optimale Formation-Breite</returns>
    public static int CalculateOptimalFormationWidth(int unitCount)
    {
        if (unitCount <= 0) return 1;
        if (unitCount <= 3) return unitCount;
        if (unitCount <= 9) return 3;
        if (unitCount <= 16) return 4;
        return 5; // Maximum 5 Units pro Reihe
    }
    
    /// <summary>
    /// Berechnet die Formation-Höhe basierend auf Unit-Anzahl und Breite.
    /// </summary>
    /// <param name="unitCount">Anzahl der Units</param>
    /// <param name="formationWidth">Breite der Formation</param>
    /// <returns>Formation-Höhe</returns>
    public static int CalculateFormationHeight(int unitCount, int formationWidth)
    {
        if (unitCount <= 0 || formationWidth <= 0) return 0;
        return (unitCount + formationWidth - 1) / formationWidth; // Aufrunden
    }
    
    /// <summary>
    /// Berechnet den lokalen Offset für eine Position in der Formation.
    /// Diese Methode ist identisch mit der im FormationSystem, aber als Utility verfügbar.
    /// </summary>
    /// <param name="formationPos">Position in der Formation (x=Spalte, y=Reihe)</param>
    /// <param name="formationWidth">Breite der Formation</param>
    /// <param name="unitSpacing">Abstand zwischen Units</param>
    /// <param name="formationDistance">Abstand der Formation hinter dem Flag-Bearer</param>
    /// <returns>Lokaler Offset relativ zum Flag-Bearer</returns>
    public static float3 CalculateFormationOffset(int2 formationPos, int formationWidth, float unitSpacing, float formationDistance)
    {
        // Berechne X-Offset (links-rechts) - zentriert um Flag-Bearer
        float xOffset = (formationPos.x - (formationWidth - 1) * 0.5f) * unitSpacing;
        
        // Berechne Z-Offset (hinter dem Flag-Bearer)
        float zOffset = -(formationPos.y + 1) * unitSpacing - formationDistance;
        
        return new float3(xOffset, 0, zOffset);
    }
    
    /// <summary>
    /// Berechnet die Weltposition für eine Formation-Position relativ zu einem Flag-Bearer.
    /// </summary>
    /// <param name="flagBearerPosition">Position des Flag-Bearers</param>
    /// <param name="flagBearerRotation">Rotation des Flag-Bearers</param>
    /// <param name="formationPos">Position in der Formation</param>
    /// <param name="formationWidth">Breite der Formation</param>
    /// <param name="unitSpacing">Abstand zwischen Units</param>
    /// <param name="formationDistance">Abstand der Formation hinter dem Flag-Bearer</param>
    /// <returns>Weltposition für die Formation-Position</returns>
    public static float3 CalculateWorldFormationPosition(
        float3 flagBearerPosition,
        quaternion flagBearerRotation,
        int2 formationPos,
        int formationWidth,
        float unitSpacing,
        float formationDistance)
    {
        float3 localOffset = CalculateFormationOffset(formationPos, formationWidth, unitSpacing, formationDistance);
        float3 rotatedOffset = math.mul(flagBearerRotation, localOffset);
        return flagBearerPosition + rotatedOffset;
    }
    
    /// <summary>
    /// Prüft ob eine Formation-Position gültig ist.
    /// </summary>
    /// <param name="formationPos">Position in der Formation</param>
    /// <param name="formationWidth">Breite der Formation</param>
    /// <param name="formationHeight">Höhe der Formation</param>
    /// <returns>True wenn die Position gültig ist</returns>
    public static bool IsValidFormationPosition(int2 formationPos, int formationWidth, int formationHeight)
    {
        return formationPos.x >= 0 && formationPos.x < formationWidth &&
               formationPos.y >= 0 && formationPos.y < formationHeight;
    }
    
    /// <summary>
    /// Konvertiert eine lineare Index zu einer Formation-Position.
    /// </summary>
    /// <param name="index">Linearer Index</param>
    /// <param name="formationWidth">Breite der Formation</param>
    /// <returns>Formation-Position</returns>
    public static int2 IndexToFormationPosition(int index, int formationWidth)
    {
        int row = index / formationWidth;
        int col = index % formationWidth;
        return new int2(col, row);
    }
    
    /// <summary>
    /// Konvertiert eine Formation-Position zu einem linearen Index.
    /// </summary>
    /// <param name="formationPos">Formation-Position</param>
    /// <param name="formationWidth">Breite der Formation</param>
    /// <returns>Linearer Index</returns>
    public static int FormationPositionToIndex(int2 formationPos, int formationWidth)
    {
        return formationPos.y * formationWidth + formationPos.x;
    }
}