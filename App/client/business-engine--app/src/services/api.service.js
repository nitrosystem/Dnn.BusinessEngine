export class ApiService {
    constructor() {
    }

    async getApi(module = 'BusinessEngineApp', controller, methodName, params, headers = {}) {
        const queryString = Object.entries(params ?? {})
            .map(([key, value]) => `${key}=${value}`)
            .join('&');

        const baseUrl = `${window.bEngineServiceRoot}API/`;
        const apiUrl = baseUrl + module + '/' + controller + '/' + methodName + (queryString ? `?${queryString}` : '');
        const response = await fetch(apiUrl, {
            method: 'GET',
            headers: headers
        });

        if (!response.ok) {
            throw new Error(`HTTP error! Status: ${response.status}`);
        }

        return await response.json();
    }

    async postApi(module = 'BusinessEngineApp', controller, methodName, data, headers = {}) {
        headers = {
            ...headers,
            ...{
                Requestverificationtoken: document.querySelector('[name="__RequestVerificationToken"]')?.value,
                'Content-Type': 'application/json'
            }
        };

        const baseUrl = `${window.bEngineServiceRoot}API/`;
        const apiUrl = baseUrl + module + '/' + controller + '/' + methodName;
        const response = await fetch(apiUrl, {
            method: 'POST',
            headers: headers,
            body: JSON.stringify(data ?? {})
        });

        if (!response.ok) {
            let responseBody;
            try {
                responseBody = await response.text(); //Take the text first
                //If it's JSON, we'll convert it.
                try {
                    if (typeof responseBody === 'string') responseBody = JSON.parse(responseBody);
                } catch { }
            } catch (error) {
                responseBody = null;
            }

            const serverMessage =
                responseBody && responseBody.Message
                    ? responseBody.Message
                    : typeof responseBody === 'string'
                        ? responseBody
                        : `HTTP error! Status: ${response.status}`;

            throw new Error(serverMessage);
        }

        return await response.json();
    }
}