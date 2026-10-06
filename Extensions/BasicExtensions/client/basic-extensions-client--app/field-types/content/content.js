ComponentRegistry.register('Content', function (controller) {
    this.init = (field) => {
        field.__ignoreReinit = true;
    }
});