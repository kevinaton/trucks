(function () {
    jQuery.fn.dataListInit = function (dataListOptions) {
        var $element = $(this);
        var rows = [];
        var api = {
            addRow,
            deleteRow,
            readValuesFromModel
        };
        async function reloadData() {
            try {
                //empty();
                dataListHelpers.setBusy($element);
                var rowsData = await dataListOptions.listMethod();
                rows = rowsData.map(rowData => new DataListRow(rowData, api, dataListOptions));
                redraw();
            }
            finally {
                dataListHelpers.clearBusy($element);
            }
        }
        function empty() {
            rows.forEach(r => r.destroy());
            $element.empty();
        }
        function redraw() {
            empty();
            if (!rows.length) {
                $element.append(dataListHelpers.getNoRowsPanel());
            }
            else {
                rows.forEach(row => {
                    $element.append(row.draw());
                });
            }
        }
        function readValuesFromModel() {
            rows.forEach(row => row.readValuesFromModel());
        }
        async function deleteRow(row) {
            if (row.rowData['id']) {
                if (!await window.abp.message.confirm('Are you sure you want to delete this row?')) {
                    return;
                }
                try {
                    dataListHelpers.setBusy(row.panel);
                    await dataListOptions.deleteMethod(row.rowData);
                }
                finally {
                    dataListHelpers.clearBusy(row.panel);
                }
            }
            rows.splice(rows.indexOf(row), 1);
            row.panel.remove();
            if (!rows.length) {
                $element.append(dataListHelpers.getNoRowsPanel());
            }
        }
        function addRow(rowData = {}) {
            let row = new DataListRow(rowData, api, dataListOptions);
            if (!rows.length) {
                empty(); //remove the 'no data to display' element
            }
            $element.append(row.draw());
            row.setEditable(true);
            rows.push(row);
        }
        reloadData();
        return api;
    };
    class DataListRow {
        rowData;
        dataListApi;
        dataListOptions;
        cells = [];
        panel;
        isRowEditable = false;
        #rowContainer;
        #editButton;
        #deleteButton;
        #saveButton;
        #cancelButton;
        constructor(rowData, dataListApi, dataListOptions) {
            this.rowData = rowData;
            this.dataListApi = dataListApi;
            this.dataListOptions = dataListOptions;
        }
        draw() {
            this.panel = $('<div class="card card-collapsable card-collapse bg-light mb-4">').append($('<div class="card-header bg-light">').append($('<div class="d-flex- justify-content-between-">').append(this.#rowContainer = $('<div class="d-flex align-items-end flex-wrap">'))));
            this.cells = this.dataListOptions.columns.map(columnOptions => {
                return new DataListCell(this, columnOptions);
            });
            this.cells.forEach(cell => {
                this.#rowContainer.append(cell.draw());
            });
            this.#rowContainer.append($('<div class="datalist-button-container-placeholder" style="width: 120px; height: 66px;">') //todo set width to match the width of the rendered datalist-button-container later when more/less buttons are needed
            ).append($('<div class="datalist-button-container">').append(this.#editButton = $('<button type="button" class="btn btn-primary">').attr("title", "Edit").append($('<i class="fa fa-edit">'))).append(this.#deleteButton = $('<button type="button" class="btn btn-primary">').attr("title", "Delete").append($('<i class="fa fa-trash">'))).append(this.#cancelButton = $('<button type="button" class="btn btn-secondary close-button">').attr("title", "Cancel").append($('<i class="fa fa-undo">'))).append(this.#saveButton = $('<button type="button" class="btn btn-primary">').attr("title", "Save").append($('<i class="fa fa-save"></i>'))));
            this.#editButton.click(() => {
                this.setEditable(true);
            });
            this.#cancelButton.click(async () => {
                this.readValuesFromModel();
                this.setEditable(false);
                if (!this.rowData['id']) {
                    await this.dataListApi.deleteRow(this);
                }
            });
            this.#saveButton.click(async () => {
                let rowDataCopy = $.extend({}, this.rowData);
                try {
                    await this.writeValuesToModel();
                    dataListHelpers.setBusy(this.panel);
                    var result = await this.dataListOptions.editMethod(this.rowData);
                    this.rowData['id'] = result['id'];
                    this.setEditable(false);
                    this.readValuesFromModel();
                }
                catch {
                    $.extend(this.rowData, rowDataCopy); //reverting the model to the original values so that 'cancel' button works correctly
                }
                finally {
                    dataListHelpers.clearBusy(this.panel);
                }
            });
            this.#deleteButton.click(async () => {
                await this.dataListApi.deleteRow(this);
            });
            this.setEditable(this.isRowEditable);
            return this.panel;
        }
        setEditable(isEditable) {
            this.isRowEditable = isEditable;
            this.cells.forEach(cell => cell.setRowEditable(isEditable));
            this.#editButton.toggle(!isEditable);
            this.#deleteButton.toggle(!isEditable);
            this.#saveButton.toggle(isEditable);
            this.#cancelButton.toggle(isEditable);
        }
        readValuesFromModel() {
            this.cells.forEach(cell => cell.readValueFromModel());
        }
        async writeValuesToModel() {
            for (const cell of this.cells) {
                if (cell.columnOptions.saveOnChange) {
                    continue;
                }
                await cell.writeValueToModel();
            }
            //this.cells
            //    .filter(x => !x.columnOptions.saveOnChange)
            //    .forEach(cell => cell.writeValueToModel());
        }
        destroy() {
        }
    }
    class DataListCell {
        row;
        columnOptions;
        editor;
        isRowEditable = false;
        constructor(row, columnOptions) {
            this.row = row;
            this.columnOptions = columnOptions;
            this.editor = new this.columnOptions.editor(this, this.columnOptions);
        }
        draw() {
            return this.editor.draw();
        }
        readValueFromModel() {
            this.editor.readValueFromModel();
        }
        async writeValueToModel() {
            await this.editor.writeValueToModel();
        }
        setRowEditable(isRowEditable) {
            this.isRowEditable = isRowEditable;
            this.editor.setRowEditable(isRowEditable);
        }
    }
    class DataListAbstractCellEditor {
        cell;
        row;
        columnOptions;
        domElement;
        isRowEditable = false;
        isCellEditable = false;
        constructor(cell, columnOptions) {
            this.cell = cell;
            this.row = cell.row;
            this.columnOptions = columnOptions;
        }
        async setRowEditable(isRowEditable) {
            this.isRowEditable = isRowEditable;
            if (this.columnOptions.isEditable === undefined) {
                this.isCellEditable = isRowEditable;
            }
            else if (typeof this.columnOptions.isEditable === 'boolean') {
                this.isCellEditable = this.columnOptions.isEditable;
            }
            else if (typeof this.columnOptions.isEditable === 'function') {
                this.isCellEditable = await Promise.resolve(this.columnOptions.isEditable(this.cell.row.rowData, this.isRowEditable));
            }
            this.refreshEditableState();
        }
        async readValueFromModelInternal() {
            if (this.columnOptions.readData) {
                return await Promise.resolve(this.columnOptions.readData(this.row.rowData));
            }
            if (this.columnOptions.data) {
                return this.row.rowData[this.columnOptions.data];
            }
            return null;
        }
        async writeValueToModelInternal(value) {
            value = this.validate(value);
            if (this.columnOptions.writeData) {
                await Promise.resolve(this.columnOptions.writeData(value, this.row.rowData));
            }
            else if (this.columnOptions.data) {
                this.row.rowData[this.columnOptions.data] = value;
            }
        }
        throwValidationError(message, input) {
            input.focus();
            window.abp.notify.error(message);
            throw new Error(message);
        }
    }
    class TextCellEditor extends DataListAbstractCellEditor {
        input;
        editorOptions;
        draw() {
            this.editorOptions = this.columnOptions.editorOptions || {};
            this.domElement = $('<div class="form-group mr-4">').append($('<label class="control-label">').text(this.columnOptions.title)).append(this.input = $('<input class="form-control" type="text">'));
            if (this.columnOptions.width) {
                this.domElement.css('width', this.columnOptions.width);
            }
            if (this.editorOptions.required) {
                this.input.attr('requried', 'required');
            }
            if (this.editorOptions.maxLength) {
                this.input.attr('maxlength', this.editorOptions.maxLength);
            }
            this.readValueFromModel();
            this.input.blur(() => {
                if (this.columnOptions.saveOnChange) {
                    this.writeValueToModel();
                }
            });
            return this.domElement;
        }
        refreshEditableState() {
            this.input.prop('disabled', !this.isCellEditable);
        }
        async readValueFromModel() {
            var value = await this.readValueFromModelInternal() || '';
            this.input.val(value);
        }
        async writeValueToModel() {
            await this.writeValueToModelInternal(this.input.val());
        }
        validate(value) {
            if (this.editorOptions.required && !value) {
                let message = this.columnOptions.title + ' is required';
                this.throwValidationError(message, this.input);
            }
            return value;
        }
    }
    class DecimalCellEditor extends DataListAbstractCellEditor {
        input;
        editorOptions;
        draw() {
            this.editorOptions = this.columnOptions.editorOptions || {};
            this.domElement = $('<div class="form-group mr-4">').append($('<label class="control-label">').text(this.columnOptions.title)).append(this.input = $('<input class="form-control no-numeric-spinner" type="number">'));
            if (this.columnOptions.width) {
                this.domElement.css('width', this.columnOptions.width);
            }
            if (this.editorOptions.required) {
                this.input.attr('requried', 'required');
            }
            if (this.editorOptions.max) {
                this.input.attr('max', this.editorOptions.max);
            }
            if (this.editorOptions.min) {
                this.input.attr('min', this.editorOptions.min);
            }
            this.readValueFromModel();
            this.input.blur(() => {
                if (this.columnOptions.saveOnChange) {
                    this.writeValueToModel();
                }
            });
            return this.domElement;
        }
        refreshEditableState() {
            this.input.prop('disabled', !this.isCellEditable);
        }
        async readValueFromModel() {
            var value = await this.readValueFromModelInternal();
            this.input.val(value);
        }
        async writeValueToModel() {
            await this.writeValueToModelInternal(this.input.val());
        }
        validate(value) {
            if (this.editorOptions.allowNull === false && value === '') {
                value = 0;
            }
            if (this.editorOptions.required && !value) {
                let message = this.columnOptions.title + ' is required';
                this.throwValidationError(message, this.input);
            }
            if (this.editorOptions.max !== undefined && value > this.editorOptions.max || this.editorOptions.min !== undefined && value < this.editorOptions.min) {
                let sizeValidationMessage = this.editorOptions.max !== undefined && this.editorOptions.min !== undefined
                    ? `${this.columnOptions.title} has to be a number between ${this.editorOptions.min} and ${this.editorOptions.max}`
                    : this.editorOptions.min !== undefined
                        ? `${this.columnOptions.title} has to be a number larger than ${this.editorOptions.min}`
                        : `${this.columnOptions.title} has to be a number smaller than ${this.editorOptions.max}`;
                this.throwValidationError(sizeValidationMessage, this.input);
            }
            return value;
        }
    }
    class CheckboxCellEditor extends DataListAbstractCellEditor {
        input;
        draw() {
            let id = dataListHelpers.getUniqueElementId();
            this.domElement = $('<div class="form-group mr-4">').append($('<label class="m-checkbox">').attr('for', id).text(this.columnOptions.title).prepend(this.input = $('<input type="checkbox" value="true">').attr('id', id)).append($('<span>')));
            if (this.columnOptions.width) {
                this.domElement.css('width', this.columnOptions.width);
            }
            this.readValueFromModel();
            this.input.change(() => {
                if (this.columnOptions.saveOnChange) {
                    this.writeValueToModel();
                }
            });
            return this.domElement;
        }
        refreshEditableState() {
            this.input.prop('disabled', !this.isCellEditable);
        }
        async readValueFromModel() {
            var value = await this.readValueFromModelInternal() || false;
            this.input.prop('checked', value);
        }
        async writeValueToModel() {
            await this.writeValueToModelInternal(this.input.is(':checked'));
        }
        validate(value) {
            return value;
        }
    }
    window.dataList = {
        editors: {
            text: TextCellEditor,
            checkbox: CheckboxCellEditor,
            decimal: DecimalCellEditor
        }
    };
    var dataListHelpers = {
        getNoRowsPanel: function () {
            return $('<div class="card card-collapsable card-collapse bg-light mb-4">').append($('<div class="card-header bg-light">').append($('<div class="text-center mt-4 mb-4">').text(window.app.localize("NoDataInTheListClick{0}ToAdd", window.app.localize("AddNew")))));
        },
        getUniqueElementId: function () {
            return window.abp.helper.getUniqueElementId();
        },
        setBusy: function (element) {
            window.abp.ui.setBusy(element);
        },
        clearBusy: function (element) {
            window.abp.ui.clearBusy(element);
        }
    };
})();
