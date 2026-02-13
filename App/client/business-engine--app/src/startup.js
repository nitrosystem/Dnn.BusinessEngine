import { BusinessEngineApp } from "./app";

import { GlobalService } from "./services/global.service";
import { ExpressionService } from "./services/expression.service";
import { ApiService } from "./services/api.service";
import {  DSLEngineService } from "./services/dsl-engine.service.js";

import { ModuleController } from "./controllers/module.controllers";

import { BindModel } from "./directives/data/b-model.directive";
import { BindText } from "./directives/data/b-text.directive";
import { BindHtml } from "./directives/data/b-html.directive.js";
import { BindElement } from "./directives/data/b-element.directive.js";
import { BindDate } from "./directives/data/b-date.directive.js";
import { BindImage } from "./directives/data/b-image.directive.js";
import { BindLink } from "./directives/data/b-link.directive.js";
import { BindClass } from "./directives/conditional/b-class.directive.js";
import { BindIf } from "./directives/conditional/b-if.directive.js";
import { BindShow } from "./directives/conditional/b-show.directive.js";
import { BindHide } from "./directives/conditional/b-hide.directive.js";
import { BindDisabled } from "./directives/conditional/b-disables.directive.js";
import { BindReadonly } from "./directives/conditional/b-readonly.directive.js";
import { BindList } from "./directives/list/b-list.directive.js";
import { BindFor } from "./directives/list/b-for.directive";
import { BindClick } from "./directives/events/b-click.directvife";
import { BindChange } from "./directives/events/b-change.directvife.js";

import { OrderByFilter } from "./filters/order-by.filter.js";

import { Slugify } from "./functions/url-functions.js";

const app = new BusinessEngineApp();
app.service("globalService", GlobalService);
app.service("apiService", ApiService);
app.service("expressionService", ExpressionService);
app.service("dslEngineService", DSLEngineService);

app.controller("moduleController", ModuleController);

app.directive("b-model", BindModel);
app.directive("b-text", BindText);
app.directive("b-html", BindHtml);
app.directive("b-element", BindElement);
app.directive("b-date", BindDate);
app.directive("b-image", BindImage);
app.directive("b-link", BindLink);
app.directive("b-class", BindClass);
app.directive("b-if", BindIf);
app.directive("b-show", BindShow);
app.directive("b-hide", BindHide);
app.directive("b-disabled", BindDisabled);
app.directive("b-readonly", BindReadonly);
app.directive("b-list", BindList);
app.directive("b-for", BindFor);
app.directive("b-click", BindClick);
app.directive("b-change", BindChange);

app.filter("orderBy", OrderByFilter);

app.function("slugify", Slugify);

export default app; 
