using System.Globalization;
using DevisHp.Core;
using DevisHp.Models;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DevisHp.Data;

public class DataverseReferenceRepository : IReferenceRepository
{
    private readonly ServiceClient _svc;

    public DataverseReferenceRepository(string connectionString)
    {
        _svc = new ServiceClient(connectionString);
        if (!_svc.IsReady)
        {
            throw new InvalidOperationException($"Dataverse connection failed: {_svc.LastError}");
        }
    }

    public Site GetSite(string code)
    {
        var e = GetSingle(DataverseSchema.Tables.Site, new ColumnSet("hp_code", "hp_devise", "hp_coeffchange", "hp_laizemaxidefaut"),
            new ConditionExpression("hp_code", ConditionOperator.Equal, code));

        return new Site
        {
            Code = S(e, "hp_code"),
            Devise = S(e, "hp_devise", "EUR"),
            CoeffChange = D(e, "hp_coeffchange", 1),
            LaizeMaxiDefaut = D(e, "hp_laizemaxidefaut", 330)
        };
    }

    public Machine GetMachine(string site, string type)
    {
        var q = new QueryExpression(DataverseSchema.Tables.Machine)
        {
            ColumnSet = new ColumnSet("hp_site", "hp_type", "hp_laizemini", "hp_laizemaxi", "hp_tauxhoraire", "hp_vitessebase", "hp_calageprimermin")
        };
        q.Criteria.AddCondition("hp_site", ConditionOperator.Equal, site);
        var rows = _svc.RetrieveMultiple(q).Entities;

        var e = rows.FirstOrDefault(r =>
            string.Equals(GetChoiceOrText(r, "hp_type"), type, StringComparison.OrdinalIgnoreCase));

        if (e is null)
            throw new InvalidOperationException($"Machine not found for site={site}, type={type}");

        return new Machine
        {
            Site = site,
            Type = GetChoiceOrText(e, "hp_type"),
            LaizeMini = D(e, "hp_laizemini"),
            LaizeMaxi = D(e, "hp_laizemaxi"),
            TauxHoraire = D(e, "hp_tauxhoraire"),
            VitesseBase = D(e, "hp_vitessebase"),
            CalagePrimerMin = D(e, "hp_calageprimermin")
        };
    }

    public ProfilCouleurHp GetProfilHp(string nom)
    {
        var e = GetSingle(DataverseSchema.Tables.ProfilCouleurHp,
            new ColumnSet("hp_nom", "hp_reductionvitesse", "hp_pourcentpasse", "hp_passeml", "hp_calageextrah", "hp_prixmilleclics", "hp_nbcouleurhp"),
            new ConditionExpression("hp_nom", ConditionOperator.Equal, nom));

        return new ProfilCouleurHp
        {
            Nom = S(e, "hp_nom"),
            ReductionVitesse = D(e, "hp_reductionvitesse"),
            PourcentPasse = D(e, "hp_pourcentpasse"),
            PasseMl = D(e, "hp_passeml"),
            CalageExtraH = D(e, "hp_calageextrah"),
            PrixMilleClics = D(e, "hp_prixmilleclics"),
            NbCouleurHP = I(e, "hp_nbcouleurhp")
        };
    }

    public ProfilCouleurFinition? GetProfilFinition(string site, string presse, string code)
    {
        var q = new QueryExpression(DataverseSchema.Tables.ProfilCouleurFinition)
        {
            ColumnSet = new ColumnSet("hp_site", "hp_press", "hp_codecouleur", "hp_nbcouleurflexo", "hp_nbcouleurseri", "hp_nbvernisflexo", "hp_nbvernisseri")
        };
        q.Criteria.AddCondition("hp_site", ConditionOperator.Equal, site);
        q.Criteria.AddCondition("hp_press", ConditionOperator.Equal, presse);
        q.Criteria.AddCondition("hp_codecouleur", ConditionOperator.Equal, code);
        var e = _svc.RetrieveMultiple(q).Entities.FirstOrDefault();
        if (e is null) return null;

        return new ProfilCouleurFinition
        {
            NbCouleurFlexo = I(e, "hp_nbcouleurflexo"),
            NbCouleurSeri = I(e, "hp_nbcouleurseri"),
            NbVernisFlexo = I(e, "hp_nbvernisflexo"),
            NbVernisSeri = I(e, "hp_nbvernisseri")
        };
    }

    public Support GetSupport(string cle)
    {
        var e = GetSingle(DataverseSchema.Tables.Support, new ColumnSet("hp_cle", "hp_prixm2"),
            new ConditionExpression("hp_cle", ConditionOperator.Equal, cle));
        return new Support { Cle = S(e, "hp_cle"), PrixM2 = D(e, "hp_prixm2") };
    }

    public Dorure GetDorure(string libelle)
    {
        var e = GetSingle(DataverseSchema.Tables.Dorure, new ColumnSet("hp_libelle", "hp_prixhorsfg"),
            new ConditionExpression("hp_libelle", ConditionOperator.Equal, libelle));
        return new Dorure { Libelle = S(e, "hp_libelle"), PrixHorsFG = D(e, "hp_prixhorsfg") };
    }

    public OperationFinition? GetOperation(string site, string operation)
    {
        var q = new QueryExpression(DataverseSchema.Tables.OperationFinitionInline)
        {
            ColumnSet = new ColumnSet("hp_site", "hp_operation", "hp_calageextra", "hp_passe", "hp_gache", "hp_vitessemax")
        };
        q.Criteria.AddCondition("hp_site", ConditionOperator.Equal, site);
        var e = _svc.RetrieveMultiple(q).Entities.FirstOrDefault(r =>
            string.Equals(GetChoiceOrText(r, "hp_operation"), operation, StringComparison.OrdinalIgnoreCase));
        if (e is null) return null;

        return new OperationFinition
        {
            Operation = GetChoiceOrText(e, "hp_operation"),
            CalageExtra = D(e, "hp_calageextra"),
            Passe = D(e, "hp_passe"),
            Gache = D(e, "hp_gache"),
            VitesseMax = D(e, "hp_vitessemax")
        };
    }

