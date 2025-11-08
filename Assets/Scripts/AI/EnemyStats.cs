[System.Serializable]
public struct EnemyStats
{
    public float Health;
    public float Damage;
    public float Speed;
    public float VisionRange;
    public float AttackRange;
    public float AttackInterval;
    public float DifficultyRating;

    public override readonly string ToString() =>
        $"Health: {Health}, Damage: {Damage}, Speed: {Speed}, VisionRange: {VisionRange}, AttackRange: {AttackRange}, AttackInterval: {AttackInterval}, DifficultyRating: {DifficultyRating}";
}

