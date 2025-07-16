using UnityEngine;

[CreateAssetMenu()]
public class AnimationDataSO : ScriptableObject {

    public enum AnimationType {
        None,
        SoldierIdle,
        SoldierWalk,
        ZombieIdle,
        ZombieWalk,
        SoldierAim,
        SoldierShoot,
        ZombieAttack,
        ScoutIdle,
        ScoutWalk,
        ScoutShoot,
        ScoutAim,
        CarraraKnightWalk,
        CarraraKnightIdle,
        CarraraQueenWalk,
        CarraraRookWalk,
        CarraraRookIdle,
        CarraraRookAttack,
        CarraraBishopIdle,
        CarraraBishopWalk,
        CursedQueenWalk,
        CarraraBishopAttack,
        CarraraQueenAttack,
        CarraraPawnWalk,
        CarraraPawnIdle,
        CarraraPawnAttack,
        CarraraKingIdle,
        CarraraKingWalk,
        CarraraKingAttack,
        CursedBishopIdle,
        CursedBishopWalk,
        CursedBishopAttack,
        CursedPawnIdle,
        CursedPawnWalk,
        CursedPawnAttack,
        CursedKnightIdle,
        CursedKnightWalk,
        CursedKnightAttack,
        CursedKingIdle,
        CursedKingWalk,
        CursedKingAttack,
    }


    public AnimationType animationType;
    public Mesh[] meshArray;
    public float frameTimerMax;



    public static bool IsAnimationUninterruptible(AnimationType animationType) {
        switch (animationType) {
            default:
                return false;
            case AnimationType.ScoutShoot:
            case AnimationType.SoldierShoot:
            case AnimationType.ZombieAttack:
            case AnimationType.CarraraRookAttack:
            case AnimationType.CarraraBishopAttack:
            case AnimationType.CarraraQueenAttack:
            case AnimationType.CarraraPawnAttack:
            case AnimationType.CarraraKingAttack:
            case AnimationType.CursedBishopAttack:
            case AnimationType.CursedPawnAttack:
            case AnimationType.CursedKnightAttack:
            case AnimationType.CursedKingAttack:
                return true;
        }
    }
}