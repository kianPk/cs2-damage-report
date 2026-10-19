namespace DamageReport;

public sealed class Hit
{
    public required string Attacker { get; init; }
    public required string Victim { get; init; }
    public int Damage { get; init; }
    public int Hits { get; init; } = 1;
}

public sealed class DamageTracker
{
    private readonly Dictionary<(string a, string v), Hit> _hits = new();

    public void Add(string attacker, string victim, int damage)
    {
        if (damage <= 0) return;
        var key = (attacker, victim);
        if (_hits.TryGetValue(key, out var h))
            _hits[key] = new Hit { Attacker = attacker, Victim = victim, Damage = h.Damage + damage, Hits = h.Hits + 1 };
        else
            _hits[key] = new Hit { Attacker = attacker, Victim = victim, Damage = damage };
    }

    public IEnumerable<string> ReportFor(string player)
    {
        foreach (var h in _hits.Values.Where(x => x.Attacker == player || x.Victim == player)
                     .OrderByDescending(x => x.Damage))
        {
            if (h.Attacker == player)
                yield return $"To {h.Victim}: {h.Damage} dmg / {h.Hits} hit(s)";
            else
                yield return $"From {h.Attacker}: {h.Damage} dmg / {h.Hits} hit(s)";
        }
    }

    public void Reset() => _hits.Clear();
}
