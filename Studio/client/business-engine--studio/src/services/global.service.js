import { GlobalSettings } from "../angular/angular-configs/global.settings";
import Swal from 'sweetalert2'

export class GlobalService {
    constructor($window, $timeout, $filter, $q) {
        this.$window = $window;
        this.$timeout = $timeout;
        this.$filter = $filter;
        this.$q = $q;
    }

    normalizeName(input) {
        // حذف همه حروف غیرانگلیسی و غیرعددی (از جمله فارسی)
        let cleaned = input.replace(/[^A-Za-z0-9 ]+/g, " "); // تبدیل کاراکترهای خاص و فارسی به فاصله
        // حذف همه اعداد از ابتدا و وسط رشته
        cleaned = cleaned.replace(/[0-9]+/g, " "); // تبدیل اعداد به فاصله
        // تقسیم بر اساس فاصله و حذف رشته‌های خالی
        const parts = cleaned.trim().split(/\s+/).filter(Boolean);
        // تبدیل به PascalCase
        return parts.map(w => w.charAt(0).toUpperCase() + w.slice(1)).join("");
    }

    getUrlParams(url, disableFriendlyParams) {
        var params = {};
        var parser = document.createElement("a");
        parser.href = url;
        var query = parser.search.substring(1);
        var vars = query.split("&");
        for (var i = 0; i < vars.length; i++) {
            if (vars[i]) {
                var pair = vars[i].split("=");
                params[pair[0].toLowerCase()] = decodeURIComponent(pair[1]);
            }
        }

        if (!disableFriendlyParams) {
            //parse friendly params
            var friendlyParams = location.pathname.toLowerCase().split("/");
            var index = friendlyParams.length - 2;
            while (index >= 0) {
                if (friendlyParams[index] && friendlyParams.length >= index + 2)
                    params[friendlyParams[index].toLowerCase()] = decodeURIComponent(
                        friendlyParams[index + 1]
                    );

                index = index - 2;
            }
        }

        return params;
    }

    getParameterByName(name, url) {
        const urlParams = new URLSearchParams(window.location.search);
        const _result = urlParams.get(name);
        if (_result) return _result;

        url = url ? url.toLowerCase() : document.URL.toLowerCase();

        const _url = decodeURIComponent(url);
        const _name = name.toLowerCase().replace(/[\[\]]/g, "\\$&");

        const regex = new RegExp("[?&]" + _name + "(=([^&#]*)|&|#|$)");
        const results = regex.exec(_url);

        if (!results) {
            //parse friendly params
            const friendlyParams = location.pathname
                .toLowerCase()
                .replace(/=/g, "/")
                .split("/");
            if (
                friendlyParams.indexOf(_name) >= 0 &&
                friendlyParams.length >= friendlyParams.indexOf(_name) + 2
            )
                return friendlyParams[friendlyParams.indexOf(_name) + 1];
            else return null;
        } else if (!results[2]) {
            return "";
        } else {
            return decodeURIComponent(results[2].replace(/\+/g, " "));
        }
    }

    replaceUrlParam(paramName, paramValue, url) {
        let _url = url ? url : document.URL;

        if (paramValue == null) paramValue = "";

        var pattern = new RegExp("\\b(" + paramName + "=).*?(&|#|$)");
        if (_url.search(pattern) >= 0) {
            return _url.replace(pattern, "$1" + paramValue + "$2");
        }

        _url = _url.replace(/[?#]$/, "");
        return (
            _url + (_url.indexOf("?") > 0 ? "&" : "?") + paramName + "=" + paramValue
        );
    }

    pushState(url, title, data) {
        const _title = title ? title : document.title;
        const _data = data ? data : "";
        ("");

        this.$window.history.pushState({ pageTitle: _title }, _data, url);
    }

    parseJsonItemsWithErrorHandling(items) {
        if (!items) return items;

        // If items is an array
        if (Array.isArray(items)) {
            return items.map(item => this.parseJsonItems(item));
        }

        // If items is an object
        if (typeof items === 'object') {
            for (const key in items) {
                if (!Object.hasOwn(items, key)) continue;
                items[key] = this.parseJsonItems(items[key]);
            }
            return items;
        }

        // If items is a string that looks like JSON
        if (typeof items === 'string') {
            try {
                const parsed = JSON.parse(items);
                // If parsing worked, parse deeper (in case nested JSON strings exist)
                return this.parseJsonItems(parsed);
            } catch {
                return items; // not valid JSON
            }
        }

        // Any other type (number, boolean, etc.) — return as-is
        return items;
    }

    parseJsonItems(items) {
        if (items == null) return items; // null or undefined

        // If it's an array, parse each element
        if (Array.isArray(items)) {
            return items.map(item => this.parseJsonItems(item));
        }

        // If it's an object, parse each property
        if (typeof items === 'object') {
            for (const key in items) {
                if (!Object.hasOwn(items, key)) continue;
                items[key] = this.parseJsonItems(items[key]);
            }
            return items;
        }

        // If it's a string, check if it looks like JSON
        if (typeof items === 'string') {
            const trimmed = items.trim();

            // Quick check to skip obvious non-JSON strings
            if (
                !(
                    (trimmed.startsWith('{') && trimmed.endsWith('}')) || // object
                    (trimmed.startsWith('[') && trimmed.endsWith(']')) || // array
                    /^"(?:\\.|[^"\\])*"$/.test(trimmed) || // quoted string
                    /^-?\d+(\.\d+)?([eE][+\-]?\d+)?$/.test(trimmed) || // number
                    /^(true|false|null)$/.test(trimmed) // boolean/null
                )
            ) {
                return items; // definitely not JSON
            }

            // Try parsing only if it passes the quick check
            try {
                const parsed = JSON.parse(items);
                return this.parseJsonItems(parsed); // Recursively parse
            } catch {
                return items; // invalid JSON
            }
        }

        // Primitive value (number, boolean, etc.)
        return items;
    }

