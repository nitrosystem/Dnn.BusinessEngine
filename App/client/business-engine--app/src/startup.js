import { App } from "./app";

import { GlobalService } from "./services/global.service";
import { ApiService } from "./services/api.service";
import { ActionService } from "./services/action.service";
import { ExpressionService } from "./services/expression.service";

import { ModuleController } from "./controllers/module.controllers";

import { BindModel } from "./directives/data/b-model.directive";
import { BindText } from "./directives/data/b-text.directive";
import { BindClass } from "./directives/conditional/b-class.directive.js";
import { BindIf } from "./directives/conditional/b-if.directive.js";
import { BindShow } from "./directives/conditional/b-show.directive.js";
import { BindHide } from "./directives/conditional/b-hide.directive.js";
import { BindList } from "./directives/list/b-list.directive.js";
import { BindFor } from "./directives/list/b-for.directive";
import { BindClick } from "./directives/events/b-click.directvife";

const app = new App();
app.service("apiService", ApiService);
app.service("expressionService", ExpressionService);
app.service("actionService", ActionService);
app.service("globalService", GlobalService);

app.controller("moduleController", ModuleController);

app.directive("b-model", BindModel);
app.directive("b-text", BindText);
app.directive("b-class", BindClass);
app.directive("b-show", BindIf);
app.directive("b-show", BindShow);
app.directive("b-hide", BindHide);
app.directive("b-list", BindList);
app.directive("b-for", BindFor);
app.directive("b-click", BindClick);

document.addEventListener("DOMContentLoaded", async () => {
    const appElement = document.querySelector('[b-app]');
    await app.bootstrap(appElement);
});
