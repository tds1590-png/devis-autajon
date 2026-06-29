using DevisHp.Models; using DevisHp.Data;
namespace DevisHp.Core;

// Portage fidele de la feuille Excel "Calcul". Sequentiel, sans circularite.
// Appeler Calculer() une fois par quantite (4 appels cote app).
public class DevisCalculator {
    private readonly IReferenceRepository _r;
    public DevisCalculator(IReferenceRepository r) => _r = r;

    public DevisResult Calculer(DevisInput x) {
        var P    = _r.GetParametres();
        var site = _r.GetSite(x.Site);
        var hp   = _r.GetMachine(x.Site, "HP6000");
        var fin  = _r.GetMachine(x.Site, "Finition");
        var prof = _r.GetProfilHp(x.CouleurNum);
        var sup  = _r.GetSupport($"{x.Frontal} / {x.Adhesif} / {x.Backing}");
        var pFin = x.CouleurFinition != null && x.PresseFinition != null
                 ? _r.GetProfilFinition(x.Site, x.PresseFinition, x.CouleurFinition)
                 : null;
        bool dorure = x.Dorures.Count > 0 || x.DorureGalbee;
        double fg = 1 + P.FraisGeneraux;

        // 1. IMPOSITION
        double laizeUtile = Math.Min(site.LaizeMaxiDefaut, hp.LaizeMaxi);
        int nbPoses = PoseCalculator.NbPosesLaize(x.FormatLaize, 4, 18, laizeUtile, dorure);
        if (nbPoses < 1) nbPoses = 1;
        double mlUtile = x.Quantite * (x.FormatAvance / 1000.0) / nbPoses;

        // 2. MATIERE (ml)
        var ops        = OperationsActives(x);
        double mlCalHP = prof.PasseMl * Math.Max(1, x.NbReferences);   // HP : calage + chgt produit
        double mlCalFin= ops.Sum(o => o.Passe);
        double mlGache = ops.Sum(o => o.Gache * mlUtile);
        double mlTotal = mlCalHP + mlCalFin + mlUtile + mlGache;
        double M2(double ml) => ml * laizeUtile / 1000.0;
        double mlRoulFin = mlUtile + mlGache;

        // 3. COUTS
        double cSupport  = sup.PrixM2 * M2(mlTotal) * fg;
        double cDorure   = x.Dorures.Sum(d => _r.GetDorure(d).PrixHorsFG * mlTotal * 0.27) * fg;
        double cPellic   = 0; // TODO : brancher FilmPelliculage si x.Pelliculage

        // HP6000
        double vitHP = hp.VitesseBase * (1 - prof.ReductionVitesse);
        double cHP   = prof.CalageExtraH * hp.TauxHoraire
                     + mlTotal / Math.Max(1, vitHP) * hp.TauxHoraire;

        // 2e machine finition
        double cFin = 0;
        if (ops.Count > 0) {
            double vitFin = ops.Min(o => o.VitesseMax);
            cFin = ops.Sum(o => o.CalageExtra) * fin.TauxHoraire
                 + mlRoulFin / Math.Max(1, vitFin) * fin.TauxHoraire;
        }

        double cEncres  = CoutEncres(pFin, M2(mlRoulFin));
        double cClics   = CoutClics(x, prof, hp, mlTotal);
        double cCliches = CoutCliches(x, pFin, mlRoulFin, fg, P);
        double emb      = Math.Max(P.TauxConditionnement * cSupport, P.ConditionnementMin);
        double transp   = P.TauxTransport * cSupport;
        double cTotal   = cSupport + cDorure + cPellic + cHP + cFin
                        + cEncres + cClics + cCliches + emb + transp + P.FraisFixe;

        // 4. PRIX
        double pDevis  = cTotal / P.MargeDevis * site.CoeffChange;
        double pClient = pDevis * (1 + x.MargeManuelle);
        double cMB     = cSupport + cDorure + cPellic + cEncres + cClics;
        double marge   = pDevis == 0 ? 0 : (pDevis - cMB) / pDevis;

        return new DevisResult {
            Quantite = x.Quantite, PrixDevis = (decimal)pDevis,
            PrixClient = (decimal)pClient, MargeBrute = marge,
            CoutTotal = (decimal)cTotal,
            Outillages = (decimal)Outillages(x),
            MlTotal = mlTotal, M2Total = M2(mlTotal), NbPosesLaize = nbPoses
        };
    }

