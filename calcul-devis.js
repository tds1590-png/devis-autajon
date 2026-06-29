/**
 * MOTEUR DE CALCUL DEVIS - JavaScript
 * Port du C# DevisCalculator
 * Basé sur la logique HP6000 AUTAJON
 */

// ─── PARAMETRES GLOBAUX ───────────────────────────────────────────────────

const PARAMETERS = {
    FraisGeneraux: 0.20,
    MargeDevis: 0.80,
    TauxConditionnement: 0.009,
    ConditionnementMin: 14.81,
    TauxTransport: 0.063,
    FraisFixe: 50
};

// ─── DONNÉES DE RÉFÉRENCE (Hardcodées - AEAT par défaut) ───────────────────

const SITES = {
    'AEAT': {
        code: 'AEAT',
        laizeMaxiDefaut: 330,
        devise: 'EUR',
        coeffChange: 1.0
    }
};

const MACHINES = {
    'AEAT-HP6000': {
        site: 'AEAT',
        type: 'HP6000',
        laizeMini: 50,
        laizeMaxi: 330,
        tauxHoraire: 150,
        vitesseBase: 150,
        calageMin: 30
    },
    'AEAT-Finition': {
        site: 'AEAT',
        type: 'Finition',
        laizeMini: 50,
        laizeMaxi: 330,
        tauxHoraire: 80,
        vitesseBase: 100,
        calageMin: 20
    }
};

// Profils couleur simplifiés (on récupère depuis seed-data en réalité)
const PROFILS_HP = {
    'Noir': { nom: 'Noir', reductionVitesse: 0.05, passeMl: 100, calageExtraH: 15, prixMilleClics: 5, nbCouleur: 1 },
    'Noir+1 Silver': { nom: 'Noir+1 Silver', reductionVitesse: 0.10, passeMl: 150, calageExtraH: 20, prixMilleClics: 10, nbCouleur: 2 },
    '2 Couleurs': { nom: '2 Couleurs', reductionVitesse: 0.12, passeMl: 200, calageExtraH: 25, prixMilleClics: 15, nbCouleur: 2 },
    '3 Couleurs': { nom: '3 Couleurs', reductionVitesse: 0.15, passeMl: 250, calageExtraH: 30, prixMilleClics: 20, nbCouleur: 3 }
};

// Support (Frontal/Adhésif/Backing) avec prix approximatifs
const SUPPORTS_PRIX = {
    'Coton blanc/Permanent PLUS/Glassine': 1.80,
    'Coton blanc/Lavable/Glassine': 1.70,
    'PP 50 Blanc/Permanent PLUS/Glassine': 0.44,
    'PP 60 Blanc/Permanent PLUS/Glassine': 0.50,
    'Velin Blanc/Permanent PLUS/Glassine': 1.20,
    'Vergé blanc/Permanent PLUS/Glassine': 1.30
};

// ─── LOGIQUE DE CALCUL ─────────────────────────────────────────────────────

/**
 * Calcule le nombre de poses selon la largeur de format
 * Formule: (laizeMaxi - rives) / (formatLaize + interposes) 
 * 
 * @param {number} formatLaize - Largeur du format (mm)
 * @param {number} interPoses - Distance entre poses (mm)
 * @param {number} rives - Rives min (mm)
 * @param {number} laizeMaxi - Largeur machine (mm)
 * @param {boolean} dorure - Y a-t-il dorure?
 * @returns {number} Nombre de poses
 */
function calculerNbPoses(formatLaize, interPoses = 4, rives = 18, laizeMaxi = 330, dorure = false) {
    const espacementMin = dorure ? 25 : interPoses;
    const laizeDisponible = laizeMaxi - 2 * rives;
    const nbPoses = Math.floor(laizeDisponible / (formatLaize + espacementMin));
    return Math.max(1, nbPoses);
}

/**
 * Convertit ml en m²
 * @param {number} ml - Mètres linéaires
 * @param {number} laizeUtile - Largeur utile (mm)
 * @returns {number} Surface en m²
 */
function mlVersM2(ml, laizeUtile) {
    return (ml * laizeUtile) / 1000.0;
}

/**
 * CALCUL PRINCIPAL - Retourne tous les résultats
 */
