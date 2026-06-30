/**
 * MOTEUR DE CALCUL DEVIS - JavaScript EXACT
 * Port FIDÈLE du C# DevisCalculator.cs
 * Source: Azure Function DevisCalculator
 */

// ============================================================================
// 1. DONNÉES DE RÉFÉRENCE (À charger depuis seed-data en production)
// ============================================================================

// Paramètres globaux (à récupérer depuis données)
const PARAMETRES = {
    FraisGeneraux: 0.20,
    MargeDevis: 0.80,
    TauxConditionnement: 0.009,
    ConditionnementMin: 14.81,
    TauxTransport: 0.063,
    FraisFixe: 50,
    PrixCliche: 45.00
};

// Sites
const SITES_DATA = {
    'AEAT': {
        code: 'AEAT',
        laizeMaxiDefaut: 330,
        devise: 'EUR',
        coeffChange: 1.0
    }
};

// Machines
const MACHINES_DATA = {
    'AEAT_HP6000': {
        site: 'AEAT',
        type: 'HP6000',
        laizeMaxi: 330,
        tauxHoraire: 150,
        vitesseBase: 150
    },
    'AEAT_Finition': {
        site: 'AEAT',
        type: 'Finition',
        laizeMaxi: 330,
        tauxHoraire: 80,
        vitesseBase: 100
    }
};

// Profils couleur HP6000 (VRAIES DONNÉES)
const PROFILS_HP_DATA = {
    'Noir': {
        nom: 'Noir',
        nbCouleurHP: 1,
        reductionVitesse: 0.05,
        calageExtraH: 15,
        passeMl: 100,
        prixMilleClics: 5.00
    },
    'Noir+1 Silver': {
        nom: 'Noir+1 Silver',
        nbCouleurHP: 2,
        reductionVitesse: 0.10,
        calageExtraH: 20,
        passeMl: 150,
        prixMilleClics: 10.00
    },
    '2 Couleurs': {
        nom: '2 Couleurs',
        nbCouleurHP: 2,
        reductionVitesse: 0.12,
        calageExtraH: 25,
        passeMl: 200,
        prixMilleClics: 15.00
    },
    '3 Couleurs': {
        nom: '3 Couleurs',
        nbCouleurHP: 3,
        reductionVitesse: 0.15,
        calageExtraH: 30,
        passeMl: 250,
        prixMilleClics: 20.00
    }
};

// Supports (Frontal/Adhésif/Backing) - PrixM2
const SUPPORTS_DATA = {
    'Coton blanc/Permanent PLUS/Glassine': 1.80,
    'Coton blanc/Lavable/Glassine': 1.70,
    'PP 50 Blanc/Permanent PLUS/Glassine': 0.44,
    'PP 60 Blanc/Permanent PLUS/Glassine': 0.50,
    'Velin Blanc/Permanent PLUS/Glassine': 1.20,
    'Vergé blanc/Permanent PLUS/Glassine': 1.30
};

// Dorures (prixHorsFG)
const DORURES_DATA = {
    'Or': 3.50,
    'Argent': 3.50,
    'Copper': 3.75
};

// Opérations de finition (avec gâche et calage)
const OPERATIONS_DATA = {
    'Dorure': {
        nom: 'Dorure',
        gache: 0.27,
        calageExtra: 45,
        vitesseMax: 80,
        passe: 50
    },
    'Dorure galbee': {
        nom: 'Dorure galbee',
        gache: 0.27,
        calageExtra: 60,
        vitesseMax: 60,
        passe: 60
    },
    'Galbe a sec': {
        nom: 'Galbe a sec',
        gache: 0.15,
        calageExtra: 40,
        vitesseMax: 90,
        passe: 40
    },
    'Gaufrage': {
        nom: 'Gaufrage',
        gache: 0.20,
        calageExtra: 50,
        vitesseMax: 70,
        passe: 50
    },
    'Foulage': {
        nom: 'Foulage',
        gache: 0.18,
        calageExtra: 35,
        vitesseMax: 85,
        passe: 40
    },
    'Pelliculage': {
        nom: 'Pelliculage',
        gache: 0.10,
        calageExtra: 30,
        vitesseMax: 100,
        passe: 30
    }
};

