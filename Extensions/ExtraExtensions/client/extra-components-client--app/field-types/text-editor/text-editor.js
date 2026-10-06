ComponentRegistry.register("TextEditor", function (controller, globalService) {
    this.init = (field, element) => {
        if (!element) return;

        const editorElement = element.querySelector(`#ckeditor_${field.Id}`);
        if (editorElement && globalService.isElementVisible(editorElement) && !editorElement.__inited) {
            const editor = CKEDITOR.replace(`ckeditor_${field.Id}`, {
                height: field.Settings.Height || 300,
                language: field.Settings.Language || 'en',
                toolbar: globalService.parseJson(field.Settings.Toolbar) ??
                    [
                        { name: 'document', items: ['Source', '-', 'Save', 'NewPage', 'Preview'] },
                        { name: 'clipboard', items: ['Cut', 'Copy', 'Paste', 'PasteText', 'PasteFromWord', '-', 'Undo', 'Redo'] },
                        { name: 'editing', items: ['Find', 'Replace', '-', 'SelectAll'] },
                        '/',
                        { name: 'basicstyles', items: ['Bold', 'Italic', 'Underline', 'Strike'] },
                        { name: 'paragraph', items: ['NumberedList', 'BulletedList', '-', 'JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock'] },
                        { name: 'links', items: ['Link', 'Unlink'] },
                        { name: 'insert', items: ['Image', 'Table', 'HorizontalRule'] },
                        { name: 'styles', items: ['Styles', 'Format', 'Font', 'FontSize'] },
                        { name: 'colors', items: ['TextColor', 'BGColor'] }
                    ]
            });

            editor.on('instanceReady', function () {
                if (field.FieldValueProperty) {
                    controller.watch(field.FieldValueProperty, (value) => {
                        if (!field._ignoreWatch) setEditorContent(value || '');
                    });

                    const currentValue = controller.get(field.FieldValueProperty);
                    setEditorContent(currentValue || '');
                }
            });

            editor.on('change', function () {
                field._ignoreWatch = true;

                const value = editor.getData();
                controller.set(field.FieldValueProperty, value);

                globalService.nextMicroTask(() => {
                    field._ignoreWatch = false;
                });
            });

            editorElement.__inited = true;

            function setEditorContent(content) {
                editor.setData(content);
            }

            // 3. API برای گرفتن محتوای ادیتور
            function getEditorContent() {
                return editor.getData();
            }

            // 4. API برای پاک کردن محتوا
            function clearEditor() {
                editor.setData('');
            }

            // 5. API برای اضافه کردن متن به انتهای محتوا
            function appendToEditor(content) {
                var currentData = editor.getData();
                editor.setData(currentData + content);
            }

            // // 7. گوش دادن به focus و blur
            // editor.on('focus', function () {
            //     console.log('ادیتور فوکوس شد');
            // });

            // editor.on('blur', function () {
            //     console.log('ادیتور فوکوس را از دست داد');
            // });

            // مثال استفاده:
            // setEditorContent('<p>سلام دنیا!</p>');
            // var content = getEditorContent();
            // clearEditor();
        }
    }
});