function calculerDevis(input) {
    const {
        site = 'AEAT',
        quantite = 1000,
        nbReferences = 1,
        formatLaize = 100,
        formatAvance = 150,
        frontal = 'Coton blanc',
        adhesif = 'Permanent PLUS',
        backing = 'Glassine',
        couleurNum = 'Noir',
        numerotation = false,
        dorures = [],
        dorureGalbee = false,
        gaufrage = false,
        foulage = false,
        pelliculage = false,
        margeManuelle = 0
    } = input;

    // Récupérer les données de référence
    const siteData = SITES[site] || SITES['AEAT'];
    const hpData = MACHINES[`${site}-HP6000`] || MACHINES['AEAT-HP6000'];
    const finData = MACHINES[`${site}-Finition`] || MACHINES['AEAT-Finition'];
    const profData = PROFILS_HP[couleurNum] || PROFILS_HP['Noir'];
    
    // Prix du support (approximation)
    const supportKey = `${frontal}/${adhesif}/${backing}`;
    const prixSupport = SUPPORTS_PRIX[supportKey] || 1.50;

    const P = PARAMETERS;
    const fg = 1 + P.FraisGeneraux;  // Frais généraux

    // ─── 1. IMPOSITION ─────────────────────────────────────────────────────
    const laizeUtile = Math.min(siteData.laizeMaxiDefaut, hpData.laizeMaxi);
    const dorure = dorures.length > 0 || dorureGalbee;
    const nbPoses = calculerNbPoses(formatLaize, 4, 18, laizeUtile, dorure);
    const mlUtile = (quantite * (formatAvance / 1000.0)) / nbPoses;

    // ─── 2. MATIÈRE (ML) ───────────────────────────────────────────────────
    const mlCalHP = profData.passeMl * Math.max(1, nbReferences);  // Calage HP + changement
    
    // Opérations de finition
    let mlCalFin = 0;
    let mlGache = 0;
    if (gaufrage) { mlCalFin += 50; mlGache += 0.05 * mlUtile; }
    if (foulage) { mlCalFin += 40; mlGache += 0.04 * mlUtile; }
    if (pelliculage) { mlCalFin += 30; mlGache += 0.03 * mlUtile; }
    
    const mlTotal = mlCalHP + mlCalFin + mlUtile + mlGache;
    const m2Total = mlVersM2(mlTotal, laizeUtile);
    const mlRoulFin = mlUtile + mlGache;

    // ─── 3. COÛTS ─────────────────────────────────────────────────────────
    // Support
    const cSupport = prixSupport * m2Total * fg;

    // Dorure
    let cDorure = 0;
    if (dorures.length > 0) {
        cDorure = dorures.length * 3.50 * mlTotal * 0.27 * fg;  // ~3.50€/dorure
    }
    if (dorureGalbee) {
        cDorure += 2.80 * mlTotal * 0.27 * fg;  // ~2.80€ dorure galbée
    }

    // Pelliculage
    let cPellic = 0;
    if (pelliculage) {
        cPellic = 0.976 * m2Total * fg;  // ~0.976€/m² pour pelliculage
    }

    // HP6000
    const vitHP = hpData.vitesseBase * (1 - profData.reductionVitesse);
    const cHP = profData.calageExtraH * hpData.tauxHoraire 
              + (mlTotal / Math.max(1, vitHP)) * hpData.tauxHoraire;

    // Finition
    let cFin = 0;
    if (mlCalFin > 0) {
        cFin = mlCalFin * finData.tauxHoraire 
             + (mlRoulFin / Math.max(1, finData.vitesseBase)) * finData.tauxHoraire;
    }

    // Encres (estimation simple)
    const cEncres = profData.nbCouleur * 8.50 * 0.001 * m2Total;  // ~8.50€/kg encre

    // Clics
    const cClics = (mlTotal / 1000) * profData.prixMilleClics;

    // Clichés (estimation)
    const cCliches = nbReferences * profData.nbCouleur * 15;  // ~15€ par cliché

    // Conditionnement & Transport
    const emb = Math.max(P.TauxConditionnement * cSupport, P.ConditionnementMin);
    const transp = P.TauxTransport * cSupport;

    // COÛT TOTAL
    const cTotal = cSupport + cDorure + cPellic + cHP + cFin 
                 + cEncres + cClics + cCliches + emb + transp + P.FraisFixe;

    // ─── 4. PRIX ──────────────────────────────────────────────────────────
    const pDevis = (cTotal / P.MargeDevis) * siteData.coeffChange;
    const pClient = pDevis * (1 + margeManuelle);

    // Marge brute = (Prix - Coûts Matière) / Prix
    const cMB = cSupport + cDorure + cPellic + cEncres + cClics;
    const marge = pDevis > 0 ? (pDevis - cMB) / pDevis : 0;

    // Outillages (estimation)
    let outillages = 200;  // Base: découpe
    if (dorure) outillages += 150;
    if (gaufrage) outillages += 100;
    if (pelliculage) outillages += 50;

    // ─── RÉSULTAT ─────────────────────────────────────────────────────────
    return {
        quantite: quantite,
        prixDevis: Math.round(pDevis * 100) / 100,
        prixClient: Math.round(pClient * 100) / 100,
        margeBrute: Math.round(marge * 10000) / 100,  // En %
        coutTotal: Math.round(cTotal * 100) / 100,
        outillages: Math.round(outillages * 100) / 100,
        mlTotal: Math.round(mlTotal * 10) / 10,
        m2Total: Math.round(m2Total * 1000) / 1000,
        nbPosesLaize: nbPoses,
        
        // Détails pour debug
        details: {
            laizeUtile: laizeUtile,
            mlUtile: Math.round(mlUtile * 10) / 10,
            mlCalHP: mlCalHP,
            mlCalFin: mlCalFin,
            mlGache: Math.round(mlGache * 10) / 10,
            cSupport: Math.round(cSupport * 100) / 100,
            cDorure: Math.round(cDorure * 100) / 100,
            cPellic: Math.round(cPellic * 100) / 100,
            cHP: Math.round(cHP * 100) / 100,
            cFin: Math.round(cFin * 100) / 100,
            cEncres: Math.round(cEncres * 100) / 100,
            cClics: Math.round(cClics * 100) / 100,
            cCliches: Math.round(cCliches * 100) / 100,
            emb: Math.round(emb * 100) / 100,
            transp: Math.round(transp * 100) / 100
        }
    };
}

// ─── EXPORTS ──────────────────────────────────────────────────────────────

if (typeof module !== 'undefined' && module.exports) {
    module.exports = { calculerDevis, calculerNbPoses, mlVersM2 };
}
