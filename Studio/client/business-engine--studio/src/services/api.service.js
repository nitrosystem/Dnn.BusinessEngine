import { GlobalSettings } from "../angular/angular-configs/global.settings";
import Swal from 'sweetalert2'

export class ApiService {
    constructor($rootScope, $http, $q, $templateCache, $templateRequest, $timeout, Upload, notificationService) {
        this.$rootScope = $rootScope;
        this.$http = $http;
        this.$q = $q;
        this.$templateCache = $templateCache;
        this.$templateRequest = $templateRequest;
        this.$timeout = $timeout;
        this.uploadService = Upload;
        this.notifyService = notificationService;
    }

    get(controller, methodName, params, customHeaders) {
        return this.getApi('BusinessEngineStudio', controller, methodName, params, customHeaders);
    }

    getApi(module, controller, methodName, params, customHeaders) {
        const defer = this.$q.defer();

        const url = `${GlobalSettings.siteRoot}API/${module}/${controller}/${methodName}`;

        var headers = customHeaders ?? GlobalSettings.apiHeaders;
        headers = { ...headers, ... { Requestverificationtoken: $('[name="__RequestVerificationToken"]').val() } };

        this.$http({
            method: "GET",
            url: url,
            headers: headers,
            params: params,
        }).then((data) => {
            defer.resolve((data ?? {}).data);
        }, (error) => {
            if (error.status == 401) {
                let timerInterval;
                Swal.fire({
                    title: "ERROR 401 - You are logged out",
                    html: "Refresh Page in <b></b> milliseconds.",
                    timer: 10000,
                    timerProgressBar: true,
                    didOpen: () => {
                        Swal.showLoading();
                        const timer = Swal.getPopup().querySelector("b");
                        timerInterval = setInterval(() => {
                            timer.textContent = `${Swal.getTimerLeft()
                                }`;
                        }, 100);
                    },
                    willClose: () => {
                        location.reload();
                        clearInterval(timerInterval);
                    }
                });
            }

            defer.reject(error);

            this.notifyService.error(((error ?? {}).data || {}).Message);
            console.error(error);
        });

        return defer.promise;
    }

    post(controller, methodName, data, params, customHeaders) {
        return this.postApi('BusinessEngineStudio', controller, methodName, data, params, customHeaders);
    }

    postApi(module, controller, methodName, data, params, customHeaders) {
        const defer = this.$q.defer();

        const url = `${GlobalSettings.siteRoot}API/${module}/${controller}/${methodName}`;

        var headers = customHeaders ?? GlobalSettings.apiHeaders;
        headers = { ...headers, ... { Requestverificationtoken: $('[name="__RequestVerificationToken"]').val() } };

        this.$http({
            method: "POST",
            url: url,
            headers: headers,
            data: data,
            params: params,
        }).then((data) => {
            defer.resolve((data ?? {}).data);
        }, (error) => {
            if (error.status == 401) {
                let timerInterval;
                Swal.fire({
                    title: "ERROR 401 - You are logged out",
                    html: "Refresh Page in <b></b> milliseconds.",
                    timer: 10000,
                    timerProgressBar: true,
                    didOpen: () => {
                        Swal.showLoading();
                        const timer = Swal.getPopup().querySelector("b");
                        timerInterval = setInterval(() => {
                            timer.textContent = `${Swal.getTimerLeft()} `;
                        }, 100);
                    },
                    willClose: () => {
                        location.reload();
                        clearInterval(timerInterval);
                    }
                });
            }

            defer.reject(error);

            this.notifyService.error(((error ?? {}).data || {}).Message);
            console.error(error);
        });

        return defer.promise;
    }

    postWithMonitoring(controller, methodName, hubId, data, params, customHeaders) {
        var headers = customHeaders ?? GlobalSettings.apiHeaders;

        return this.postApi('BusinessEngineStudio', controller, methodName, data, params, headers);
    }

    uploadFile(controller, methodName, data, customHeaders) {
        const url = `${GlobalSettings.siteRoot}API/BusinessEngineStudio/${controller}/${methodName}`;

        return this.upload(url, data, customHeaders);
    }

    upload(apiUrl, data, customHeaders) {
        const defer = this.$q.defer();

        var headers = customHeaders ?? GlobalSettings.apiHeaders;
        headers = { ...headers, ... { Requestverificationtoken: $('[name="__RequestVerificationToken"]').val() } };

        this.uploadService.upload({
            url: apiUrl,
            headers: headers,
            data: data,
        }).then((data) => {
            defer.resolve(data.data);
        }, (error) => {
            defer.reject(error);

            this.notifyService.error(((error ?? {}).data || {}).Message);
            console.error(error);
        }, (evt) => {
            defer.notify(evt);
        });

        return defer.promise;
    }

    uploadFileByAngular(controller, methodName, file, customHeaders) {
        const defer = this.$q.defer();

        const url = `${GlobalSettings.siteRoot}API/${module}/${controller}/${methodName}`;

        var headers = customHeaders ?? GlobalSettings.apiHeaders;
        headers = { ...headers, ... { Requestverificationtoken: $('[name="__RequestVerificationToken"]').val() } };

        const formData = new FormData();
        formData.append("file", file);

        this.$http({
            url: url,
            method: 'POST',
            headers: headers,
            data: formData
        }).then((data) => {
            defer.resolve(data);
        }, (error) => {
            defer.reject(error);

            console.error(error);
        })

        return defer.promise;
    }

    getContent(url) {
        const defer = this.$q.defer();

        this.$http.get(url + "?ver=" + GlobalSettings.version).then(
            (content) => {
                defer.resolve(content.data);
            },
            (error) => {
                defer.reject(error);
            }
        );

        return defer.promise;
    }

    getContents(urls, useCache) {
        const $q = this.$q;
        const $templateRequest = this.$templateRequest;
        const version = GlobalSettings.version;

        if (!Array.isArray(urls) || urls.length === 0) {
            return $q.reject('No URLs provided');
        }

        const requests = urls.map((url) => {
            const fullUrl = url + '?ver=' + version;
            return $templateRequest(fullUrl, useCache);
        });

        return $q.all(requests);
    }

    async getAsync(controller, methodName, data) {
        const url = GlobalSettings.apiBaseUrl + controller + '/' + methodName;
        var headers = GlobalSettings.apiHeaders;

        const ajaxPromise = await new Promise((resolve, reject) => {
            this._$http({
                method: 'GET',
                url: url,
                headers: headers,
                params: data
            }).then((data) => {
                resolve(data.data);
            }, (error) => {
                reject(error);
            });
        });

        return ajaxPromise;
    }

    loadScript(FILE_URL, type = "text/javascript") {
        const scriptEle = document.createElement("script");
        scriptEle.type = type;
        scriptEle.src = FILE_URL;

        scriptEle.addEventListener("load", (ev) => {
            //resolve({ status: true });
        });

        document.body.appendChild(scriptEle);
    }
}