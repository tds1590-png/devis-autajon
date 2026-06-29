using DevisHp.Models;
namespace DevisHp.Data;

public interface IReferenceRepository {
    Site                   GetSite(string code);
    Machine                GetMachine(string site, string type);
    ProfilCouleurHp        GetProfilHp(string nom);
    ProfilCouleurFinition? GetProfilFinition(string site, string presse, string code);
    Support                GetSupport(string cle);       // "Frontal / Adhesif / Backing"
    Dorure                 GetDorure(string libelle);
    OperationFinition?     GetOperation(string site, string operation);
    Consommable            GetConsommable(string type);  // "EncreFlexo" | "EncreSeri" | …
    double                 GetOutillage(string operation, string site);
    double                 GetPrixCliche(string site);
    /// <summary>
    /// Développé-repeat machine (mm) utilisé pour le comptage des clics.
    /// Implémentation : PoseCalculator.DeveloppeRepeat(avance, 4, DeveloppeMaxi(site, presse))
    /// DeveloppeMaxi = table Pose.csv (HP6000=980, AESU HP6000=981, finitions voir CSV)
    /// </summary>
    double                 DeveloppeParPoseAvance(string site, string presse, double avance);
    Parametres             GetParametres();
}