    public Consommable GetConsommable(string type)
    {
        var q = new QueryExpression(DataverseSchema.Tables.Consommable)
        {
            ColumnSet = new ColumnSet("hp_type", "hp_prixkg", "hp_grammem2", "hp_grammefixe", "hp_tauxencrage")
        };
        var e = _svc.RetrieveMultiple(q).Entities.FirstOrDefault(r =>
            string.Equals(GetChoiceOrText(r, "hp_type"), type, StringComparison.OrdinalIgnoreCase));

        if (e is null)
            throw new InvalidOperationException($"Consommable not found: {type}");

        return new Consommable
        {
            Type = GetChoiceOrText(e, "hp_type"),
            PrixKg = D(e, "hp_prixkg"),
            GrammeM2 = D(e, "hp_grammem2"),
            GrammeFixe = D(e, "hp_grammefixe"),
            TauxEncrage = D(e, "hp_tauxencrage")
        };
    }

    public double GetOutillage(string operation, string site)
    {
        var q = new QueryExpression(DataverseSchema.Tables.Outillage)
        {
            ColumnSet = new ColumnSet("hp_operation", "hp_site", "hp_prix")
        };
        q.Criteria.AddCondition("hp_site", ConditionOperator.Equal, site);
        var e = _svc.RetrieveMultiple(q).Entities.FirstOrDefault(r =>
            string.Equals(GetChoiceOrText(r, "hp_operation"), operation, StringComparison.OrdinalIgnoreCase));
        return e is null ? 0 : D(e, "hp_prix");
    }

    public double GetPrixCliche(string site)
    {
        return GetOutillage("ClicheFlexo", site);
    }

    public double DeveloppeParPoseAvance(string site, string presse, double avance)
    {
        var q = new QueryExpression(DataverseSchema.Tables.Pose)
        {
            ColumnSet = new ColumnSet("hp_site", "hp_type", "hp_presse", "hp_developpemaxi")
        };
        q.Criteria.AddCondition("hp_site", ConditionOperator.Equal, site);

        var rows = _svc.RetrieveMultiple(q).Entities;
        var row = rows.FirstOrDefault(r =>
            string.Equals(GetChoiceOrText(r, "hp_type"), "HP6000", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(presse, "HP6000", StringComparison.OrdinalIgnoreCase))
            ?? rows.FirstOrDefault(r => string.Equals(S(r, "hp_presse"), presse, StringComparison.OrdinalIgnoreCase));

        var developpeMaxi = row is null ? 980 : D(row, "hp_developpemaxi", 980);
        return PoseCalculator.DeveloppeRepeat(avance, 4, developpeMaxi);
    }

    public Parametres GetParametres()
    {
        var q = new QueryExpression(DataverseSchema.Tables.ParametreGlobal)
        {
            ColumnSet = new ColumnSet("hp_cle", "hp_valeur")
        };

        var rows = _svc.RetrieveMultiple(q).Entities;
        var p = new Parametres();

        foreach (var e in rows)
        {
            var key = S(e, "hp_cle");
            var value = D(e, "hp_valeur");
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

    private Entity GetSingle(string table, ColumnSet columns, params ConditionExpression[] conditions)
    {
        var q = new QueryExpression(table)
        {
            ColumnSet = columns,
            TopCount = 1
        };

        foreach (var c in conditions)
            q.Criteria.AddCondition(c);

        var e = _svc.RetrieveMultiple(q).Entities.FirstOrDefault();
        if (e is null)
            throw new InvalidOperationException($"No row found in {table} for requested criteria.");

        return e;
    }

    private static string GetChoiceOrText(Entity e, string attr)
    {
        if (e.FormattedValues.TryGetValue(attr, out var label) && !string.IsNullOrWhiteSpace(label))
            return label;

        return S(e, attr);
    }

    private static string S(Entity e, string attr, string fallback = "")
    {
        if (!e.Contains(attr) || e[attr] is null) return fallback;
        return e[attr] switch
        {
            string s => s,
            AliasedValue a when a.Value is string s => s,
            OptionSetValue o => o.Value.ToString(CultureInfo.InvariantCulture),
            _ => e[attr].ToString() ?? fallback
        };
    }

    private static int I(Entity e, string attr, int fallback = 0)
    {
        if (!e.Contains(attr) || e[attr] is null) return fallback;
        return e[attr] switch
        {
            int i => i,
            long l => (int)l,
            decimal d => (int)d,
            double d => (int)d,
            _ => int.TryParse(S(e, attr), out var v) ? v : fallback
        };
    }

    private static double D(Entity e, string attr, double fallback = 0)
    {
        if (!e.Contains(attr) || e[attr] is null) return fallback;
        return e[attr] switch
        {
            decimal d => (double)d,
            double d => d,
            float f => f,
            int i => i,
            long l => l,
            Money m => m.Value,
            _ => ParseDouble(S(e, attr), fallback)
        };
    }

    private static double ParseDouble(string value, double fallback = 0)
    {
        if (string.IsNullOrWhiteSpace(value)) return fallback;
        if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var inv)) return inv;
        if (double.TryParse(value, NumberStyles.Any, CultureInfo.GetCultureInfo("fr-FR"), out var fr)) return fr;
        return fallback;
    }
}
