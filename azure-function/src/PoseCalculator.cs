namespace DevisHp.Core;

/// <summary>
/// Portage exact de la feuille Excel "Pose".
/// Interpose = 4 mm entre étiquettes ; Rives = 18 mm (9 + 9) marges latérales.
/// </summary>
public static class PoseCalculator {

    /// <summary>
    /// Nb d'étiquettes dans la laize.
    /// 1re pose = laize + rives ; chaque suivante ajoute (laize + interpose).
    /// Si dorure active : -2 poses (contrainte mécanique).
    /// </summary>
    public static int NbPosesLaize(
        double laize, double interpose, double rives, double laizeMaxi, bool dorure) {
        int    n = 0;
        double w = laize + rives;
        while (w <= laizeMaxi) { n++; w += laize + interpose; }
        if (dorure) n -= 2;
        return Math.Max(1, n);
    }

    /// <summary>
    /// Nb de poses dans le sens avance (finition) :
    /// floor(developpeMaxiMachine / (avance + interpose))
    /// </summary>
    public static int NbPosesAvance(double avance, double interpose, double developpeMaxi) {
        double pas = avance + interpose;
        return pas <= 0 ? 1 : Math.Max(1, (int)Math.Floor(developpeMaxi / pas));
    }

    /// <summary>
    /// Développé-repeat machine (mm) = (avance + interpose) × NbPosesAvance.
    /// Sert au comptage des clics HP : frappes = totalMl / (repeat/1000) × nbCouleurs.
    /// </summary>
    public static double DeveloppeRepeat(double avance, double interpose, double developpeMaxi)
        => (avance + interpose) * NbPosesAvance(avance, interpose, developpeMaxi);
}

// DeveloppeMaxi par site (table seed-data/Pose.csv) :
// HP6000 : AEME/AEEP/AEAT/AEBO/AELO/AELI = 980 mm | AESU = 981 mm
// Finition : AEME-Cartes=305 AEEP-Cartes=356 AEAT-Galaxie=330
//            AEBO-Cartes=320 AELO-Galaxie=320 AESU-Cartes=320 AELI-Cartes=305
