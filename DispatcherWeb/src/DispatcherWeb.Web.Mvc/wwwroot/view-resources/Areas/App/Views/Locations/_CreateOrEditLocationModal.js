(function ($) {
    app.modals.CreateOrEditLocationModal = function () {

        var _modalManager;
        var _locationService = abp.services.app.location;
        var _dtHelper = abp.helper.dataTables;
        var _permissions = {
            merge: abp.auth.hasPermission('Pages.Locations.Merge')
        };

        var _createOrEditLocationModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Locations/CreateOrEditLocationModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Locations/_CreateOrEditLocationModal.js',
            modalClass: 'CreateOrEditLocationModal',
            modalSize: 'lg'
        });

        var _modal = null;
        var _$form = null;
        var _locationId = null;
        var _mergeWithDuplicateSilently = null;
        var _map = null;
        var _mapMarker = null;
        var _nameAutocomplete = null;
        var _addressAutocomplete = null;
        var _geocoder = null;

        var _latitudeField = null;
        var _longitudeField = null;
        var _placeIdField = null;
        var _nameField = null;
        var _addressField = null;
        var _cityField = null;
        var _stateField = null;
        var _zipField = null;
        var _countryField = null;


        var saveLocationAsync = async function (forceStopOnDuplicate) {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                throw new Error("Form is not valid");
            }

            var location = _$form.serializeFormToObject();

            abp.ui.setBusy(_$form);
            _modalManager.setBusy(true);
            try {
                if (!_locationId) {
                    let duplicate = await _locationService.findExistingLocationDuplicate(location);
                    if (duplicate != null) {
                        if (forceStopOnDuplicate) {
                            if (await abp.message.confirm("This location already exists. Do you want to edit the existing location instead?")) {
                                _modalManager.close();
                                setTimeout(() => {
                                    _createOrEditLocationModal.open({ id: duplicate.id });
                                }, 300);
                            }
                            //in both cases (confirmed or not) we should stop the save process / reject the promise
                            throw new Error("Duplicate exists");
                        }
                        if (_mergeWithDuplicateSilently) {
                            abp.event.trigger('app.createOrEditLocationModalSaved', {
                                item: duplicate
                            });
                            _modalManager.setResult(duplicate);
                            _modalManager.close();
                            return;
                        }
                        abp.message.warn("This location already exists. You can't add it since it would be a duplicate entry.");
                        throw new Error("Form is not valid");
                    }
                }
                let editResult = await _locationService.editLocation(location);
                abp.notify.info('Saved successfully.');
                _$form.find("#Id").val(editResult.id);
                _locationId = editResult.id;
                location.Id = editResult.id;
                abp.event.trigger('app.createOrEditLocationModalSaved', {
                    item: editResult
                });
                return editResult;
            } finally {
                abp.ui.clearBusy(_$form);
                _modalManager.setBusy(false);
            }
        };

        this.init = function (modalManager) {
            _modalManager = modalManager;
            _modal = _modalManager.getModal();

            _$form = _modal.find('form');
            _$form.validate();
            $.validator.addMethod(
                "regex",
                function (value, element, regexp) {
                    var re = new RegExp(regexp, 'i');
                    return this.optional(element) || re.test(value);
                },
                "Please check your input."
            );
            //_$form.find('#Latitude').rules('add', { regex: app.regex.latitudeLongitude });
            //_$form.find('#Longitude').rules('add', { regex: app.regex.latitudeLongitude });
            _mergeWithDuplicateSilently = _$form.find('#MergeWithDuplicateSilently').val() === 'true';

            _latitudeField = _$form.find('#Latitude');
            _longitudeField = _$form.find('#Longitude');
            _placeIdField = _$form.find('#PlaceId');
            _nameField = _$form.find('#Name');
            _addressField = _$form.find('#StreetAddress');
            _cityField = _$form.find('#City'); //locality
            _stateField = _$form.find('#State');
            _zipField = _$form.find('#ZipCode');
            _countryField = _$form.find('#CountryCode');

            abp.helper.ui.initControls();

            _locationId = _$form.find("#Id").val();

            _$form.find("#CategoryId").select2Init({
                abpServiceMethod: abp.services.app.location.getLocationCategorySelectList,
                showAll: true,
                allowClear: false
            });

            var _createOrEditSupplierContactModal = new app.ModalManager({
                viewUrl: abp.appPath + 'app/Locations/CreateOrEditSupplierContactModal',
                scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Locations/_CreateOrEditSupplierContactModal.js',
                modalClass: 'CreateOrEditSupplierContactModal',
                modalSize: 'md'
            });

            var supplierContactsTable = $('#SupplierContactsTable');
            var supplierContactsGrid = supplierContactsTable.DataTableInit({
                paging: false,
                serverSide: true,
                processing: true,
                info: false,
                language: {
                    emptyTable: app.localize("NoDataInTheTableClick{0}ToAdd", app.localize("AddNewContact"))
                },
                ajax: function (data, callback, settings) {
                    if (_locationId === '') {
                        callback(_dtHelper.getEmptyResult());
                        return;
                    }
                    var abpData = _dtHelper.toAbpData(data);
                    $.extend(abpData, { locationId: _locationId });
                    _locationService.getSupplierContacts(abpData).done(function (abpResult) {
                        callback(_dtHelper.fromAbpResult(abpResult));
                        //abp.helper.ui.initControls();
                    });
                },
                columns: [
                    {
                        width: '20px',
                        className: 'control responsive',
                        orderable: false,
                        render: function () {
                            return '';
                        }
                    },
                    {
                        data: "name",
                        title: "Name"
                    },
                    {
                        data: "title",
                        title: "Title"
                    },
                    {
                        data: "phone",
                        title: "Phone"
                    },
                    {
                        data: "email",
                        title: "Email"
                    },
                    {
                        targets: 5,
                        data: null,
                        orderable: false,
                        autoWidth: false,
                        width: "10px",
                        responsivePriority: 1,
                        defaultContent: '',
                        rowAction: {
                            items: [{
                                text: '<i class="fa fa-edit"></i> ' + app.localize('Edit'),
                                className: "btn btn-sm btn-default",
                                action: function (data) {
                                    _createOrEditSupplierContactModal.open({ id: data.record.id });
                                }
                            }, {
                                text: '<i class="fa fa-trash"></i> ' + app.localize('Delete'),
                                className: "btn btn-sm btn-default",
                                action: function (data) {
                                    deleteSupplierContact(data.record);
                                }
                            }]
                        }
                    }
                ]
            });


            _modal.on('shown.bs.modal', function () {
                supplierContactsGrid
                    .columns.adjust()
                    .responsive.recalc();
            });

            _modal.find('#ContactsTabButton').click(function () {
                setTimeout(function () {
                    supplierContactsGrid
                        .columns.adjust()
                        .responsive.recalc();
                }, 30);
            });

            var reloadSupplierContactGrid = function () {
                supplierContactsGrid.ajax.reload();
            };

            abp.event.on('app.createOrEditSupplierContactModalSaved', function () {
                reloadSupplierContactGrid();
            });

            _modal.find("#CreateNewSupplierContactButton").click(async function (e) {
                e.preventDefault();
                if (_locationId === '') {
                    await saveLocationAsync(true);
                }
                _createOrEditSupplierContactModal.open({ locationId: _locationId });
            });

            supplierContactsTable.on('click', '.btnEditRow', function () {
                var supplierContactId = _dtHelper.getRowData(this).id;
                _createOrEditSupplierContactModal.open({ id: supplierContactId });
            });


            async function deleteSupplierContact(record) {
                if (await abp.message.confirm(
                    'Are you sure you want to delete the contact?'
                )) {
                    _locationService.deleteSupplierContact({
                        id: record.id
                    }).done(function () {
                        abp.notify.info('Successfully deleted.');
                        reloadSupplierContactGrid();
                    });
                }
            }

            initMapAndAddressControlsAsync();

        };

        this.focusOnDefaultElement = function () {
            if (_locationId) {
                return;
            }
            _nameField.focus();
        }

        function getLocationLatLng() {
            let lat = parseFloat(_latitudeField.val());
            let lng = parseFloat(_longitudeField.val());
            if (isNaN(lat) || isNaN(lng)) {
                return null;
            }
            return { lat, lng };
        }

        function setLocationLatLng(latLng) {
            _latitudeField.val(latLng ? latLng.lat : '');
            _longitudeField.val(latLng ? latLng.lng : '');
        }

        async function getMapFallbackLocationAsync() {
            //let billingAddress = abp.setting.get('App.UserManagement.BillingAddress');
            //if (billingAddress) {
            //    let place = await geocodeAddressAsync(billingAddress);
            //    if (place && place.geometry && place.geometry.location) {
            //        return place.geometry.location;
            //    }
            //}
            let defaultMapLocation = abp.setting.get('App.General.DefaultMapLocation');
            if (defaultMapLocation) {
                let [lat, lng] = defaultMapLocation.split(',', 2);
                return { lat: parseFloat(lat), lng: parseFloat(lng) };
            }
            let userLatLng = await getCurrentLocationAsync();
            if (userLatLng) {
                return userLatLng;
            }

            const fallbackPosition = { lat: 34.81683, lng: -82.37566 };
            return fallbackPosition;
        }

        function getCurrentLocationAsync() {
            return new Promise((resolve, reject) => {
                if (navigator.geolocation) {
                    navigator.geolocation.getCurrentPosition((pos) => resolve(pos ? { lat: pos.coords.latitude, lng: pos.coords.longitude } : null), onError);
                } else {
                    resolve(null);
                }

                function onError(error) {
                    console.error(error);
                    resolve(null);
                }
            });
        }

        async function initMapAndAddressControlsAsync() {
            await abp.maps.waitForGoogleMaps();

            _geocoder = new google.maps.Geocoder();
            if (_locationId && !getLocationLatLng() && _addressField.val()) {
                let place = await geocodeAddressAsync(getFilledAddress());
                if (place) {
                    setMarkerFromPlace(place);
                    _placeIdField.val(place.place_id);
                }
            }


            let markerPosition = getLocationLatLng();
            let mapPosition = markerPosition || await getMapFallbackLocationAsync();

            _map = new google.maps.Map(_modal.find("#map")[0], {
                center: mapPosition,
                zoom: 15,
                styles: [{
                    featureType: 'poi',
                    stylers: [{ visibility: 'off' }]  // Turn off points of interest.
                }, {
                    featureType: 'transit.station',
                    stylers: [{ visibility: 'off' }]  // Turn off bus stations, train stations, etc.
                }],
                disableDoubleClickZoom: true,
                streetViewControl: false,
                fullscreenControl: false
            });

            _map.controls[google.maps.ControlPosition.RIGHT_TOP].push(_modal.find('#mapHelp')[0]);

            _map.addListener("dblclick", (e) => {
                setLocationLatLng(e.latLng);
                placeMarkerAndPanTo(e.latLng);
                findAndSetAddressAsync(e.latLng);
            });

            if (markerPosition) {
                placeMarkerAndPanTo(markerPosition);
            }


            _addressAutocomplete = new google.maps.places.Autocomplete(_addressField[0], {
                //componentRestrictions: { country: ["us", "ca"] },
                fields: ["address_components", "geometry", "place_id", "name"],
                types: ["address"],
            });

            _addressAutocomplete.addListener("place_changed", () => {
                let place = _addressAutocomplete.getPlace();
                //console.log(place);
                fillAddressFromPlace(place);
                setMarkerFromPlace(place);
                _placeIdField.val(place.place_id);
            });

            _nameAutocomplete = new google.maps.places.Autocomplete(_nameField[0], {
                fields: ["address_components", "geometry", "place_id", "name"],
                types: ["establishment"],
            });

            _nameAutocomplete.addListener("place_changed", () => {
                let place = _nameAutocomplete.getPlace();
                //console.log(place);
                fillAddressFromPlace(place);
                setMarkerFromPlace(place);
                _placeIdField.val(place.place_id);
                _nameField.val(place.name);
            });
        }

        function setMarkerFromPlace(place) {
            if (!place.geometry || !place.geometry.location) {
                console.log("Returned place contains no geometry");
                return;
            }
            setLocationLatLng(place.geometry.location);
            placeMarkerAndPanTo(place.geometry.location);
        }

        function fillAddressFromPlace(place) {
            //if (!place.address_components) {
            //    console.log("Returned place contains no address_components");
            //    return;
            //}

            var address = abp.helper.googlePlacesHelper.parseAddressComponents(place.address_components);

            _addressField.val(address.streetAddress);
            _cityField.val(address.city);
            _stateField.val(address.state);
            _zipField.val(address.zipCode);
            _countryField.val(address.countryCode);
        }

        function placeMarkerAndPanTo(latLng) {
            if (_mapMarker) {
                _mapMarker.setMap(null);
                _mapMarker = null;
            }
            if (latLng) {
                _mapMarker = new google.maps.Marker({
                    position: latLng,
                    map: _map,
                    draggable: true,
                });
                _map.panTo(latLng);
                _mapMarker.addListener("dblclick", () => {
                    placeMarkerAndPanTo(null);
                });
                _mapMarker.addListener("dragend", (e) => {
                    setLocationLatLng(e.latLng);
                    findAndSetAddressAsync(e.latLng);
                });
            }
        }

        async function findAndSetAddressAsync(latLng) {
            //console.log('Updating address for: ' + JSON.stringify(latLng));
            abp.ui.setBusy(_$form);
            _modalManager.setBusy(true);
            try {
                let geocodeResponse = await _geocoder.geocode({ location: latLng });
                //console.log(geocodeResponse);
                if (!geocodeResponse.results[0]) {
                    return;
                }
                let place = geocodeResponse.results[0];
                fillAddressFromPlace(place);
                _placeIdField.val(place.place_id);
            } finally {
                abp.ui.clearBusy(_$form);
                _modalManager.setBusy(false);
            }
        }

        async function geocodeAddressAsync(address) {
            let geocodeResponse = await _geocoder.geocode({ address });
            //console.log(geocodeResponse);
            if (!geocodeResponse.results[0]) {
                return null;
            }
            let place = geocodeResponse.results[0];
            return place;
        }

        function getFilledAddress() {
            return `${_addressField.val()} ${_cityField.val()}, ${_stateField.val()} ${_zipField.val()}, ${_countryField.val()}`;
        }

        this.save = async function () {
            var editResult = await saveLocationAsync();
            if (editResult) {
                _modalManager.setResult(editResult);
            }
            _modalManager.close();
        };
    };
})(jQuery);