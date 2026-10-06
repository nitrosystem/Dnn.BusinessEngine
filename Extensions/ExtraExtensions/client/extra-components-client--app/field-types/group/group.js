function GroupController(controller, globalService) {
    this.init = (field) => {
        field.__ignoreReinit = true;
    };
}
