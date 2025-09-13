export class GlobalSettings {
    static scenarioId = window.bEngineGlobalSettings.scenarioId;
    static scenarioName = window.bEngineGlobalSettings.scenarioName;
    static siteRoot = window.bEngineGlobalSettings.siteRoot || '/';

    static apiHeaders = {
        ScenarioId: this.scenarioId
    };
}