    // -- Helpers -------------------------------------------------------------

    private static int NbEt(double laize, double ip, double rives, double maxi, bool dor)
        => PoseCalculator.NbPosesLaize(laize, ip, rives, maxi, dor);

    private List<OperationFinition> OperationsActives(DevisInput x) {
        var n = new List<string>();
        if (x.Dorures.Count > 0) n.Add("Dorure");
        if (x.DorureGalbee)      n.Add("Dorure galbee");
        if (x.GalbeASec)         n.Add("Galbe a sec");
        if (x.Gaufrage)          n.Add("Gaufrage");
        if (x.Foulage)           n.Add("Foulage");
        if (x.Pelliculage)       n.Add("Pelliculage");
        return n.Select(op => _r.GetOperation(x.Site, op))
                .Where(o => o != null).Cast<OperationFinition>().ToList();
    }

    // Encres calage = somme nbCouleur x gFixe/1000 x prix/kg
    // Encres roulage = somme nbCouleur x gM2/1000 x prix/kg x taux x m2
    private double CoutEncres(ProfilCouleurFinition? pf, double m2) {
        if (pf == null) return 0;
        double Calage(int nb, Consommable c) => nb * c.GrammeFixe / 1000.0 * c.PrixKg;
        double Roulage(int nb, Consommable c) => nb * c.GrammeM2 / 1000.0 * c.PrixKg * c.TauxEncrage * m2;
        var ef = _r.GetConsommable("EncreFlexo");  var es = _r.GetConsommable("EncreSeri");
        var vf = _r.GetConsommable("VernisFlexo"); var vs = _r.GetConsommable("VernisSeri");
        return Calage(pf.NbCouleurFlexo,ef) + Calage(pf.NbCouleurSeri,es)
             + Calage(pf.NbVernisFlexo,vf) + Calage(pf.NbVernisSeri,vs)
             + Roulage(pf.NbCouleurFlexo,ef) + Roulage(pf.NbCouleurSeri,es)
             + Roulage(pf.NbVernisFlexo,vf) + Roulage(pf.NbVernisSeri,vs);
    }

    // Clics = (ml / developpeRepeat) x nbCouleur ; cout = frappes/1000 x prix/1000 clics
    private double CoutClics(DevisInput x, ProfilCouleurHp prof, Machine hp, double ml) {
        double devR = _r.DeveloppeParPoseAvance(x.Site, "HP6000", x.FormatAvance);
        if (devR <= 0) return 0;
        return ml / (devR / 1000.0) * prof.NbCouleurHP / 1000.0 * prof.PrixMilleClics;
    }

    // Renouvellement cliches : terme roulage + prixCliche x nbRef x (1+FG) x nbCouleurs
    private double CoutCliches(DevisInput x, ProfilCouleurFinition? pf,
                                double mlRoulFin, double fg, Parametres P) {
        if (pf == null) return 0;
        int nb = pf.NbCouleurFlexo + pf.NbCouleurSeri + pf.NbVernisFlexo + pf.NbVernisSeri;
        double devR = _r.DeveloppeParPoseAvance(x.Site, x.PresseFinition ?? "", x.FormatAvance);
        double t1 = devR > 0 ? mlRoulFin / (devR / 1000.0) * nb / 1000.0 : 0;
        double t2 = _r.GetPrixCliche(x.Site) * x.NbReferences * fg * nb;
        return t1 + t2;
    }

    private double Outillages(DevisInput x) {
        double t = _r.GetOutillage("Decoupe", x.Site);
        if (x.Dorures.Count > 0)          t += _r.GetOutillage("Dorure", x.Site);
        if (x.DorureGalbee || x.GalbeASec) t += _r.GetOutillage("Galbe", x.Site);
        if (x.Gaufrage) t += _r.GetOutillage("Gaufrage", x.Site);
        if (x.Foulage)  t += _r.GetOutillage("Foulage", x.Site);
        return t;
    }
}
