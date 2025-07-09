using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

/// <summary>
/// System für die Berechnung von Formation-Positionen.
/// Berechnet für jede Formation-Follower Unit ihre Zielposition relativ zum Flag-Bearer.
/// </summary>
[UpdateAfter(typeof(FlagBearerMovementSystem))]
[UpdateBefore(typeof(FormationFollowerMovementSystem))]
partial struct FormationSystem : ISystem
{
    private ComponentLookup<LocalTransform> localTransformLookup;
    private ComponentLookup<FlagBearer> flagBearerLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        localTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true);
        flagBearerLookup = SystemAPI.GetComponentLookup<FlagBearer>(true);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Update Lookups
        localTransformLookup.Update(ref state);
        flagBearerLookup.Update(ref state);

        // Job für Formation-Berechnung
        FormationCalculationJob formationJob = new FormationCalculationJob
        {
            localTransformLookup = localTransformLookup,
            flagBearerLookup = flagBearerLookup
        };
        state.Dependency = formationJob.ScheduleParallel(state.Dependency);
    }
}

/// <summary>
/// Job für die Berechnung der Formation-Positionen.
/// </summary>
[BurstCompile]
public partial struct FormationCalculationJob : IJobEntity
{
    [ReadOnly] public ComponentLookup<LocalTransform> localTransformLookup;
    [ReadOnly] public ComponentLookup<FlagBearer> flagBearerLookup;

    public void Execute(ref FormationFollower formationFollower)
    {
        // Prüfe ob Flag-Bearer existiert
        if (formationFollower.flagBearerEntity == Entity.Null ||
            !localTransformLookup.HasComponent(formationFollower.flagBearerEntity) ||
            !flagBearerLookup.HasComponent(formationFollower.flagBearerEntity))
        {
            return;
        }

        // Hole Flag-Bearer Daten
        LocalTransform flagBearerTransform = localTransformLookup[formationFollower.flagBearerEntity];
        FlagBearer flagBearer = flagBearerLookup[formationFollower.flagBearerEntity];

        // Berechne Formation-Position wenn:
        // 1. Flag-Bearer sich bewegt ODER
        // 2. FormationFollower hat noch keine gültige Zielposition (erste Berechnung)
        bool hasValidTargetPosition = !math.all(formationFollower.targetPosition == float3.zero);
        if (!flagBearer.isMoving && !formationFollower.isMoving && hasValidTargetPosition)
        {
            return; // Flag-Bearer steht still und Follower ist bereits an Position
        }

        // Berechne Formation-Position relativ zum Flag-Bearer
        float3 formationOffset = CalculateFormationOffset(
            formationFollower.formationPosition,
            flagBearer.formationWidth,
            flagBearer.unitSpacing,
            flagBearer.formationDistance
        );

        // Rotiere Offset basierend auf Flag-Bearer Rotation
        float3 rotatedOffset = math.mul(flagBearerTransform.Rotation, formationOffset);

        // Berechne neue Zielposition
        float3 newTargetPosition = flagBearerTransform.Position + rotatedOffset;
        
        // Nur Position aktualisieren wenn sie sich signifikant geändert hat
        float distanceToNewTarget = math.lengthsq(newTargetPosition - formationFollower.targetPosition);
        if (distanceToNewTarget > 0.1f) // Mindestabstand für Update
        {
            formationFollower.targetPosition = newTargetPosition;
        }
    }

    /// <summary>
    /// Berechnet den lokalen Offset für eine Position in der Formation.
    /// </summary>
    /// <param name="formationPos">Position in der Formation (x=Spalte, y=Reihe)</param>
    /// <param name="formationWidth">Breite der Formation</param>
    /// <param name="unitSpacing">Abstand zwischen Units</param>
    /// <param name="formationDistance">Abstand der Formation hinter dem Flag-Bearer</param>
    /// <returns>Lokaler Offset relativ zum Flag-Bearer</returns>
    private static float3 CalculateFormationOffset(int2 formationPos, int formationWidth, float unitSpacing, float formationDistance)
    {
        // Berechne X-Offset (links-rechts)
        float xOffset = (formationPos.x - (formationWidth - 1) * 0.5f) * unitSpacing;
        
        // Berechne Z-Offset (hinter dem Flag-Bearer)
        float zOffset = -(formationPos.y + 1) * unitSpacing - formationDistance;
        
        return new float3(xOffset, 0, zOffset);
    }
}