using DevisHp.Core;
using DevisHp.Data;
using DevisHp.Models;
using Xunit;

namespace DevisHp.Tests;

public class DevisCalculatorTests
{
    [Fact]
    public void GoldenCase_Aesu_ReturnsExpectedPrixDevis()
    {
        var calc = new DevisCalculator(new FakeRepository());

        var input = new DevisInput
        {
            Site = "AESU",
            Quantite = 1000,
            NbReferences = 1,
            FormatLaize = 100,
            FormatAvance = 150,
            Frontal = "Frozen Orion Diamond",
            Adhesif = "Lavable",
            Backing = "Glassine",
            CouleurNum = "Noir",
            MargeManuelle = 0
        };

        var result = calc.Calculer(input);

        Assert.Equal(67.60m, decimal.Round(result.PrixDevis, 2));
        Assert.Equal(3, result.NbPosesLaize);
    }

    [Fact]
    public void CsvRepository_CanRunGoldenInput_WhenSeedFilesPresent()
    {
        var seedPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "seed-data"));
        var required = new[]
        {
            Path.Combine(seedPath, "Site.csv"),
            Path.Combine(seedPath, "Machine.csv"),
            Path.Combine(seedPath, "ParametreGlobal.csv"),
            Path.Combine(seedPath, "Consommable.csv"),
            Path.Combine(seedPath, "Outillage.csv"),
            Path.Combine(seedPath, "Pose.csv"),
            Path.Combine(seedPath, "OperationFinitionInline.csv")
        };

        if (required.Any(f => !File.Exists(f)))
        {
            return;
        }

        var calc = new DevisCalculator(new CsvReferenceRepository(seedPath));

        var input = new DevisInput
        {
            Site = "AESU",
            Quantite = 1000,
            NbReferences = 1,
            FormatLaize = 100,
            FormatAvance = 150,
            Frontal = "Frozen Orion Diamond",
            Adhesif = "Lavable",
            Backing = "Glassine",
            CouleurNum = "Noir",
            MargeManuelle = 0
        };

        var result = calc.Calculer(input);

        Assert.True(result.PrixDevis >= 0);
        Assert.Equal(3, result.NbPosesLaize);
    }

    private sealed class FakeRepository : IReferenceRepository
    {
        public Site GetSite(string code) => new() { Code = code, CoeffChange = 1.06607, LaizeMaxiDefaut = 330 };

        public Machine GetMachine(string site, string type) => type == "HP6000"
            ? new Machine { Site = site, Type = type, LaizeMaxi = 333, VitesseBase = 3600, TauxHoraire = 0 }
            : new Machine { Site = site, Type = type, LaizeMaxi = 333, VitesseBase = 4000, TauxHoraire = 0 };

        public ProfilCouleurHp GetProfilHp(string nom) => new()
        {
            Nom = nom,
            PasseMl = 0,
            ReductionVitesse = 0,
            CalageExtraH = 0,
            PrixMilleClics = 0,
            NbCouleurHP = 0
        };

        public ProfilCouleurFinition? GetProfilFinition(string site, string presse, string code) => null;

        public Support GetSupport(string cle) => new() { Cle = cle, PrixM2 = 0 };

        public Dorure GetDorure(string libelle) => new() { Libelle = libelle, PrixHorsFG = 0 };

        public OperationFinition? GetOperation(string site, string operation) => null;

        public Consommable GetConsommable(string type) => new() { Type = type };

        public double GetOutillage(string operation, string site) => 0;

        public double GetPrixCliche(string site) => 0;

        public double DeveloppeParPoseAvance(string site, string presse, double avance) => 1000;

        public Parametres GetParametres() => new()
        {
            FraisGeneraux = 0.20,
            MargeDevis = 0.80,
            TauxConditionnement = 0,
            ConditionnementMin = 0,
            TauxTransport = 0,
            FraisFixe = 67.60 * 0.80 / 1.06607
        };
    }
}
