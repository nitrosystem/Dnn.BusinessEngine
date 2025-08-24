export class ApiService {
    constructor() {
    }

    async getAsync(controller, methodName, params) {
        const queryString = Object.entries(params ?? {})
            .map(([key, value]) => `${key}=${value}`)
            .join('&');

        const sf = $.ServicesFramework();
        const apiUrl = sf.getServiceRoot(`BusinessEngine/${controller}/${methodName}`) +
            (queryString ? `?${queryString}` : '');

        const response = await fetch(apiUrl, {
            method: 'GET',
            headers: {
                Requestverificationtoken: $('[name="__RequestVerificationToken"]').val(),
                'Content-Type': 'application/json'
            }
        });

        if (!response.ok) {
            throw new Error(`HTTP error! Status: ${response.status}`);
        }

        return await response.json();
    }

    async postAsync(controller, methodName, data) {
        const sf = $.ServicesFramework();
        const apiUrl = sf.getServiceRoot(`BusinessEngine/${controller}/${methodName}`);
        const response = await fetch(apiUrl, {
            method: 'POST',
            headers: {
                Requestverificationtoken: $('[name="__RequestVerificationToken"]').val(),
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(data ?? {})
        });

        if (!response.ok) {
            throw new Error(`HTTP error! Status: ${response.status}`);
        }

        return await response.json();
    }

    loadScript(FILE_URL, async = true, type = "text/javascript") {
        return new Promise((resolve, reject) => {
            try {
                const scriptEle = document.createElement("script");
                scriptEle.type = type;
                scriptEle.async = async;
                scriptEle.src = FILE_URL;

                scriptEle.addEventListener("load", (ev) => {
                    resolve({ status: true });
                });

                scriptEle.addEventListener("error", (ev) => {
                    reject({
                        status: false,
                        message: `Failed to load the script ${FILE_URL}`
                    });
                });

                document.body.appendChild(scriptEle);
            } catch (error) {
                reject(error);
            }
        });
    }
}