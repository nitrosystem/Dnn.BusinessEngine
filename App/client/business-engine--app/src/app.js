//Css
import "./styles/global.css"

//Js
import angular from "angular";
import "ng-file-upload/dist/ng-file-upload-shim";
import "ng-file-upload/dist/ng-file-upload";

//Configs
import { config as appConfig } from "./configs/app.config";

//Providers
import { DeferredBroadcast, DeferredEmit, DeferredEvent } from "./providers/deferred-events.provider";

//Services
import { GlobalService } from "./services/global.service";
import { ApiService } from "./services/api.service";
import { ExpressionService } from "./services/expression.service";
import { ActionService } from "./services/action.service";

//Directives
import { bShow, bFor, bClick } from "./directives/angular-extended.directive";
import { FieldDirective } from "./directives/field.directive";
import {
    BindDate,
    BindHtml,
    BindImage,
    BindText,
    bindUrl,
    FocusDirective,
} from "./directives/custom-items.directive";

//Controllers
import { ModuleController } from "./controllers/module.controllers";

const app = angular
    .module("BusinessEngineClientApp", [require("angular-sanitize"), "angular.filter", "ngFileUpload"])
    .config(appConfig)
    .provider("$deferredEvent", DeferredEvent)
    .provider("$deferredEmit", DeferredEmit)
    .provider("$deferredBroadcast", DeferredBroadcast)
    .service("globalService", GlobalService)
    .service("apiService", ApiService)
    .service("expressionService", ExpressionService)
    .service("actionService", ActionService)
    .directive("bShow", bShow)
    .directive("bFor", bFor)
    .directive("bClick", bClick)
    .directive("bFocus", FocusDirective)
    .directive("bindText", BindText)
    .directive("bindHtml", BindHtml)
    .directive("bindDate", BindDate)
    .directive("bindImage", BindImage)
    .directive("bindUrl", bindUrl)
    .directive("field", FieldDirective)
    .controller("moduleController", ModuleController);

window.bEngineApp = app;