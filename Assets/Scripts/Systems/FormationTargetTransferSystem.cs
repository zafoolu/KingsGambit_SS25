using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

/// <summary>
/// System das Targets vom FlagBearer an seine FormationFollower weitergibt.
/// Läuft nach dem FindTargetSystem.
/// </summary>
[UpdateAfter(typeof(FindTargetSystem))]
partial struct FormationTargetTransferSystem : ISystem {

    private ComponentLookup<Target> targetComponentLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state) {
        targetComponentLookup = state.GetComponentLookup<Target>(false);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        targetComponentLookup.Update(ref state);

        // Job für FlagBearer - übertrage Target an FormationFollower
        FormationTargetTransferJob formationTargetTransferJob = new FormationTargetTransferJob {
            targetComponentLookup = targetComponentLookup,
        };
        state.Dependency = formationTargetTransferJob.Schedule(state.Dependency);
    }

    [BurstCompile]
    public partial struct FormationTargetTransferJob : IJobEntity {

        public ComponentLookup<Target> targetComponentLookup;

        public void Execute(
            in FormationFollower formationFollower,
            in Entity followerEntity) {

            // Prüfe ob der FlagBearer ein Target hat
            if (targetComponentLookup.HasComponent(formationFollower.flagBearerEntity)) {
                Target flagBearerTarget = targetComponentLookup[formationFollower.flagBearerEntity];
                
                // Übertrage Target vom FlagBearer an FormationFollower
                if (targetComponentLookup.HasComponent(followerEntity)) {
                    targetComponentLookup[followerEntity] = new Target {
                        targetEntity = flagBearerTarget.targetEntity
                    };
                }
            }
        }
    }
}