// Profils couleur finition (simplifié)
const PROFILS_FINITION_DATA = {
    'Standard': {
        nbCouleurFlexo: 1,
        nbCouleurSeri: 0,
        nbVernisFlexo: 0,
        nbVernisSeri: 0
    },
    'Premium': {
        nbCouleurFlexo: 2,
        nbCouleurSeri: 1,
        nbVernisFlexo: 1,
        nbVernisSeri: 0
    }
};

// Consommables (encres, vernis)
const CONSOMMABLES_DATA = {
    'EncreFlexo': { grammeFixe: 2.0, grammeM2: 0.8, prixKg: 25.0, tauxEncrage: 1.0 },
    'EncreSeri': { grammeFixe: 3.0, grammeM2: 1.2, prixKg: 28.0, tauxEncrage: 1.0 },
    'VernisFlexo': { grammeFixe: 1.5, grammeM2: 0.5, prixKg: 18.0, tauxEncrage: 0.8 },
    'VernisSeri': { grammeFixe: 2.0, grammeM2: 0.7, prixKg: 20.0, tauxEncrage: 0.8 }
};

// Développé par pose (ml/pose selon format d'avance)
const DEVELOPPE_DATA = {
    'AEAT_HP6000_150': 2500,  // 2500 ml pour 150mm d'avance
    'AEAT_HP6000_200': 3000,
    'AEAT_HP6000_300': 4000,
    'AEAT_Finition_150': 2000,
    'AEAT_Finition_200': 2500,
    'AEAT_Finition_300': 3000
};

// Outillages
const OUTILLAGES_DATA = {
    'Decoupe': 200,
    'Dorure': 150,
    'Galbe': 100,
    'Gaufrage': 120,
    'Foulage': 80
};

// ============================================================================
// 2. FONCTIONS UTILITAIRES
// ============================================================================

/**
 * Calcul du nombre de poses selon la largeur
 * Formule: floor((LaizeMaxi - 2*Rives) / (FormatLaize + Interposes))
 */
function calculerNbPoses(formatLaize, interPoses, rives, laizeMaxi, dorure) {
    const espacementMin = dorure ? 25 : interPoses;
    const laizeDisponible = laizeMaxi - 2 * rives;
    const nbPoses = Math.floor(laizeDisponible / (formatLaize + espacementMin));
    return Math.max(1, nbPoses);
}

/**
 * Convertit mètres linéaires en m²
 * m² = (ml × laizeUtile) / 1000
 */
function mlVersM2(ml, laizeUtile) {
    return (ml * laizeUtile) / 1000.0;
}

/**
 * Récupère les opérations actives selon les options
 */
function operationsActives(input) {
    const ops = [];
    if (input.dorures && input.dorures.length > 0) ops.push('Dorure');
    if (input.dorureGalbee) ops.push('Dorure galbee');
    if (input.galbeASec) ops.push('Galbe a sec');
    if (input.gaufrage) ops.push('Gaufrage');
    if (input.foulage) ops.push('Foulage');
    if (input.pelliculage) ops.push('Pelliculage');
    return ops.map(op => OPERATIONS_DATA[op]).filter(o => o);
}

/**
 * Calcul des coûts d'encres et vernis
 * Calage = nbCouleur × gFixe/1000 × prix/kg
 * Roulage = nbCouleur × gM2/1000 × prix/kg × taux × m2
 */
