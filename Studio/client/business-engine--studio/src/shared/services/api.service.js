import Swal from 'sweetalert2'

export class ApiService {
    constructor($http, $q, $templateRequest, baseOptions) {
        this.$http = $http;
        this.$q = $q;
        this.$templateRequest = $templateRequest;
        this.baseOptions = baseOptions;
    }

    get(controller, methodName, params, customHeaders) {
        const defer = this.$q.defer();
        const url = `${this.baseOptions.siteRoot}API/BusinessEngineStudio/${controller}/${methodName}`;

        this.$http({
            method: 'GET',
            url: url,
            headers: customHeaders,
            params: params,
        }).then((data) => {
            defer.resolve((data ?? {}).data);
        }, (error) => {
            if (error.status === 401) {
                let timerInterval;
                Swal.fire({
                    title: 'ERROR 401 - You are logged out',
                    html: 'Refresh Page in <b></b> milliseconds.',
                    timer: 10000,
                    timerProgressBar: true,
                    didOpen: () => {
                        Swal.showLoading();
                        const timer = Swal.getPopup().querySelector('b');
                        timerInterval = setInterval(() => {
                            timer.textContent = `${Swal.getTimerLeft()}`;
                        }, 100);
                    },
                    willClose: () => {
                        location.reload();
                        clearInterval(timerInterval);
                    }
                });
            }

            defer.reject(error);
            console.error(error);
        });

        return defer.promise;
    }

    post(controller, methodName, data, params, customHeaders) {
        const defer = this.$q.defer();
        const url = `${this.baseOptions.siteRoot}API/BusinessEngineStudio/${controller}/${methodName}`;
        const headers = { ...customHeaders, ...{ Requestverificationtoken: $('[name="__RequestVerificationToken"]').val() } };

        this.$http({
            method: 'POST',
            url: url,
            headers: headers,
            data: data,
            params: params,
        }).then((data) => {
            defer.resolve((data ?? {}).data);
        }, (error) => {
            if (error.status === 401) {
                let timerInterval;
                Swal.fire({
                    title: 'ERROR 401 - You are logged out',
                    html: 'Refresh Page in <b></b> milliseconds.',
                    timer: 10000,
                    timerProgressBar: true,
                    didOpen: () => {
                        Swal.showLoading();
                        const timer = Swal.getPopup().querySelector('b');
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
            console.error(error);
        });

        return defer.promise;
    }

    upload(controller, methodName, file, customHeaders, onProgress) {
        const defer = this.$q.defer();
        const url = `${this.baseOptions.siteRoot}API/BusinessEngineStudio/${controller}/${methodName}`;
        const headers = { ...customHeaders, ...{ Requestverificationtoken: $('[name="__RequestVerificationToken"]').val() } };
        const formData = new FormData();
        formData.append('file', file);

        this.$http.post(url, formData, {
            transformRequest: angular.identity,
            headers: headers,
            uploadEventHandlers: {
                progress: function (e) {
                    if (e.lengthComputable && onProgress) {
                        var percent = Math.round((e.loaded / e.total) * 100);
                        onProgress(percent);
                    }
                }
            }
        }).then((data) => {
            defer.resolve((data ?? {}).data);
        }, (error) => {
            if (error.status === 401) {
                let timerInterval;
                Swal.fire({
                    title: 'ERROR 401 - You are logged out',
                    html: 'Refresh Page in <b></b> milliseconds.',
                    timer: 10000,
                    timerProgressBar: true,
                    didOpen: () => {
                        Swal.showLoading();
                        const timer = Swal.getPopup().querySelector('b');
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
            console.error(error);
        });

        return defer.promise;
    }

    getContent(url) {
        const defer = this.$q.defer();

        this.$http.get(url + '?ver=' + this.baseOptions.version).then(
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
        const version = this.baseOptions.version;

        if (!Array.isArray(urls) || urls.length === 0) {
            return $q.reject('No URLs provided');
        }

        const requests = urls.map((url) => {
            const fullUrl = url + '?ver=' + version;
            return $templateRequest(fullUrl, useCache);
        });

        return $q.all(requests);
    }
}