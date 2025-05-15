using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class DOTSEventsManager : MonoBehaviour {


    public static DOTSEventsManager Instance { get; private set; }


    public event EventHandler OnBarracksUnitQueueChanged;
    public event EventHandler OnGameWin;
    public event EventHandler OnHQDead;
    public event EventHandler OnHealthDead;
    public event EventHandler OnHordeStartedSpawning;
    public event EventHandler OnHordeStartSpawningSoon;


    private void Awake() {
        Instance = this;
    }


    public void TriggerOnBarracksUnitQueueChanged(NativeList<Entity> entityNativeList) {
        foreach (Entity entity in entityNativeList) {
            OnBarracksUnitQueueChanged?.Invoke(entity, EventArgs.Empty);
        }
    }

    public void TriggerOnHQDead() {
        OnHQDead?.Invoke(this, EventArgs.Empty);
    }

    public void TriggerOnHealthDead(NativeList<Entity> entityNativeList) {
        foreach (Entity entity in entityNativeList) {
            OnHealthDead?.Invoke(entity, EventArgs.Empty);
        }
    }

    public void TriggerOnHordeStartedSpawning(NativeList<Entity> entityNativeList) {
        foreach (Entity entity in entityNativeList) {
            OnHordeStartedSpawning?.Invoke(entity, EventArgs.Empty);
        }
    }

    public void TriggerOnHordeStartSpawningSoon(NativeList<Entity> entityNativeList) {
        foreach (Entity entity in entityNativeList) {
            OnHordeStartSpawningSoon?.Invoke(entity, EventArgs.Empty);
        }
    }

    public void TriggerOnGameWin() {
        OnGameWin?.Invoke(this, EventArgs.Empty);
    }

}