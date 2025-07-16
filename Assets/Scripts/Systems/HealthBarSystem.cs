using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
partial struct HealthBarSystem : ISystem {


    private ComponentLookup<LocalTransform> localTransformComponentLookup;
    private ComponentLookup<Health> healthComponentLookup;
    private ComponentLookup<PostTransformMatrix> postTransformMatrixComponentLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state) {
        localTransformComponentLookup = state.GetComponentLookup<LocalTransform>();
        healthComponentLookup = state.GetComponentLookup<Health>(true);
        postTransformMatrixComponentLookup = state.GetComponentLookup<PostTransformMatrix>(false);
    }

    //[BurstCompile]
    public void OnUpdate(ref SystemState state) {
        Vector3 cameraPosition = Vector3.zero;
        Camera mainCamera = Camera.main;
        
        // Fallback: If Camera.main is null, find the camera with MainCamera tag
        if (mainCamera == null) {
            GameObject cameraObject = GameObject.FindGameObjectWithTag("MainCamera");
            if (cameraObject != null) {
                mainCamera = cameraObject.GetComponent<Camera>();
            }
        }
        
        if (mainCamera != null) {
            cameraPosition = mainCamera.transform.position;
        }

        localTransformComponentLookup.Update(ref state);
        healthComponentLookup.Update(ref state);
        postTransformMatrixComponentLookup.Update(ref state);
        HealthBarJob healthBarJob = new HealthBarJob {
            cameraPosition = cameraPosition,
            localTransformComponentLookup = localTransformComponentLookup,
            healthComponentLookup = healthComponentLookup,
            postTransformMatrixComponentLookup = postTransformMatrixComponentLookup,
        };
        healthBarJob.ScheduleParallel();

        /*
        foreach ((
            RefRW<LocalTransform> localTransform,
            RefRO<HealthBar> healthBar) 
            in SystemAPI.Query<
                RefRW<LocalTransform>, 
                RefRO<HealthBar>>()) {

            LocalTransform parentLocalTransform = SystemAPI.GetComponent<LocalTransform>(healthBar.ValueRO.healthEntity);
            if (localTransform.ValueRO.Scale == 1f) {
                // Health bar is visible
                localTransform.ValueRW.Rotation = parentLocalTransform.InverseTransformRotation(quaternion.LookRotation(cameraForward, math.up()));
            }

            Health health = SystemAPI.GetComponent<Health>(healthBar.ValueRO.healthEntity);

            if (!health.onHealthChanged) {
                continue;
            }

            float healthNormalized = (float)health.healthAmount / health.healthAmountMax;

            if (healthNormalized == 1f) {
                localTransform.ValueRW.Scale = 0f;
            } else {
                localTransform.ValueRW.Scale = 1f;
            }

            RefRW<PostTransformMatrix> barVisualPostTransformMatrix = 
                SystemAPI.GetComponentRW<PostTransformMatrix>(healthBar.ValueRO.barVisualEntity);
            barVisualPostTransformMatrix.ValueRW.Value = float4x4.Scale(healthNormalized, 1, 1);
        }
        */
    }


}


[BurstCompile]
public partial struct HealthBarJob : IJobEntity {


    [ReadOnly] public ComponentLookup<Health> healthComponentLookup;

    [NativeDisableParallelForRestriction] public ComponentLookup<LocalTransform> localTransformComponentLookup;
    [NativeDisableParallelForRestriction] public ComponentLookup<PostTransformMatrix> postTransformMatrixComponentLookup;


    public float3 cameraPosition;


    public void Execute(in HealthBar healthBar, Entity entity) {
        RefRW<LocalTransform> localTransform = localTransformComponentLookup.GetRefRW(entity);
        LocalTransform parentLocalTransform = localTransformComponentLookup[healthBar.healthEntity];
        
        if (localTransform.ValueRO.Scale == 1f) {
            // Health bar is visible - make it face the camera
            if (math.lengthsq(cameraPosition) > 0.001f) {
                // Get the world position of the healthbar
                float3 healthBarWorldPos = math.transform(parentLocalTransform.ToMatrix(), localTransform.ValueRO.Position);
                
                // Calculate direction from healthbar to camera
                float3 directionToCamera = math.normalize(cameraPosition - healthBarWorldPos);
                
                // Create rotation that looks towards the camera
                quaternion lookRotation = quaternion.LookRotation(directionToCamera, math.up());
                
                // Convert to local space relative to parent
                localTransform.ValueRW.Rotation = math.mul(math.inverse(parentLocalTransform.Rotation), lookRotation);
            }
        }

        Health health = healthComponentLookup[healthBar.healthEntity];

        if (!health.onHealthChanged) {
            return;
        }

        float healthNormalized = (float)health.healthAmount / health.healthAmountMax;

        if (healthNormalized == 1f) {
            localTransform.ValueRW.Scale = 0f;
        } else {
            localTransform.ValueRW.Scale = 1f;
        }

        RefRW<PostTransformMatrix> barVisualPostTransformMatrix =
            postTransformMatrixComponentLookup.GetRefRW(healthBar.barVisualEntity);

        barVisualPostTransformMatrix.ValueRW.Value = float4x4.Scale(healthNormalized, 1, 1);
    }
}