import $ from "jquery";

export class GlobalSettings {
    static scenarioName = window.bEngineGlobalSettings.scenarioName;
    static portalID = window.bEngineGlobalSettings.portalID;
    static portalAliasID = window.bEngineGlobalSettings.portalAliasID;
    static dnnModuleId = window.bEngineGlobalSettings.dnnModuleId;
    static moduleId = window.bEngineGlobalSettings.moduleId;
    static moduleType = window.bEngineGlobalSettings.moduleType;
    static scenarioId = window.bEngineGlobalSettings.scenarioId;
    static siteRoot = window.bEngineGlobalSettings.siteRoot;
    static apiBaseUrl = window.bEngineGlobalSettings.apiBaseUrl;
    static modulePath = window.bEngineGlobalSettings.modulePath;
    static userId = window.bEngineGlobalSettings.userId;
    static version = window.bEngineGlobalSettings.version;
    static debugMode = window.bEngineGlobalSettings.debugMode;

    static apiHeaders = {
        ScenarioId: this.scenarioId,
        Requestverificationtoken: $('[name="__RequestVerificationToken"]').val()
    };
}