namespace DamageReport;

public sealed class Hit
{
    public required string Attacker { get; init; }
    public required string Victim { get; init; }
    public int Damage { get; init; }
    public int Hits { get; init; } = 1;
    public int Headshots { get; init; }
}

public sealed class PlayerTotals
{
    public required string Player { get; init; }
    public int DamageDealt { get; init; }
    public int DamageTaken { get; init; }
    public int HitsDealt { get; init; }
    public int HitsTaken { get; init; }
    public int Headshots { get; init; }
}

public sealed class DamageTracker
{
    private readonly Dictionary<(string a, string v), Hit> _hits = new();

    public void Add(string attacker, string victim, int damage, bool headshot = false)
    {
        if (damage <= 0) return;
        var key = (attacker, victim);
        if (_hits.TryGetValue(key, out var h))
            _hits[key] = new Hit
            {
                Attacker = attacker,
                Victim = victim,
                Damage = h.Damage + damage,
                Hits = h.Hits + 1,
                Headshots = h.Headshots + (headshot ? 1 : 0)
            };
        else
            _hits[key] = new Hit
            {
                Attacker = attacker,
                Victim = victim,
                Damage = damage,
                Headshots = headshot ? 1 : 0
            };
    }

    public IEnumerable<string> ReportFor(string player)
    {
        foreach (var h in _hits.Values.Where(x => x.Attacker == player || x.Victim == player)
                     .OrderByDescending(x => x.Damage))
        {
            if (h.Attacker == player)
                yield return $"To {h.Victim}: {h.Damage} dmg / {h.Hits} hit(s)" +
                             (h.Headshots > 0 ? $" / {h.Headshots} HS" : "");
            else
                yield return $"From {h.Attacker}: {h.Damage} dmg / {h.Hits} hit(s)" +
                             (h.Headshots > 0 ? $" / {h.Headshots} HS" : "");
        }
    }

    /// <summary>Aggregate damage dealt/taken per player.</summary>
    public IReadOnlyList<PlayerTotals> Totals()
    {
        var map = new Dictionary<string, PlayerTotals>(StringComparer.OrdinalIgnoreCase);
        PlayerTotals Get(string p)
        {
            if (!map.TryGetValue(p, out var t))
            {
                t = new PlayerTotals { Player = p };
                map[p] = t;
            }
            return t;
        }

        foreach (var h in _hits.Values)
        {
            var a = Get(h.Attacker);
            a.DamageDealt += h.Damage;
            a.HitsDealt += h.Hits;
            a.Headshots += h.Headshots;
            var v = Get(h.Victim);
            v.DamageTaken += h.Damage;
            v.HitsTaken += h.Hits;
        }

        return map.Values.OrderByDescending(t => t.DamageDealt).ToList();
    }

    public void Reset() => _hits.Clear();
}
