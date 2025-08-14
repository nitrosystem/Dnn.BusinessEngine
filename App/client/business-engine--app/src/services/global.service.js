export class GlobalService {
    constructor($window, $timeout, $filter, $q) {
        this.$window = $window;
        this.$timeout = $timeout;
        this.$filter = $filter;
        this.$q = $q;
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

    bindParams(params, source) {
        _.remove(params, (p) => {
            return (!p.IsCustomParam &&
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
                    IsCustomParam: sp.IsCustomParam,
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

    capitalizeFirstLetter(string) {
        return string.charAt(0).toUpperCase() + string.slice(1);
    }

    decodeProtectedData(base64String) {
        if (!base64String) return null;

        // Base64 decode
        const binaryString = atob(base64String);
        const binaryData = new Uint8Array(binaryString.length);

        for (let i = 0; i < binaryString.length; i++) {
            binaryData[i] = binaryString.charCodeAt(i);
        }

        // GZip decompress
        const decompressed = pako.inflate(binaryData, { to: 'string' });

        // Parse JSON
        return JSON.parse(decompressed);
    }
}