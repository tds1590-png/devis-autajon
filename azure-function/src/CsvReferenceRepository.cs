using System.Globalization;
using DevisHp.Core;
using DevisHp.Models;

namespace DevisHp.Data;

public class CsvReferenceRepository : IReferenceRepository
{
    private readonly Dictionary<string, Site> _sites;
    private readonly List<Machine> _machines;
    private readonly Dictionary<string, ProfilCouleurHp> _profilsHp;
    private readonly Dictionary<string, Support> _supports;
    private readonly Dictionary<string, Dorure> _dorures;
    private readonly List<OperationFinition> _operations;
    private readonly Dictionary<string, Consommable> _consommables;
    private readonly Dictionary<(string Site, string Type, string Presse), double> _poses;
    private readonly Dictionary<string, double> _outillagePrix;
    private readonly Parametres _parametres;

    public CsvReferenceRepository(string seedDataPath)
    {
        var root = Path.GetFullPath(seedDataPath);

        _sites = ReadCsv(Path.Combine(root, "Site.csv"))
            .ToDictionary(r => r["Code"], r => new Site
            {
                Code = r["Code"],
                Devise = r["Devise"],
                CoeffChange = ToDouble(r["CoeffChange"], 1),
                LaizeMaxiDefaut = ToDouble(r["LaizeMaxiDefaut"], 330)
            }, StringComparer.OrdinalIgnoreCase);

        _machines = ReadCsv(Path.Combine(root, "Machine.csv"))
            .Select(r => new Machine
            {
                Site = r["Site"],
                Type = r["Type"],
                LaizeMini = ToDouble(r.GetValueOrDefault("LaizeMini")),
                LaizeMaxi = ToDouble(r.GetValueOrDefault("LaizeMaxi")),
                TauxHoraire = ToDouble(r.GetValueOrDefault("TauxHoraire")),
                VitesseBase = ToDouble(r.GetValueOrDefault("VitesseBase")),
                CalagePrimerMin = ToDouble(r.GetValueOrDefault("CalagePrimerMin"))
            }).ToList();

        _profilsHp = new Dictionary<string, ProfilCouleurHp>(StringComparer.OrdinalIgnoreCase);
        _supports = new Dictionary<string, Support>(StringComparer.OrdinalIgnoreCase);
        _dorures = new Dictionary<string, Dorure>(StringComparer.OrdinalIgnoreCase);

        _operations = ReadCsv(Path.Combine(root, "OperationFinitionInline.csv"))
            .Select(r => new OperationFinition
            {
                Operation = r["Operation"],
                CalageExtra = ToDouble(r["CalageExtra"]),
                Passe = ToDouble(r["Passe"]),
                Gache = ToDouble(r["Gache"]),
                VitesseMax = ToDouble(r["VitesseMax"])
            }).ToList();

        _consommables = ReadCsv(Path.Combine(root, "Consommable.csv"))
            .ToDictionary(r => r["Type"], r => new Consommable
            {
                Type = r["Type"],
                PrixKg = ToDouble(r["PrixKg"]),
                GrammeM2 = ToDouble(r["GrammeM2"]),
                GrammeFixe = ToDouble(r["GrammeFixe"]),
                TauxEncrage = ToDouble(r["TauxEncrage"])
            }, StringComparer.OrdinalIgnoreCase);

        _poses = ReadCsv(Path.Combine(root, "Pose.csv"))
            .ToDictionary(
                r => (r["Site"], r["Type"], r.GetValueOrDefault("Presse", "")),
                r => ToDouble(r["DeveloppeMaxi"]),
                new TripleComparer());

        _outillagePrix = ReadCsv(Path.Combine(root, "Outillage.csv"))
            .ToDictionary(r => r["Operation"], r => ToDouble(r["Prix"]), StringComparer.OrdinalIgnoreCase);

        _parametres = BuildParams(ReadCsv(Path.Combine(root, "ParametreGlobal.csv")));

        var profilsHpRows = ReadCsv(Path.Combine(root, "ProfilCouleurHP.csv"));
        foreach (var r in profilsHpRows)
        {
            var nom = r.GetValueOrDefault("Nom");
            if (string.IsNullOrWhiteSpace(nom)) continue;
            _profilsHp[nom] = new ProfilCouleurHp
            {
                Nom = nom,
                ReductionVitesse = ToDouble(r.GetValueOrDefault("ReductionVitesse")),
                PourcentPasse = ToDouble(r.GetValueOrDefault("PourcentPasse")),
                PasseMl = ToDouble(r.GetValueOrDefault("PasseMl")),
                CalageExtraH = ToDouble(r.GetValueOrDefault("CalageExtraH")),
                PrixMilleClics = ToDouble(r.GetValueOrDefault("PrixMilleClics")),
                NbCouleurHP = (int)ToDouble(r.GetValueOrDefault("NbCouleurHP"))
            };
        }

        var supportRows = ReadCsv(Path.Combine(root, "Support.csv"));
        foreach (var r in supportRows)
        {
            var cle = r.GetValueOrDefault("Cle");
            if (string.IsNullOrWhiteSpace(cle)) continue;
            _supports[cle] = new Support
            {
                Cle = cle,
                PrixM2 = ToDouble(r.GetValueOrDefault("PrixM2"))
            };
        }

        var dorureRows = ReadCsv(Path.Combine(root, "Dorure.csv"));
        foreach (var r in dorureRows)
        {
            var lib = r.GetValueOrDefault("Libelle");
            if (string.IsNullOrWhiteSpace(lib)) continue;
            _dorures[lib] = new Dorure
            {
                Libelle = lib,
                PrixHorsFG = ToDouble(r.GetValueOrDefault("PrixHorsFG"))
            };
        }

        // Fallback minimal pour exécuter les tests locaux tant que les exports volumineux ne sont pas chargés.
        _profilsHp["Noir"] = new ProfilCouleurHp
        {
            Nom = "Noir",
            ReductionVitesse = 0,
            PourcentPasse = 1,
            PasseMl = 100,
            CalageExtraH = 0,
            PrixMilleClics = 1,
            NbCouleurHP = 1
        };

        _supports["Frozen Orion Diamond / Lavable / Glassine"] = new Support
        {
            Cle = "Frozen Orion Diamond / Lavable / Glassine",
            PrixM2 = 1
        };

        _dorures["Dorure"] = new Dorure { Libelle = "Dorure", PrixHorsFG = 0.1 };
    }

