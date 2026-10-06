ComponentRegistry.register("UploadImage", function (controller, globalService) {
    this.init = (field) => {
        field.__ignoreReinit = true;

        const input = document.getElementById(`uploadImage_${field.Id}`);
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
                await this._validateImage(file, {
                    allowedTypes: allowedContentTypes,
                    maxSize: (field.Settings.MaxFileSize * 1024) || (5 * 1024 * 1024),
                    minWidth: field.Settings.MinWidth || 50,
                    maxWidth: field.Settings.MaxWidth || 2000,
                    minHeight: field.Settings.MinHeight || 50,
                    maxHeight: field.Settings.MaxHeight || 2000,
                });

                const previewUrl = this._createPreview(file);
                field.previewImage = previewUrl;
                field.error = null;

                if (field.FieldValueProperty) {
                    const baseUrl = window.bEngineServiceRoot + 'API/BE_ExtraExtensions/';
                    const apiUrl = baseUrl + 'Service/UploadImage';
                    const result = await this._uploadFile(file, {
                        url: apiUrl,
                        extraFields: {
                            Id: field.Id
                        },
                        onProgress: (percent) => {
                            setTimeout(() => {
                                const percentEl = document.getElementById(`uploadImagePercent_${field.Id}`);
                                if (percentEl) percentEl.style.width = percent;
                            });
                        },
                    });

                    if (result.IsSuccess) {
                        field.isUploaded = true;
                        field.previewImage = null;

                        controller.set(field.FieldValueProperty, result.FilePath, true);

                        URL.revokeObjectURL(previewUrl);
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

        field.previewImage = null;
        field.error = null;

        const input = document.getElementById(`uploadImage_${field.Id}`);
        input.value = '';
    };

    this._createPreview = (file) => {
        return URL.createObjectURL(file);
    }

    this._uploadFile = (file, { url, fieldName = 'file', extraFields = {}, onProgress, signal } = {}) => {
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
            signal?.addEventListener('abort', () => xhr.abort());
            xhr.send(fd);
        });
    }

    this._validateImage = async (file, {
        allowedTypes = ['image*'],
        maxSize = 5 * 1024 * 1024, // 5MB
        minSize = 0,
        minWidth = 0,
        maxWidth = Infinity,
        minHeight = 0,
        maxHeight = Infinity,
    } = {}) => {
        const isAllowed = allowedTypes.some((allowed) => {
            if (allowed.endsWith('/*')) {
                return file.type.startsWith(allowed.slice(0, -1)); // "image/"
            }
            return file.type === allowed;
        });
        if (!isAllowed) {
        throw new Error(`File type is not allowed. Allowed types: ${allowedTypes.join(', ')}`);
        }

        if (file.size > maxSize) {
            throw new Error(`File size must not exceed ${this._formatBytes(maxSize)} (current: ${globalService.formatFileSize(file.size)})`);
        }

        if (file.size < minSize) {
            throw new Error(`File size must not be less than ${this._formatBytes(minSize)}`);
        }

        const { width, height } = await this._getImageDimensions(file);
        if (width < minWidth) {
            throw new Error(`Image width must not be less than ${minWidth}px (current: ${width}px)`);
        }

        if (width > maxWidth) {
            throw new Error(`Image width must not exceed ${maxWidth}px (current: ${width}px)`);
        }

        if (height < minHeight) {
            throw new Error(`Image height must not be less than ${minHeight}px (current: ${height}px)`);
        }

        if (height > maxHeight) {
            throw new Error(`Image height must not exceed ${maxHeight}px (current: ${height}px)`);
        }

        return { width, height, size: file.size, type: file.type };
    }

    this._getImageDimensions = (file) => {
        return new Promise((resolve, reject) => {
            const url = URL.createObjectURL(file);
            const img = new Image();
            img.onload = () => {
                URL.revokeObjectURL(url);
                resolve({ width: img.naturalWidth, height: img.naturalHeight });
            };
            img.onerror = () => {
                URL.revokeObjectURL(url);
                reject(new Error('Image is not valid!'));
            };
            img.src = url;
        });
    }

    this._formatBytes = (bytes) => {
        if (bytes < 1024) return `${bytes} B`;
        if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
        return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
    }
});