    isJsonString(str) {
        if (!str) return false;
        try {
            JSON.parse(str);
        } catch (e) {
            return false;
        }
        return true;
    }

    getJsonString(str) {
        try {
            var result = JSON.parse(str);
            return result;
        } catch (e) {
            return str;
        }
    }

    deepClone(obj) {
        if (typeof structuredClone === "function") {
            return structuredClone(obj);
        } else {
            return JSON.parse(JSON.stringify(obj));
        }
    }

    deepEqual(obj1, obj2) {
        return JSON.stringify(this.deepClone(obj1)) === JSON.stringify(this.deepClone(obj2));
    }

    cleanAngularHashkeyObject(obj) {
        return JSON.parse(JSON.stringify(obj, function (key, value) {
            if (key === "$$hashKey") return undefined;
            return value;
        }));
    }

    capitalizeFirstLetter(str) {
        const capitalize = (str && str[0].toUpperCase() + str.slice(1)) || "";
        return capitalize;
    }

    addResourceToPage(resources, property) {
        const $defer = this.$q.defer();

        const appendResource = (resource, index) => {
            if (index < 0) $defer.resolve();
            else {
                var head = document.head;
                var link = document.createElement('link');
                link.rel = 'stylesheet';
                link.type = 'text/css';
                link.href = resource[property] + "?ver=" + GlobalSettings.version;
                link.media = 'all';
                head.appendChild(link);

                --index;

                appendResource(resources[index], index);
            }
        }

        appendResource(resources[resources.length - 1], resources.length - 1);

        return $defer.promise;
    }

    resetProp(parent, prop) {
        parent[prop] = true;
        this.$timeout(() => delete parent[prop]);
    }

    bindParams(params, source) {
        _.remove(params, (p) => {
            return (
                _.filter(source, (sp) => {
                    return sp.ParamName == p.ParamName;
                }).length == 0
            );
        });

        _.forEach(_.sortBy(source, ["ViewOrder"]), (sp) => {
            if (
                _.filter(params, (p) => {
                    return sp.ParamName == p.ParamName;
                }).length == 0
            )
                params.push({
                    ParamName: sp.ParamName,
                    ParamValue: sp.ParamValue,
                    ViewOrder: sp.ViewOrder
                });
        });
    }

