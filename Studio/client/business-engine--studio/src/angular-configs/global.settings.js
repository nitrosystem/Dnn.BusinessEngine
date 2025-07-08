import $ from "jquery";

export class GlobalSettings {
    static scenarioName = window.bEngineGlobalSettings.scenarioName;
    static portalId = window.bEngineGlobalSettings.portalId;
    static portalAliasId = window.bEngineGlobalSettings.portalAliasId;
    static dnnModuleId = window.bEngineGlobalSettings.dnnModuleId;
    static dnnTabId = window.bEngineGlobalSettings.dnnTabId;
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