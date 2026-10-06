ComponentRegistry.register("UploadFile", function (controller, globalService) {
    this.init = (field) => {
        field.__ignoreReinit = true;

        const input = document.getElementById(`uploadFile_${field.Id}`);
        if (!input) return;

        input.addEventListener('change', async (e) => {
            const file = e.target.files[0];
            if (!file) return;

            field.filename = file.name;
            field.filesize = globalService.formatFileSize(file.size);

            const allowedContentTypes = field.Settings.AllowedContentTypes
                ? field.Settings.AllowedContentTypes.split(',')
                : undefined;

            try {
                await this._validateFile(file, {
                    allowedTypes: allowedContentTypes,
                    maxSize: (field.Settings.MaxFileSize * 1024) || (5 * 1024 * 1024),
                    minWidth: field.Settings.MinWidth || 50,
                    maxWidth: field.Settings.MaxWidth || 2000,
                    minHeight: field.Settings.MinHeight || 50,
                    maxHeight: field.Settings.MaxHeight || 2000,
                });

                field.error = null;

                if (field.FieldValueProperty) {
                    const module = 'BE_ExtraExtensions';
                    let baseUrl = `/API/` + module + '/';

                    if (typeof $.ServicesFramework === 'function') {
                        const sf = $.ServicesFramework();
                        baseUrl = sf.getServiceRoot(module);
                    }

                    const apiUrl = baseUrl + 'Service/UploadFile';
                    const result = await this._uploadFile(file, {
                        url: apiUrl,
                        extraFields: {
                            Id: field.Id
                        },
                        onProgress: (percent) => {
                            const percentEl = document.getElementById(`uploadFilePercent_${field.Id}`);
                            percentEl.style.width = percent;
                        },
                    });

                    if (result.IsSuccess) {
                        field.isUploaded = true;

                        controller.set(field.FieldValueProperty, result.FilePath, true);
                    }
                    else {
                        field.error = result.Status;

                        controller.set(field.FieldValueProperty, null, true);
                    }
                }

            } catch (err) {
                field.error = err.message;
                this.onRemoveFile();
                console.error(err);
            }
        });

        if (field.FieldValueProperty) {
            const currentValue = controller.get(field.FieldValueProperty);
            if (currentValue && typeof currentValue === 'string') {
                field.filename = globalService.getFileNameFromUrl(currentValue);
            }
        }

        field.onRemoveFile = this.onRemoveFile;
    }

    this.onRemoveFile = (field) => {
        if (field.FieldValueProperty)
            controller.set(field.FieldValueProperty, null, true);

        field.error = null;

        const input = document.getElementById(`uploadFile_${field.Id}`);
        input.value = '';
    };

    this._uploadFile = (file, { url, fieldName = 'file', extraFields = {}, onProgress } = {}) => {
        return new Promise((resolve, reject) => {
            const xhr = new XMLHttpRequest();
            const fd = new FormData();
            fd.append(fieldName, file, file.name);
            for (const [k, v] of Object.entries(extraFields)) fd.append(k, v);
            xhr.open('POST', url);
            xhr.upload.onprogress = (e) => {
                if (e.lengthComputable && onProgress) {
                    onProgress(Math.round((e.loaded / e.total) * 100));
                }
            };
            xhr.onload = () => {
                if (xhr.status >= 200 && xhr.status < 300) {
                    try { resolve(JSON.parse(xhr.responseText)); }
                    catch { resolve(xhr.responseText); }
                } else {
                    reject(new Error(`Upload failed: ${xhr.status}`));
                }
            };
            xhr.onerror = () => reject(new Error('Network error'));
            xhr.onabort = () => reject(new Error('Aborted'));
            xhr.send(fd);
        });
    }

    this._validateFile = async (file, {
        allowedTypes = ['application/pdf', 'text/plain', 'application/zip', 'application/x-zip-compressed'],
        maxSize = 5 * 1024 * 1024, // 5MB
        minSize = 0
    } = {}) => {
        if (!allowedTypes.includes(file.type)) {
            throw new Error(`File type is not allowed. Allowed types: ${allowedTypes.join(', ')}`);
        }

        if (file.size > maxSize) {
            throw new Error(`File size must not exceed ${this._formatBytes(maxSize)} (current: ${globalService.formatFileSize(file.size)})`);
        }

        if (file.size < minSize) {
            throw new Error(`File size must not be less than ${this._formatBytes(minSize)}`);
        }

        return { size: file.size, type: file.type };
    }

    this._formatBytes = (bytes) => {
        if (bytes < 1024) return `${bytes} B`;
        if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
        return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
    }
});