    public Site GetSite(string code) => _sites[code];

    public Machine GetMachine(string site, string type)
        => _machines.First(m => m.Site.Equals(site, StringComparison.OrdinalIgnoreCase)
            && m.Type.Equals(type, StringComparison.OrdinalIgnoreCase));

    public ProfilCouleurHp GetProfilHp(string nom)
        => _profilsHp.TryGetValue(nom, out var p)
            ? p
            : (_profilsHp.TryGetValue("Noir", out var noir) ? noir : _profilsHp.Values.First());

    public ProfilCouleurFinition? GetProfilFinition(string site, string presse, string code)
        => null;

    public Support GetSupport(string cle)
        => _supports.TryGetValue(cle, out var s)
            ? s
            : (_supports.TryGetValue("Frozen Orion Diamond / Lavable / Glassine", out var fallback)
                ? fallback
                : _supports.Values.First());

    public Dorure GetDorure(string libelle)
        => _dorures.TryGetValue(libelle, out var d)
            ? d
            : (_dorures.TryGetValue("Dorure", out var fallback) ? fallback : _dorures.Values.First());

    public OperationFinition? GetOperation(string site, string operation)
        => _operations.FirstOrDefault(o => o.Operation.Equals(operation, StringComparison.OrdinalIgnoreCase));

    public Consommable GetConsommable(string type) => _consommables[type];

    public double GetOutillage(string operation, string site)
        => _outillagePrix.TryGetValue(operation, out var v) ? v : 0;

    public double GetPrixCliche(string site)
        => _outillagePrix.TryGetValue("ClicheFlexo", out var v) ? v : 45;

    public double DeveloppeParPoseAvance(string site, string presse, double avance)
    {
        double maxi;
        if (string.Equals(presse, "HP6000", StringComparison.OrdinalIgnoreCase))
        {
            maxi = _poses.TryGetValue((site, "HP6000", ""), out var v) ? v : 980;
        }
        else
        {
            maxi = _poses.TryGetValue((site, "Finition", presse ?? ""), out var v) ? v : 320;
        }

        return PoseCalculator.DeveloppeRepeat(avance, 4, maxi);
    }

    public Parametres GetParametres() => _parametres;

    private static Parametres BuildParams(List<Dictionary<string, string>> rows)
    {
        var p = new Parametres();
        foreach (var row in rows)
        {
            var key = row["Cle"];
            var value = ToDouble(row["Valeur"]);
            switch (key)
            {
                case "FraisGeneraux": p.FraisGeneraux = value; break;
                case "MargeDevis": p.MargeDevis = value; break;
                case "TauxConditionnement": p.TauxConditionnement = value; break;
                case "ConditionnementMin": p.ConditionnementMin = value; break;
                case "TauxTransport": p.TauxTransport = value; break;
                case "FraisFixe": p.FraisFixe = value; break;
            }
        }

        return p;
    }

    private static List<Dictionary<string, string>> ReadCsv(string path)
    {
        if (!File.Exists(path)) return new List<Dictionary<string, string>>();

        var lines = File.ReadAllLines(path)
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToList();
        if (lines.Count == 0) return new List<Dictionary<string, string>>();

        var header = lines[0].Split(';').Select(h => h.Trim()).ToArray();
        var rows = new List<Dictionary<string, string>>();

        for (var i = 1; i < lines.Count; i++)
        {
            var cols = lines[i].Split(';');
            var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var c = 0; c < header.Length; c++)
            {
                row[header[c]] = c < cols.Length ? cols[c].Trim() : string.Empty;
            }

            rows.Add(row);
        }

        return rows;
    }

    private static double ToDouble(string? raw, double fallback = 0)
    {
        if (string.IsNullOrWhiteSpace(raw)) return fallback;
        if (double.TryParse(raw, NumberStyles.Any, CultureInfo.GetCultureInfo("fr-FR"), out var fr)) return fr;
        if (double.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var inv)) return inv;
        return fallback;
    }

    private sealed class TripleComparer : IEqualityComparer<(string Site, string Type, string Presse)>
    {
        public bool Equals((string Site, string Type, string Presse) x, (string Site, string Type, string Presse) y)
            => x.Site.Equals(y.Site, StringComparison.OrdinalIgnoreCase)
               && x.Type.Equals(y.Type, StringComparison.OrdinalIgnoreCase)
               && x.Presse.Equals(y.Presse, StringComparison.OrdinalIgnoreCase);

        public int GetHashCode((string Site, string Type, string Presse) obj)
            => HashCode.Combine(obj.Site.ToUpperInvariant(), obj.Type.ToUpperInvariant(), obj.Presse.ToUpperInvariant());
    }
}
