export class DevisCalculator {

	private _container: HTMLDivElement | null = null;
	private _context: any;
	private _notifyOutputChanged: (() => void) | null = null;
	private _iframe: HTMLIFrameElement | null = null;
	private _githubUrl: string = "https://tds1590-png.github.io/devis-autajon/devis-final.html";

	/**
	 * Empty constructor.
	 */
	constructor()
	{

	}

	/**
	 * Used to initialize the control instance. Controls can kick off remote server calls and other initialization actions here.
	 * Data-set values are not initialized here, use updateView.
	 * @param context The entire property bag available to control via Context Object; It contains values as set up by the creator of this control and all values the property pane provides.
	 * @param notifyOutputChanged A function to raise when the output value has to notify the framework that this object has been updated.
	 * @param state A piece of this control instance state that persists across several calls to getOutputs() and updateView().
	 */
	public init(context: any, notifyOutputChanged: () => void, state: any, container: HTMLDivElement): void
	{
		this._context = context;
		this._container = container;
		this._notifyOutputChanged = notifyOutputChanged;

		// Créer l'iframe pour charger le formulaire
		this._iframe = document.createElement("iframe");
		this._iframe.src = this._githubUrl;
		this._iframe.style.width = "100%";
		this._iframe.style.height = "1200px";
		this._iframe.style.border = "none";
		this._iframe.style.borderRadius = "8px";
		
		// Ajouter des attributs de sécurité
		this._iframe.setAttribute("sandbox", "allow-same-origin allow-scripts allow-forms allow-popups allow-top-navigation");
		
		if (this._container) {
			this._container.appendChild(this._iframe);
		}

		// Configuration du postMessage pour la communication bidirectionnelle
		window.addEventListener("message", (event) => {
			// Validation de l'origine (optionnel, pour la sécurité)
			if (event.origin !== "https://tds1590-png.github.io") return;

			// Recevoir les données du formulaire
			if (event.data.type === "DEVIS_CALCULATED") {
				const devisData = event.data.payload;
				// Optionnel: Envoyer à Dataverse via Web API
				this._saveDevisToDataverse(devisData);
			}
		});
	}

	/**
	 * Called when any value in the property bag has changed. This includes field values, data-sets, global values such as container height and width, offline status, control metadata values such as label, visible, etc.
	 * @param context The entire property bag available to control via Context Object; It contains values as set up by the creator of this control and all values the property pane provides.
	 */
	public updateView(context: any): void
	{
		// No update needed
	}

	/**
	 * It is called by the framework prior to a control receiving new data.
	 * @returns an object based on nomenclature defined in manifest, expecting object[s] for property marked as "bound" or "output"
	 */
	public getOutputs(): any
	{
		return {};
	}

	/**
	 * Called when the control is to be removed from the DOM tree. Controls should use this call to augment cleaning up after themselves. Ensure that removeEventListener is called for events registered during init or updateView.
	 */
	public destroy(): void
	{
		// Cleanup
		if (this._iframe) {
			this._iframe.remove();
		}
	}

	/**
	 * Sauvegarder les données de devis dans Dataverse via Web API
	 */
	private async _saveDevisToDataverse(devisData: any): Promise<void> {
		try {
			// Récupérer le Web API client
			const clientUrl = this._context?.organizationSettings?.organizationUrl;
			
			if (!clientUrl) {
				console.log("✅ Devis data received (offline mode):", devisData);
				return;
			}
			
			// Créer l'entité hp_devis
			const devisEntity = {
				"hp_name": devisData.clientName || "Devis sans nom",
				"hp_commercial": devisData.commercial,
				"hp_width": devisData.width,
				"hp_height": devisData.height,
				"hp_quantity": devisData.quantity,
				"hp_price": devisData.prixClient,
				"hp_margin": devisData.margeBrute,
				"hp_frontal": devisData.frontal,
				"hp_adhesif": devisData.adhesif,
				"hp_color": devisData.color
			};

			// POST vers Dataverse
			const response = await fetch(
				`${clientUrl}/api/data/v9.2/hp_devis`,
				{
					method: "POST",
					headers: {
						"Content-Type": "application/json; charset=utf-8",
						"OData-MaxVersion": "4.0",
						"OData-Version": "4.0"
					},
					body: JSON.stringify(devisEntity)
				}
			);

			if (response.ok) {
				console.log("✅ Devis sauvegardé dans Dataverse");
			} else {
				console.error("❌ Erreur sauvegarde Dataverse:", response.statusText);
			}
		} catch (error) {
			console.error("Erreur Web API:", error);
		}
	}
}
