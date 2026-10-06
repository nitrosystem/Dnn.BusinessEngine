const options = window.bEngineBaseOptions ?? {};

const BASE_URL = '/DesktopModules/BusinessEngine/Studio.aspx';
const SCENARIO_ID = options.scenarioId;
const SCENARIO_NAME = options.scenarioName;
const SITE_ROOT = options.siteRoot;
const VERSION = options.version;

export const BaseOptionsConstants = {
    baseUrl: BASE_URL,
    scenarioId: SCENARIO_ID,
    scenarioName: SCENARIO_NAME,
    siteRoot: SITE_ROOT,
    version: VERSION,
};
