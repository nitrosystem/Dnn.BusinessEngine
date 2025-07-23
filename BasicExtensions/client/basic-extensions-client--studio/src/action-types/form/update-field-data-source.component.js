import template from "./update-field-data-source.html";

class UpdateFieldDataSourceController {
    constructor() {
        "ngInject";
    }

    $onInit() {
        if (this.action.Settings.FieldId) this.updateFieldChange()
    }

    updateFieldChange() {
        const field = _.find(this.fields, (f) => {
            return f.Id == this.action.Settings.FieldId;
        });

        const datasource = field.DataSource;
        this.service = _.find(this.services, (s) => { return s.Id == datasource.ServiceId; });

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