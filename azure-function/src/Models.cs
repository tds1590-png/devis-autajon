namespace DevisHp.Models;

// ── Entrée (1 appel par quantité) ─────────────────────────────────────────
public record DevisInput {
    public string Site { get; init; } = "";
    public int    Quantite { get; init; }
    public int    NbReferences { get; init; }
    public double FormatLaize { get; init; }    // mm largeur
    public double FormatAvance { get; init; }   // mm hauteur/développé
    public string Frontal { get; init; } = "";
    public string Adhesif { get; init; } = "";
    public string Backing { get; init; } = "";
    public string CouleurNum { get; init; } = "";
    public bool   Numerotation { get; init; }
    public string? CouleurFinition { get; init; }
    public string? PresseFinition  { get; init; }
    public string? TypeVernis { get; init; }
    public List<string> Dorures { get; init; } = new();
    public bool DorureGalbee { get; init; }
    public bool GalbeASec    { get; init; }
    public bool Gaufrage     { get; init; }
    public bool Foulage      { get; init; }
    public bool Pelliculage  { get; init; }
    public double MargeManuelle { get; init; }  // ex. 0.15 = +15 %
}

// ── Sortie (1 résultat par quantité) ─────────────────────────────────────
public record DevisResult {
    public int     Quantite      { get; init; }
    public decimal PrixDevis     { get; init; }   // /1000 unités, devise site
    public decimal PrixClient    { get; init; }
    public double  MargeBrute    { get; init; }
    public decimal CoutTotal     { get; init; }
    public decimal Outillages    { get; init; }   // facturés séparément
    public double  MlTotal       { get; init; }
    public double  M2Total       { get; init; }
    public int     NbPosesLaize  { get; init; }
}

// ── Bloc client + devis (pour la lettre et l'email) ───────────────────────
public record InfoDevis {
    public string   Client { get; init; } = "";
    public string   Interlocuteur { get; init; } = "";
    public string   Adresse { get; init; } = "";
    public string   Ville { get; init; } = "";
    public string   CodePostal { get; init; } = "";
    public string   EmailClient { get; init; } = "";
    public string   Designation { get; init; } = "";
    public string   NumDevis { get; init; } = "";
    public DateTime Date { get; init; } = DateTime.Today;
    public string   Site { get; init; } = "";
    public string   Commercial { get; init; } = "";
    public string   CommercialTel { get; init; } = "";
    public string   CommercialEmail { get; init; } = "";
    public int      NbReferences { get; init; }
}

// ── Entités référentielles (Dataverse) ────────────────────────────────────
public record Site {
    public string Code { get; init; } = "";
    public string Devise { get; init; } = "EUR";
    public double CoeffChange { get; init; } = 1;
    public double LaizeMaxiDefaut { get; init; } = 330;
}

public record Machine {
    public string Site { get; init; } = "";
    public string Type { get; init; } = "";      // "HP6000" | "Finition"
    public double LaizeMini { get; init; }
    public double LaizeMaxi { get; init; }
    public double TauxHoraire { get; init; }
    public double VitesseBase { get; init; }
    public double CalagePrimerMin { get; init; }
}

public record ProfilCouleurHp {
    public string Nom { get; init; } = "";
    public double ReductionVitesse { get; init; }
    public double PourcentPasse { get; init; }
    public double PasseMl { get; init; }
    public double CalageExtraH { get; init; }
    public double PrixMilleClics { get; init; }
    public int    NbCouleurHP { get; init; }
}

public record ProfilCouleurFinition {
    public int NbCouleurFlexo { get; init; }
    public int NbCouleurSeri  { get; init; }
    public int NbVernisFlexo  { get; init; }
    public int NbVernisSeri   { get; init; }
}

public record Support {
    public string Cle   { get; init; } = "";
    public double PrixM2 { get; init; }
}

public record Dorure {
    public string Libelle    { get; init; } = "";
    public double PrixHorsFG { get; init; }
}

public record OperationFinition {
    public string Operation  { get; init; } = "";
    public double CalageExtra { get; init; }
    public double Passe      { get; init; }
    public double Gache      { get; init; }
    public double VitesseMax { get; init; }
}

public record Consommable {
    public string Type        { get; init; } = "";
    public double PrixKg      { get; init; }
    public double GrammeM2    { get; init; }
    public double GrammeFixe  { get; init; }
    public double TauxEncrage { get; init; }
}

public class Parametres {
    public double FraisGeneraux       = 0.20;
    public double MargeDevis          = 0.80;
    public double TauxConditionnement = 0.009;
    public double ConditionnementMin  = 14.81;
    public double TauxTransport       = 0.063;
    public double FraisFixe           = 50;
}
