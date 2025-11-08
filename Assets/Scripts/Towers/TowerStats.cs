[System.Serializable]
public struct TowerStats
{
    public float Health;
    public int Cost;
    public float Damage;
    public float Range;
    public float AttackInterval;
    public TargetingType TargetingType;

    public override readonly string ToString() =>
        $"Health: {Health}, Cost: {Cost}, Damage: {Damage}, Range: {Range}, AttackInterval: {AttackInterval}, TargetingType: {TargetingType}";
}