function coutEncres(profilFinition, m2) {
    if (!profilFinition) return 0;
    
    function calage(nb, consommable) {
        const c = CONSOMMABLES_DATA[consommable];
        return nb * (c.grammeFixe / 1000.0) * c.prixKg;
    }
    
    function roulage(nb, consommable) {
        const c = CONSOMMABLES_DATA[consommable];
        return nb * (c.grammeM2 / 1000.0) * c.prixKg * c.tauxEncrage * m2;
    }
    
    const pf = profilFinition;
    const ef = CONSOMMABLES_DATA['EncreFlexo'];
    const es = CONSOMMABLES_DATA['EncreSeri'];
    const vf = CONSOMMABLES_DATA['VernisFlexo'];
    const vs = CONSOMMABLES_DATA['VernisSeri'];
    
    return calage(pf.nbCouleurFlexo, 'EncreFlexo') + calage(pf.nbCouleurSeri, 'EncreSeri')
         + calage(pf.nbVernisFlexo, 'VernisFlexo') + calage(pf.nbVernisSeri, 'VernisSeri')
         + roulage(pf.nbCouleurFlexo, 'EncreFlexo') + roulage(pf.nbCouleurSeri, 'EncreSeri')
         + roulage(pf.nbVernisFlexo, 'VernisFlexo') + roulage(pf.nbVernisSeri, 'VernisSeri');
}

/**
 * Calcul des coûts de clics
 * Clics = (ml / développéRepeat) × nbCouleur
 * Coût = frappes/1000 × prix/1000 clics
 */
function coutClics(input, profilHP, ml) {
    const devR = DEVELOPPE_DATA[`${input.site}_HP6000_${input.formatAvance}`] || 2500;
    if (devR <= 0) return 0;
    return (ml / (devR / 1000.0)) * profilHP.nbCouleurHP / 1000.0 * profilHP.prixMilleClics;
}

/**
 * Calcul des coûts de clichés
 * Terme roulage + prix cliché × nbRef × (1+FG) × nbCouleurs
 */
function coutCliches(input, profilFinition, mlRoulFin, fg) {
    if (!profilFinition) return 0;
    
    const nbColors = profilFinition.nbCouleurFlexo + profilFinition.nbCouleurSeri 
                   + profilFinition.nbVernisFlexo + profilFinition.nbVernisSeri;
    
    const devR = DEVELOPPE_DATA[`${input.site}_Finition_${input.formatAvance}`] || 2000;
    const termeRoulage = devR > 0 ? (mlRoulFin / (devR / 1000.0)) * nbColors / 1000.0 : 0;
    const termePrix = PARAMETRES.PrixCliche * input.nbReferences * fg * nbColors;
    
    return termeRoulage + termePrix;
}

/**
 * Calcul des outillages
 */
function calculerOutillages(input) {
    let total = OUTILLAGES_DATA['Decoupe'] || 200;
    if (input.dorures && input.dorures.length > 0) {
        total += OUTILLAGES_DATA['Dorure'] || 150;
    }
    if (input.dorureGalbee || input.galbeASec) {
        total += OUTILLAGES_DATA['Galbe'] || 100;
    }
    if (input.gaufrage) {
        total += OUTILLAGES_DATA['Gaufrage'] || 120;
    }
    if (input.foulage) {
        total += OUTILLAGES_DATA['Foulage'] || 80;
    }
    return total;
}

// ============================================================================
// 3. CALCUL PRINCIPAL
// ============================================================================

