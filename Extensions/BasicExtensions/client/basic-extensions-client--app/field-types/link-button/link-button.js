ComponentRegistry.register('LinkButton', function (controller) {
    this.init = (field) => {
        field.__ignoreReinit = true;
    }
});