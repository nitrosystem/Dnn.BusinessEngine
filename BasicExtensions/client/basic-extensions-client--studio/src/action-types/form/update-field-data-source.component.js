import template from "./update-field-data-source.html";

class UpdateFieldDataSourceController {
    constructor() {
        "ngInject";
    }

    $onInit() {
        if (this.action.Settings.FieldID) this.updateFieldChange()
    }

    updateFieldChange() {
        const field = _.find(this.fields, (f) => {
            return f.FieldID == this.action.Settings.FieldID;
        });

        const datasource = field.DataSource;
        this.service = _.find(this.services, (s) => {
            return s.ServiceId == datasource.ServiceId;
        });

        this.action.Params = datasource.ServiceParams;
    }
}

const UpdateFieldDataSource = {
    bindings: {
        action: "<",
        fields: "<",
        services: "<"
    },
    controller: UpdateFieldDataSourceController,
    controllerAs: "$",
    templateUrl: template,
};

export default UpdateFieldDataSource;