    generateGuid() {
        return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, (c) => {
            var r = (Math.random() * 16) | 0,
                v = c == "x" ? r : (r & 0x3) | 0x8;
            return v.toString(16);
        });
    }

    getErrorHtmlFormat(error) {
        const html = `
        <table class="table table-bordered text-light error-table">
            <tbody>
                <tr>
                    <td class="w-50">Status Code</td>
                    <td>${error.status}</td>
                </tr>
                <tr>
                    <td>Status Text</td>
                    <td>${error.statusText}</td>
                </tr>
                <tr>
                    <td>Error Message</td>
                    <td>${error.data.Message}</td>
                </tr>
                <tr>
                    <td>HResult</td>
                    <td>${error.data.HResult}</td>
                </tr>
                <tr>
                    <td>Exception Method</td>
                    <td>${error.data.ExceptionMethod}</td>
                </tr>
                <tr>
                    <td>Source</td>
                    <td>${error.data.Source}</td>
                </tr>
                <tr>
                    <td>Stack Trace String</td>
                    <td>${error.StackTraceString}</td>
                </tr>
                <tr>
                    <td>Class Name</td>
                    <td>${error.data.ClassName}</td>
                </tr>
            </tbody>
        </table>`;
        return html;
    }

    checkSqlTypes(type) {
        const _type = type ? type.toString().toLowerCase() : "";
        if (!_type.match(/^([a-z]+)(\((\d+(,\d+)?|max)\))?$/)) return false;

        const match = /^([a-z]+)(\((\d+(,\d+)?|max)\))?$/.exec(_type);
        const sqlType = match[1];
        const length = match[3];
        const length2 = match[4];

        switch (sqlType) {
            case "int":
            case "bigit":
            case "smallint":
            case "tinyint":
            case "bit":
            case "float":
            case "real":
            case "money":
            case "smallmoney":
            case "date":
            case "datetime":
            case "smalldatetime":
            case "timestamp":
            case "geography":
            case "geometry":
            case "hierarchyid":
            case "image":
            case "text":
            case "ntext":
            case "sql_variant":
            case "uniqueidentifier":
            case "xml":
                if (!length) return true;
                break;
            case "binary":
            case "varbinary":
            case "char":
            case "datetime2":
            case "datetimeoffset":
            case "varchar":
            case "nvarchar":
            case "time":
                if (length && !length2) return true;
                break;
            case "decimal":
            case "numeric":
                if (length && length2) return true;
                break;
        }
    }

    getUrlQueryFromObject(paramsObject) {
        var params = [];
        _.forEach(paramsObject, (value, key) => {
            params.push(key + "=" + value);
        });

        return params.join("&");
    }

    getParamsFromSqlQuery(query) {
        if (!query) return [];

        var result = [];

        _.forEach(query.match(/@\w+/gim), (paramName) => {
            if (
                this.$filter("filter")(result, (i) => {
                    return i.ParamName == paramName;
                }).length == 0
            )
                result.push({ ParamName: paramName });
        });

        return result;
    }

    previewImage(file) {
        var defer = this.$q.defer();

        var reader = new FileReader();
        reader.onload = (e) => {
            defer.resolve(e.target.result);
        };
        reader.readAsDataURL(file);

        return defer.promise;
    }

    capitalizeFirstLetter(string) {
        return string.charAt(0).toUpperCase() + string.slice(1);
    }

    async sleep(time) {
        return new Promise((resolve, reject) => {
            this.$timeout(_ => resolve(), time)
        });
    }

    deleteConfirmAlert(options) {
        const $defer = this.$q.defer();

        options = {
            ...(options ?? {}),
            ... {
                title: "Are you sure?",
                text: "You won't be able to remove this!",
                icon: "warning",
                showCancelButton: true,
                confirmButtonColor: "#3085d6",
                cancelButtonColor: "#d33",
                confirmButtonText: "Yes, delete it!"
            }
        };

        Swal.fire(options).then((result) => {
            $defer.resolve(result.isConfirmed);
        });

        return $defer.promise;
    }

    bEval(exp) {
        try {
            return eval(exp);
        } catch (error) {
            console.error(error);

            return exp;
        }
    }

    addAdjectiveCssClass($target, className, delay) {
        delay = delay ?? 5000;

        $target.addClass(className);
        setTimeout(() => {
            $target.removeClass(className);
        }, delay);
    }

    compareTwoObject(sourceObject, destObject, ignoredFields = []) {
        const changes = _.reduce(sourceObject, (result, value, key) => {
            const oldValue = destObject[key];

            if (ignoredFields.includes(key)) return result;

            const isEmptyValue = (val) =>
                val === null || val === undefined || val === '' || (Array.isArray(val) && val.length === 0);

            if (_.isEqual(value, oldValue) || (isEmptyValue(value) && isEmptyValue(oldValue))) {
                return result;
            }

            result.push(key);
            return result;
        }, []);

        return changes;
    }

    // کمکی برای تبدیل رشته به camelCase
    toCamelCase(str) {
        return str
            .replace(/[-_ ]+([a-zA-Z])/g, (_, chr) => chr.toUpperCase()) // تبدیل بعد از -, _, space
            .replace(/^[A-Z]/, chr => chr.toLowerCase()); // اولین حرف کوچک
    }

    // متد بازگشتی برای تبدیل کلیدهای آبجکت
    keysToCamelCase(obj) {
        if (Array.isArray(obj)) {
            return obj.map(v => this.keysToCamelCase(v));
        } else if (obj !== null && typeof obj === 'object') {
            return Object.entries(obj).reduce((acc, [key, value]) => {
                const camelKey = this.toCamelCase(key);
                acc[camelKey] = this.keysToCamelCase(value);
                return acc;
            }, {});
        }
        return obj;
    }
}