/**
 * Calcul complet du devis
 * PORT EXACT du C# DevisCalculator.Calculer()
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
        dorures = [],
        dorureGalbee = false,
        galbeASec = false,
        gaufrage = false,
        foulage = false,
        pelliculage = false,
        profilFinition = 'Standard',
        margeManuelle = 0
    } = input;

    // Données de référence
    const P = PARAMETRES;
    const siteData = SITES_DATA[site] || SITES_DATA['AEAT'];
    const hpData = MACHINES_DATA[`${site}_HP6000`] || MACHINES_DATA['AEAT_HP6000'];
    const finData = MACHINES_DATA[`${site}_Finition`] || MACHINES_DATA['AEAT_Finition'];
    const profilHp = PROFILS_HP_DATA[couleurNum] || PROFILS_HP_DATA['Noir'];
    const supportKey = `${frontal}/${adhesif}/${backing}`;
    const prixSupport = SUPPORTS_DATA[supportKey] || 1.50;
    const pf = PROFILS_FINITION_DATA[profilFinition] || PROFILS_FINITION_DATA['Standard'];
    
    const fg = 1 + P.FraisGeneraux;  // Frais généraux

    // ────── 1. IMPOSITION ──────────────────────────────────────────────────
    const laizeUtile = Math.min(siteData.laizeMaxiDefaut, hpData.laizeMaxi);
    const dorure = (dorures && dorures.length > 0) || dorureGalbee;
    const nbPoses = calculerNbPoses(formatLaize, 4, 18, laizeUtile, dorure);
    const mlUtile = (quantite * (formatAvance / 1000.0)) / nbPoses;

    // ────── 2. MATIÈRE (ML) ────────────────────────────────────────────────
    const ops = operationsActives(input);
    const mlCalHP = profilHp.passeMl * Math.max(1, nbReferences);
    const mlCalFin = ops.reduce((sum, op) => sum + op.passe, 0);
    const mlGache = ops.reduce((sum, op) => sum + op.gache * mlUtile, 0);
    const mlTotal = mlCalHP + mlCalFin + mlUtile + mlGache;
    const m2Total = mlVersM2(mlTotal, laizeUtile);
    const mlRoulFin = mlUtile + mlGache;

    // ────── 3. COÛTS ───────────────────────────────────────────────────────
    // Support
    const cSupport = prixSupport * m2Total * fg;

    // Dorure
    let cDorure = 0;
    if (dorures && dorures.length > 0) {
        for (const dorure of dorures) {
            const prixDorure = DORURES_DATA[dorure] || 3.50;
            cDorure += prixDorure * mlTotal * 0.27;  // gâche ~27%
        }
        cDorure *= fg;
    }

    // Pelliculage
    let cPellic = 0;
    if (pelliculage) {
        cPellic = 0.976 * m2Total * fg;
    }

    // HP6000
    const vitHP = hpData.vitesseBase * (1 - profilHp.reductionVitesse);
    const cHP = profilHp.calageExtraH * hpData.tauxHoraire 
              + (mlTotal / Math.max(1, vitHP)) * hpData.tauxHoraire;

    // Finition
    let cFin = 0;
    if (ops.length > 0) {
        const vitFinMin = Math.min(...ops.map(o => o.vitesseMax));
        const calageFinTotal = ops.reduce((sum, op) => sum + op.calageExtra, 0);
        cFin = calageFinTotal * finData.tauxHoraire 
             + (mlRoulFin / Math.max(1, vitFinMin)) * finData.tauxHoraire;
    }

    // Encres
    const cEncres = coutEncres(pf, m2Total);

    // Clics
    const cClics = coutClics(input, profilHp, mlTotal);

    // Clichés
    const cCliches = coutCliches(input, pf, mlRoulFin, fg);

    // Conditionnement & Transport
    const emb = Math.max(P.TauxConditionnement * cSupport, P.ConditionnementMin);
    const transp = P.TauxTransport * cSupport;

    // COÛT TOTAL
    const cTotal = cSupport + cDorure + cPellic + cHP + cFin 
                 + cEncres + cClics + cCliches + emb + transp + P.FraisFixe;

    // ────── 4. PRIX ────────────────────────────────────────────────────────
    const pDevis = (cTotal / P.MargeDevis) * siteData.coeffChange;
    const pClient = pDevis * (1 + margeManuelle);

    // Marge brute = (Devis - MatièreBrute) / Devis
    const cMB = cSupport + cDorure + cPellic + cEncres + cClics;
    const marge = pDevis > 0 ? (pDevis - cMB) / pDevis : 0;

    const outillages = calculerOutillages(input);

    // ────── RÉSULTAT ────────────────────────────────────────────────────────
    return {
        quantite: quantite,
        prixDevis: Math.round(pDevis * 100) / 100,
        prixClient: Math.round(pClient * 100) / 100,
        margeBrute: Math.round(marge * 10000) / 100,  // %
        coutTotal: Math.round(cTotal * 100) / 100,
        outillages: Math.round(outillages * 100) / 100,
        mlTotal: Math.round(mlTotal * 10) / 10,
        m2Total: Math.round(m2Total * 1000) / 1000,
        nbPosesLaize: nbPoses,
        
        // Détails
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

// ============================================================================
// EXPORT
// ============================================================================

if (typeof module !== 'undefined' && module.exports) {
    module.exports = { calculerDevis, calculerNbPoses, mlVersM2 };
}
